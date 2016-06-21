using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public static class FeatureServerManager
	{
		public static Task CreateSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return FeatureServerAdapter.CreateSchemaAsync(context);
		}

		public static Task DropSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return FeatureServerAdapter.DropSchemaAsync(context);
		}

		public static async Task ReplicateAsync(string userName, string version, ITransactionContext serverContext, ClientData clientData)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientData == null) throw new ArgumentNullException(nameof(clientData));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (version == null) throw new ArgumentNullException(nameof(version));

			var versionId = await FeatureServerAdapter.GetOrCreateVersionAsync(serverContext, version);
			var userId = await FeatureServerAdapter.GetOrCreateUserAsync(serverContext, userName, versionId);

			var clientContextRows = clientData.ContextRows;
			var clientStepRows = clientData.StepRows;
			var clientFeatureRows = clientData.FeatureRows;
			var clientFeatureEntryRows = clientData.FeatureEntryRows;
			var clientFeatureEntryStepRows = clientData.EntryStepRows;
			var clientExceptionRows = clientData.ExceptionRows;
			var clientFeatureExceptionEntryRows = clientData.ExceptionEntryRows;

			var contextsMap = await ReplicateContextsAsync(serverContext, clientContextRows);
			var stepsMap = await ReplicateStepsAsync(serverContext, clientStepRows);
			var exceptionsMap = await ReplicateExceptionsAsync(serverContext, clientExceptionRows);
			var featuresMap = await ReplicateFeaturesAsync(serverContext, clientFeatureRows, contextsMap);
			var featureEntriesMap = await ReplicateFeatureEntriesAsync(serverContext, clientFeatureEntryRows, featuresMap, userId, versionId);

			foreach (var row in clientFeatureEntryStepRows)
			{
				var mappedFeatureEntryId = featureEntriesMap[row.FeatureEntryId];
				var mappedStepId = stepsMap[row.FeatureStepId];
				await FeatureAdapter.InsertStepEntryAsync(serverContext, mappedFeatureEntryId, mappedStepId, row.TimeSpent);
			}

			foreach (var row in clientFeatureExceptionEntryRows)
			{
				var mappedExceptionId = exceptionsMap[row.Id];
				var mappedFeatureId = featuresMap[row.FeatureId];

				await FeatureServerAdapter.InsertExceptionEntryAsync(serverContext, mappedExceptionId, row.CreatedAt, mappedFeatureId, userId, versionId);
			}

			await FeatureServerAdapter.UpdateLastChangedFlagAsync(serverContext);
		}

		private static async Task<Dictionary<int, int>> ReplicateContextsAsync(ITransactionContext serverContext, List<DbFeatureContextRow> clientContextRows)
		{
			var map = new Dictionary<int, int>(clientContextRows.Count);

			var serverContexts = await FeatureServerAdapter.GetContextsAsync(serverContext);
			foreach (var context in clientContextRows)
			{
				var name = context.Name;

				int serverContextId;
				if (!serverContexts.TryGetValue(name, out serverContextId))
				{
					serverContextId = await FeatureAdapter.InsertContextAsync(serverContext, name);
				}

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static async Task<Dictionary<long, long>> ReplicateStepsAsync(ITransactionContext context, List<DbFeatureStepRow> clientStepRows)
		{
			var map = new Dictionary<long, long>(clientStepRows.Count);

			var serverSteps = await FeatureServerAdapter.GetStepsAsync(context);
			foreach (var step in clientStepRows)
			{
				var name = step.Name;

				int serverStepId;
				if (!serverSteps.TryGetValue(name, out serverStepId))
				{
					serverStepId = await FeatureAdapter.InsertStepAsync(context, name);
				}

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static async Task<Dictionary<long, long>> ReplicateExceptionsAsync(ITransactionContext context, List<DbFeatureExceptionRow> clientExceptionRows)
		{
			var map = new Dictionary<long, long>(clientExceptionRows.Count);

			var serverExceptions = await FeatureServerAdapter.GetExceptionsAsync(context);
			foreach (var exception in clientExceptionRows)
			{
				var contents = exception.Contents;

				int serverExceptionId;
				if (!serverExceptions.TryGetValue(contents, out serverExceptionId))
				{
					serverExceptionId = await FeatureAdapter.InsertExceptionAsync(context, contents);
				}

				var clientStepId = exception.Id;
				map.Add(clientStepId, serverExceptionId);
			}

			return map;
		}

		private static async Task<Dictionary<long, long>> ReplicateFeaturesAsync(ITransactionContext context, List<DbFeatureRow> clientFeatureRows, Dictionary<int, int> contextsMap)
		{
			var serverFeaturesByContext = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in await FeatureAdapter.GetFeaturesAsync(context))
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

			var featuresMap = new Dictionary<long, long>(clientFeatureRows.Count);

			foreach (var feature in clientFeatureRows)
			{
				var name = feature.Name;
				var contextId = contextsMap[feature.ContextId];

				long featureId;
				Dictionary<string, long> byContext;

				// Entirely New feature or New feature in the context
				if (!serverFeaturesByContext.TryGetValue(contextId, out byContext) || !byContext.TryGetValue(name, out featureId))
				{
					featureId = await FeatureAdapter.InsertFeatureAsync(context, name, contextId);
				}

				featuresMap.Add(feature.Id, featureId);
			}

			return featuresMap;
		}

		private static async Task<Dictionary<long, long>> ReplicateFeatureEntriesAsync(ITransactionContext context, List<DbFeatureEntryRow> featureEntryRows, Dictionary<long, long> featuresMap, long userId, long versionId)
		{
			var map = new Dictionary<long, long>(featureEntryRows.Count);

			foreach (var row in featureEntryRows)
			{
				var mappedFeatureId = featuresMap[row.FeatureId];
				map.Add(row.Id, await FeatureServerAdapter.InsertFeatureEntryAsync(context, row.TimeSpent, row.Details, row.CreatedAt, mappedFeatureId, userId, versionId));
			}

			return map;
		}
	}
}