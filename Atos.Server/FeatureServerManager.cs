using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Atos.Client.Data;

namespace Atos.Server
{
	public static class FeatureServerManager
	{
		private sealed class ServerFeatureRow
		{
			public long Id;
			public string Name;
			public long ContextId;
		}

		private static readonly string SqliteFullDateTimeFormat = @"yyyy-MM-dd HH:mm:ss.fffffff";

		private static readonly Query<long> GetNewIdQuery = new Query<long>(@"SELECT LAST_INSERT_ROWID()", IdCreator);

		private static readonly Query<ServerFeatureRow> GetFeaturesQuery =
			new Query<ServerFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator);

		private static readonly Query InsertExceptionQuery =
			new Query(@"INSERT INTO FEATURE_EXCEPTIONS(CONTENTS) VALUES (@CONTENTS)", new[]
			{
				new QueryParameter(@"@CONTENTS", string.Empty)
			});

		private static readonly Query<long> GetVersionQuery =
			new Query<long>(@"SELECT ID FROM FEATURE_VERSIONS WHERE NAME = @NAME", IdCreator, new[]
			{
				new QueryParameter(@"NAME", string.Empty)
			});

		private static readonly Query<long> GetUserQuery =
			new Query<long>(@"SELECT ID FROM FEATURE_USERS WHERE NAME = @NAME", IdCreator, new[]
			{
				new QueryParameter(@"NAME", string.Empty)
			});

		private static readonly Query InsertVersionQuery =
			new Query(@"INSERT INTO FEATURE_VERSIONS(NAME) VALUES (@NAME)", new[]
			{
				new QueryParameter(@"NAME", string.Empty)
			});

		private static readonly Query InsertUserQuery =
			new Query(@"INSERT INTO FEATURE_USERS(NAME, REPLICATED_AT, VERSION_ID) VALUES (@NAME, @REPLICATED_AT, @VERSION_ID)",
				new[]
				{
					new QueryParameter(@"NAME", string.Empty),
					new QueryParameter(@"REPLICATED_AT", DateTime.Today),
					new QueryParameter(@"@VERSION_ID", 0),
				});

		private static readonly Query UpdateUserQuery =
			new Query(@"UPDATE FEATURE_USERS SET REPLICATED_AT = @REPLICATED_AT, VERSION_ID = @VERSION WHERE ID = @ID",
				new[]
				{
				new QueryParameter(@"@ID", 0L),
				new QueryParameter(@"@REPLICATED_AT", DateTime.Now),
				new QueryParameter(@"@VERSION", 0L),
				});

		private static readonly Query<KeyValuePair<long, string>> ContextsQuery =
			new Query<KeyValuePair<long, string>>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", IdNameCreator);

		private static readonly Query<KeyValuePair<long, string>> ExceptionsQuery =
			new Query<KeyValuePair<long, string>>(@"SELECT ID, CONTENTS FROM FEATURE_EXCEPTIONS", IdNameCreator);

