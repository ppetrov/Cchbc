using System;
using System.Collections.Generic;
using System.Linq;
using Cchbc.Data;
using Cchbc.Features.ExceptionsModule.Objects;
using Cchbc.Features.ExceptionsModule.Rows;

namespace Cchbc.Features.ExceptionsModule
{
	public static class ExceptionsDataProvider
	{
		public static IEnumerable<TimePeriodRow> GetTimePeriods(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return new[]
			{
				new TimePeriodRow(@"Last 1 hour", TimeSpan.FromHours(1), 30),
				new TimePeriodRow(@"Last 24 hours", TimeSpan.FromDays(1), 30),
				new TimePeriodRow(@"Last 7 days", TimeSpan.FromDays(7), 30),
				new TimePeriodRow(@"Last 30 days", TimeSpan.FromDays(30), 30),
				new TimePeriodRow(@"Last 365 days", TimeSpan.FromDays(365), 30),
			};
		}

		public static IEnumerable<VersionRow> GetVersions(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var query = @"SELECT ID, NAME FROM FEATURE_VERSIONS ORDER BY NAME";

			var versions = context.Execute(new Query<VersionRow>(query, r => new VersionRow(r.GetInt64(0), r.GetString(1))));
			versions.Insert(0, new VersionRow(-1, @"All"));
			return versions;
		}

		public static IEnumerable<FeatureExceptionEntry> GetExceptions(ExceptionsDataLoadParams dataLoadParams)
		{
			if (dataLoadParams == null) throw new ArgumentNullException(nameof(dataLoadParams));

			var settings = dataLoadParams.Settings;

			var query = @"
			SELECT E.ID, E.FEATURE_ID, E.EXCEPTION_ID, E.USER_ID, E.VERSION_ID, E.CREATED_AT
			FROM FEATURE_EXCEPTION_ENTRIES E
			ORDER BY E.CREATED_AT DESC
			LIMIT @maxEntries";

			if (settings.RemoveExcluded)
			{
				query = @"
			SELECT E.ID, E.FEATURE_ID, E.EXCEPTION_ID, E.USER_ID, E.VERSION_ID, E.CREATED_AT
			FROM FEATURE_EXCEPTION_ENTRIES E
			WHERE NOT EXISTS
				(SELECT 1
					FROM FEATURE_EXCEPTIONS_EXCLUDED EX
					WHERE E.EXCEPTION_ID = EX.EXCEPTION_ID)
			ORDER BY E.CREATED_AT DESC
			LIMIT @maxEntries";
			}

			var sqlParams = new[]
			{
				new QueryParameter(@"@maxEntries", settings.MaxExceptionEntries)
			};

			var context = dataLoadParams.Context;
			var exceptionEntries = context.Execute(new Query<ExceptionEntryRow>(query, FeatureExceptionEntryRowCreator, sqlParams));
			if (exceptionEntries.Count == 0) return Enumerable.Empty<FeatureExceptionEntry>().ToArray();

			var features = GetFeatures(context, exceptionEntries);
			var exceptions = GetExceptions(context, exceptionEntries);
			var users = GetUsers(context, exceptionEntries);
			var versions = GetVersions(context, exceptionEntries);

			var featureExceptions = new FeatureExceptionEntry[exceptionEntries.Count];
			for (var i = 0; i < exceptionEntries.Count; i++)
			{
				var r = exceptionEntries[i];
				featureExceptions[i] = new FeatureExceptionEntry(r.Id, features[r.FeatureId], exceptions[r.ExceptionId], users[r.UserId], versions[r.VersionId], r.CreatedAt);
			}

			return featureExceptions;
		}

