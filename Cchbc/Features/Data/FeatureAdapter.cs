using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Data
{
	public static class FeatureAdapter
	{
		private static readonly Query InsertContextQuery = new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)",
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
				@"INSERT INTO FEATURE_ENTRIES(DETAILS, CREATED_AT, FEATURE_ID ) VALUES (@DETAILS, @CREATED_AT, @FEATURE)",
				new[]
				{
					new QueryParameter(@"@DETAILS", string.Empty),
					new QueryParameter(@"@CREATED_AT", DateTime.MinValue),
					new QueryParameter(@"@FEATURE", 0L)
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
		private static readonly Query<DbFeatureExceptionRow> GetDbFeatureExceptionsRowQuery = new Query<DbFeatureExceptionRow>(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS", DbFeatureExceptionRowCreator);
		private static readonly Query<DbFeatureRow> GetFeaturesQuery = new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);
		private static readonly Query<DbFeatureEntryRow> GetFeatureEntriesQuery = new Query<DbFeatureEntryRow>(@"SELECT ID, DETAILS, CREATED_AT, FEATURE_ID FROM FEATURE_ENTRIES", DbFeatureEntryRowCreator);
		private static readonly Query<DbFeatureExceptionEntryRow> GetDbFeatureExceptionEntryRowQuery = new Query<DbFeatureExceptionEntryRow>(@"SELECT EXCEPTION_ID, CREATED_AT, FEATURE_ID FROM FEATURE_EXCEPTION_ENTRIES", DbFeatureExceptionEntryRowCreator);

		private static readonly Query<int> GetExceptionQuery =
			new Query<int>(@"SELECT ID FROM FEATURE_EXCEPTIONS WHERE CONTENTS = @CONTENTS", r => r.GetInt32(0), new[]
			{
				new QueryParameter(@"@CONTENTS", string.Empty)
			});

		public static void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_CONTEXTS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_STEPS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTIONS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Contents nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Name nvarchar(254) NOT NULL, 
	Context_Id integer NOT NULL, 
	FOREIGN KEY (Context_Id)
		REFERENCES FEATURE_CONTEXTS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_ENTRIES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Details nvarchar(254) NOT NULL, 
	Created_At datetime NOT NULL, 
	Feature_Id integer NOT NULL, 
	FOREIGN KEY (Feature_Id)
		REFERENCES FEATURES (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTION_ENTRIES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Exception_Id integer NOT NULL, 
	Created_At datetime NOT NULL, 
	Feature_Id integer NOT NULL, 
	FOREIGN KEY (Feature_Id)
		REFERENCES FEATURES (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (Exception_Id)
		REFERENCES FEATURE_EXCEPTIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));
		}

		public static void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			foreach (var table in new[]
			{
				@"FEATURE_EXCEPTIONS_EXCLUDED",
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
		}

		public static ClientData GetData(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var contextRows = context.Execute(GetContextsQuery);
			var exceptionRows = context.Execute(GetDbFeatureExceptionsRowQuery);
			var featureRows = context.Execute(GetFeaturesQuery);
			var featureEntryRows = context.Execute(GetFeatureEntriesQuery);
			var exceptionEntryRows = context.Execute(GetDbFeatureExceptionEntryRowQuery);

			return new ClientData(contextRows, exceptionRows, featureRows, featureEntryRows, exceptionEntryRows);
		}

		public static Dictionary<string, DbFeatureContextRow> GetContexts(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var result = new Dictionary<string, DbFeatureContextRow>();

			context.Fill(result, (r, map) =>
			{
				var row = DbContextCreator(r);
				map.Add(row.Name, row);
			}, new Query(GetContextsQuery.Statement));

			return result;
		}

		public static List<DbFeatureRow> GetFeatures(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return context.Execute(GetFeaturesQuery);
		}

		public static int GetOrCreateException(ITransactionContext context, string contents)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			GetExceptionQuery.Parameters[0].Value = contents;

			var exceptions = context.Execute(GetExceptionQuery);
			if (exceptions.Count > 0)
			{
				return exceptions[0];
			}
			return InsertException(context, contents);
		}

		public static int InsertContext(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertContextQuery.Parameters[0].Value = name;

			context.Execute(InsertContextQuery);

			return (int)context.GetNewId();
		}

		public static int InsertStep(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertStepQuery.Parameters[0].Value = name;

			context.Execute(InsertStepQuery);

			return (int)context.GetNewId();
		}

		public static int InsertException(ITransactionContext context, string contents)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			// Set parameters values
			InsertExceptionQuery.Parameters[0].Value = contents;

			context.Execute(InsertExceptionQuery);

			return (int)context.GetNewId();
		}

		public static int InsertFeature(ITransactionContext context, string name, long contextId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertFeatureQuery.Parameters[0].Value = name;
			InsertFeatureQuery.Parameters[1].Value = contextId;

			context.Execute(InsertFeatureQuery);

			return (int)context.GetNewId();
		}

		public static void InsertFeatureEntry(ITransactionContext context, int featureId, TimeSpan timeSpent, string details)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (details == null) throw new ArgumentNullException(nameof(details));

			// Set parameters values
			InsertClientFeatureEntryQuery.Parameters[0].Value = Convert.ToDecimal(timeSpent.TotalMilliseconds);
			InsertClientFeatureEntryQuery.Parameters[1].Value = details;
			InsertClientFeatureEntryQuery.Parameters[2].Value = DateTime.Now;
			InsertClientFeatureEntryQuery.Parameters[3].Value = featureId;

			// Insert the record
			context.Execute(InsertClientFeatureEntryQuery);
		}

		public static void InsertExceptionEntry(ITransactionContext context, int featureId, int exceptionId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			InsertExceptionEntryQuery.Parameters[0].Value = exceptionId;
			InsertExceptionEntryQuery.Parameters[1].Value = DateTime.Now;
			InsertExceptionEntryQuery.Parameters[2].Value = featureId;

			context.Execute(InsertExceptionEntryQuery);
		}

		public static long ExecuteInsert(ITransactionContext context, Query query)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (query == null) throw new ArgumentNullException(nameof(query));

			// Insert the record
			context.Execute(query);

			// Get new Id back
			return context.GetNewId();
		}

		private static DbFeatureContextRow DbContextCreator(IFieldDataReader r)
		{
			return new DbFeatureContextRow(r.GetInt32(0), r.GetString(1));
		}

		private static DbFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new DbFeatureRow(r.GetInt32(0), r.GetString(1), r.GetInt32(2));
		}

		private static DbFeatureEntryRow DbFeatureEntryRowCreator(IFieldDataReader r)
		{
			return new DbFeatureEntryRow(r.GetInt64(0), r.GetString(1), r.GetDateTime(2), r.GetInt32(3));
		}

		private static DbFeatureExceptionRow DbFeatureExceptionRowCreator(IFieldDataReader r)
		{
			return new DbFeatureExceptionRow(r.GetInt32(0), r.GetString(1));
		}

		private static DbFeatureExceptionEntryRow DbFeatureExceptionEntryRowCreator(IFieldDataReader r)
		{
			return new DbFeatureExceptionEntryRow(r.GetInt32(0), r.GetDateTime(1), r.GetInt32(2));
		}
	}
}