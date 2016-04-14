using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class FeatureEntryRow
	{
		public readonly long Id;
		public readonly decimal TimeSpent;
		public readonly string Details;
		public readonly DateTime CreatedAt;
		public long FeatureId;

		public FeatureEntryRow(long id, decimal timeSpent, string details, DateTime createdAt, long featureId)
		{
			Id = id;
			TimeSpent = timeSpent;
			Details = details;
			CreatedAt = createdAt;
			FeatureId = featureId;
		}
	}

	public sealed class FeatureEntryStepRow
	{
		public readonly decimal TimeSpent;
		public readonly string Details;
		public long FeatureEntryId;
		public long FeatureStepId;

		public FeatureEntryStepRow(decimal timeSpent, string details, long featureEntryId, long featureStepId)
		{
			TimeSpent = timeSpent;
			Details = details;
			FeatureEntryId = featureEntryId;
			FeatureStepId = featureStepId;
		}
	}

	public static class DbFeatureServerManager
	{
		public static void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureAdapter.CreateServerSchema(context);
		}

		public static void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureAdapter.DropServerSchema(context);
		}

		public static void Replicate(ITransactionContext serverContext, ITransactionContext clientContext, string userName)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientContext == null) throw new ArgumentNullException(nameof(clientContext));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			var clientContextRows = GetContextRows(clientContext);
			var contextsMap = ReplicateContexts(serverContext, clientContextRows);

			var clientStepRows = GetDbFeatureStepRows(clientContext);
			var stepsMap = ReplicateSteps(serverContext, clientStepRows);

			var clientFeatureRows = GetFeatureRows(clientContext);
			var featuresMap = ReplicateFeatures(serverContext, clientFeatureRows, contextsMap);

			var featureEntryRows = GetFeatureEntryRows(clientContext);
			var featureEntriesMap = ReplicateFeatureEntries(serverContext, featureEntryRows, featuresMap, userName);

			var featureEntryStepRows = GetEntryStepRows(clientContext);
			ReplicateFeatureEntryStepRow(serverContext, featureEntryStepRows, featureEntriesMap, stepsMap);
		}

		private static Dictionary<long, long> ReplicateContexts(ITransactionContext serverContext, List<DbContextRow> clientContextRows)
		{
			var contextRows = GetContextRows(serverContext);

			var serverContextsRows = new Dictionary<string, long>(contextRows.Count);
			foreach (var context in contextRows)
			{
				serverContextsRows.Add(context.Name, context.Id);
			}

			var map = new Dictionary<long, long>(clientContextRows.Count);

			foreach (var context in clientContextRows)
			{
				long serverId;
				var serverContextId = serverContextsRows.TryGetValue(context.Name, out serverId)
					? serverId
					: InsertContext(serverContext, context.Name);

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static Dictionary<long, long> ReplicateSteps(ITransactionContext serverContext, List<DbFeatureStepRow> clientStepRows)
		{
			var steps = GetDbFeatureStepRows(serverContext);

			var serverStepRows = new Dictionary<string, long>(steps.Count);
			foreach (var step in steps)
			{
				serverStepRows.Add(step.Name, step.Id);
			}

			var map = new Dictionary<long, long>(clientStepRows.Count);

			foreach (var step in clientStepRows)
			{
				long serverId;
				var serverStepId = serverStepRows.TryGetValue(step.Name, out serverId)
					? serverId
					: InsertStep(serverContext, step.Name);

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static Dictionary<long, long> ReplicateFeatures(ITransactionContext serverContext, List<DbFeatureRow> clientFeatureRows, Dictionary<long, long> contextsMap)
		{
			var serverFeatures = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in GetFeatureRows(serverContext))
			{
				Dictionary<string, long> byContext;

				var contextId = feature.ContextId;
				if (!serverFeatures.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
					serverFeatures.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature.Id);
			}


			var featuresMap = new Dictionary<long, long>(clientFeatureRows.Count);

			foreach (var feature in clientFeatureRows)
			{
				var contextId = contextsMap[feature.ContextId];
				var name = feature.Name;

				// We are sure that the key exists in the dictionary.
				long serverFeatureId;
				var byContext = serverFeatures[contextId];
				var featureId = byContext.TryGetValue(name, out serverFeatureId)
					? serverFeatureId
					: InsertFeature(serverContext, name, contextId);

				featuresMap.Add(feature.Id, featureId);
			}

			return featuresMap;
		}

		private static Dictionary<long, long> ReplicateFeatureEntries(ITransactionContext serverContext, List<FeatureEntryRow> featureEntryRows, Dictionary<long, long> featuresMap, string userName)
		{
			var user = GetOrCreateUser(serverContext, userName);

			var map = new Dictionary<long, long>(featureEntryRows.Count);

			foreach (var entry in featureEntryRows)
			{
				entry.FeatureId = featuresMap[entry.FeatureId];
				map.Add(entry.Id, InsertFeatureEntry(serverContext, entry, user.Id));
			}

			return map;
		}

		private static void ReplicateFeatureEntryStepRow(ITransactionContext serverContext, List<FeatureEntryStepRow> featureEntryStepRows, Dictionary<long, long> featureEntriesMap, Dictionary<long, long> stepsMap)
		{
			foreach (var row in featureEntryStepRows)
			{
				row.FeatureEntryId = featureEntriesMap[row.FeatureEntryId];
				row.FeatureStepId = stepsMap[row.FeatureStepId];

				InsertFeatureEntryStepRow(serverContext, row);
			}
		}

		private static long InsertContext(ITransactionContext context, string name)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", name),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", sqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static long InsertStep(ITransactionContext context, string name)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", name),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", sqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static long InsertFeature(ITransactionContext context, string name, long contextId)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"@NAME", name),
				new QueryParameter(@"@CONTEXT", contextId),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURES(NAME, CONTEXT_ID) VALUES (@NAME, @CONTEXT)", sqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static long InsertFeatureEntry(ITransactionContext context, FeatureEntryRow row, long userId)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"@TIMESPENT", row.TimeSpent),
				new QueryParameter(@"@DETAILS", row.Details),
				new QueryParameter(@"@CREATEDAT", row.CreatedAt),
				new QueryParameter(@"@FEATURE", row.FeatureId),
				new QueryParameter(@"@USER", userId),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRIES(TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID, USER_ID ) VALUES (@TIMESPENT, @DETAILS, @CREATEDAT, @FEATURE, @USER)", sqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static void InsertFeatureEntryStepRow(ITransactionContext context, FeatureEntryStepRow row)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"@ENTRY", row.FeatureEntryId),
				new QueryParameter(@"@STEP", row.FeatureStepId),
				new QueryParameter(@"@TIMESPENT", row.TimeSpent),
				new QueryParameter(@"@DETAILS", row.Details),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_ENTRY_STEPS(FEATURE_ENTRY_ID, FEATURE_STEP_ID, TIMESPENT, DETAILS) VALUES (@ENTRY, @STEP, @TIMESPENT, @DETAILS)", sqlParams));
		}

		private static List<DbContextRow> GetContextRows(ITransactionContext context)
		{
			return context.Execute(new Query<DbContextRow>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", DbContextRowCreator));
		}

		private static List<DbFeatureStepRow> GetDbFeatureStepRows(ITransactionContext context)
		{
			return context.Execute(new Query<DbFeatureStepRow>(@"SELECT ID, NAME FROM FEATURE_STEPS", DbFeatureStepRowCreator));
		}

		private static List<DbFeatureRow> GetFeatureRows(ITransactionContext context)
		{
			return context.Execute(new Query<DbFeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", DbFeatureRowCreator));
		}

		private static List<FeatureEntryRow> GetFeatureEntryRows(ITransactionContext context)
		{
			return context.Execute(new Query<FeatureEntryRow>(@"SELECT ID, TIMESPENT, DETAILS, CREATEDAT, FEATURE_ID FROM FEATURE_ENTRIES", FeatureEntryRowCreator));
		}

		private static List<FeatureEntryStepRow> GetEntryStepRows(ITransactionContext context)
		{
			return context.Execute(new Query<FeatureEntryStepRow>(@"SELECT TIMESPENT, DETAILS, FEATURE_ENTRY_ID, FEATURE_STEP_ID FROM FEATURE_ENTRY_STEPS", EntryStepRowCreator));
		}

		private static DbFeatureUserRow GetOrCreateUser(ITransactionContext context, string userName)
		{
			var userId = -1L;
			var user = GetUserId(context, userName);
			if (!user.HasValue)
			{
				userId = CreateUser(context, userName);
			}
			return new DbFeatureUserRow(userId, userName);
		}

		private static long? GetUserId(ITransactionContext context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", name),
			};

			// select the record
			var ids = context.Execute(new Query<long>(@"SELECT ID FROM FEATURE_USERS WHERE NAME = @NAME", r => r.GetInt64(0), sqlParams));
			if (ids.Count > 0)
			{
				return ids[0];
			}
			return null;
		}

		private static long CreateUser(ITransactionContext context, string name)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", name),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_USERS(NAME) VALUES (@NAME)", sqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static DbContextRow DbContextRowCreator(IFieldDataReader r)
		{
			return new DbContextRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureStepRow DbFeatureStepRowCreator(IFieldDataReader r)
		{
			return new DbFeatureStepRow(r.GetInt64(0), r.GetString(1));
		}

		private static DbFeatureRow DbFeatureRowCreator(IFieldDataReader r)
		{
			return new DbFeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}

		private static FeatureEntryRow FeatureEntryRowCreator(IFieldDataReader r)
		{
			return new FeatureEntryRow(r.GetInt64(0), r.GetDecimal(1), r.GetString(2), r.GetDateTime(3), r.GetInt64(4));
		}

		private static FeatureEntryStepRow EntryStepRowCreator(IFieldDataReader r)
		{
			return new FeatureEntryStepRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2), r.GetInt64(3));
		}
	}
}