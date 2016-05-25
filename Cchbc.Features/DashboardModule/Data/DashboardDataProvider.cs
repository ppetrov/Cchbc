using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.DashboardModule.Objects;
using Cchbc.Features.Db.Adapters;
using Cchbc.Logs;

namespace Cchbc.Features.DashboardModule.Data
{
	public static class DashboardDataProvider
	{
		public static Task<DashboardSettings> GetSettingsAsync(CoreContext coreContext)
		{
			if (coreContext == null) throw new ArgumentNullException(nameof(coreContext));

			coreContext.Feature.AddStep(nameof(GetSettingsAsync));

			var maxUsers = default(int?);
			var maxMostUsedFeatures = default(int?);

			var log = coreContext.Log;
			// TODO : Load settings from db for the context
			var contextForSettings = nameof(Dashboard);
			foreach (var setting in new List<Setting>())
			{
				var name = setting.Name;
				var value = setting.Value;

				if (name.Equals(nameof(DashboardSettings.MaxUsers), StringComparison.OrdinalIgnoreCase))
				{
					maxUsers = ValueParser.ParseInt(value, log);
					break;
				}
				if (name.Equals(nameof(DashboardSettings.MaxMostUsedFeatures), StringComparison.OrdinalIgnoreCase))
				{
					maxMostUsedFeatures = ValueParser.ParseInt(value, log);
					break;
				}
			}

			if (maxUsers == null)
			{
				log($@"Unable to find value for '{nameof(DashboardSettings.MaxUsers)}'", LogLevel.Warn);
			}
			if (maxMostUsedFeatures == null)
			{
				log($@"Unable to find value for '{nameof(DashboardSettings.MaxMostUsedFeatures)}'", LogLevel.Warn);
			}

			return Task.FromResult(DashboardSettings.Default);
		}

		public static async Task<DashboardCommonData> GetCommonDataAsync(CoreContext coreContext)
		{
			if (coreContext == null) throw new ArgumentNullException(nameof(coreContext));

			coreContext.Feature.AddStep(nameof(GetCommonDataAsync));

			var dbContext = coreContext.DbContext;
			var contexts = await DbFeatureAdapter.GetContextsMappedByIdAsync(dbContext);
			var features = await DbFeatureAdapter.GetFeaturesMappedByIdAsync(dbContext);

			return new DashboardCommonData(contexts, features);
		}

		public static Task<List<DashboardUser>> GetUsersAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			loadParams.CoreContext.Feature.AddStep(nameof(GetUsersAsync));

			var query = new Query<DashboardUser>(
				@"SELECT U.ID, U.NAME UNAME, U.REPLICATED_AT, V.NAME VNAME FROM FEATURE_USERS U INNER JOIN FEATURE_VERSIONS V ON U.VERSION_ID = V.ID ORDER BY REPLICATED_AT DESC LIMIT @MAXUSERS",
				r => new DashboardUser(r.GetInt64(0), r.GetString(1), r.GetDateTime(2), r.GetString(3)),
				new[]
				{
					new QueryParameter(@"@MAXUSERS", loadParams.Settings.MaxUsers),
				});

			var users = loadParams.CoreContext.DbContext.Execute(query);

			return Task.FromResult(users);
		}

		public static Task<List<DashboardVersion>> GetVersionsAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			loadParams.CoreContext.Feature.AddStep(nameof(GetVersionsAsync));

			var maxVersions = loadParams.Settings.MaxVersions;

			var query = new Query<DbFeatureVersionRow>(
				@"SELECT ID, NAME FROM FEATURE_VERSIONS ORDER BY ID DESC LIMIT @MAXVERSIONS",
				r => new DbFeatureVersionRow(r.GetInt64(0), r.GetString(1)),
				new[]
				{
					new QueryParameter(@"@MAXVERSIONS", maxVersions),
				});

			var dbContext = loadParams.CoreContext.DbContext;
			var versions = dbContext.Execute(query);

			var dashboardVersions = new List<DashboardVersion>(versions.Count);

			var exceptionsByVersion = CountExceptionsByVersion(dbContext, versions);
			var usersByVersion = CountUsersByVersion(dbContext, versions);

			foreach (var version in versions)
			{
				var versionId = version.Id;

				int usersCount;
				usersByVersion.TryGetValue(versionId, out usersCount);

				int exceptionsCount;
				exceptionsByVersion.TryGetValue(versionId, out exceptionsCount);

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

			var dbContext = loadParams.CoreContext.DbContext;
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


			dbContext.Fill(new Dictionary<long, int>(0), (r, m) =>
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

			// TODO : Add ability to exclude features - Sync Data for example
			return GetUsedFeaturesAsync(loadParams, @"SELECT FEATURE_ID, COUNT(*) CNT FROM FEATURE_ENTRIES GROUP BY FEATURE_ID ORDER BY CNT DESC LIMIT @MAXFEATURES", new[]
			{
				new QueryParameter(@"@MAXFEATURES", loadParams.Settings.MaxMostUsedFeatures),
			});
		}

		public static Task<List<DashboardFeatureByCount>> GetLeastUsedFeaturesAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			// TODO : Add ability to exclude features - View Params for example
			return GetUsedFeaturesAsync(loadParams, @"SELECT FEATURE_ID, COUNT(*) CNT FROM FEATURE_ENTRIES GROUP BY FEATURE_ID ORDER BY CNT ASC LIMIT @MAXFEATURES", new[]
			{
				new QueryParameter(@"@MAXFEATURES", loadParams.Settings.MaxLeastUsedFeatures),
			});
		}

		private static Task<List<DashboardFeatureByCount>> GetUsedFeaturesAsync(DashboarLoadParams loadParams, string query, QueryParameter[] sqlParams)
		{
			var dbContext = loadParams.CoreContext.DbContext;
			var features = loadParams.Data.Features;
			var contexts = loadParams.Data.Contexts;

			var mostUsedFeatures = dbContext.Execute(new Query<DashboardFeatureByCount>(query, r =>
			{
				var featureId = r.GetInt64(0);
				var feature = features[featureId];
				var context = contexts[feature.ContextId];
				return new DashboardFeatureByCount(context, feature, r.GetInt32(1));
			}, sqlParams));

			return Task.FromResult(mostUsedFeatures);
		}

		public static Task<List<DashboardFeatureByTime>> GetSlowestFeaturesAsync(DashboarLoadParams loadParams)
		{
			if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

			return Task.FromResult(new List<DashboardFeatureByTime>(0));
		}

		private static Dictionary<long, int> CountExceptionsByVersion(ITransactionContext context, List<DbFeatureVersionRow> versions)
		{
			return CountByVersion(context, versions, @"SELECT VERSION_ID, COUNT(*) FROM FEATURE_EXCEPTIONS GROUP BY VERSION_ID");
		}

		private static Dictionary<long, int> CountUsersByVersion(ITransactionContext context, List<DbFeatureVersionRow> versions)
		{
			return CountByVersion(context, versions, @"SELECT VERSION_ID, COUNT(*) FROM FEATURE_USERS GROUP BY VERSION_ID");
		}

		private static Dictionary<long, int> CountByVersion(ITransactionContext context, List<DbFeatureVersionRow> versions, string query)
		{
			var result = new Dictionary<long, int>(versions.Count);

			foreach (var version in versions)
			{
				result.Add(version.Id, 0);
			}

			context.Fill(result, (r, map) =>
			{
				var versionId = r.GetInt64(0);
				var count = r.GetInt32(1);

				map[versionId] = count;
			}, new Query(query));

			return result;
		}
	}
}