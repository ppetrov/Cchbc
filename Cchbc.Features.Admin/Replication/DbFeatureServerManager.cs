using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.Replication
{
	public static class DbFeatureServerManager
	{
		public static void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbServerFeatureAdapter.CreateSchema(context);
		}

		public static void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbServerFeatureAdapter.DropSchema(context);
		}

		public static void Replicate(ITransactionContext serverContext, ITransactionContext clientContext, string userName)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientContext == null) throw new ArgumentNullException(nameof(clientContext));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			var userId = DbServerFeatureAdapter.GetOrCreateUser(serverContext, userName);

			var clientContextRows = DbServerFeatureAdapter.GetContexts(clientContext);
			var contextsMap = ReplicateContexts(serverContext, clientContextRows);

			var clientStepRows = DbServerFeatureAdapter.GetSteps(clientContext);
			var stepsMap = ReplicateSteps(serverContext, clientStepRows);

			var clientFeatureRows = DbServerFeatureAdapter.GetFeatures(clientContext);
			var featuresMap = ReplicateFeatures(serverContext, clientFeatureRows, contextsMap);

			var featureEntryRows = DbServerFeatureAdapter.GetFeatureEntryRows(clientContext);
			var featureEntriesMap = ReplicateFeatureEntries(serverContext, featureEntryRows, featuresMap, userId);

			foreach (var stepRow in DbServerFeatureAdapter.GetEntryStepRows(clientContext))
			{
				DbServerFeatureAdapter.InsertStepEntry(serverContext, featureEntriesMap[stepRow.FeatureEntryId], stepsMap[stepRow.FeatureStepId], stepRow.TimeSpent, stepRow.Details);
			}

			foreach (var exceptionRow in DbServerFeatureAdapter.GetExceptions(clientContext))
			{
				DbServerFeatureAdapter.InsertExceptionEntry(serverContext, userId, featuresMap[exceptionRow.FeatureId], exceptionRow);
			}
		}

		private static Dictionary<long, long> ReplicateContexts(ITransactionContext serverContext, List<DbContextRow> clientContextRows)
		{
			var contextRows = DbServerFeatureAdapter.GetContexts(serverContext);

			var serverContextsRows = new Dictionary<string, long>(contextRows.Count);
			foreach (var context in contextRows)
			{
				serverContextsRows.Add(context.Name, context.Id);
			}

			var map = new Dictionary<long, long>(clientContextRows.Count);

			foreach (var context in clientContextRows)
			{
				long serverContextId;
				if (!serverContextsRows.TryGetValue(context.Name, out serverContextId))
				{
					serverContextId = DbServerFeatureAdapter.InsertContext(serverContext, context.Name);
				}

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static Dictionary<long, long> ReplicateSteps(ITransactionContext serverContext, List<DbFeatureStepRow> clientStepRows)
		{
			var steps = DbServerFeatureAdapter.GetSteps(serverContext);

			var serverStepRows = new Dictionary<string, long>(steps.Count);
			foreach (var step in steps)
			{
				serverStepRows.Add(step.Name, step.Id);
			}

			var map = new Dictionary<long, long>(clientStepRows.Count);

			foreach (var step in clientStepRows)
			{
				long serverStepId;
				if (!serverStepRows.TryGetValue(step.Name, out serverStepId))
				{
					serverStepId = DbServerFeatureAdapter.InsertStep(serverContext, step.Name);
				}

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static Dictionary<long, long> ReplicateFeatures(ITransactionContext serverContext, List<DbFeatureRow> clientFeatureRows, Dictionary<long, long> contextsMap)
		{
			var serverFeatures = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in DbServerFeatureAdapter.GetFeatures(serverContext))
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

				long featureId;

				Dictionary<string, long> byContext;

				// Entirely New feature or New feature in the context
				if (!serverFeatures.TryGetValue(contextId, out byContext) || !byContext.TryGetValue(name, out featureId))
				{
					featureId = DbServerFeatureAdapter.InsertFeature(serverContext, name, contextId);
				}

				featuresMap.Add(feature.Id, featureId);
			}

			return featuresMap;
		}

		private static Dictionary<long, long> ReplicateFeatureEntries(ITransactionContext serverContext, List<DbFeatureEntryRow> featureEntryRows, Dictionary<long, long> featuresMap, long userId)
		{
			var map = new Dictionary<long, long>(featureEntryRows.Count);

			foreach (var row in featureEntryRows)
			{
				map.Add(row.Id, DbServerFeatureAdapter.InsertFeatureEntry(serverContext, userId, featuresMap[row.FeatureId], row.Details, row.TimeSpent, row.CreatedAt));
			}

			return map;
		}
	}
}