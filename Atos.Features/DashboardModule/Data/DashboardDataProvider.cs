using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atos.Common;
using Atos.Data;
using Atos.Features.DashboardModule.Objects;
using Atos.Features.Data;
using Atos.Logs;
using Atos.Settings;

namespace Atos.Features.DashboardModule.Data
{
	public static class DashboardDataProvider
	{
		//public static Task<DashboardSettings> GetSettingsAsync(FeatureContext featureContext)
		//{
		//	if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));

		//	var feature = featureContext.Feature;

		//	{
		//		var maxUsers = default(int?);
		//		var maxMostUsedFeatures = default(int?);

		//		var log = featureContext.MainContext.Log;
		//		// TODO : Load settings from db for the context
		//		var contextForSettings = nameof(Dashboard);
		//		var settings = new List<Setting>();
		//		foreach (var setting in settings)
		//		{
		//			var name = setting.Name;
		//			var value = setting.Value;

		//			if (name.Equals(nameof(DashboardSettings.MaxUsers), StringComparison.OrdinalIgnoreCase))
		//			{
		//				maxUsers = ValueParser.ParseInt(value, log);
		//				break;
		//			}
		//			if (name.Equals(nameof(DashboardSettings.MaxMostUsedFeatures), StringComparison.OrdinalIgnoreCase))
		//			{
		//				maxMostUsedFeatures = ValueParser.ParseInt(value, log);
		//				break;
		//			}
		//		}
		//		if (maxUsers == null)
		//		{
		//			log($@"Unable to find value for '{nameof(DashboardSettings.MaxUsers)}'", LogLevel.Warn);
		//		}
		//		if (maxMostUsedFeatures == null)
		//		{
		//			log($@"Unable to find value for '{nameof(DashboardSettings.MaxMostUsedFeatures)}'", LogLevel.Warn);
		//		}
		//		return Task.FromResult(DashboardSettings.Default);
		//	}
		//}

		//public static Task<DashboardCommonData> GetCommonDataAsync(FeatureContext featureContext)
		//{
		//	if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));

		//	{
		//		var contexts = new Dictionary<long, DbFeatureContextRow>();
		//		var features = new Dictionary<long, DbFeatureRow>();

		//		var dbContext = featureContext.DbContext;
		//		dbContext.Fill(contexts, (r, map) =>
		//		{
		//			var row = new DbFeatureContextRow(r.GetInt32(0), r.GetString(1));
		//			map.Add(row.Id, row);
		//		}, new Query(@"SELECT ID, NAME FROM FEATURE_CONTEXTS"));

		//		dbContext.Fill(features, (r, map) =>
		//		{
		//			var row = new DbFeatureRow(r.GetInt32(0), r.GetString(1), r.GetInt32(2));
		//			map.Add(row.Id, row);
		//		}, new Query(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES"));

		//		return Task.FromResult(new DashboardCommonData(contexts, features));
		//	}
		//}

		//public static Task<List<DashboardUser>> GetUsersAsync(DashboarLoadParams loadParams)
		//{
		//	if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

		//	var context = loadParams.FeatureContext;
		//	{
		//		var statement = @"SELECT U.ID, U.NAME UNAME, U.REPLICATED_AT, V.NAME VNAME FROM FEATURE_USERS U INNER JOIN FEATURE_VERSIONS V ON U.VERSION_ID = V.ID ORDER BY REPLICATED_AT DESC LIMIT @MAXUSERS";

		//		var sqlParams = new[]
		//		{
		//			new QueryParameter(@"@MAXUSERS", loadParams.Settings.MaxUsers),
		//		};

		//		var query = new Query<DashboardUser>(statement, r => new DashboardUser(r.GetInt64(0), r.GetString(1), r.GetDateTime(2), r.GetString(3)), sqlParams);
		//		var users = context.DbContext.Execute(query);

		//		return Task.FromResult(users);
		//	}
		//}

		//public static Task<List<DashboardVersion>> GetVersionsAsync(DashboarLoadParams loadParams)
		//{
		//	if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

		//	var context = loadParams.FeatureContext;
		//	var feature = context.Feature;
		//	{
		//		var maxVersions = loadParams.Settings.MaxVersions;

		//		var statement = @"SELECT ID, NAME FROM FEATURE_VERSIONS ORDER BY ID DESC LIMIT @MAXVERSIONS";

		//		var sqlParams = new[]
		//		{
		//			new QueryParameter(@"@MAXVERSIONS", maxVersions),
		//		};

		//		var query = new Query<DbFeatureVersionRow>(statement, r => new DbFeatureVersionRow(r.GetInt32(0), r.GetString(1)), sqlParams);

		//		var dbContext = context.DbContext;
		//		var versions = dbContext.Execute(query);
		//		var exceptionsByVersion = CountExceptionsByVersion(dbContext);
		//		var usersByVersion = CountUsersByVersion(dbContext);

		//		var dashboardVersions = new List<DashboardVersion>(versions.Count);
		//		foreach (var version in versions)
		//		{
		//			var versionId = version.Id;

		//			int usersCount;
		//			usersByVersion.TryGetValue(versionId, out usersCount);

		//			int exceptionsCount;
		//			exceptionsByVersion.TryGetValue(versionId, out exceptionsCount);

		//			dashboardVersions.Add(new DashboardVersion(version, usersCount, exceptionsCount));
		//		}

		//		dashboardVersions.Sort((x, y) =>
		//		{
		//			var cmp = string.Compare(x.Version.Name, y.Version.Name, StringComparison.OrdinalIgnoreCase);
		//			return cmp;
		//		});

		//		return Task.FromResult(dashboardVersions);
		//	}
		//}

		//public static Task<List<DashboardException>> GetExceptionsAsync(DashboarLoadParams loadParams)
		//{
		//	if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

