using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.Features.Data
{
	public static class FeatureAdapter
	{
		private static readonly Query InsertContext = new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)",
			new[]
			{
				new QueryParameter(@"@NAME", string.Empty)
			});

		private static readonly Query InsertStepQuery = new Query(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)",
			new[]
			{
				new QueryParameter(@"@NAME", string.Empty)
			});

		private static readonly Query InsertFeatureQuery =
			new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", new[]
			{
				new QueryParameter(@"@NAME", string.Empty),
				new QueryParameter(@"@CONTEXT", 0L)
			});

		private static readonly Query InsertClientFeatureEntryQuery =
			new Query(
				@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATED_AT, @FEATURE)",
				new[]
				{
					new QueryParameter(@"@TIMESPENT", 0M),
					new QueryParameter(@"@DETAILS", string.Empty),
					new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
					new QueryParameter(@"@FEATURE", 0L)
				});

		private static readonly Query InsertStepEntryQuery =
			new Query(
				@"INSERT INTO FEATURE_STEP_ENTRIES(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)",
				new[]
				{
					new QueryParameter(@"@ENTRY", 0L),
					new QueryParameter(@"@STEP", 0L),
					new QueryParameter(@"@TIMESPENT", 0M),
					new QueryParameter(@"@DETAILS", string.Empty)
				});

		private static readonly Query InsertExceptionQuery =
			new Query(@"INSERT INTO FEATURE_EXCEPTIONS(CONTENTS) VALUES (@CONTENTS)", new[]
			{
				new QueryParameter(@"@CONTENTS", string.Empty)
			});

		private static readonly Query InsertExceptionEntryQuery =
			new Query(
				@"INSERT INTO FEATURE_EXCEPTION_ENTRIES(EXCEPTION_ID, CREATED_AT, FEATURE_ID) VALUES (@EXCEPTION, @CREATEAT, @FEATURE)",
				new[]
				{
					new QueryParameter(@"@EXCEPTION", 0L),
					new QueryParameter(@"@CREATEAT", DateTime.Now),
					new QueryParameter(@"@FEATURE", 0L)
				});

		private static readonly Query<DbFeatureContextRow> GetContextsQuery = new Query<DbFeatureContextRow>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbContextCreator);
		private static readonly Query<DbFeatureStepRow> GetStepsQuery = new Query<DbFeatureStepRow>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepCreator);
		private static readonly Query<DbFeatureExceptionRow> GetDbFeatureExceptionsRowQuery = new Query<DbFeatureExceptionRow>(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS", DbFeatureExceptionRowCreator);
		private static readonly Query<DbFeatureRow> GetFeaturesQuery = new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);
		private static readonly Query<DbFeatureEntryRow> GetFeatureEntriesQuery = new Query<DbFeatureEntryRow>(@"SELECT ID, TIMESPENT, DETAILS, CREATED_AT, FEATURE_ID FROM FEATURE_ENTRIES", DbFeatureEntryRowCreator);
		private static readonly Query<DbFeatureEntryStepRow> GetFeatureEntryStepsQuery = new Query<DbFeatureEntryStepRow>(@"SELECT TIMESPENT, DETAILS, FEATURE_ENTRY_ID, FEATURE_STEP_ID FROM FEATURE_STEP_ENTRIES", EntryStepRowCreator);
		private static readonly Query<DbFeatureExceptionEntryRow> GetDbFeatureExceptionEntryRowQuery = new Query<DbFeatureExceptionEntryRow>(@"SELECT EXCEPTION_ID, CREATED_AT, FEATURE_ID FROM FEATURE_EXCEPTION_ENTRIES", DbFeatureExceptionEntryRowCreator);

		private static readonly Query<long> GetExceptionQuery =
			new Query<long>(@"SELECT ID FROM FEATURE_EXCEPTIONS WHERE CONTENTS = @CONTENTS", IdCreator, new[]
			{
				new QueryParameter(@"@CONTENTS", string.Empty)
			});

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

			foreach (var table in new[]
			{
				@"FEATURE_STEP_ENTRIES",
				@"FEATURE_EXCEPTION_ENTRIES",
				@"FEATURE_ENTRIES",
				@"FEATURES",
				@"FEATURE_STEPS",
				@"FEATURE_CONTEXTS",
				@"FEATURE_EXCEPTIONS"
			})
			{
				context.Execute(new Query(@"DROP TABLE " + table));
			}

			return Task.FromResult(true);
		}

		public static Task<ClientData> GetDataAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var contextRows = context.Execute(GetContextsQuery);
			var stepRows = context.Execute(GetStepsQuery);
			var exceptionRows = context.Execute(GetDbFeatureExceptionsRowQuery);
			var featureRows = context.Execute(GetFeaturesQuery);
			var featureEntryRows = context.Execute(GetFeatureEntriesQuery);
			var entryStepRows = context.Execute(GetFeatureEntryStepsQuery);
			var exceptionEntryRows = context.Execute(GetDbFeatureExceptionEntryRowQuery);

			return Task.FromResult(new ClientData(contextRows, stepRows, exceptionRows, featureRows, featureEntryRows, entryStepRows, exceptionEntryRows));
		}

		public static Task<Dictionary<string, DbFeatureContextRow>> GetContextsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<string, DbFeatureContextRow>();

			context.Fill(result, (r, map) =>
			{
				var row = DbContextCreator(r);
				map.Add(row.Name, row);
			}, new Query(GetContextsQuery.Statement));

			return Task.FromResult(result);
		}

		public static Task<Dictionary<string, DbFeatureStepRow>> GetStepsAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<string, DbFeatureStepRow>();

			context.Fill(result, (r, map) =>
			{
				var row = DbFeatureStepCreator(r);
				map.Add(row.Name, row);
			}, new Query(GetStepsQuery.Statement));

			return Task.FromResult(result);
		}

		public static Task<List<DbFeatureRow>> GetFeaturesAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return Task.FromResult(context.Execute(GetFeaturesQuery));
		}

		public static Task<long> GetOrCreateExceptionAsync(ITransactionContext context, string contents)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			GetExceptionQuery.Parameters[0].Value = contents;

			var exceptions = context.Execute(GetExceptionQuery);
			if (exceptions.Count > 0)
			{
				return Task.FromResult(exceptions[0]);
			}
			return InsertExceptionAsync(context, contents);
		}

		public static Task<long> InsertContextAsync(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertContext.Parameters[0].Value = name;

			return ExecuteInsertAsync(context, InsertContext);
		}

		public static Task<long> InsertStepAsync(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertStepQuery.Parameters[0].Value = name;

			return ExecuteInsertAsync(context, InsertStepQuery);
		}

		public static Task<long> InsertExceptionAsync(ITransactionContext context, string contents)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			InsertExceptionQuery.Parameters[0].Value = contents;

			return ExecuteInsertAsync(context, InsertExceptionQuery);
		}

		public static Task<long> InsertFeatureAsync(ITransactionContext context, string name, long contextId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertFeatureQuery.Parameters[0].Value = name;
			InsertFeatureQuery.Parameters[1].Value = contextId;

			// Insert the record
			return ExecuteInsertAsync(context, InsertFeatureQuery);
		}

		public static Task<long> InsertFeatureEntryAsync(ITransactionContext context, long featureId, TimeSpan timeSpent, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (details == null) throw new ArgumentNullException(nameof(details));

			// Set parameters values
			InsertClientFeatureEntryQuery.Parameters[0].Value = Convert.ToDecimal(timeSpent.TotalMilliseconds);
			InsertClientFeatureEntryQuery.Parameters[1].Value = details;
			InsertClientFeatureEntryQuery.Parameters[2].Value = DateTime.Now;
			InsertClientFeatureEntryQuery.Parameters[3].Value = featureId;

			return ExecuteInsertAsync(context, InsertClientFeatureEntryQuery);
		}

		public static Task<long> InsertExceptionEntryAsync(ITransactionContext context, long featureId, long exceptionId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			InsertExceptionEntryQuery.Parameters[0].Value = exceptionId;
			InsertExceptionEntryQuery.Parameters[1].Value = DateTime.Now;
			InsertExceptionEntryQuery.Parameters[2].Value = featureId;

			return ExecuteInsertAsync(context, InsertExceptionEntryQuery);
		}

		public static Task InsertStepEntryAsync(ITransactionContext context, long featureEntryId, long stepId, decimal timeSpent, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// Set parameters values
			InsertStepEntryQuery.Parameters[0].Value = featureEntryId;
			InsertStepEntryQuery.Parameters[1].Value = stepId;
			InsertStepEntryQuery.Parameters[2].Value = timeSpent;
			InsertStepEntryQuery.Parameters[3].Value = details;

			// Insert the record
			context.Execute(InsertStepEntryQuery);

			return Task.FromResult(true);
		}

		private static long IdCreator(IFieldDataReader r)
		{
			return r.GetInt64(0);
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

		public static Task<long> ExecuteInsertAsync(ITransactionContext context, Query query)
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