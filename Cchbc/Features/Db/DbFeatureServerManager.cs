using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class DbFeatureServerManager : DbFeatureManager
	{
		private Dictionary<long, DbContext> ContextMappings { get; } = new Dictionary<long, DbContext>();
		private Dictionary<long, long> StepsMappings { get; } = new Dictionary<long, long>();

		public void Merge(ITransactionContext context, Dictionary<string, DbContext> clientContexts)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (clientContexts == null) throw new ArgumentNullException(nameof(clientContexts));

			// Add the new contexts
			this.Insert(context, this.GetNewContexts(clientContexts));

			// Map client to server contexts
			this.ContextMappings.Clear();
			foreach (var c in clientContexts.Values)
			{
				this.ContextMappings.Add(c.Id, this.Contexts[c.Name]);
			}
		}

		public void Merge(ITransactionContext context, Dictionary<string, DbFeatureStep> clientSteps)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (clientSteps == null) throw new ArgumentNullException(nameof(clientSteps));

			// Add the new steps
			this.Insert(context, this.GetNewSteps(clientSteps));

			// Map client to server steps
			this.StepsMappings.Clear();
			foreach (var s in clientSteps.Values)
			{
				this.StepsMappings.Add(s.Id, this.Steps[s.Name].Id);
			}
		}

		public void Merge(ITransactionContext context, Dictionary<long, Dictionary<string, DbFeature>> clientFeatures)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (clientFeatures == null) throw new ArgumentNullException(nameof(clientFeatures));

			var newFeatures = new List<DbFeature>();

			foreach (var byContext in clientFeatures)
			{
				var dbContext = this.ContextMappings[byContext.Key];

				Dictionary<string, DbFeature> contextFeatures;
				this.Features.TryGetValue(dbContext.Id, out contextFeatures);
				contextFeatures = contextFeatures ?? new Dictionary<string, DbFeature>(0, StringComparer.OrdinalIgnoreCase);

				foreach (var feature in byContext.Value)
				{
					// Check exists
					if (!contextFeatures.ContainsKey(feature.Key))
					{
						Debug.Assert(feature.Key == feature.Value.Name);
						newFeatures.Add(new DbFeature(-1, feature.Key, dbContext));
					}
				}
			}

			this.Insert(context, newFeatures);
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

		private void Insert(ITransactionContext context, List<DbFeature> features)
		{
			foreach (var dbFeature in features)
			{
				var dbContextId = dbFeature.Context.Id;

				var newFeatureId = DbFeatureAdapter.InsertFeature(context, dbFeature.Name, dbContextId);
				dbFeature.Id = newFeatureId;

				Dictionary<string, DbFeature> byContext;
				if (!this.Features.TryGetValue(dbContextId, out byContext))
				{
					byContext = new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(dbContextId, byContext);
				}

				byContext.Add(dbFeature.Name, dbFeature);
			}
		}
	}
}