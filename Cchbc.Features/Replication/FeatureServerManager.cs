using System;
using System.Collections.Generic;
using System.IO;
using Cchbc.Data;

namespace Cchbc.Features.Replication
{
	public static class FeatureServerManager
	{
		public static void CreateSchema(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			FeatureServerAdapter.CreateSchema(dbContext);
		}

		public static void DropSchema(IDbContext dbContext)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

			FeatureServerAdapter.DropSchema(dbContext);
		}

		public static void Replicate(string userName, string version, IDbContext dbContext, byte[] clientData)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (clientData == null) throw new ArgumentNullException(nameof(clientData));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (version == null) throw new ArgumentNullException(nameof(version));

			var versionId = FeatureServerAdapter.GetOrInsertVersion(dbContext, version);
			var userId = FeatureServerAdapter.GetOrInsertUser(dbContext, userName, versionId);

			using (var ms = new MemoryStream(clientData))
			{
				using (var r = new BinaryReader(ms))
				{
					var contextsClientToServerMap = ReplicateContexts(dbContext, r);
					var exceptionsClientToServerMap = ReplicateExceptions(dbContext, r);
					var featuresClientToServerMap = ReplicateFeatures(dbContext, r, contextsClientToServerMap);
					ReplicateFeatureEntries(dbContext, r, userId, versionId, featuresClientToServerMap);
					ReplicateExceptionEntries(dbContext, r, userId, versionId, featuresClientToServerMap, exceptionsClientToServerMap);

					// Exception Entry
					//public readonly long ExceptionId;
					//public readonly DateTime CreatedAt;
					//public readonly long FeatureId;

					//ReplicateFeatureEntries(serverContext, clientData.FeatureEntries, featuresMap, userId, versionId);

					//var batchSize = 256;

					//// Process Exceptions
					//var clientFeatureExceptionEntryRows = clientData.ExceptionEntries;
					//var total = clientFeatureExceptionEntryRows.Length;
					//var remaining = total % batchSize;
					//var totalBatches = total / batchSize;
					//for (var i = 0; i < totalBatches; i++)
					//{
					//	var offset = i * batchSize;
					//	FeatureServerAdapter.InsertExceptionEntry(serverContext, new ArraySegment<FeatureExceptionEntryRow>(clientFeatureExceptionEntryRows, offset, batchSize), userId, versionId, exceptionsMap, featuresMap);
					//}
					//if (remaining > 0)
					//{
					//	var offset = totalBatches * batchSize;
					//	FeatureServerAdapter.InsertExceptionEntry(serverContext, new ArraySegment<FeatureExceptionEntryRow>(clientFeatureExceptionEntryRows, offset, remaining), userId, versionId, exceptionsMap, featuresMap);
					//}
				}
			}

