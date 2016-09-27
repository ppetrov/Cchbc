using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public static class FeatureServerManager
	{
		public static void CreateSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			FeatureServerAdapter.CreateSchema(context);
		}

		public static void DropSchemaAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			FeatureServerAdapter.DropSchema(context);
		}

		public static ServerData GetServerDataAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var versions = FeatureServerAdapter.GetVersions(context);
			var users = FeatureServerAdapter.GetUsers(context);
			var serverContexts = FeatureServerAdapter.GetContexts(context);
			var serverSteps = FeatureServerAdapter.GetSteps(context);
			var serverExceptions = FeatureServerAdapter.GetExceptions(context);
			var serverFeaturesByContext = GetFeaturesByContextAsync(context);

			return new ServerData(versions, users, serverContexts, serverSteps, serverExceptions, serverFeaturesByContext);
		}

		public static void ReplicateAsync(string userName, string version, ITransactionContext serverContext, ClientData clientData, ServerData serverData)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientData == null) throw new ArgumentNullException(nameof(clientData));
			if (serverData == null) throw new ArgumentNullException(nameof(serverData));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (version == null) throw new ArgumentNullException(nameof(version));

			long versionId;
			if (!serverData.Versions.TryGetValue(version, out versionId))
			{
				versionId = FeatureServerAdapter.InsertVersion(serverContext, version);
				serverData.Versions.Add(version, versionId);
			}
			long userId;
			if (!serverData.Users.TryGetValue(userName, out userId))
			{
				userId = FeatureServerAdapter.InsertUser(serverContext, userName, versionId);
				serverData.Users.Add(userName, userId);
			}
			var contextsMap = ReplicateContextsAsync(serverContext, clientData.ContextRows, serverData.Contexts);
			var stepsMap = ReplicateStepsAsync(serverContext, clientData.StepRows, serverData.Steps);
			var exceptionsMap = ReplicateExceptionsAsync(serverContext, clientData.ExceptionRows, serverData.Exceptions);
			var featuresMap = ReplicateFeaturesAsync(serverContext, clientData.FeatureRows, contextsMap, serverData.FeaturesByContext);
			var featureEntriesMap = ReplicateFeatureEntriesAsync(serverContext, clientData.FeatureEntryRows, featuresMap, userId, versionId);

			var batchSize = 256;

			// Process Steps
			var clientFeatureEntryStepRows = clientData.EntryStepRows;
			var total = clientFeatureEntryStepRows.Count;
			var remaining = total % batchSize;
			var totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				FeatureServerAdapter.InsertStepEntries(serverContext, GetRange(clientFeatureEntryStepRows, offset, batchSize), featureEntriesMap, stepsMap);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				FeatureServerAdapter.InsertStepEntries(serverContext, GetRange(clientFeatureEntryStepRows, offset, remaining), featureEntriesMap, stepsMap);
			}

			// Process Exceptions
			var clientFeatureExceptionEntryRows = clientData.ExceptionEntryRows;
			total = clientFeatureExceptionEntryRows.Count;
			remaining = total % batchSize;
			totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				FeatureServerAdapter.InsertExceptionEntry(serverContext, GetRange(clientFeatureExceptionEntryRows, offset, batchSize), userId, versionId, exceptionsMap, featuresMap);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				FeatureServerAdapter.InsertExceptionEntry(serverContext, GetRange(clientFeatureExceptionEntryRows, offset, remaining), userId, versionId, exceptionsMap, featuresMap);
			}

			var usageRows = clientData.UsageRows;
			total = usageRows.Count;
			remaining = total % batchSize;
			totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				FeatureServerAdapter.InsertUsageEntry(serverContext, GetRange(usageRows, offset, batchSize), userId, versionId, featuresMap);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				FeatureServerAdapter.InsertUsageEntry(serverContext, GetRange(usageRows, offset, remaining), userId, versionId, featuresMap);
			}

			FeatureServerAdapter.UpdateUser(serverContext, userId, versionId);
		}

		private static Dictionary<long, Dictionary<string, int>> GetFeaturesByContextAsync(ITransactionContext context)
		{
			var serverFeaturesByContext = new Dictionary<long, Dictionary<string, int>>();

			foreach (var feature in FeatureAdapter.GetFeatures(context))
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

		private static Dictionary<int, long> ReplicateContextsAsync(ITransactionContext serverContext, List<DbFeatureContextRow> clientContextRows, Dictionary<string, long> serverContexts)
		{
			var map = new Dictionary<int, long>(clientContextRows.Count);

			foreach (var context in clientContextRows)
			{
				var name = context.Name;

				long serverContextId;
				if (!serverContexts.TryGetValue(name, out serverContextId))
				{
					serverContextId = FeatureAdapter.InsertContext(serverContext, name);
					serverContexts.Add(name, serverContextId);
				}

				var clientContextId = context.Id;
				map.Add(clientContextId, serverContextId);
			}

			return map;
		}

		private static Dictionary<int, long> ReplicateStepsAsync(ITransactionContext context, List<DbFeatureStepRow> clientStepRows, Dictionary<string, long> serverSteps)
		{
			var map = new Dictionary<int, long>(clientStepRows.Count);

			foreach (var step in clientStepRows)
			{
				var name = step.Name;

				long serverStepId;
				if (!serverSteps.TryGetValue(name, out serverStepId))
				{
					serverStepId = FeatureAdapter.InsertStep(context, name);
					serverSteps.Add(name, serverStepId);
				}

				var clientStepId = step.Id;
				map.Add(clientStepId, serverStepId);
			}

			return map;
		}

		private static Dictionary<int, long> ReplicateExceptionsAsync(ITransactionContext context, List<DbFeatureExceptionRow> clientExceptionRows, Dictionary<string, long> serverExceptions)
		{
			var map = new Dictionary<int, long>(clientExceptionRows.Count);

			foreach (var exception in clientExceptionRows)
			{
				var contents = exception.Contents;

				long serverExceptionId;
				if (!serverExceptions.TryGetValue(contents, out serverExceptionId))
				{
					serverExceptionId = FeatureAdapter.InsertException(context, contents);
					serverExceptions.Add(contents, serverExceptionId);
				}

				var clientStepId = exception.Id;
				map.Add(clientStepId, serverExceptionId);
			}

			return map;
		}

		private static Dictionary<int, long> ReplicateFeaturesAsync(ITransactionContext context, List<DbFeatureRow> clientFeatureRows, Dictionary<int, long> contextsMap, Dictionary<long, Dictionary<string, int>> serverFeaturesByContext)
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
					featureId = FeatureAdapter.InsertFeature(context, name, contextId);

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

		private static Dictionary<long, long> ReplicateFeatureEntriesAsync(ITransactionContext context, List<DbFeatureEntryRow> featureEntryRows, Dictionary<int, long> featuresMap, long userId, long versionId)
		{
			var map = new Dictionary<long, long>(featureEntryRows.Count);
			if (featureEntryRows.Count == 0) return map;

			var batchSize = 16;
			var nextSeed = FeatureServerAdapter.GetFeatureEntryNextSeed(context);

			// Process Steps in batches
			var total = featureEntryRows.Count;
			var remaining = total % batchSize;
			var totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				FeatureServerAdapter.InsertFeatureEntry(context, GetRange(featureEntryRows, offset, batchSize), userId, versionId, featuresMap, map, ref nextSeed);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				FeatureServerAdapter.InsertFeatureEntry(context, GetRange(featureEntryRows, offset, remaining), userId, versionId, featuresMap, map, ref nextSeed);
			}

			return map;
		}

		private static IEnumerable<T> GetRange<T>(List<T> entries, int offset, int total)
		{
			for (var i = 0; i < total; i++)
			{
				yield return entries[offset + i];
			}
		}
	}
}