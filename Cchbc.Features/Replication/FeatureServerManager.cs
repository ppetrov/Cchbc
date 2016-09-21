﻿using System;
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

		public static async Task<ServerData> GetServerDataAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var versions = await FeatureServerAdapter.GetVersionsAsync(context);
			var users = await FeatureServerAdapter.GetUsersAsync(context);
			var serverContexts = await FeatureServerAdapter.GetContextsAsync(context);
			var serverSteps = await FeatureServerAdapter.GetStepsAsync(context);
			var serverExceptions = await FeatureServerAdapter.GetExceptionsAsync(context);
			var serverFeaturesByContext = await GetFeaturesByContextAsync(context);

			return new ServerData(versions, users, serverContexts, serverSteps, serverExceptions, serverFeaturesByContext);
		}

		public static async Task ReplicateAsync(string userName, string version, ITransactionContext serverContext, ClientData clientData, ServerData serverData)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientData == null) throw new ArgumentNullException(nameof(clientData));
			if (serverData == null) throw new ArgumentNullException(nameof(serverData));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (version == null) throw new ArgumentNullException(nameof(version));

			long versionId;
			if (!serverData.Versions.TryGetValue(version, out versionId))
			{
				versionId = await FeatureServerAdapter.InsertVersionAsync(serverContext, version);
				serverData.Versions.Add(version, versionId);
			}
			long userId;
			if (!serverData.Users.TryGetValue(userName, out userId))
			{
				userId = Convert.ToInt32(await FeatureServerAdapter.InsertUserAsync(serverContext, userName, versionId));
				serverData.Users.Add(userName, userId);
			}

			var clientContextRows = clientData.ContextRows;
			var clientStepRows = clientData.StepRows;
			var clientFeatureRows = clientData.FeatureRows;
			var clientFeatureEntryRows = clientData.FeatureEntryRows;
			var clientFeatureEntryStepRows = clientData.EntryStepRows;
			var clientExceptionRows = clientData.ExceptionRows;
			var clientFeatureExceptionEntryRows = clientData.ExceptionEntryRows;

			var contextsMap = await ReplicateContextsAsync(serverContext, clientContextRows, serverData.Contexts);
			var stepsMap = await ReplicateStepsAsync(serverContext, clientStepRows, serverData.Steps);
			var exceptionsMap = await ReplicateExceptionsAsync(serverContext, clientExceptionRows, serverData.Exceptions);
			var featuresMap = await ReplicateFeaturesAsync(serverContext, clientFeatureRows, contextsMap, serverData.FeaturesByContext);
			var featureEntriesMap = await ReplicateFeatureEntriesAsync(serverContext, clientFeatureEntryRows, featuresMap, userId, versionId, clientFeatureEntryStepRows);

			var batchSize = 256;

			// Process Steps in batches
			var total = clientFeatureEntryStepRows.Count;
			var remaining = total % batchSize;
			var totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				await FeatureServerAdapter.InsertStepEntriesAsync(serverContext, GetRange(clientFeatureEntryStepRows, offset, batchSize), featureEntriesMap, stepsMap);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				await FeatureServerAdapter.InsertStepEntriesAsync(serverContext, GetRange(clientFeatureEntryStepRows, offset, remaining), featureEntriesMap, stepsMap);
			}


			total = clientFeatureExceptionEntryRows.Count;
			remaining = total % batchSize;
			totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				await FeatureServerAdapter.InsertExceptionEntryAsync(serverContext, GetRange(clientFeatureExceptionEntryRows, offset, batchSize), userId, versionId, exceptionsMap, featuresMap);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				await FeatureServerAdapter.InsertExceptionEntryAsync(serverContext, GetRange(clientFeatureExceptionEntryRows, offset, remaining), userId, versionId, exceptionsMap, featuresMap);
			}

			await FeatureServerAdapter.UpdateUserAsync(serverContext, userId, versionId);
		}

		private static async Task<Dictionary<long, Dictionary<string, int>>> GetFeaturesByContextAsync(ITransactionContext context)
		{
			var serverFeaturesByContext = new Dictionary<long, Dictionary<string, int>>();

			var features = await FeatureAdapter.GetFeaturesAsync(context);
			foreach (var feature in features)
			{
				Dictionary<string, int> byContext;

				var contextId = feature.ContextId;
				if (!serverFeaturesByContext.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
					serverFeaturesByContext.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature.Id);
			}

			return serverFeaturesByContext;
		}

		private static async Task<Dictionary<int, long>> ReplicateContextsAsync(ITransactionContext serverContext, List<DbFeatureContextRow> clientContextRows, Dictionary<string, long> serverContexts)
		{
			var map = new Dictionary<int, long>(clientContextRows.Count);

			foreach (var context in clientContextRows)
			{
				var name = context.Name;

				long serverContextId;
				if (!serverContexts.TryGetValue(name, out serverContextId))
				{
					serverContextId = await FeatureAdapter.InsertContextAsync(serverContext, name);
					serverContexts.Add(name, serverContextId);
				}

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static async Task<Dictionary<int, long>> ReplicateStepsAsync(ITransactionContext context, List<DbFeatureStepRow> clientStepRows, Dictionary<string, long> serverSteps)
		{
			var map = new Dictionary<int, long>(clientStepRows.Count);

			foreach (var step in clientStepRows)
			{
				var name = step.Name;

				long serverStepId;
				if (!serverSteps.TryGetValue(name, out serverStepId))
				{
					serverStepId = await FeatureAdapter.InsertStepAsync(context, name);
					serverSteps.Add(name, serverStepId);
				}

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static async Task<Dictionary<int, long>> ReplicateExceptionsAsync(ITransactionContext context, List<DbFeatureExceptionRow> clientExceptionRows, Dictionary<string, long> serverExceptions)
		{
			var map = new Dictionary<int, long>(clientExceptionRows.Count);

			foreach (var exception in clientExceptionRows)
			{
				var contents = exception.Contents;

				long serverExceptionId;
				if (!serverExceptions.TryGetValue(contents, out serverExceptionId))
				{
					serverExceptionId = await FeatureAdapter.InsertExceptionAsync(context, contents);
					serverExceptions.Add(contents, serverExceptionId);
				}

				var clientStepId = exception.Id;
				map.Add(clientStepId, serverExceptionId);
			}

			return map;
		}

		private static async Task<Dictionary<int, long>> ReplicateFeaturesAsync(ITransactionContext context, List<DbFeatureRow> clientFeatureRows, Dictionary<int, long> contextsMap, Dictionary<long, Dictionary<string, int>> serverFeaturesByContext)
		{
			var featuresMap = new Dictionary<int, long>(clientFeatureRows.Count);

			foreach (var feature in clientFeatureRows)
			{
				var name = feature.Name;
				var contextId = contextsMap[feature.ContextId];

				int featureId;
				Dictionary<string, int> byContext;

				// Entirely New feature or New feature in the context
				if (!serverFeaturesByContext.TryGetValue(contextId, out byContext) || !byContext.TryGetValue(name, out featureId))
				{
					featureId = await FeatureAdapter.InsertFeatureAsync(context, name, contextId);

					if (byContext == null)
					{
						byContext = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
						serverFeaturesByContext.Add(contextId, byContext);
					}
					byContext.Add(name, featureId);
				}

				featuresMap.Add(feature.Id, featureId);
			}

			return featuresMap;
		}

		private static async Task<Dictionary<long, long>> ReplicateFeatureEntriesAsync(ITransactionContext context, List<DbFeatureEntryRow> featureEntryRows, Dictionary<int, long> featuresMap, long userId, long versionId, List<DbFeatureEntryStepRow> entrySteps)
		{
			var featuresWithSteps = new HashSet<long>();

			foreach (var step in entrySteps)
			{
				featuresWithSteps.Add(step.FeatureEntryId);
			}

			var map = new Dictionary<long, long>(featureEntryRows.Count);

			foreach (var row in featureEntryRows)
			{
				var mappedFeatureId = featuresMap[row.FeatureId];
				var needNewId = featuresWithSteps.Contains(row.FeatureId);
				map.Add(row.Id, await FeatureServerAdapter.InsertFeatureEntryAsync(context, row.TimeSpent, row.Details, row.CreatedAt, mappedFeatureId, userId, versionId, needNewId));
			}

			return map;
		}

		private static IEnumerable<DbFeatureEntryStepRow> GetRange(List<DbFeatureEntryStepRow> entries, int offset, int total)
		{
			for (var i = 0; i < total; i++)
			{
				yield return entries[offset + i];
			}
		}

		private static IEnumerable<DbFeatureExceptionEntryRow> GetRange(List<DbFeatureExceptionEntryRow> entries, int offset, int total)
		{
			for (var i = 0; i < total; i++)
			{
				yield return entries[offset + i];
			}
		}
	}
}