		//	var feature = loadParams.FeatureContext.Feature;
		//	{
		//		var dbContext = loadParams.FeatureContext.DbContext;
		//		var settings = loadParams.Settings;
		//		var relativeTimePeriod = settings.ExceptionsRelativeTimePeriod;
		//		var samples = settings.ExceptionsChartEntries;

		//		var rangeTimePeriod = relativeTimePeriod.ToRange(DateTime.Now);
		//		var unit = ((long)relativeTimePeriod.TimeOffset.TotalMilliseconds) / samples;
		//		var baseDate = rangeTimePeriod.FromDate;

		//		var map = new int[samples];

		//		var statement = @"SELECT CREATED_AT FROM FEATURE_EXCEPTION_ENTRIES WHERE @FROMDATE <= CREATED_AT AND CREATED_AT <= @TODATE";

		//		var sqlParams = new[]
		//		{
		//			new QueryParameter(@"@FROMDATE", rangeTimePeriod.FromDate),
		//			new QueryParameter(@"@TODATE", rangeTimePeriod.ToDate),
		//		};

		//		var query = new Query(statement, sqlParams);
		//		dbContext.Fill(new Dictionary<long, int>(0), (r, m) =>
		//		{
		//			var delta = ((long)(r.GetDateTime(0) - baseDate).TotalMilliseconds) / unit;
		//			map[delta]++;
		//		}, query);

		//		var result = new List<DashboardException>(samples);

		//		for (var i = 0; i < map.Length; i++)
		//		{
		//			result.Add(new DashboardException(baseDate.AddMilliseconds(unit * i), map[i]));
		//		}

		//		return Task.FromResult(result);
		//	}
		//}

		//public static Task<List<DashboardFeatureByCount>> GetMostUsedFeaturesAsync(DashboarLoadParams loadParams)
		//{
		//	if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

		//	var tmp = new List<DashboardFeatureByCount>();

		//	var ctxs = loadParams.Data.Contexts;
		//	var fs = loadParams.Data.Features;

		//	var f = fs[1];
		//	var c = ctxs[f.ContextId];

		//	tmp.Add(new DashboardFeatureByCount(c, f, 7));
		//	tmp.Add(new DashboardFeatureByCount(c, f, 11));
		//	tmp.Add(new DashboardFeatureByCount(c, f, 13));
		//	tmp.Add(new DashboardFeatureByCount(c, f, 15));
		//	tmp.Add(new DashboardFeatureByCount(c, f, 17));
		//	tmp.Add(new DashboardFeatureByCount(c, f, 23));

		//	return Task.FromResult(tmp);

		//	// TODO : Add ability to exclude features - Sync Data for example
		//	return GetUsedFeaturesAsync(loadParams, @"SELECT FEATURE_ID, COUNT(*) CNT FROM FEATURE_ENTRIES GROUP BY FEATURE_ID ORDER BY CNT DESC LIMIT @MAXFEATURES", new[]
		//	{
		//		new QueryParameter(@"@MAXFEATURES", loadParams.Settings.MaxMostUsedFeatures),
		//	});
		//}

		//public static Task<List<DashboardFeatureByCount>> GetLeastUsedFeaturesAsync(DashboarLoadParams loadParams)
		//{
		//	if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

		//	// TODO : Add ability to exclude features - View Params for example
		//	return GetUsedFeaturesAsync(loadParams, @"SELECT FEATURE_ID, COUNT(*) CNT FROM FEATURE_ENTRIES GROUP BY FEATURE_ID ORDER BY CNT ASC LIMIT @MAXFEATURES", new[]
		//	{
		//		new QueryParameter(@"@MAXFEATURES", loadParams.Settings.MaxLeastUsedFeatures),
		//	});
		//}

		//private static Task<List<DashboardFeatureByCount>> GetUsedFeaturesAsync(DashboarLoadParams loadParams, string query, QueryParameter[] sqlParams)
		//{
		//	var dbContext = loadParams.FeatureContext.DbContext;
		//	var features = loadParams.Data.Features;
		//	var contexts = loadParams.Data.Contexts;

		//	var mostUsedFeatures = dbContext.Execute(new Query<DashboardFeatureByCount>(query, r =>
		//	{
		//		var featureId = r.GetInt64(0);
		//		var feature = features[featureId];
		//		var context = contexts[feature.ContextId];
		//		return new DashboardFeatureByCount(context, feature, r.GetInt32(1));
		//	}, sqlParams));

		//	return Task.FromResult(mostUsedFeatures);
		//}

		//public static Task<List<DashboardFeatureByTime>> GetSlowestFeaturesAsync(DashboarLoadParams loadParams)
		//{
		//	if (loadParams == null) throw new ArgumentNullException(nameof(loadParams));

		//	return Task.FromResult(new List<DashboardFeatureByTime>(0));
		//}

		//private static Dictionary<long, int> CountExceptionsByVersion(IDbContext context)
		//{
		//	return CountByVersion(context, @"SELECT VERSION_ID, COUNT(*) FROM FEATURE_EXCEPTION_ENTRIES GROUP BY VERSION_ID");
		//}

		//private static Dictionary<long, int> CountUsersByVersion(IDbContext context)
		//{
		//	return CountByVersion(context, @"SELECT VERSION_ID, COUNT(*) FROM FEATURE_USERS GROUP BY VERSION_ID");
		//}

		//private static Dictionary<long, int> CountByVersion(IDbContext context, string query)
		//{
		//	var result = new Dictionary<long, int>();

		//	context.Fill(result, (r, map) =>
		//	{
		//		var versionId = r.GetInt64(0);
		//		var count = r.GetInt32(1);

		//		map[versionId] = count;
		//	}, new Query(query));

		//	return result;
		//}
	}
}