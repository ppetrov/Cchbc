using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Db.Managers
{
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

			var userId = DbFeatureAdapter.GetOrCreateUser(serverContext, userName);

			var clientContextRows = DbFeatureAdapter.GetContexts(clientContext);
			var contextsMap = ReplicateContexts(serverContext, clientContextRows);

			var clientStepRows = DbFeatureAdapter.GetSteps(clientContext);
			var stepsMap = ReplicateSteps(serverContext, clientStepRows);

			var clientFeatureRows = DbFeatureAdapter.GetFeatures(clientContext);
			var featuresMap = ReplicateFeatures(serverContext, clientFeatureRows, contextsMap);

			var featureEntryRows = DbFeatureAdapter.GetFeatureEntryRows(clientContext);
			var featureEntriesMap = ReplicateFeatureEntries(serverContext, featureEntryRows, featuresMap, userId);

			foreach (var stepRow in DbFeatureAdapter.GetEntryStepRows(clientContext))
			{
				DbFeatureAdapter.InsertStepEntry(serverContext, featureEntriesMap[stepRow.FeatureEntryId], stepsMap[stepRow.FeatureStepId], stepRow.TimeSpent, stepRow.Details);
			}

			var dbExceptionRows = DbFeatureAdapter.GetExceptions(clientContext);
			foreach (var exceptionRow in dbExceptionRows)
			{
				DbFeatureAdapter.InsertExceptionEntry(serverContext, featuresMap[exceptionRow.FeatureId], exceptionRow, userId);
			}
		}

		private static Dictionary<long, long> ReplicateContexts(ITransactionContext serverContext, List<DbContextRow> clientContextRows)
		{
			var contextRows = DbFeatureAdapter.GetContexts(serverContext);

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
					: DbFeatureAdapter.InsertContext(serverContext, context.Name);

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static Dictionary<long, long> ReplicateSteps(ITransactionContext serverContext, List<DbFeatureStepRow> clientStepRows)
		{
			var steps = DbFeatureAdapter.GetSteps(serverContext);

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
					: DbFeatureAdapter.InsertStep(serverContext, step.Name);

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static Dictionary<long, long> ReplicateFeatures(ITransactionContext serverContext, List<DbFeatureRow> clientFeatureRows, Dictionary<long, long> contextsMap)
		{
			var serverFeatures = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in DbFeatureAdapter.GetFeatures(serverContext))
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
					: DbFeatureAdapter.InsertFeature(serverContext, name, contextId);

				featuresMap.Add(feature.Id, featureId);
			}

			return featuresMap;
		}

		private static Dictionary<long, long> ReplicateFeatureEntries(ITransactionContext serverContext, List<DbFeatureEntryRow> featureEntryRows, Dictionary<long, long> featuresMap, long userId)
		{
			var map = new Dictionary<long, long>(featureEntryRows.Count);

			foreach (var row in featureEntryRows)
			{
				map.Add(row.Id, DbFeatureAdapter.InsertFeatureEntry(serverContext, featuresMap[row.FeatureId], row.Details, row.TimeSpent, row.CreatedAt, userId));
			}

			return map;
		}
	}
}