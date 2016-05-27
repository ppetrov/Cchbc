using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public static class DbFeatureServerManager
	{
		public static Task CreateSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return DbFeatureServerAdapter.CreateSchemaAsync(context);
		}

		public static Task DropSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return DbFeatureServerAdapter.DropSchemaAsync(context);
		}

		public static async Task ReplicateAsync(string userName, string version, ITransactionContext serverContext, FeatureClientData clientData)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientData == null) throw new ArgumentNullException(nameof(clientData));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (version == null) throw new ArgumentNullException(nameof(version));

			var versionId = await DbFeatureServerAdapter.GetOrCreateVersionAsync(serverContext, version);
			var userId = await DbFeatureServerAdapter.GetOrCreateUserAsync(serverContext, userName, versionId);

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
				await DbFeatureAdapter.InsertStepEntryAsync(serverContext, mappedFeatureEntryId, mappedStepId, row.TimeSpent, row.Details);
			}

			foreach (var row in clientFeatureExceptionEntryRows)
			{
				var mappedExceptionId = exceptionsMap[row.ExceptionRowId];
				var mappedFeatureId = featuresMap[row.FeatureId];

				await DbFeatureServerAdapter.InsertExceptionEntryAsync(serverContext, mappedExceptionId, row.CreatedAt, mappedFeatureId, userId, versionId);
			}

			await DbFeatureServerAdapter.UpdateLastChangedFlagAsync(serverContext);
		}

		private static async Task<Dictionary<long, long>> ReplicateContextsAsync(ITransactionContext serverContext, List<DbFeatureContextRow> clientContextRows)
		{
			var contexts = await DbFeatureAdapter.GetContextsAsync(serverContext);
			var serverContexts = new Dictionary<string, DbFeatureContextRow>(contexts.Count);
			foreach (var row in contexts)
			{
				serverContexts.Add(row.Name, row);
			}

			var map = new Dictionary<long, long>(clientContextRows.Count);

			foreach (var context in clientContextRows)
			{
				long serverContextId;
				var name = context.Name;

				DbFeatureContextRow serverContextRow;
				if (serverContexts.TryGetValue(name, out serverContextRow))
				{
					serverContextId = serverContextRow.Id;
				}
				else
				{
					serverContextId = await DbFeatureAdapter.InsertContextAsync(serverContext, name);
				}

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static async Task<Dictionary<long, long>> ReplicateStepsAsync(ITransactionContext context, List<DbFeatureStepRow> clientStepRows)
		{
			var steps = await DbFeatureAdapter.GetStepsAsync(context);
			var serverSteps = new Dictionary<string, DbFeatureStepRow>(steps.Count);
			foreach (var row in steps)
			{
				serverSteps.Add(row.Name, row);
			}

			var map = new Dictionary<long, long>(clientStepRows.Count);

			foreach (var step in clientStepRows)
			{
				long serverStepId;
				var name = step.Name;

				DbFeatureStepRow serverStepRow;
				if (serverSteps.TryGetValue(name, out serverStepRow))
				{
					serverStepId = serverStepRow.Id;
				}
				else
				{
					serverStepId = await DbFeatureAdapter.InsertStepAsync(context, name);
				}

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static async Task<Dictionary<long, long>> ReplicateExceptionsAsync(ITransactionContext context, List<DbFeatureExceptionRow> clientExceptionRows)
		{
			var exceptions = await DbFeatureAdapter.GetExceptionsAsync(context);
			var serverExceptions = new Dictionary<string, DbFeatureExceptionRow>(exceptions.Count);
			foreach (var row in exceptions)
			{
				serverExceptions.Add(row.Contents, row);
			}

			var map = new Dictionary<long, long>(clientExceptionRows.Count);

			foreach (var exception in clientExceptionRows)
			{
				long serverExceptionId;
				var contents = exception.Contents;

				DbFeatureExceptionRow serverExceptionRow;
				if (serverExceptions.TryGetValue(contents, out serverExceptionRow))
				{
					serverExceptionId = serverExceptionRow.Id;
				}
				else
				{
					serverExceptionId = await DbFeatureAdapter.InsertExceptionAsync(context, contents);
				}

				var clientStepId = exception.Id;
				map.Add(clientStepId, serverExceptionId);
			}

			return map;
		}

		private static async Task<Dictionary<long, long>> ReplicateFeaturesAsync(ITransactionContext context, List<DbFeatureRow> clientFeatureRows, Dictionary<long, long> contextsMap)
		{
			var serverFeatures = new Dictionary<long, Dictionary<string, long>>();

			foreach (var feature in await DbFeatureAdapter.GetFeaturesAsync(context))
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
					featureId = await DbFeatureAdapter.InsertFeatureAsync(context, name, contextId);
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
				map.Add(row.Id, await DbFeatureServerAdapter.InsertFeatureEntryAsync(context, userId, versionId, mappedFeatureId, row.Details, row.TimeSpent, row.CreatedAt));
			}

			return map;
		}
	}
}