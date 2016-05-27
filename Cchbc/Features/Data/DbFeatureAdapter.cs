using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.Features.Data
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
		private static readonly Query InsertStepEntryQuery = new Query(@"INSERT INTO FEATURE_STEP_ENTRIES(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", InsertFeatureStepEntrySqlParams);

		private static readonly Query<DbFeatureContextRow> GetContextsQuery = new Query<DbFeatureContextRow>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbContextCreator);
		private static readonly Query<DbFeatureStepRow> GetStepsQuery = new Query<DbFeatureStepRow>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator);
		private static readonly Query<DbFeatureExceptionRow> GetDbFeatureExceptionsRowQuery = new Query<DbFeatureExceptionRow>(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS", DbFeatureExceptionRowCreator);
		private static readonly Query<DbFeatureRow> GetFeaturesQuery = new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);
		private static readonly Query<DbFeatureEntryRow> GetFeatureEntriesQuery = new Query<DbFeatureEntryRow>(@"SELECT ID, TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID FROM FEATURE_ENTRIES", DbFeatureEntryRowCreator);
		private static readonly Query<DbFeatureEntryStepRow> GetFeatureEntryStepsQuery = new Query<DbFeatureEntryStepRow>(@"SELECT TIMESPENT, DETAILS, FEATURE_ENTRY_ID, FEATURE_STEP_ID FROM FEATURE_STEP_ENTRIES", EntryStepRowCreator);
		private static readonly Query<DbFeatureExceptionEntryRow> GetDbFeatureExceptionEntryRowQuery = new Query<DbFeatureExceptionEntryRow>(@"SELECT EXCEPTION_ID, CREATED_AT, FEATURE_ID FROM FEATURE_EXCEPTION_ENTRIES", DbFeatureExceptionEntryRowCreator);

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
CREATE TABLE[FEATURE_EXCEPTIONS] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	[Contents] nvarchar(254) NOT NULL
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
CREATE TABLE [FEATURE_EXCEPTION_ENTRIES] (
	[Id] integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	[Exception_Id] integer NOT NULL, 
	[Created_At] datetime NOT NULL, 
	[Feature_Id] integer NOT NULL, 
	FOREIGN KEY ([Feature_Id])
		REFERENCES [FEATURES] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY ([Exception_Id])
		REFERENCES [FEATURE_EXCEPTIONS] ([Id])
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE [FEATURE_STEP_ENTRIES] (
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

			context.Execute(new Query(@"DROP TABLE FEATURE_STEP_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_EXCEPTION_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_ENTRIES"));
			context.Execute(new Query(@"DROP TABLE FEATURES"));
			context.Execute(new Query(@"DROP TABLE FEATURE_STEPS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_CONTEXTS"));
			context.Execute(new Query(@"DROP TABLE FEATURE_EXCEPTIONS"));

			return Task.FromResult(true);
		}

		public static async Task<FeatureClientData> GetDataAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var contextRows = await GetContextsAsync(context);
			var stepRows = await GetStepsAsync(context);
			var exceptionRows = await GetExceptionsAsync(context);
			var featureRows = await GetFeaturesAsync(context);
			var featureEntryRows = await Task.FromResult(context.Execute(GetFeatureEntriesQuery));
			var entryStepRows = await Task.FromResult(context.Execute(GetFeatureEntryStepsQuery));
			var exceptionEntryRows = await Task.FromResult(context.Execute(GetDbFeatureExceptionEntryRowQuery));

			return new FeatureClientData(contextRows, stepRows, exceptionRows, featureRows, featureEntryRows, entryStepRows, exceptionEntryRows);
		}

		public static Task<List<DbFeatureContextRow>> GetContextsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetContextsQuery));
		}

		public static Task<List<DbFeatureStepRow>> GetStepsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetStepsQuery));
		}

		public static Task<List<DbFeatureExceptionRow>> GetExceptionsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetDbFeatureExceptionsRowQuery));
		}

		public static Task<List<DbFeatureRow>> GetFeaturesAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetFeaturesQuery));
		}

		public static Task<long> GetExceptionAsync(ITransactionContext context, string contents)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			var query =
				new Query<long>(@"SELECT Id FROM FEATURE_EXCEPTIONS WHERE CONTENTS = @CONTENTS",
					r => r.GetInt64(0), new[]
					{
						new QueryParameter(@"@CONTENTS", contents),
					});

			var exceptionId = -1L;
			var exceptions = context.Execute(query);
			if (exceptions.Count > 0)
			{
				exceptionId = exceptions[0];
			}
			return Task.FromResult(exceptionId);
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

		public static Task<long> InsertExceptionAsync(ITransactionContext context, string contents)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			var query = new Query(@"INSERT INTO FEATURE_EXCEPTIONS(CONTENTS) VALUES (@CONTENTS)", new[]
					{
						new QueryParameter(@"@CONTENTS", contents),
					});

			return ExecureInsertAsync(context, query);
		}

		public static Task<long> InsertExceptionEntryAsync(ITransactionContext context, long featureId, long exceptionId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var query = new Query(@"INSERT INTO FEATURE_EXCEPTION_ENTRIES(EXCEPTION_ID, CREATED_AT, FEATURE_ID) VALUES (@EXCEPTION, @CREATEAT, @FEATURE)", new[]
					{
						new QueryParameter(@"@EXCEPTION", exceptionId),
						new QueryParameter(@"@CREATEAT", DateTime.Now),
						new QueryParameter(@"@FEATURE", featureId),
					});

			return ExecureInsertAsync(context, query);
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

		private static DbFeatureEntryStepRow EntryStepRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryStepRow(r.GetDecimal(0), r.GetString(1), r.GetInt64(2), r.GetInt64(3));
		}

		private static DbFeatureExceptionRow DbFeatureExceptionRowCreator(IFieldDataReader r)
		{
			return new DbFeatureExceptionRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureExceptionEntryRow DbFeatureExceptionEntryRowCreator(IFieldDataReader r)
		{
			return new DbFeatureExceptionEntryRow(r.GetInt64(0), r.GetDateTime(1), r.GetInt64(2));
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