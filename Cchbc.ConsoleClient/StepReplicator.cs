using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.ConsoleClient
{
	public static class StepReplicator
	{
		public static Dictionary<long, long> ReplicateSteps(ITransactionContext serverContext, ITransactionContext clientContext)
		{
			var serverSteps = GetServerSteps(serverContext);
			var clientSteps = GetSteps(clientContext);

			var map = new Dictionary<long, long>(clientSteps.Count);

			foreach (var step in clientSteps)
			{
				Step server;

				var clientId = step.Id;
				var serverId = serverSteps.TryGetValue(step.Name, out server)
					? server.Id
					: InsertStep(serverContext, step.Name);

				map.Add(clientId, serverId);
			}

			return map;
		}

		private static Dictionary<string, Step> GetServerSteps(ITransactionContext context)
		{
			var steps = GetSteps(context);

			var map = new Dictionary<string, Step>(steps.Count);

			foreach (var step in steps)
			{
				map.Add(step.Name, step);
			}

			return map;
		}

		private static long InsertStep(ITransactionContext context, string name)
		{
			// Set parameters values
			var sqlParams = new[]
			{
				new QueryParameter(@"NAME", name),
			};

			// Insert the record
			context.Execute(new Query(@"INSERT INTO FEATURE_STEPS(NAME) VALUES (@NAME)", sqlParams));

			// Get new Id back
			return context.GetNewId();
		}

		private static List<Step> GetSteps(ITransactionContext context)
		{
			return context.Execute(new Query<Step>(@"SELECT ID, NAME FROM FEATURE_STEPS", StepCreator));
		}

		private static Step StepCreator(IFieldDataReader r)
		{
			return new Step(r.GetInt64(0), r.GetString(1));
		}
	}
}