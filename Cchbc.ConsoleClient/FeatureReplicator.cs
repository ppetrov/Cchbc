using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.ConsoleClient
{
	public static class FeatureReplicator
	{
		public static Dictionary<long, long> ReplicateFeatures(ITransactionContext serverContext, ITransactionContext clientContext, Dictionary<long, long> contextMap)
		{
			var featuresMap = new Dictionary<long, long>();

			var serverFeatures = GetServerFeatures(serverContext);
			var clientFeatures = GetClientFeatures(clientContext);

			foreach (var clientFeature in clientFeatures)
			{
				var contextId = contextMap[clientFeature.ContextId];
				var name = clientFeature.Name;

				// We are sure that the key exists in the dictionary.
				FeatureRow match;
				var byContext = serverFeatures[contextId];
				var featureId = byContext.TryGetValue(name, out match)
					? match.Id
					: CreateFeature(serverContext, name, contextId);

				featuresMap.Add(clientFeature.Id, featureId);
			}
			return featuresMap;
		}

		private static long CreateFeature(ITransactionContext context, string name, long contextId)
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

		private static List<FeatureRow> GetClientFeatures(ITransactionContext context)
		{
			return GetFeatures(context);
		}

		private static Dictionary<long, Dictionary<string, FeatureRow>> GetServerFeatures(ITransactionContext context)
		{
			var map = new Dictionary<long, Dictionary<string, FeatureRow>>();

			foreach (var feature in GetFeatures(context))
			{
				Dictionary<string, FeatureRow> byContext;

				var contextId = feature.ContextId;
				if (!map.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, FeatureRow>(StringComparer.OrdinalIgnoreCase);
					map.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature);
			}

			return map;
		}

		private static List<FeatureRow> GetFeatures(ITransactionContext context)
		{
			return context.Execute(new Query<FeatureRow>(@"SELECT ID, NAME, CONTEXT_ID FROM FEATURES", FeatureCreator));
		}

		private static FeatureRow FeatureCreator(IFieldDataReader r)
		{
			return new FeatureRow(r.GetInt64(0), r.GetString(1), r.GetInt64(2));
		}
	}
}