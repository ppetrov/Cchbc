using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		public static async Task ReplicateAsync(ITransactionContext serverContext, ITransactionContext clientContext, string userName, string version)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientContext == null) throw new ArgumentNullException(nameof(clientContext));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (version == null) throw new ArgumentNullException(nameof(version));

			DbServerFeatureAdapter.UpdateLastChangedFlag(serverContext);

			var versionId = await DbServerFeatureAdapter.GetOrCreateVersionAsync(serverContext, version);
			var userId = await DbServerFeatureAdapter.GetOrCreateUserAsync(serverContext, userName, versionId);

			var clientContextRows = await DbServerFeatureAdapter.GetContextsAsync(clientContext);
			var serverContexts = await GetServerContextsAsync(serverContext);
			var contextsMap = await ReplicateContextsAsync(serverContext, clientContextRows, serverContexts);

			var clientStepRows = await DbServerFeatureAdapter.GetStepsAsync(clientContext);
			var serverSteps = await GetServerStepAsync(serverContext);
			var stepsMap = await ReplicateStepsAsync(serverContext, clientStepRows, serverSteps);

			var clientFeatureRows = await DbServerFeatureAdapter.GetFeaturesAsync(clientContext);
			var featuresMap = await ReplicateFeaturesAsync(serverContext, clientFeatureRows, contextsMap);

			var featureEntryRows = await DbServerFeatureAdapter.GetFeatureEntryRowsAsync(clientContext);
			var featureEntriesMap = await ReplicateFeatureEntriesAsync(serverContext, featureEntryRows, featuresMap, userId, versionId);

			foreach (var stepRow in await DbServerFeatureAdapter.GetEntryStepRowsAsync(clientContext))
			{
				await DbServerFeatureAdapter.InsertStepEntryAsync(serverContext, featureEntriesMap[stepRow.FeatureEntryId], stepsMap[stepRow.FeatureStepId], stepRow.TimeSpent, stepRow.Details);
			}

			foreach (var exceptionRow in await DbServerFeatureAdapter.GetExceptionsAsync(clientContext))
			{
				await DbServerFeatureAdapter.InsertExceptionEntryAsync(serverContext, userId, versionId, featuresMap[exceptionRow.FeatureId], exceptionRow);
			}
		}

		private static async Task<Dictionary<long, long>> ReplicateContextsAsync(ITransactionContext serverContext, List<DbContextRow> clientContextRows, Dictionary<string, long> serverContexts)
		{
			var map = new Dictionary<long, long>(clientContextRows.Count);

			foreach (var context in clientContextRows)
			{
				long serverContextId;
				if (!serverContexts.TryGetValue(context.Name, out serverContextId))
				{
					serverContextId = await DbServerFeatureAdapter.InsertContextAsync(serverContext, context.Name);
				}

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static async Task<Dictionary<string, long>> GetServerContextsAsync(ITransactionContext serverContext)
		{
			var contexts = await DbServerFeatureAdapter.GetContextsAsync(serverContext);

			var serverContextsRows = new Dictionary<string, long>(contexts.Count);
			foreach (var context in contexts)
			{
				serverContextsRows.Add(context.Name, context.Id);
			}

			return serverContextsRows;
		}

		private static async Task<Dictionary<string, long>> GetServerStepAsync(ITransactionContext serverContext)
		{
			var steps = await DbServerFeatureAdapter.GetStepsAsync(serverContext);

			var serverStepRows = new Dictionary<string, long>(steps.Count);
			foreach (var step in steps)
			{
				serverStepRows.Add(step.Name, step.Id);
			}

			return serverStepRows;
		}

		private static async Task<Dictionary<long, long>> ReplicateStepsAsync(ITransactionContext serverContext, List<DbFeatureStepRow> clientStepRows, Dictionary<string, long> serverSteps)
		{
			var map = new Dictionary<long, long>(clientStepRows.Count);

			foreach (var step in clientStepRows)
			{
				long serverStepId;
				if (!serverSteps.TryGetValue(step.Name, out serverStepId))
				{
					serverStepId = await DbServerFeatureAdapter.InsertStepAsync(serverContext, step.Name);
				}

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static async Task<Dictionary<long, long>> ReplicateFeaturesAsync(ITransactionContext serverContext, List<DbFeatureRow> clientFeatureRows, Dictionary<long, long> contextsMap)
		{
			var serverFeatures = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in await DbServerFeatureAdapter.GetFeaturesAsync(serverContext))
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
					featureId = await DbServerFeatureAdapter.InsertFeatureAsync(serverContext, name, contextId);
				}

				featuresMap.Add(feature.Id, featureId);
			}

			return featuresMap;
		}

		private static async Task<Dictionary<long, long>> ReplicateFeatureEntriesAsync(ITransactionContext serverContext, List<DbFeatureEntryRow> featureEntryRows, Dictionary<long, long> featuresMap, long userId, long versionId)
		{
			var map = new Dictionary<long, long>(featureEntryRows.Count);

			foreach (var row in featureEntryRows)
			{
				map.Add(row.Id, await DbServerFeatureAdapter.InsertFeatureEntryAsync(serverContext, userId, versionId, featuresMap[row.FeatureId], row.Details, row.TimeSpent, row.CreatedAt));
			}

			return map;
		}
	}
}