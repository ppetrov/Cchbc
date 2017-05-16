using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cchbc.Data;
//using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public sealed class ServerFeatureEntryRow
	{
		public long FeatureId;
		public string Details;
		public DateTime CreatedAt;
		public double Timespent;
	}

	public sealed class ServerFeatureExceptionEntryRow
	{
		public long ExceptionId;
		public DateTime CreatedAt;
		public long FeatureId;
	}

	public sealed class ServerFeatureRow
	{
		public long Id { get; }
		public string Name { get; }
		public long ContextId { get; }

		public ServerFeatureRow(long id, string name, long contextId)
		{
			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}

	public static class FeatureServerAdapter
	{
		private static readonly string SqliteFullDateTimeFormat = @"yyyy-MM-dd HH:mm:ss.fffffff";

		private static readonly Query<long> GetNewIdQuery = new Query<long>(@"SELECT LAST_INSERT_ROWID()", r => r.GetInt64(0));
		private static readonly Query<ServerFeatureRow> GetFeaturesQuery = new Query<ServerFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);

		public static void CreateSchema(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_CONTEXTS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTIONS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Contents nvarchar(254) NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_VERSIONS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL
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
CREATE TABLE FEATURE_USERS (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT,
	Name nvarchar(254) NOT NULL,
	Replicated_At datetime NOT NULL,
	Version_Id integer NOT NULL
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTION_ENTRIES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Exception_Id integer NOT NULL, 
	Created_At datetime NOT NULL, 
	Feature_Id integer NOT NULL, 
	User_Id integer NOT NULL, 
	Version_Id integer NOT NULL, 
	FOREIGN KEY (Feature_Id)
		REFERENCES FEATURES (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (User_Id)
		REFERENCES FEATURE_USERS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (Version_Id)
		REFERENCES FEATURE_VERSIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (Exception_Id)
		REFERENCES FEATURE_EXCEPTIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));

			context.Execute(new Query(@"
CREATE TABLE FEATURE_ENTRIES (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Details nvarchar(254) NULL, 
	Replicated_At datetime NOT NULL, 
	Timespent numeric NOT NULL,
	Feature_Id integer NOT NULL, 
	User_Id integer NOT NULL, 
	Version_Id integer NOT NULL, 
	FOREIGN KEY (Feature_Id)
		REFERENCES FEATURES (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (User_Id)
		REFERENCES FEATURE_USERS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
	FOREIGN KEY (Version_Id)
		REFERENCES FEATURE_VERSIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));
			context.Execute(new Query(@"
CREATE TABLE FEATURE_EXCEPTIONS_EXCLUDED (
	Id integer NOT NULL PRIMARY KEY AUTOINCREMENT, 
	Exception_Id bigint NOT NULL, 
	FOREIGN KEY (Exception_Id)
		REFERENCES FEATURE_EXCEPTIONS (Id)
		ON UPDATE CASCADE ON DELETE CASCADE
)"));
		}

		public static void DropSchema(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			foreach (var name in new[]
			{
				@"FEATURE_EXCEPTIONS_EXCLUDED",
				@"FEATURE_EXCEPTION_ENTRIES",
				@"FEATURE_ENTRIES",
				@"FEATURE_USERS",
				@"FEATURES",
				@"FEATURE_CONTEXTS",
				@"FEATURE_VERSIONS",
				@"FEATURE_EXCEPTIONS"
			})
			{
				context.Execute(new Query(@"DROP TABLE " + name));
			}
		}

		public static long GetOrInsertVersion(IDbContext dbContext, string version)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (version == null) throw new ArgumentNullException(nameof(version));

			// TODO : Avoid allocation
			// - of query
			// - of params
			var sqlParams = new[] { new QueryParameter(@"NAME", version) };

			var query = new Query<long?>(@"SELECT ID FROM FEATURE_VERSIONS WHERE NAME = @NAME", r =>
			{
				if (!r.IsDbNull(0))
				{
					return r.GetInt64(0);
				}
				return null;
			}, sqlParams);

			var versionId = dbContext.Execute(query).SingleOrDefault();
			if (versionId.HasValue)
			{
				return versionId.Value;
			}

			dbContext.Execute(new Query(@"INSERT INTO FEATURE_VERSIONS(NAME) VALUES (@NAME)", sqlParams));

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		public static long GetOrInsertUser(IDbContext dbContext, string userName, long versionId)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			// TODO : Avoid allocation
			// - of query
			// - of params
			var sqlParams = new[] { new QueryParameter(@"NAME", userName) };

			var query = new Query<long?>(@"SELECT ID FROM FEATURE_USERS WHERE NAME = @NAME", r =>
			{
				if (!r.IsDbNull(0))
				{
					return r.GetInt64(0);
				}
				return null;
			}, sqlParams);

			var userId = dbContext.Execute(query).SingleOrDefault();
			if (userId.HasValue)
			{
				return userId.Value;
			}

			sqlParams = new[]
			{
				new QueryParameter(@"NAME", userName),
				new QueryParameter(@"REPLICATED_AT", DateTime.Now),
				new QueryParameter(@"@VERSION_ID", versionId),
			};
			dbContext.Execute(new Query(@"INSERT INTO FEATURE_USERS(NAME, REPLICATED_AT, VERSION_ID) VALUES (@NAME, @REPLICATED_AT, @VERSION_ID)", sqlParams));

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}




























		private static readonly Query InsertExceptionQuery =
			new Query(@"INSERT INTO FEATURE_EXCEPTIONS(CONTENTS) VALUES (@CONTENTS)", new[]
			{
				new QueryParameter(@"@CONTENTS", string.Empty)
			});

		public static long InsertException(IDbContext dbContext, string contents)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			// Set parameters values
			InsertExceptionQuery.Parameters[0].Value = contents;

			dbContext.Execute(InsertExceptionQuery);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}



		public static void UpdateUser(IDbContext context, long userId, long versionId)
		{
			var sqlParams = new[]
			{
				new QueryParameter(@"@ID", userId),
				new QueryParameter(@"@REPLICATED_AT", DateTime.Now),
				new QueryParameter(@"@VERSION", versionId),
			};

			context.Execute(new Query(@"UPDATE FEATURE_USERS SET REPLICATED_AT = @REPLICATED_AT, VERSION_ID = @VERSION WHERE ID = @ID", sqlParams));
		}

		public static Dictionary<string, long> GetContexts(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, NAME FROM FEATURE_CONTEXTS");
		}

		public static Dictionary<string, long> GetExceptions(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return GetDataMapped(context, @"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS");
		}

		//TODO !!!
		public static Dictionary<long, Dictionary<string, long>> GetFeaturesByContext(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			var featuresByContext = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in dbContext.Execute(GetFeaturesQuery))
			{
				Dictionary<string, long> byContext;

				var contextId = feature.ContextId;
				if (!featuresByContext.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
					featuresByContext.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature.Id);
			}

			return featuresByContext;
		}

		private static ServerFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new ServerFeatureRow(r.GetInt32(0), r.GetString(1), r.GetInt32(2));
		}

		public static void InsertExceptionEntry(IDbContext context, ServerFeatureExceptionEntryRow[] exceptionEntryRows, long userId, long versionId, Dictionary<long, long> featuresMap, Dictionary<long, long> exceptionsMap)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (exceptionEntryRows == null) throw new ArgumentNullException(nameof(exceptionEntryRows));
			if (exceptionsMap == null) throw new ArgumentNullException(nameof(exceptionsMap));
			if (featuresMap == null) throw new ArgumentNullException(nameof(featuresMap));

			var buffer = new StringBuilder(@"INSERT INTO FEATURE_EXCEPTION_ENTRIES(EXCEPTION_ID, CREATED_AT, FEATURE_ID, USER_ID, VERSION_ID) VALUES ");

			var addComma = false;
			foreach (var r in exceptionEntryRows)
			{
				if (addComma)
				{
					buffer.Append(',');
				}

				buffer.Append('(');
				buffer.Append(exceptionsMap[r.ExceptionId]);
				buffer.Append(',');
				buffer.Append('\'');
				buffer.Append(r.CreatedAt.ToString(SqliteFullDateTimeFormat));
				buffer.Append('\'');
				buffer.Append(',');
				buffer.Append(featuresMap[r.FeatureId]);
				buffer.Append(',');
				buffer.Append(userId);
				buffer.Append(',');
				buffer.Append(versionId);
				buffer.Append(')');

				addComma = true;
			}

			context.Execute(new Query(buffer.ToString()));
		}

		public static void InsertFeatureEntry(IDbContext context, ServerFeatureEntryRow[] rows, long userId, long versionId, Dictionary<long, long> featuresMap)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (featuresMap == null) throw new ArgumentNullException(nameof(featuresMap));

			var buffer = new StringBuilder(@"INSERT INTO FEATURE_ENTRIES(DETAILS, REPLICATED_AT, TIMESPENT, FEATURE_ID, USER_ID, VERSION_ID) VALUES ");

			var addComma = false;
			foreach (var row in rows)
			{
				if (addComma)
				{
					buffer.Append(',');
				}

				buffer.Append('(');
				buffer.Append('\'');
				buffer.Append(row.Details);
				buffer.Append('\'');
				buffer.Append(',');
				buffer.Append('\'');
				buffer.Append(row.CreatedAt.ToString(SqliteFullDateTimeFormat));
				buffer.Append('\'');
				buffer.Append(',');
				buffer.Append(row.Timespent);
				buffer.Append(',');
				buffer.Append(featuresMap[row.FeatureId]);
				buffer.Append(',');
				buffer.Append(userId);
				buffer.Append(',');
				buffer.Append(versionId);
				buffer.Append(')');

				addComma = true;
			}

			context.Execute(new Query(buffer.ToString()));
		}

		private static Dictionary<string, long> GetDataMapped(IDbContext context, string statement)
		{
			var result = new Dictionary<string, long>(32);

			context.Fill(result, (r, map) => { map.Add(r.GetString(1), r.GetInt64(0)); }, new Query(statement));

			return result;
		}

		public static long InsertContext(IDbContext dbContext, string name)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (name == null) throw new ArgumentNullException(nameof(name));

			var query = new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)",
			new[]
			{
				new QueryParameter(@"@NAME", name)
			});

			dbContext.Execute(query);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		public static long InsertFeature(IDbContext dbContext, string name, long contextId)
		{
			var query = new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", new[]
			{
				new QueryParameter(@"@NAME", name),
				new QueryParameter(@"@CONTEXT", contextId)
			});

			dbContext.Execute(query);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}
	}
}