using System;
using System.Collections.Generic;
using System.Linq;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class DbFeatureServerManager : DbFeatureManager
	{
		private Dictionary<long, DbContext> Client2ServerContexts { get; } = new Dictionary<long, DbContext>();
		private Dictionary<long, DbFeatureStep> Client2ServerSteps { get; } = new Dictionary<long, DbFeatureStep>();

		public void SaveClientData(ITransactionContext context, string userName, DbFeatureClientManager client, List<FeatureEntryRow> featureRows, List<FeatureEntryStepRow> steps)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (userName == null) throw new ArgumentNullException(nameof(userName));
			if (client == null) throw new ArgumentNullException(nameof(client));

			var user = UserHelper.GetOrCreateUser(context, userName);

			Merge(context, client.Contexts);
			Merge(context, client.Steps);
			var mappedFeaturesIds = Merge(context, client.Features);

			var map = steps.GroupBy(v => v.FeatureEntryId).ToDictionary(v => v.Key, v => v.ToList());

			foreach (var row in featureRows)
			{
				var oldFeatureId = row.FeatureId;
				var newFeatureId = mappedFeaturesIds[oldFeatureId];

				row.FeatureId = newFeatureId;

				var featureEntryId = DbFeatureServerAdapter.Insert(context, user, row);

				List<FeatureEntryStepRow> local;
				if (map.TryGetValue(oldFeatureId, out local))
				{
					foreach (var stepRow in local)
					{
						var oldStepId = stepRow.FeatureStepId;
						var newStepId = this.Client2ServerSteps[oldStepId].Id;

						stepRow.FeatureStepId = newStepId;

						DbFeatureServerAdapter.Insert(context, featureEntryId, stepRow);
					}
				}
			}
		}





		private void Merge(ITransactionContext context, Dictionary<string, DbContext> clientContexts)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (clientContexts == null) throw new ArgumentNullException(nameof(clientContexts));

			// Add the new contexts
			this.Insert(context, this.GetNewContexts(clientContexts));

			// Map client to server contexts
			this.Client2ServerContexts.Clear();
			foreach (var dbContext in clientContexts.Values)
			{
				this.Client2ServerContexts.Add(dbContext.Id, this.Contexts[dbContext.Name]);
			}
		}

		private void Merge(ITransactionContext context, Dictionary<string, DbFeatureStep> clientSteps)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (clientSteps == null) throw new ArgumentNullException(nameof(clientSteps));

			// Add the new steps
			this.Insert(context, this.GetNewSteps(clientSteps));

			// Map client to server steps
			this.Client2ServerSteps.Clear();
			foreach (var step in clientSteps.Values)
			{
				this.Client2ServerSteps.Add(step.Id, this.Steps[step.Name]);
			}
		}

		private Dictionary<long, long> Merge(ITransactionContext context, Dictionary<long, Dictionary<string, DbFeature>> clientFeatures)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (clientFeatures == null) throw new ArgumentNullException(nameof(clientFeatures));

			var mappedIds = new Dictionary<long, long>();

			var newFeatures = new List<DbFeature>();

			foreach (var clientContext in clientFeatures)
			{
				var clientContextId = clientContext.Key;

				// Map client 2 server context
				var serverContext = this.Client2ServerContexts[clientContextId];

				// Get the server features via serverContext
				Dictionary<string, DbFeature> serverFeatures;
				this.Features.TryGetValue(serverContext.Id, out serverFeatures);
				serverFeatures = serverFeatures ?? new Dictionary<string, DbFeature>(0, StringComparer.OrdinalIgnoreCase);

				// Get the new features
				foreach (var feature in clientContext.Value)
				{
					DbFeature value;

					var name = feature.Key;
					if (!serverFeatures.TryGetValue(name, out value))
					{
						newFeatures.Add(new DbFeature(feature.Value.Id, name, serverContext));
					}
					else
					{

					}

				}
			}

			return this.Insert(context, newFeatures);
		}

		private List<DbContext> GetNewContexts(Dictionary<string, DbContext> contexts)
		{
			var newContexts = new List<DbContext>();

			foreach (var clientContext in contexts.Values)
			{
				DbContext serverContext;
				if (!this.Contexts.TryGetValue(clientContext.Name, out serverContext))
				{
					newContexts.Add(clientContext);
				}
			}

			return newContexts;
		}

		private List<DbFeatureStep> GetNewSteps(Dictionary<string, DbFeatureStep> steps)
		{
			var newSteps = new List<DbFeatureStep>();

			foreach (var clientStep in steps.Values)
			{
				DbFeatureStep serverStep;
				if (!this.Steps.TryGetValue(clientStep.Name, out serverStep))
				{
					newSteps.Add(clientStep);
				}
			}

			return newSteps;
		}

		private void Insert(ITransactionContext context, List<DbContext> contexts)
		{
			foreach (var dbContext in contexts)
			{
				var name = dbContext.Name;
				var newContext = new DbContext(DbFeatureAdapter.InsertContext(context, name), name);

				this.Contexts.Add(name, newContext);
			}
		}

		private void Insert(ITransactionContext context, List<DbFeatureStep> steps)
		{
			foreach (var dbStep in steps)
			{
				var name = dbStep.Name;
				var newStep = new DbFeatureStep(DbFeatureAdapter.InsertStep(context, name), name);

				this.Steps.Add(name, newStep);
			}
		}

		private Dictionary<long, long> Insert(ITransactionContext context, List<DbFeature> features)
		{
			var mappedIds = new Dictionary<long, long>(features.Count);

			foreach (var dbFeature in features)
			{
				var dbContextId = dbFeature.Context.Id;

				var oldFeatureId = dbFeature.Id;
				var newFeatureId = DbFeatureAdapter.InsertFeature(context, dbFeature.Name, dbContextId);
				dbFeature.Id = newFeatureId;

				mappedIds.Add(oldFeatureId, newFeatureId);

				Dictionary<string, DbFeature> byContext;
				if (!this.Features.TryGetValue(dbContextId, out byContext))
				{
					byContext = new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(dbContextId, byContext);
				}

				byContext.Add(dbFeature.Name, dbFeature);
			}

			return mappedIds;
		}
	}

	public static class UserHelper
	{
		public static DbFeatureUser GetOrCreateUser(ITransactionContext context, string userName)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			return GetUser(context, userName) ?? CreateUser(context, userName);
		}

		private static DbFeatureUser GetUser(ITransactionContext context, string userName)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			var userId = DbFeatureServerAdapter.GetUser(context, userName);
			if (userId.HasValue)
			{
				return new DbFeatureUser(userId.Value, userName);
			}
			return null;
		}

		private static DbFeatureUser CreateUser(ITransactionContext context, string userName)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (userName == null) throw new ArgumentNullException(nameof(userName));

			return new DbFeatureUser(DbFeatureServerAdapter.InsertUser(context, userName), userName);
		}
	}


	
}