using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.ConsoleClient
{
	public static class ContextReplicator
	{
		public static Dictionary<long, long> ReplicateContexts(ITransactionContext serverContext, ITransactionContext clientContext)
		{
			var serverContexts = GetServerContexts(serverContext);
			var clientContexts = GetContexts(clientContext);

			var map = new Dictionary<long, long>(clientContexts.Count);

			foreach (var context in clientContexts)
			{
				Context server;

				var clientId = context.Id;
				var serverId = serverContexts.TryGetValue(context.Name, out server)
					? server.Id
					: InsertContext(serverContext, context.Name);

				map.Add(clientId, serverId);
			}

			return map;
		}

		private static Dictionary<string, Context> GetServerContexts(ITransactionContext serverContext)
		{
			var contexts = GetContexts(serverContext);

			var map = new Dictionary<string, Context>(contexts.Count);

			foreach (var context in contexts)
			{
				map.Add(context.Name, context);
			}

			return map;
		}

		private static long InsertContext(ITransactionContext context, string name)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", name),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_CONTEXTS(NAME) VALUES (@NAME)", sqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static List<Context> GetContexts(ITransactionContext context)
		{
			return context.Execute(new Query<Context>(@"SELECT ID, NAME FROM FEATURE_CONTEXTS", ContextCreator));
		}

		private static Context ContextCreator(IFieldDataReader r)
		{
			return new Context(r.GetInt64(0), r.GetString(1));
		}
	}
}