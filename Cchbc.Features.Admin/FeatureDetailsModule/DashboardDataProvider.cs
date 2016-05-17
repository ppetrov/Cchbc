using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public static class DashboardDataProvider
	{
		public static Task<List<DashboardUser>> GetUsersAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var statement = @"SELECT ID, NAME, REPLICATED_AT, VERSION_ID FROM FEATURE_USERS ORDER BY REPLICATED_AT DESC LIMIT @MAXUSERS";
			var sqlParams = new[]
			{
				new QueryParameter(@"@MAXUSERS", loadParams.Settings.MaxUsers),
			};
			var query = new Query<DbFeatureUserRow>(statement, r => new DbFeatureUserRow(r.GetInt64(0), r.GetString(1), r.GetDateTime(2), r.GetInt64(3)), sqlParams);

			var users = new List<DashboardUser>();
			var versions = loadParams.DataProvider.Versions;

			foreach (var userRow in loadParams.Context.Execute(query))
			{
				users.Add(new DashboardUser(userRow.Id, userRow.Name, versions[userRow.VersionId].Name, userRow.ReplicatedAt));
			}

			return Task.FromResult(users);
		}

		public static Task<List<DashboardVersion>> GetVersionsAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var context = loadParams.Context;
			var versions = loadParams.DataProvider.Versions;
			var dashboardVersions = new List<DashboardVersion>(versions.Count);

			var exceptions = CountExceptionsByVersion(context, versions);
			var users = CountUsersByVersion(context, versions);

			foreach (var versionPair in versions)
			{
				var versionId = versionPair.Key;
				var version = versionPair.Value;

				int usersCount;
				users.TryGetValue(versionId, out usersCount);

				int exceptionsCount;
				exceptions.TryGetValue(versionId, out exceptionsCount);

				dashboardVersions.Add(new DashboardVersion(version, usersCount, exceptionsCount));
			}

			dashboardVersions.Sort((x, y) =>
			{
				var cmp = string.Compare(x.Version.Name, y.Version.Name, StringComparison.OrdinalIgnoreCase);
				return cmp;
			});

			return Task.FromResult(dashboardVersions);
		}

		public static Task<List<DashboardException>> GetExceptionsAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var settings = loadParams.Settings;
			var relativeTimePeriod = settings.ExceptionsRelativeTimePeriod;
			var samples = settings.ExceptionsChartEntries;

			var rangeTimePeriod = relativeTimePeriod.ToRange(DateTime.Now);

			var query = new Query(@"SELECT CREATED_AT FROM FEATURE_EXCEPTIONS WHERE @FROMDATE <= CREATED_AT AND CREATED_AT <= @TODATE", new[]
			{
				new QueryParameter(@"@FROMDATE", rangeTimePeriod.FromDate),
				new QueryParameter(@"@TODATE", rangeTimePeriod.ToDate),
			});

			var step = ((long)relativeTimePeriod.TimeOffset.TotalMilliseconds) / samples;
			var baseDate = rangeTimePeriod.FromDate;

			var map = new int[samples];

			loadParams.Context.Fill(new Dictionary<DateTime, int>(0), (r, m) =>
			{
				var delta = ((long)(r.GetDateTime(0) - baseDate).TotalMilliseconds) / step;
				map[delta]++;
			}, query);

			var result = new List<DashboardException>(samples);

			for (var i = 0; i < map.Length; i++)
			{
				result.Add(new DashboardException(baseDate.AddMilliseconds(step * i), map[i]));
			}

			return Task.FromResult(result);
		}

		public static Task<List<DashboardFeatureByCount>> GetMostUsedFeaturesAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var query = @"SELECT FEATURE_ID, COUNT(*) CNT FROM FEATURE_ENTRIES GROUP BY FEATURE_ID ORDER BY CNT DESC LIMIT @MAXFEATURES";
			var sqlParams = new[]
			{
				new QueryParameter(@"@MAXFEATURES", loadParams.Settings.MaxMostUsedFeatures),
			};
			var result = loadParams.Context.Execute(new Query<Tuple<long, int>>(query, r => Tuple.Create(r.GetInt64(0), r.GetInt32(1)), sqlParams));

			var features = loadParams.DataProvider.Features;
			var mostUsedFeatures = new List<DashboardFeatureByCount>(result.Count);

			foreach (var tuple in result)
			{
				var featureId = tuple.Item1;
				var count = tuple.Item2;
				mostUsedFeatures.Add(new DashboardFeatureByCount(featureId, features[featureId].Name, count));
			}

			return Task.FromResult(mostUsedFeatures);
		}

		public static Task<List<DashboardFeatureByCount>> GetLeastUsedFeaturesAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			var mostUsedFeatures = new List<DashboardFeatureByCount>();

			mostUsedFeatures.Add(new DashboardFeatureByCount(3, @"Delete Activity", 3));

			return Task.FromResult(mostUsedFeatures);
		}

		public static Task<List<DashboardFeatureByTime>> GetSlowestFeaturesAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			return Task.FromResult(new List<DashboardFeatureByTime>(0));
		}

		private static Dictionary<long, int> CountExceptionsByVersion(ITransactionContext context, Dictionary<long, DbFeatureVersionRow> versions)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (versions == null) throw new ArgumentNullException(nameof(versions));

			var map = new Dictionary<long, int>(versions.Count);

			foreach (var row in versions)
			{
				map.Add(row.Key, 0);
			}

			context.Fill(map, (r, m) =>
			{
				var versionId = r.GetInt64(0);
				var count = r.GetInt32(1);

				m[versionId] = count;

			}, new Query(@"SELECT VERSION_ID, COUNT(*) FROM FEATURE_EXCEPTIONS GROUP BY VERSION_ID"));

			return map;
		}

		private static Dictionary<long, int> CountUsersByVersion(ITransactionContext context, Dictionary<long, DbFeatureVersionRow> versions)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (versions == null) throw new ArgumentNullException(nameof(versions));

			var map = new Dictionary<long, int>(versions.Count);

			foreach (var row in versions)
			{
				map.Add(row.Key, 0);
			}

			context.Fill(map, (r, m) =>
			{
				var versionId = r.GetInt64(0);
				var count = r.GetInt32(1);

				m[versionId] = count;

			}, new Query(@"SELECT VERSION_ID, COUNT(*) FROM FEATURE_USERS GROUP BY VERSION_ID"));

			return map;
		}
	}
}