using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features.Replication
{
	public static class FeatureServerManager
	{
		public static void CreateSchema(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			FeatureServerAdapter.CreateSchema(context);
		}

		public static void DropSchema(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			FeatureServerAdapter.DropSchema(context);
		}

		public static ServerData GetServerData(IDbContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var versions = FeatureServerAdapter.GetVersions(context);
			var users = FeatureServerAdapter.GetUsers(context);
			var serverContexts = FeatureServerAdapter.GetContexts(context);
			var serverExceptions = FeatureServerAdapter.GetExceptions(context);
			var serverFeaturesByContext = FeatureServerAdapter.GetFeaturesByContext(context);

			return new ServerData(versions, users, serverContexts, serverExceptions, serverFeaturesByContext);
		}

		public static void Replicate(string userName, string version, IDbContext serverContext, ClientData clientData, ServerData serverData)
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
			var contextsMap = ReplicateContexts(serverContext, clientData.Contexts, serverData.Contexts);
			var exceptionsMap = ReplicateExceptions(serverContext, clientData.Exceptions, serverData.Exceptions);
			var featuresMap = ReplicateFeatures(serverContext, clientData.Features, contextsMap, serverData.FeaturesByContext);
			ReplicateFeatureEntries(serverContext, clientData.FeatureEntries, featuresMap, userId, versionId);

			var batchSize = 256;

			// Process Exceptions
			var clientFeatureExceptionEntryRows = clientData.ExceptionEntries;
			var total = clientFeatureExceptionEntryRows.Length;
			var remaining = total % batchSize;
			var totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				FeatureServerAdapter.InsertExceptionEntry(serverContext, new ArraySegment<FeatureExceptionEntryRow>(clientFeatureExceptionEntryRows, offset, batchSize), userId, versionId, exceptionsMap, featuresMap);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				FeatureServerAdapter.InsertExceptionEntry(serverContext, new ArraySegment<FeatureExceptionEntryRow>(clientFeatureExceptionEntryRows, offset, remaining), userId, versionId, exceptionsMap, featuresMap);
			}

			FeatureServerAdapter.UpdateUser(serverContext, userId, versionId);
		}

		private static Dictionary<long, long> ReplicateContexts(IDbContext serverContext, FeatureContextRow[] clientContextRows, Dictionary<string, long> serverContexts)
		{
			var map = new Dictionary<long, long>(clientContextRows.Length);

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

		private static Dictionary<long, long> ReplicateExceptions(IDbContext context, FeatureExceptionRow[] clientExceptionRows, Dictionary<string, long> serverExceptions)
		{
			var map = new Dictionary<long, long>(clientExceptionRows.Length);

			foreach (var exception in clientExceptionRows)
			{
				var contents = exception.Contents;

				long serverExceptionId;
				if (!serverExceptions.TryGetValue(contents, out serverExceptionId))
				{
					serverExceptionId = FeatureServerAdapter.InsertException(context, contents);
					serverExceptions.Add(contents, serverExceptionId);
				}

				var clientStepId = exception.Id;
				map.Add(clientStepId, serverExceptionId);
			}

			return map;
		}

		private static Dictionary<long, long> ReplicateFeatures(IDbContext context, FeatureRow[] clientFeatureRows, Dictionary<long, long> contextsMap, Dictionary<long, Dictionary<string, long>> serverFeaturesByContext)
		{
			var featuresMap = new Dictionary<long, long>(clientFeatureRows.Length);

			foreach (var feature in clientFeatureRows)
			{
				var name = feature.Name;
				var contextId = contextsMap[feature.ContextId];

				long featureId;
				Dictionary<string, long> byContext;

				// Entirely New feature or New feature in the context
				if (!serverFeaturesByContext.TryGetValue(contextId, out byContext) || !byContext.TryGetValue(name, out featureId))
				{
					featureId = FeatureAdapter.InsertFeature(context, name, contextId);

					if (byContext == null)
					{
						byContext = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
						serverFeaturesByContext.Add(contextId, byContext);
					}
					byContext.Add(name, featureId);
				}

				featuresMap.Add(feature.Id, featureId);
			}

			return featuresMap;
		}

		private static void ReplicateFeatureEntries(IDbContext context, FeatureEntryRow[] featureEntryRows, Dictionary<long, long> featuresMap, long userId, long versionId)
		{
			if (featureEntryRows.Length == 0) return;

			var batchSize = 16;

			// Process Steps in batches
			var total = featureEntryRows.Length;
			var remaining = total % batchSize;
			var totalBatches = total / batchSize;
			for (var i = 0; i < totalBatches; i++)
			{
				var offset = i * batchSize;
				FeatureServerAdapter.InsertFeatureEntry(context, new ArraySegment<FeatureEntryRow>(featureEntryRows, offset, batchSize), userId, versionId, featuresMap);
			}
			if (remaining > 0)
			{
				var offset = totalBatches * batchSize;
				FeatureServerAdapter.InsertFeatureEntry(context, new ArraySegment<FeatureEntryRow>(featureEntryRows, offset, remaining), userId, versionId, featuresMap);
			}
		}
	}
}