using System;
using System.Collections.Generic;
using System.Linq;
using Cchbc.Data;

namespace Cchbc.Features.Data
{
	public static class FeatureAdapter
	{
		private static readonly Query<long> GetNewIdQuery = new Query<long>(@"SELECT LAST_INSERT_ROWID()", r => r.GetInt64(0));

		private static readonly Query InsertContextQuery = new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)",
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

		private static readonly Query<FeatureContextRow> GetContextsQuery = new Query<FeatureContextRow>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbContextCreator);
		private static readonly Query<FeatureExceptionRow> GetDbFeatureExceptionsRowQuery = new Query<FeatureExceptionRow>(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS", DbFeatureExceptionRowCreator);
		private static readonly Query<FeatureRow> GetFeaturesQuery = new Query<FeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);
		private static readonly Query<FeatureEntryRow> GetFeatureEntriesQuery = new Query<FeatureEntryRow>(@"SELECT FEATURE_ID, DETAILS, CREATED_AT FROM FEATURE_ENTRIES", DbFeatureEntryRowCreator);
		private static readonly Query<FeatureExceptionEntryRow> GetFeatureExceptionEntriesQuery = new Query<FeatureExceptionEntryRow>(@"SELECT EXCEPTION_ID, CREATED_AT, FEATURE_ID FROM FEATURE_EXCEPTION_ENTRIES", DbFeatureExceptionEntryRowCreator);

		private static readonly Query<long> GetExceptionQuery =
			new Query<long>(@"SELECT ID FROM FEATURE_EXCEPTIONS WHERE CONTENTS = @CONTENTS", r => r.GetInt64(0), new[]
			{
				new QueryParameter(@"@CONTENTS", string.Empty)
			});

		/// <summary>
		/// Create the SQL schema
		/// </summary>
		/// <param name="dbContext"></param>
		public static void CreateSchema(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			dbContext.Execute(new Query(@"
CREATE TABLE FEATURE_CONTEXTS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
)"));

			dbContext.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTIONS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Contents nvarchar(254) NOT NULL
)"));

			dbContext.Execute(new Query(@"
CREATE TABLE FEATURES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Name nvarchar(254) NOT NULL, 
	Context_Id integer NOT NULL, 
	FOREIGN KEY (Context_Id)
		REFERENCES FEATURE_CONTEXTS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			dbContext.Execute(new Query(@"
CREATE TABLE FEATURE_ENTRIES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Details nvarchar(254) NOT NULL, 
	Created_At datetime NOT NULL, 
	Feature_Id integer NOT NULL, 
	FOREIGN KEY (Feature_Id)
		REFERENCES FEATURES (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			dbContext.Execute(new Query(@"
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

		/// <summary>
		/// Drop the SQL schema
		/// </summary>
		/// <param name="dbContext"></param>
		public static void DropSchema(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			foreach (var table in new[]
			{
				@"FEATURE_EXCEPTION_ENTRIES",
				@"FEATURE_ENTRIES",
				@"FEATURES",
				@"FEATURE_CONTEXTS",
				@"FEATURE_EXCEPTIONS"
			})
			{
				dbContext.Execute(new Query(@"DROP TABLE " + table));
			}
		}

		/// <summary>
		/// Get the new Id
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static long GetNewId(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		/// <summary>
		/// Get Contexts
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static Dictionary<string, FeatureContextRow> GetContexts(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			var contexts = new Dictionary<string, FeatureContextRow>(StringComparer.OrdinalIgnoreCase);

			dbContext.Fill(contexts, (r, map) =>
			{
				var row = DbContextCreator(r);
				map.Add(row.Name, row);
			}, new Query(GetContextsQuery.Statement));

			return contexts;
		}

		/// <summary>
		/// Get Features with ContextId as Key
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static Dictionary<long, List<FeatureRow>> GetFeatures(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			var featuresByContext = new Dictionary<long, List<FeatureRow>>();

			// Fetch & add new values
			foreach (var feature in dbContext.Execute(GetFeaturesQuery))
			{
				var contextId = feature.ContextId;

				// Find features by context
				List<FeatureRow> byContext;
				if (!featuresByContext.TryGetValue(contextId, out byContext))
				{
					byContext = new List<FeatureRow>();
					featuresByContext.Add(contextId, byContext);
				}

				byContext.Add(feature);
			}

			return featuresByContext;
		}

		/// <summary>
		/// Get Exceptions
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static IEnumerable<FeatureExceptionRow> GetExceptions(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			return dbContext.Execute(GetDbFeatureExceptionsRowQuery);
		}

		/// <summary>
		/// Get Feature Entries
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static IEnumerable<FeatureEntryRow> GetFeatureEntries(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			return dbContext.Execute(GetFeatureEntriesQuery);
		}

		/// <summary>
		/// Get Feature Exception entries
		/// </summary>
		/// <param name="dbContext"></param>
		/// <returns></returns>
		public static IEnumerable<FeatureExceptionEntryRow> GetFeatureExceptionEntries(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			return dbContext.Execute(GetFeatureExceptionEntriesQuery);
		}

		//TODO !!!
		public static Dictionary<long, Dictionary<string, long>> GetFeaturesByContext(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			var serverFeaturesByContext = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in dbContext.Execute(GetFeaturesQuery))
			{
				Dictionary<string, long> byContext;

				var contextId = feature.ContextId;
				if (!serverFeaturesByContext.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
					serverFeaturesByContext.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature.Id);
			}

			return serverFeaturesByContext;
		}

		/// <summary>
		/// Get or Create ExceptionId
		/// </summary>
		/// <param name="context"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public static long GetOrCreateException(IDbContext context, string contents)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			// Set parameters values
			GetExceptionQuery.Parameters[0].Value = contents;

			foreach (var exceptionId in context.Execute(GetExceptionQuery))
			{
				return exceptionId;
			}

			return InsertException(context, contents);
		}

		/// <summary>
		/// Insert Feature Context
		/// </summary>
		/// <param name="context"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public static long InsertContext(IDbContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertContextQuery.Parameters[0].Value = name;

			context.Execute(InsertContextQuery);

			return GetNewId(context);
		}

		/// <summary>
		/// Insert New Exception
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="contents"></param>
		/// <returns></returns>
		public static long InsertException(IDbContext dbContext, string contents)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			// Set parameters values
			InsertExceptionQuery.Parameters[0].Value = contents;

			dbContext.Execute(InsertExceptionQuery);

			return GetNewId(dbContext);
		}

		/// <summary>
		/// Insert Feature
		/// </summary>
		/// <param name="context"></param>
		/// <param name="name"></param>
		/// <param name="contextId"></param>
		/// <returns></returns>
		public static long InsertFeature(IDbContext context, string name, long contextId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			InsertFeatureQuery.Parameters[0].Value = name;
			InsertFeatureQuery.Parameters[1].Value = contextId;

			context.Execute(InsertFeatureQuery);

			return GetNewId(context);
		}

		/// <summary>
		/// Insert Feature Entry
		/// </summary>
		/// <param name="dbContext"></param>
		/// <param name="featureId"></param>
		/// <param name="details"></param>
		public static void InsertFeatureEntry(IDbContext dbContext, long featureId, string details)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (details == null) throw new ArgumentNullException(nameof(details));

			// Set parameters values
			InsertClientFeatureEntryQuery.Parameters[0].Value = details;
			InsertClientFeatureEntryQuery.Parameters[1].Value = DateTime.Now;
			InsertClientFeatureEntryQuery.Parameters[2].Value = featureId;

			// Insert the record
			dbContext.Execute(InsertClientFeatureEntryQuery);
		}

		/// <summary>
		/// Insert Exception entry
		/// </summary>
		/// <param name="context"></param>
		/// <param name="exceptionId"></param>
		/// <param name="featureId"></param>
		public static void InsertExceptionEntry(IDbContext context, long exceptionId, long featureId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			InsertExceptionEntryQuery.Parameters[0].Value = exceptionId;
			InsertExceptionEntryQuery.Parameters[1].Value = DateTime.Now;
			InsertExceptionEntryQuery.Parameters[2].Value = featureId;

			context.Execute(InsertExceptionEntryQuery);
		}

		private static FeatureContextRow DbContextCreator(IFieldDataReader r)
		{
			return new FeatureContextRow(r.GetInt64(0), r.GetString(1));
		}

		private static FeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new FeatureRow(r.GetInt32(0), r.GetString(1), r.GetInt32(2));
		}

		private static FeatureEntryRow DbFeatureEntryRowCreator(IFieldDataReader r)
		{
			return new FeatureEntryRow(r.GetInt64(0), r.GetDateTime(1), r.GetString(2));
		}

		private static FeatureExceptionRow DbFeatureExceptionRowCreator(IFieldDataReader r)
		{
			return new FeatureExceptionRow(r.GetInt32(0), r.GetString(1));
		}

		private static FeatureExceptionEntryRow DbFeatureExceptionEntryRowCreator(IFieldDataReader r)
		{
			return new FeatureExceptionEntryRow(r.GetInt32(0), r.GetDateTime(1), r.GetInt32(2));
		}
	}
}