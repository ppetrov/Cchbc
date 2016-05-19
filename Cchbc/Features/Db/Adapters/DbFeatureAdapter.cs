using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Db.Adapters
{
	public static class DbFeatureAdapter
	{
		private static readonly QueryParameter[] NameSqlParams =
		{
			new QueryParameter(@"@NAME", string.Empty)
		};

		private static readonly QueryParameter[] InsertFeatureSqlParams =
		{
			new QueryParameter(@"@NAME", string.Empty),
			new QueryParameter(@"@CONTEXT", 0L)
		};

		private static readonly QueryParameter[] InsertFeatureEntrySqlParams =
		{
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty),
			new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
			new QueryParameter(@"@FEATURE", 0L)
		};

		private static readonly QueryParameter[] InsertExcetionEntrySqlParams =
		{
			new QueryParameter(@"@MESSAGE", string.Empty),
			new QueryParameter(@"@STACKTRACE", string.Empty),
			new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
			new QueryParameter(@"@FEATURE", 0L)
		};

		private static readonly QueryParameter[] InsertFeatureStepEntrySqlParams =
		{
			new QueryParameter(@"@ENTRY", 0L),
			new QueryParameter(@"@STEP", 0L),
			new QueryParameter(@"@TIMESPENT", 0M),
			new QueryParameter(@"@DETAILS", string.Empty)
		};

		private static readonly Query InsertContextQuery = new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", NameSqlParams);
		private static readonly Query InsertStepQuery = new Query(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", NameSqlParams);
		private static readonly Query InsertFeatureQuery = new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", InsertFeatureSqlParams);
		private static readonly Query InsertClientFeatureEntryQuery = new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATED_AT, @FEATURE)", InsertFeatureEntrySqlParams);
		private static readonly Query InsertExceptionQuery = new Query(@"INSERT INTO FEATURE_EXCEPTIONS(MESSAGE, STACKTRACE, CREATED_AT, FEATURE_ID ) VALUES (@MESSAGE, @STACKTRACE, @CREATED_AT, @FEATURE)", InsertExcetionEntrySqlParams);
		private static readonly Query InsertStepEntryQuery = new Query(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", InsertFeatureStepEntrySqlParams);

		private static readonly Query<DbFeatureContextRow> GetContextsQuery = new Query<DbFeatureContextRow>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbContextCreator);
		private static readonly Query<DbFeatureStepRow> GetStepsQuery = new Query<DbFeatureStepRow>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator);
		private static readonly Query<DbFeatureRow> GetFeaturesQuery = new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);
		private static readonly Query<DbFeatureEntryRow> GetFeatureEntriesQuery = new Query<DbFeatureEntryRow>(@"SELECT ID, TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID FROM FEATURE_ENTRIES", DbFeatureEntryRowCreator);
		private static readonly Query<DbFeatureEntryStepRow> GetFeatureEntryStepsQuery = new Query<DbFeatureEntryStepRow>(@"SELECT TIMESPENT, DETAILS, FEATURE_ENTRY_ID, FEATURE_STEP_ID FROM FEATURE_ENTRY_STEPS", EntryStepRowCreator);
		private static readonly Query<DbFeatureExceptionRow> GetExceptionsQuery = new Query<DbFeatureExceptionRow>(@"SELECT MESSAGE, STACKTRACE, CREATED_AT, FEATURE_ID FROM FEATURE_EXCEPTIONS", DbExceptionRowCreator);

		public static Task CreateSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"