			FeatureServerAdapter.UpdateUser(dbContext, userId, versionId);
		}

		private static Dictionary<long, long> ReplicateContexts(IDbContext dbContext, BinaryReader r)
		{
			var contexts = FeatureServerAdapter.GetContexts(dbContext);

			var count = r.ReadInt32();
			var clientToServerMap = new Dictionary<long, long>(count);

			for (var i = 0; i < count; i++)
			{
				var id = r.ReadInt64();
				var name = r.ReadString();

				long serverId;
				if (!contexts.TryGetValue(name, out serverId))
				{
					serverId = FeatureServerAdapter.InsertContext(dbContext, name);
					contexts.Add(name, serverId);
				}

				clientToServerMap.Add(id, serverId);
			}

			return clientToServerMap;
		}

		private static Dictionary<long, long> ReplicateExceptions(IDbContext dbContext, BinaryReader r)
		{
			var exceptions = FeatureServerAdapter.GetExceptions(dbContext);

			var count = r.ReadInt32();
			var clientToServerMap = new Dictionary<long, long>(count);

			for (var i = 0; i < count; i++)
			{
				var id = r.ReadInt64();
				var contents = r.ReadString();

				long serverId;
				if (!exceptions.TryGetValue(contents, out serverId))
				{
					serverId = FeatureServerAdapter.InsertException(dbContext, contents);
					exceptions.Add(contents, serverId);
				}

				clientToServerMap.Add(id, serverId);
			}

			return clientToServerMap;
		}

		private static Dictionary<long, long> ReplicateFeatures(IDbContext dbContext, BinaryReader r, Dictionary<long, long> contextsClientToServerMap)
		{
			var serverByContext = FeatureServerAdapter.GetFeaturesByContext(dbContext);

			var count = r.ReadInt32();
			var clientToServerMap = new Dictionary<long, long>(count);

			for (var i = 0; i < count; i++)
			{
				var id = r.ReadInt64();
				var name = r.ReadString();
				var contextId = r.ReadInt64();
				var serverContextId = contextsClientToServerMap[contextId];

				long featureId;
				Dictionary<string, long> byContext;

				//New feature or New feature in the context
				if (!serverByContext.TryGetValue(serverContextId, out byContext) || !byContext.TryGetValue(name, out featureId))
				{
					featureId = FeatureServerAdapter.InsertFeature(dbContext, name, serverContextId);

					if (byContext == null)
					{
						byContext = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
						serverByContext.Add(serverContextId, byContext);
					}
					byContext.Add(name, featureId);
				}

				clientToServerMap.Add(id, featureId);
			}

			return clientToServerMap;
		}

		private static void ReplicateFeatureEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap)
		{
			const int batchSize = 16;

			var count = r.ReadInt32();

			var batches = count / batchSize;
			if (batches > 0)
			{
				var rows = new ServerFeatureEntryRow[batchSize];
				ReplicateFeatureEntries(dbContext, r, userId, versionId, featuresClientToServerMap, rows, batches);
			}
			var remaining = count % batchSize;
			if (remaining > 0)
			{
				var rows = new ServerFeatureEntryRow[remaining];
				ReplicateFeatureEntries(dbContext, r, userId, versionId, featuresClientToServerMap, rows, 1);
			}
		}

		private static void ReplicateFeatureEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap, ServerFeatureEntryRow[] rows, int batches)
		{
			for (var i = 0; i < rows.Length; i++)
			{
				rows[i] = new ServerFeatureEntryRow();
			}
			for (var i = 0; i < batches; i++)
			{
				foreach (var row in rows)
				{
					row.FeatureId = r.ReadInt64();
					row.Details = r.ReadString();
					row.CreatedAt = DateTime.FromBinary(r.ReadInt64());
					row.Timespent = r.ReadDouble();
				}
				FeatureServerAdapter.InsertFeatureEntry(dbContext, rows, userId, versionId, featuresClientToServerMap);
			}
		}

		private static void ReplicateExceptionEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap, Dictionary<long, long> exceptionsClientToServerMap)
		{
			const int batchSize = 16;

			var count = r.ReadInt32();

			var batches = count / batchSize;
			if (batches > 0)
			{
				var rows = new ServerFeatureExceptionEntryRow[batchSize];
				ReplicateExceptionEntries(dbContext, r, userId, versionId, featuresClientToServerMap, exceptionsClientToServerMap, rows, batches);
			}
			var remaining = count % batchSize;
			if (remaining > 0)
			{
				var rows = new ServerFeatureExceptionEntryRow[remaining];
				ReplicateExceptionEntries(dbContext, r, userId, versionId, featuresClientToServerMap, exceptionsClientToServerMap, rows, 1);
			}
		}

		private static void ReplicateExceptionEntries(IDbContext dbContext, BinaryReader r, long userId, long versionId, Dictionary<long, long> featuresClientToServerMap, Dictionary<long, long> exceptionsClientToServerMap, ServerFeatureExceptionEntryRow[] rows, int batches)
		{
			for (var i = 0; i < rows.Length; i++)
			{
				rows[i] = new ServerFeatureExceptionEntryRow();
			}
			for (var i = 0; i < batches; i++)
			{
				foreach (var row in rows)
				{
					row.ExceptionId = r.ReadInt64();
					row.CreatedAt = DateTime.FromBinary(r.ReadInt64());
					row.FeatureId = r.ReadInt64();
				}
				FeatureServerAdapter.InsertExceptionEntry(dbContext, rows, userId, versionId, featuresClientToServerMap, exceptionsClientToServerMap);
			}
		}
	}
}