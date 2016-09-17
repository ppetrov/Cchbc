using System;
using System.Collections.Generic;
using System.Linq;
using Cchbc.Data;
using Cchbc.Features.ExceptionsModule.Rows;

namespace Cchbc.Features.ExceptionsModule
{
	public static class ExceptionsDataProvider
	{
		public static IEnumerable<TimePeriodRow> GetTimePeriods(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// TODO : Load from database ???
			return new[]
			{
				new TimePeriodRow(@"Last 1 hour", TimeSpan.FromHours(1)),
				new TimePeriodRow(@"Last 24 hours", TimeSpan.FromHours(24)),
				new TimePeriodRow(@"Last 7 days", TimeSpan.FromDays(7)),
				new TimePeriodRow(@"Last 30 days", TimeSpan.FromDays(30)),
				new TimePeriodRow(@"All", null),
			};
		}

		public static IEnumerable<VersionRow> GetVersions(ITransactionContext context)
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

			var query = @"
			SELECT E.ID, E.FEATURE_ID, E.EXCEPTION_ID, E.USER_ID, E.VERSION_ID, E.CREATED_AT
			FROM FEATURE_EXCEPTION_ENTRIES E
			WHERE NOT EXISTS
				(SELECT 1
				 FROM FEATURE_EXCEPTIONS_EXCLUDED EX
				 WHERE E.EXCEPTION_ID = EX.EXCEPTION_ID)
			ORDER BY E.CREATED_AT DESC
			LIMIT @maxEntries";

			var sqlParams = new[]
			{
				new QueryParameter(@"@maxEntries", dataLoadParams.MaxEntries)
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
			SELECT DATE(CREATED_AT), COUNT(*) FROM FEATURE_EXCEPTION_ENTRIES
			WHERE (@VERSION IS NULL OR VERSION_ID = @VERSION)
					AND (@FROMDATE IS NULL OR (@FROMDATE <= CREATED_AT AND CREATED_AT <= @TODATE))
			GROUP BY DATE(CREATED_AT)";

			var value = dataLoadParams.Version.Row.Id;
			var version = default(long?);
			if (value > 0)
			{
				version = value;
			}

			var fromDate = default(DateTime?);
			var toDate = DateTime.Now;

			var timePeriod = dataLoadParams.TimePeriod.Row.TimeOffset;
			if (timePeriod.HasValue)
			{
				fromDate = toDate.Add(-timePeriod.Value);
			}

			var sqlParams = new[]
			{
				new QueryParameter(@"@VERSION", version),
				new QueryParameter(@"@FROMDATE", fromDate),
				new QueryParameter(@"@TODATE", toDate),
			};

			return dataLoadParams.Context.Execute(new Query<ExceptionsCount>(query, r => new ExceptionsCount(r.GetDateTime(0), r.GetInt32(1)), sqlParams));
		}

		private static Dictionary<long, FeatureRow> GetFeatures(ITransactionContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, FeatureRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<FeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new FeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.FeatureId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
			}

			return values;
		}

		private static Dictionary<long, VersionRow> GetVersions(ITransactionContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, VersionRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<VersionRow>(@"SELECT ID, NAME FROM FEATURE_VERSIONS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new VersionRow(r.GetInt64(0), r.GetString(1)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.VersionId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
			}

			return values;
		}

		private static Dictionary<long, ExceptionRow> GetExceptions(ITransactionContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, ExceptionRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<ExceptionRow>(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new ExceptionRow(r.GetInt64(0), r.GetString(1)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.ExceptionId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
			}

			return values;
		}

		private static Dictionary<long, UserRow> GetUsers(ITransactionContext context, List<ExceptionEntryRow> entries)
		{
			var values = new Dictionary<long, UserRow>(entries.Count);

			var batchSize = 16;
			var sqlParams = CreateBatchParams(batchSize);

			var query = new Query<UserRow>(@"SELECT ID, NAME FROM FEATURE_USERS WHERE ID IN (@p0,@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,@p11,@p12,@p13,@p14,@p15)", r => new UserRow(r.GetInt64(0), r.GetString(1)), sqlParams);

			var index = 0;
			foreach (var id in entries.Select(r => r.UserId).Distinct())
			{
				if (index < batchSize)
				{
					sqlParams[index++].Value = id;
					continue;
				}

				context.Fill(values, v => v.Id, query);

				sqlParams[0].Value = id;
				index = 1;
			}
			if (index > 0)
			{
				for (var i = index; i < sqlParams.Length; i++)
				{
					sqlParams[i].Value = -1;
				}
				context.Fill(values, v => v.Id, query);
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