CREATE TABLE[FEATURE_CONTEXTS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE[FEATURE_STEPS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Name] nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Name] nvarchar(254) NOT NULL, 
	[Context_Id] integer NOT NULL, 
	FOREIGN KEY ([Context_Id])
		REFERENCES [FEATURE_CONTEXTS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NOT NULL, 
	[Created_At] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_EXCEPTIONS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Message] nvarchar(254) NOT NULL, 
	[StackTrace] nvarchar(254) NOT NULL, 
	[Created_At] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_ENTRY_STEPS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[TimeSpent] decimal(38, 0) NOT NULL, 
	[Details] nvarchar(254) NULL, 
	[Feature_Entry_Id] integer NOT NULL, 
	[Feature_Step_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Entry_Id])
		REFERENCES [FEATURE_ENTRIES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Feature_Step_Id])
		REFERENCES [FEATURE_STEPS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			return Task.FromResult(true);
		}

		public static Task DropSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRY_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_EXCEPTIONS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));

			return Task.FromResult(true);
		}

		public static async Task<FeatureClientData> GetDataAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var clientContextRows = await GetContextsAsync(context);
			var clientStepRows = await GetStepsAsync(context);
			var clientFeatureRows = await GetFeaturesAsync(context);
			var clientFeatureEntryRows = await GetFeatureEntryRowsAsync(context);
			var clientEntryStepRows = await GetEntryStepRowsAsync(context);
			var clientExceptionRows = await GetExceptionsAsync(context);

			return new FeatureClientData(clientContextRows, clientStepRows, clientFeatureRows, clientFeatureEntryRows, clientEntryStepRows, clientExceptionRows);
		}

		public static Task<List<DbFeatureContextRow>> GetContextsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetContextsQuery));
		}

		public static Task<Dictionary<long, DbFeatureContextRow>> GetContextsMappedByIdAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<long, DbFeatureContextRow>();

			context.Fill(result, (r, map) =>
			{
				var row = GetContextsQuery.Creator(r);
				map.Add(row.Id, row);
			}, new Query(GetContextsQuery.Statement));

			return Task.FromResult(result);
		}

		public static Task<Dictionary<string, DbFeatureContextRow>> GetContextsMappedByNameAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<string, DbFeatureContextRow>();

			context.Fill(new Dictionary<long, DbFeatureContextRow>(0), (r, map) =>
			{
				var row = GetContextsQuery.Creator(r);
				result.Add(row.Name, row);
			}, new Query(GetContextsQuery.Statement));

			return Task.FromResult(result);
		}

		public static Task<List<DbFeatureStepRow>> GetStepsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetStepsQuery));
		}

		public static Task<Dictionary<long, DbFeatureStepRow>> GetStepsMappedByIdAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<long, DbFeatureStepRow>();

			context.Fill(result, (r, map) =>
			{
				var row = GetStepsQuery.Creator(r);
				map.Add(row.Id, row);
			}, new Query(GetStepsQuery.Statement));

			return Task.FromResult(result);
		}

		public static Task<Dictionary<string, DbFeatureStepRow>> GetStepsMappedByNameAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<string, DbFeatureStepRow>();

			context.Fill(new Dictionary<long, DbFeatureStepRow>(0), (r, map) =>
			{
				var row = GetStepsQuery.Creator(r);
				result.Add(row.Name, row);
			}, new Query(GetStepsQuery.Statement));

			return Task.FromResult(result);
		}

		public static Task<List<DbFeatureRow>> GetFeaturesAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetFeaturesQuery));
		}

		public static Task<Dictionary<long, DbFeatureRow>> GetFeaturesMappedByIdAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<long, DbFeatureRow>();

			context.Fill(result, (r, map) =>
			{
				var row = GetFeaturesQuery.Creator(r);
				map.Add(row.Id, row);
			}, new Query(GetFeaturesQuery.Statement));

			return Task.FromResult(result);
		}

		public static Task<List<DbFeatureEntryRow>> GetFeatureEntryRowsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetFeatureEntriesQuery));
		}

		public static Task<List<DbFeatureEntryStepRow>> GetEntryStepRowsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetFeatureEntryStepsQuery));
		}

		public static Task<List<DbFeatureExceptionRow>> GetExceptionsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetExceptionsQuery));
		}

		public static Task<long> InsertContextAsync(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			NameSqlParams[0].Value = name;

			return ExecureInsertAsync(context, InsertContextQuery);
		}

		public static Task<long> InsertStepAsync(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			NameSqlParams[0].Value = name;

			return ExecureInsertAsync(context, InsertStepQuery);
		}

		public static Task<long> InsertFeatureAsync(ITransactionContext context, string name, long contextId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertFeatureSqlParams[0].Value = name;
			InsertFeatureSqlParams[1].Value = contextId;

			// Insert the record
			return ExecureInsertAsync(context, InsertFeatureQuery);
		}

		public static Task<long> InsertFeatureEntryAsync(ITransactionContext context, long featureId, Feature feature, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (details == null) throw new ArgumentNullException(nameof(details));

			// Set parameters values
			InsertFeatureEntrySqlParams[0].Value = Convert.ToDecimal(feature.TimeSpent.TotalMilliseconds);
			InsertFeatureEntrySqlParams[1].Value = details;
			InsertFeatureEntrySqlParams[2].Value = DateTime.Now;
			InsertFeatureEntrySqlParams[3].Value = featureId;

			return ExecureInsertAsync(context, InsertClientFeatureEntryQuery);
		}

		public static Task InsertExceptionEntryAsync(ITransactionContext context, long featureId, Exception exception)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			// Set parameters values
			InsertExcetionEntrySqlParams[0].Value = exception.Message;
			InsertExcetionEntrySqlParams[1].Value = exception.StackTrace;
			InsertExcetionEntrySqlParams[2].Value = DateTime.Now;
			InsertExcetionEntrySqlParams[3].Value = featureId;

			// Insert the record
			context.Execute(InsertExceptionQuery);

			return Task.FromResult(true);
		}

		public static Task InsertStepEntryAsync(ITransactionContext context, long featureEntryId, long stepId, decimal timeSpent, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// Set parameters values
			InsertFeatureStepEntrySqlParams[0].Value = featureEntryId;
			InsertFeatureStepEntrySqlParams[1].Value = stepId;
			InsertFeatureStepEntrySqlParams[2].Value = timeSpent;
			InsertFeatureStepEntrySqlParams[3].Value = details;

			// Insert the record
			context.Execute(InsertStepEntryQuery);

			return Task.FromResult(true);
		}

		private static DbFeatureContextRow DbContextCreator(IFieldDataReader r)
		{
			return new DbFeatureContextRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureStepRow DbFeatureStepCreator(IFieldDataReader r)
		{
			return new DbFeatureStepRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new DbFeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static DbFeatureEntryRow DbFeatureEntryRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryRow(r.GetInt64(0), r.GetDecimal(1), r.GetString(2), r.GetDateTime(3), r.GetInt64(4));
		}

		private static DbFeatureExceptionRow DbExceptionRowCreator(IFieldDataReader r)
		{
			return new DbFeatureExceptionRow(r.GetString(0), r.GetString(1), r.GetDateTime(2), r.GetInt64(3));
		}

		private static DbFeatureEntryStepRow EntryStepRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryStepRow(r.GetDecimal(0), r.GetString(1), r.GetInt64(2), r.GetInt64(3));
		}

		public static Task<long> ExecureInsertAsync(ITransactionContext context, Query query)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (query == null) throw new ArgumentNullException(nameof(query));

			// Insert the record
			context.Execute(query);

			// Get new Id back
			return Task.FromResult(context.GetNewId());
		}
	}
}