using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace ConsoleClient
{
	public sealed class Activation
	{
		public string Id { get; }
		public ConcurrentQueue<long> Outlets { get; }

		public Activation(string id, ConcurrentQueue<long> outlets)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));

			this.Id = id;
			this.Outlets = outlets;
		}
	}

	public static class ActivationHelper
	{
		private sealed class OutletMatch
		{
			public long Outlet { get; }
			public int GroupMatches { get; set; }

			public OutletMatch(long outlet)
			{
				this.Outlet = outlet;
			}
		}

		public static IEnumerable<Activation> GetActivations(OracleConnection cn)
		{
			if (cn == null) throw new ArgumentNullException(nameof(cn));

			var activations = new Dictionary<string, HashSet<string>>();

			using (var cmd = cn.CreateCommand())
			{
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = @"
				SELECT DISTINCT AD.ACTIVATION_ID, C.PARAM_NAME
				FROM PHOENIX.ACTIVATION_DEFINITIONS AD
				INNER JOIN PHOENIX.ACTIVATION_COMPLIANCES C
				ON AD.ACTIVATION_ID = C.ACTIVATION_ID
				  AND AD.NUMBER_OF_ACTIVITIES = 0
				  AND AD.IS_MANDATORY = 1
				  AND TRUNC(SYSDATE) <= TRUNC(NVL(ACTIVATION_END, SYSDATE))";

				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						if (r.IsDBNull(0) ||
							r.IsDBNull(1))
						{
							continue;
						}
						var activationId = r.GetString(0);
						var name = r.GetString(1);

						HashSet<string> groups;
						if (!activations.TryGetValue(activationId, out groups))
						{
							groups = new HashSet<string>();
							activations.Add(activationId, groups);
						}

						groups.Add(name);
					}
				}
			}

			if (activations.Count == 0)
			{
				Trace.WriteLine(@"All activations are processed(Activities > 0) and respective activities are generated");
				yield break;
			}

			foreach (var a in activations)
			{
				var id = a.Key;
				var groups = a.Value;

				var outlets = GetOutlets(cn, id, groups.ToArray());
				if (outlets.Count == 0)
				{
					Trace.WriteLine($@"No matching outlets for Activation('{id}')");
					continue;
				}

				yield return new Activation(id, outlets);
			}
		}

		public static int CountOutlets(OracleConnection cn, string activationId, string[] activationGroups)
		{
			if (cn == null) throw new ArgumentNullException(nameof(cn));
			if (activationId == null) throw new ArgumentNullException(nameof(activationId));
			if (activationGroups == null) throw new ArgumentNullException(nameof(activationGroups));

			var outletMatches = GetOutletMatches(cn, activationId, activationGroups);

			var count = 0;

			var totalGroups = activationGroups.Length;
			foreach (var match in outletMatches.Values)
			{
				if (match.GroupMatches == totalGroups)
				{
					count++;
				}
			}

			return count;
		}

		private static ConcurrentQueue<long> GetOutlets(OracleConnection cn, string activationId, string[] activationGroups)
		{
			var outletMatches = GetOutletMatches(cn, activationId, activationGroups);

			var outlets = new ConcurrentQueue<long>();

			var totalGroups = activationGroups.Length;
			foreach (var match in outletMatches.Values)
			{
				if (match.GroupMatches == totalGroups)
				{
					outlets.Enqueue(match.Outlet);
				}
			}

			return outlets;
		}

		private static Dictionary<long, OutletMatch> GetOutletMatches(OracleConnection cn, string activationId, string[] activationGroups)
		{
			var outletMatches = new Dictionary<long, OutletMatch>();

			foreach (var g in activationGroups)
			{
				var groupOutlets = default(IEnumerable<long>);

				var stringComparison = StringComparison.OrdinalIgnoreCase;

				if (g.Equals(@"HIER", stringComparison))
				{
					groupOutlets = GetOutletsByHierLevel(cn, activationId);
				}
				if (g.Equals(@"TRADECH", stringComparison))
				{
					groupOutlets = GetOutletsByTradeChannels(cn, activationId);
				}
				if (g.Equals(@"SUBTRADECH", stringComparison))
				{
					groupOutlets = GetOutletsBySubTradeChannels(cn, activationId);
				}
				if (g.Equals(@"CCAF", stringComparison))
				{
					groupOutlets = GetOutletsByCcafs(cn, activationId);
				}
				if (g.Equals(@"CLUSTER", stringComparison))
				{
					groupOutlets = GetOutletsByClusters(cn, activationId);
				}
				if (g.Equals(@"CUSTOMER", stringComparison))
				{
					groupOutlets = GetOutletsByCustomers(cn, activationId);
				}
				if (groupOutlets != null)
				{
					foreach (var outlet in groupOutlets)
					{
						OutletMatch match;
						if (!outletMatches.TryGetValue(outlet, out match))
						{
							outletMatches.Add(outlet, new OutletMatch(outlet));
						}
						match.GroupMatches++;
					}
				}
			}

			return outletMatches;
		}

		private static IEnumerable<long> GetOutletsByCustomers(OracleConnection cn, string activation)
		{
			var query = @"
			SELECT o.outlet_number
			FROM phoenix.outlets o
			INNER JOIN phoenix.ACTIVATION_COMPLIANCES ac
			ON ac.param_value = o.outlet_number
			AND ac.param_name = 'CUSTOMER'
			INNER JOIN phoenix.activation_definitions ad
			ON ac.activation_id    = ad.activation_id
			WHERE ac.activation_id = :activation
			AND EXISTS
			  (SELECT 1
			  FROM phoenix.OUTLET_ASSIGNMENTS oa
			  WHERE oa.outlet_number          = o.OUTLET_NUMBER
			  AND TRUNC(ad.activation_start) <= TRUNC(oa.to_date)
			  AND TRUNC(oa.from_date)        <= TRUNC(ad.activation_end)
			  )";

			return GetOutletsByQuery(cn, activation, query);
		}

		private static IEnumerable<long> GetOutletsByClusters(OracleConnection cn, string activation)
		{
			var query = @"
			SELECT o.outlet_number
			FROM phoenix.outlets o
			INNER JOIN phoenix.outlet_market_attributes om
			ON o.outlet_number = om.OUTLET_NUMBER
			AND om.charact     = 'ZXTEL_CLUSTER'
			INNER JOIN phoenix.ACTIVATION_COMPLIANCES ac
			ON ac.param_value = om.value_neutral
			AND ac.param_name = 'CLUSTER'
			INNER JOIN phoenix.activation_definitions ad
			ON ac.activation_id    = ad.activation_id
			WHERE ac.activation_id = :activation
			AND EXISTS
			  (SELECT 1
			  FROM phoenix.OUTLET_ASSIGNMENTS oa
			  WHERE oa.outlet_number          = o.OUTLET_NUMBER
			  AND TRUNC(ad.activation_start) <= TRUNC(oa.to_date)
			  AND TRUNC(oa.from_date)        <= TRUNC(ad.activation_end)
			  )";

			return GetOutletsByQuery(cn, activation, query);
		}

		private static IEnumerable<long> GetOutletsByCcafs(OracleConnection cn, string activation)
		{
			var query = @"
			SELECT o.outlet_number
			FROM phoenix.outlets o
			INNER JOIN phoenix.outlet_market_attributes om
			ON o.outlet_number = om.OUTLET_NUMBER
			AND om.charact     = 'Z_CCAF'
			INNER JOIN phoenix.ACTIVATION_COMPLIANCES ac
			ON ac.param_value = om.value_neutral
			AND ac.param_name = 'CCAF'
			INNER JOIN phoenix.activation_definitions ad
			ON ac.activation_id    = ad.activation_id
			WHERE ac.activation_id = :activation
			AND EXISTS
			  (SELECT 1
			  FROM phoenix.OUTLET_ASSIGNMENTS oa
			  WHERE oa.outlet_number          = o.OUTLET_NUMBER
			  AND TRUNC(ad.activation_start) <= TRUNC(oa.to_date)
			  AND TRUNC(oa.from_date)        <= TRUNC(ad.activation_end)
			  )";

			return GetOutletsByQuery(cn, activation, query);
		}

		private static IEnumerable<long> GetOutletsBySubTradeChannels(OracleConnection cn, string activation)
		{
			var query = @"
			SELECT o.outlet_number
			FROM phoenix.outlets o
			INNER JOIN phoenix.SUB_TRADE_CHANNELS stc
			ON o.trade_channel_code = stc.code
			INNER JOIN phoenix.ACTIVATION_COMPLIANCES ac
			ON ac.param_value = stc.ATTRIB_7
			AND ac.param_name = 'SUBTRADECH'
			INNER JOIN phoenix.activation_definitions ad
			ON ac.activation_id    = ad.activation_id
			WHERE ac.activation_id = :activation
			AND EXISTS
			  (SELECT 1
			  FROM phoenix.OUTLET_ASSIGNMENTS oa
			  WHERE oa.outlet_number          = o.OUTLET_NUMBER
			  AND TRUNC(ad.activation_start) <= TRUNC(oa.to_date)
			  AND TRUNC(oa.from_date)        <= TRUNC(ad.activation_end)
			  )";

			return GetOutletsByQuery(cn, activation, query);
		}

		private static IEnumerable<long> GetOutletsByTradeChannels(OracleConnection cn, string activation)
		{
			var query = @"
			SELECT o.outlet_number
			FROM phoenix.outlets o
			INNER JOIN phoenix.trade_channels tc
			ON o.trade_channel_code = tc.code
			INNER JOIN phoenix.ACTIVATION_COMPLIANCES ac
			ON ac.param_value = tc.attrib_6
			AND ac.param_name = 'TRADECH'
			INNER JOIN phoenix.activation_definitions ad
			ON ac.activation_id    = ad.activation_id
			WHERE ac.activation_id = :activation
			AND EXISTS
			  (SELECT 1
			  FROM phoenix.OUTLET_ASSIGNMENTS oa
			  WHERE oa.outlet_number          = o.OUTLET_NUMBER
			  AND TRUNC(ad.activation_start) <= TRUNC(oa.to_date)
			  AND TRUNC(oa.from_date)        <= TRUNC(ad.activation_end)
			  )";

			return GetOutletsByQuery(cn, activation, query);
		}

		private static IEnumerable<long> GetOutletsByHierLevel(OracleConnection cn, string activation)
		{
			var query = @"
			SELECT DISTINCT outlet_number
			FROM phoenix.OUTLET_HIER_LEVELS hl
			INNER JOIN phoenix.ACTIVATION_COMPLIANCES ac
			ON AC.PARAM_VALUE = hl.parent_outlet_number
			AND ac.param_name = 'HIER'
			INNER JOIN phoenix.activation_definitions ad
			ON ac.activation_id    = ad.activation_id
			WHERE ac.ACTIVATION_ID = :activation
			AND EXISTS
			  (SELECT 1
			  FROM phoenix.OUTLET_ASSIGNMENTS oa
			  WHERE oa.outlet_number          = hl.OUTLET_NUMBER
			  AND TRUNC(ad.activation_start) <= TRUNC(oa.to_date)
			  AND TRUNC(oa.from_date)        <= TRUNC(ad.activation_end)
			  )";

			return GetOutletsByQuery(cn, activation, query);
		}

		private static IEnumerable<long> GetOutletsByQuery(OracleConnection cn, string activation, string query)
		{
			using (var cmd = cn.CreateCommand())
			{
				cmd.CommandText = query;
				cmd.CommandType = CommandType.Text;

				cmd.Parameters.Add(new OracleParameter(@"activation", OracleDbType.NVarchar2) { Value = activation });

				using (var r = cmd.ExecuteReader())
				{
					while (r.Read())
					{
						yield return r.GetInt64(0);
					}
				}
			}
		}
	}
}