		private static readonly Query InsertContextQuery =
			 new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)",
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

		public static void Replicate(IDbContext dbContext, string userName, string version, byte[] clientData)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (clientData == null) throw new ArgumentNullException(nameof(clientData));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (version == null) throw new ArgumentNullException(nameof(version));

			var versionId = GetOrInsertVersion(dbContext, version);
			var userId = GetOrInsertUser(dbContext, userName, versionId);

			using (var ms = new MemoryStream(clientData))
			{
				using (var r = new BinaryReader(ms))
				{
					var contextsClientToServerMap = ReplicateContexts(dbContext, r);
					var exceptionsClientToServerMap = ReplicateExceptions(dbContext, r);
					var featuresClientToServerMap = ReplicateFeatures(dbContext, r, contextsClientToServerMap);
					ReplicateFeatureEntries(dbContext, r, userId, versionId, featuresClientToServerMap);
					ReplicateExceptionEntries(dbContext, r, userId, versionId, featuresClientToServerMap, exceptionsClientToServerMap);
				}
			}
		}

		private static long GetOrInsertVersion(IDbContext dbContext, string version)
		{
			GetVersionQuery.Parameters[0].Value = version;

			foreach (var versionId in dbContext.Execute(GetVersionQuery))
			{
				return versionId;
			}

			InsertVersionQuery.Parameters[0].Value = version;
			dbContext.Execute(InsertVersionQuery);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		private static long GetOrInsertUser(IDbContext dbContext, string userName, long versionId)
		{
			GetUserQuery.Parameters[0].Value = userName;

			foreach (var userId in dbContext.Execute(GetUserQuery))
			{
				UpdateUserQuery.Parameters[0].Value = userId;
				UpdateUserQuery.Parameters[1].Value = DateTime.Now;
				UpdateUserQuery.Parameters[2].Value = versionId;

				dbContext.Execute(UpdateUserQuery);
				return userId;
			}

			InsertUserQuery.Parameters[0].Value = userName;
			InsertUserQuery.Parameters[1].Value = DateTime.Now;
			InsertUserQuery.Parameters[2].Value = versionId;
			dbContext.Execute(InsertUserQuery);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		private static Dictionary<long, long> ReplicateContexts(IDbContext dbContext, BinaryReader r)
		{
			var contexts = GetContexts(dbContext);

			var count = r.ReadInt32();
			var clientToServerMap = new Dictionary<long, long>(count);

			for (var i = 0; i < count; i++)
			{
				var id = r.ReadInt64();
				var name = r.ReadString();

				long serverId;
				if (!contexts.TryGetValue(name, out serverId))
				{
					serverId = InsertContext(dbContext, name);
					contexts.Add(name, serverId);
				}

				clientToServerMap.Add(id, serverId);
			}

			return clientToServerMap;
		}

		private static Dictionary<long, long> ReplicateExceptions(IDbContext dbContext, BinaryReader r)
		{
			var exceptions = GetExceptions(dbContext);

			var count = r.ReadInt32();
			var clientToServerMap = new Dictionary<long, long>(count);

			for (var i = 0; i < count; i++)
			{
				var id = r.ReadInt64();
				var contents = r.ReadString();

				long serverId;
				if (!exceptions.TryGetValue(contents, out serverId))
				{
					serverId = InsertException(dbContext, contents);
					exceptions.Add(contents, serverId);
				}

				clientToServerMap.Add(id, serverId);
			}

			return clientToServerMap;
		}

		private static Dictionary<long, long> ReplicateFeatures(IDbContext dbContext, BinaryReader r, Dictionary<long, long> contextsClientToServerMap)
		{
			var serverByContext = GetFeaturesByContext(dbContext);

			var count = r.ReadInt32();
			var clientToServerMap = new Dictionary<long, long>(count);

			for (var i = 0; i < count; i++)
			{
				var id = r.ReadInt64();
				var name = r.ReadString();
				var contextId = r.ReadInt64();
				var serverContextId = contextsClientToServerMap[contextId];

				long featureId;
				Dictionary<string, long> byContext;

				//New feature or New feature in the context
				if (!serverByContext.TryGetValue(serverContextId, out byContext) || !byContext.TryGetValue(name, out featureId))
				{
					featureId = InsertFeature(dbContext, name, serverContextId);

					if (byContext == null)
					{
						byContext = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
						serverByContext.Add(serverContextId, byContext);
					}
					byContext.Add(name, featureId);
				}

				clientToServerMap.Add(id, featureId);
			}

			return clientToServerMap;
		}

		private static void ReplicateFeatureEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap)
		{
			const int batchSize = 16;

			var count = r.ReadInt32();

			var batches = count / batchSize;
			if (batches > 0)
			{
				ReplicateFeatureEntries(dbContext, r, userId, versionId, featuresClientToServerMap, batchSize, batches);
			}
			var remaining = count % batchSize;
			if (remaining > 0)
			{
				ReplicateFeatureEntries(dbContext, r, userId, versionId, featuresClientToServerMap, remaining, 1);
			}
		}

		private static void ReplicateFeatureEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap, int itemsCount, int batches)
		{
			for (var i = 0; i < batches; i++)
			{
				var buffer = new StringBuilder(@"INSERT INTO FEATURE_ENTRIES(DETAILS, REPLICATED_AT, FEATURE_ID, USER_ID, VERSION_ID) VALUES ");

				for (var j = 0; j < itemsCount; j++)
				{
					if (j > 0)
					{
						buffer.Append(',');
					}
					var featureId = r.ReadInt64();
					var details = r.ReadString();
					var createdAt = DateTime.FromBinary(r.ReadInt64());

					buffer.Append('(');
					buffer.Append('\'');
					buffer.Append(details);
					buffer.Append('\'');
					buffer.Append(',');
					buffer.Append('\'');
					buffer.Append(createdAt.ToString(SqliteFullDateTimeFormat));
					buffer.Append('\'');
					buffer.Append(',');
					buffer.Append(featuresClientToServerMap[featureId]);
					buffer.Append(',');
					buffer.Append(userId);
					buffer.Append(',');
					buffer.Append(versionId);
					buffer.Append(')');

					dbContext.Execute(new Query(buffer.ToString()));
				}
			}
		}

		private static void ReplicateExceptionEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap, Dictionary<long, long> exceptionsClientToServerMap)
		{
			const int batchSize = 16;

			var count = r.ReadInt32();

			var batches = count / batchSize;
			if (batches > 0)
			{
				ReplicateExceptionEntries(dbContext, r, userId, versionId, featuresClientToServerMap, exceptionsClientToServerMap, batchSize, batches);
			}
			var remaining = count % batchSize;
			if (remaining > 0)
			{
				ReplicateExceptionEntries(dbContext, r, userId, versionId, featuresClientToServerMap, exceptionsClientToServerMap, remaining, 1);
			}
		}

		private static void ReplicateExceptionEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap, Dictionary<long, long> exceptionsClientToServerMap, int itemsCount, int batches)
		{
			for (var i = 0; i < batches; i++)
			{
				var buffer = new StringBuilder(@"INSERT INTO FEATURE_EXCEPTION_ENTRIES(EXCEPTION_ID, CREATED_AT, FEATURE_ID, USER_ID, VERSION_ID) VALUES ");

				for (var j = 0; j < itemsCount; j++)
				{
					if (j > 0)
					{
						buffer.Append(',');
					}

					var exceptionId = r.ReadInt64();
					var createdAt = DateTime.FromBinary(r.ReadInt64());
					var featureId = r.ReadInt64();

					buffer.Append('(');
					buffer.Append(exceptionsClientToServerMap[exceptionId]);
					buffer.Append(',');
					buffer.Append('\'');
					buffer.Append(createdAt.ToString(SqliteFullDateTimeFormat));
					buffer.Append('\'');
					buffer.Append(',');
					buffer.Append(featuresClientToServerMap[featureId]);
					buffer.Append(',');
					buffer.Append(userId);
					buffer.Append(',');
					buffer.Append(versionId);
					buffer.Append(')');


					dbContext.Execute(new Query(buffer.ToString()));
				}
			}
		}

		private static long IdCreator(IFieldDataReader r)
		{
			return r.GetInt64(0);
		}

		private static long InsertException(IDbContext dbContext, string contents)
		{
			InsertExceptionQuery.Parameters[0].Value = contents;

			dbContext.Execute(InsertExceptionQuery);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		private static Dictionary<string, long> GetContexts(IDbContext context)
		{
			var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

			foreach (var pair in context.Execute(ContextsQuery))
			{
				result.Add(pair.Value, pair.Key);
			}

			return result;
		}

		private static Dictionary<string, long> GetExceptions(IDbContext context)
		{
			var result = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

			foreach (var pair in context.Execute(ExceptionsQuery))
			{
				result.Add(pair.Value, pair.Key);
			}

			return result;
		}

		private static Dictionary<long, Dictionary<string, long>> GetFeaturesByContext(IDbContext dbContext)
		{
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

		private static long InsertContext(IDbContext dbContext, string name)
		{
			InsertContextQuery.Parameters[0].Value = name;

			dbContext.Execute(InsertContextQuery);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		private static long InsertFeature(IDbContext dbContext, string name, long contextId)
		{
			InsertFeatureQuery.Parameters[0].Value = name;
			InsertFeatureQuery.Parameters[1].Value = contextId;

			dbContext.Execute(InsertFeatureQuery);

			return dbContext.Execute(GetNewIdQuery).SingleOrDefault();
		}

		private static KeyValuePair<long, string> IdNameCreator(IFieldDataReader r)
		{
			return new KeyValuePair<long, string>(r.GetInt64(0), r.GetString(1));
		}

		private static readonly ServerFeatureRow StorageFeature = new ServerFeatureRow();

		private static ServerFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			StorageFeature.Id = r.GetInt32(0);
			StorageFeature.Name = r.GetString(1);
			StorageFeature.ContextId = r.GetInt32(2);
			return StorageFeature;
		}
	}
}