		public static IEnumerable<ExceptionsCount> GetExceptionsCounts(ExceptionsDataLoadParams dataLoadParams)
		{
			if (dataLoadParams == null) throw new ArgumentNullException(nameof(dataLoadParams));

			var query = @"
			SELECT CREATED_AT FROM FEATURE_EXCEPTION_ENTRIES
			WHERE (@VERSION IS NULL OR VERSION_ID = @VERSION)
			AND (@FROMDATE <= CREATED_AT AND CREATED_AT <= @TODATE)";

			if (dataLoadParams.Settings.RemoveExcluded)
			{
				query = @"
				SELECT CREATED_AT FROM FEATURE_EXCEPTION_ENTRIES E
				WHERE (@VERSION IS NULL OR VERSION_ID = @VERSION)
				AND (@FROMDATE <= CREATED_AT AND CREATED_AT <= @TODATE)
				AND NOT EXISTS (SELECT 1 FROM FEATURE_EXCEPTIONS_EXCLUDED EX WHERE E.EXCEPTION_ID = EX.EXCEPTION_ID)";
			}

			var versionId = default(long?);
			var version = dataLoadParams.Version;
			if (version != null)
			{
				var value = version.Row.Id;
				if (value > 0)
				{
					versionId = value;
				}
			}

			var toDate = DateTime.Now;
			var timePeriodRow = dataLoadParams.TimePeriod.Row;
			var fromDate = toDate.Add(-timePeriodRow.TimeOffset);

			var sqlParams = new[]
			{
				new QueryParameter(@"@VERSION", versionId),
				new QueryParameter(@"@FROMDATE", fromDate),
				new QueryParameter(@"@TODATE", toDate),
			};

			var bukets = new int[timePeriodRow.ChartSamples];
			var interval = (toDate - fromDate).TotalSeconds / bukets.Length;

			var _ = new Dictionary<int, int>(0);
			dataLoadParams.Context.Fill(_, (r, m) =>
			{
				bukets[Math.Min((int)(Math.Round((toDate - r.GetDateTime(0)).TotalSeconds / interval, MidpointRounding.AwayFromZero)), bukets.Length - 1)]++;
			}, new Query(query, sqlParams));

			var counts = new ExceptionsCount[bukets.Length];
			for (var i = 0; i < bukets.Length; i++)
			{
				counts[i] = new ExceptionsCount(fromDate.AddSeconds(interval * i), bukets[bukets.Length - 1 - i]);
			}

			return counts;
		}

		private static Dictionary<long, FeatureRow> GetFeatures(IDbContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, FeatureRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.FeatureId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, (r, map) =>
				{
					var row = new FeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
					map.Add(row.Id, row);
				}, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, (r, map) =>
				{
					var row = new FeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
					map.Add(row.Id, row);
				}, query);
			}

			return values;
		}

		private static Dictionary<long, VersionRow> GetVersions(IDbContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, VersionRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query(@"SELECT ID, NAME FROM FEATURE_VERSIONS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.VersionId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, (r, map) =>
				{
					var row = new VersionRow(r.GetInt64(0), r.GetString(1));
					map.Add(row.Id, row);
				}, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, (r, map) =>
				{
					var row = new VersionRow(r.GetInt64(0), r.GetString(1));
					map.Add(row.Id, row);
				}, query);
			}

			return values;
		}

		private static Dictionary<long, ExceptionRow> GetExceptions(IDbContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, ExceptionRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.ExceptionId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, (r, map) =>
				{
					var row = new ExceptionRow(r.GetInt64(0), r.GetString(1));
					map.Add(row.Id, row);
				}, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, (r, map) =>
				{
					var row = new ExceptionRow(r.GetInt64(0), r.GetString(1));
					map.Add(row.Id, row);
				}, query);
			}

			return values;
		}

		private static Dictionary<long, UserRow> GetUsers(IDbContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, UserRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query(@"SELECT ID, NAME FROM FEATURE_USERS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.UserId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, (r, map) =>
				{
					var row = new UserRow(r.GetInt64(0), r.GetString(1));
					map.Add(row.Id, row);
				}, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, (r, map) =>
				{
					var row = new UserRow(r.GetInt64(0), r.GetString(1));
					map.Add(row.Id, row);
				}, query);
			}

			return values;
		}

		private static ExceptionEntryRow FeatureExceptionEntryRowCreator(IFieldDataReader r)
		{
			return new ExceptionEntryRow(r.GetInt64(0), r.GetInt64(1), r.GetInt64(2), r.GetInt64(3), r.GetInt64(4), r.GetDateTime(5));
		}

		private static QueryParameter[] CreateBatchParams(int batchSize)
		{
			var sqlParams = new QueryParameter[batchSize];

			for (var i = 0; i < sqlParams.Length; i++)
			{
				sqlParams[i] = new QueryParameter(@"p" + i);
			}

			return sqlParams;
		}
	}
}