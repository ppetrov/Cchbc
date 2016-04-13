using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class FeatureEntryRow
	{
		public long Id;
		public decimal TimeSpent;
		public string Details;
		public DateTime CreatedAt;
		public long FeatureId;
	}

	public sealed class FeatureEntryStepRow
	{
		public decimal TimeSpent;
		public string Details;
		public long FeatureEntryId;
		public long FeatureStepId;
	}

	public abstract class DbFeatureManager
	{
		public Dictionary<string, DbContext> Contexts { get; } = new Dictionary<string, DbContext>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, DbFeatureStep> Steps { get; } = new Dictionary<string, DbFeatureStep>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<long, Dictionary<string, DbFeature>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeature>>();

		public void Load(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.LoadContexts(context);
			this.LoadSteps(context);
			this.LoadFeatures(context);
		}

		private void LoadContexts(ITransactionContext transactionContext)
		{
			// Clear contexts from old values
			this.Contexts.Clear();

			// Fetch & add new values
			foreach (var context in DbFeatureAdapter.GetContexts(transactionContext))
			{
				this.Contexts.Add(context.Name, context);
			}
		}

		private void LoadSteps(ITransactionContext context)
		{
			// Clear steps from old values
			this.Steps.Clear();

			// Fetch & add new values
			foreach (var step in DbFeatureAdapter.GetSteps(context))
			{
				this.Steps.Add(step.Name, step);
			}
		}

		private void LoadFeatures(ITransactionContext context)
		{
			// Clear steps from old values
			this.Features.Clear();

			// Fetch & add new values
			foreach (var feature in DbFeatureAdapter.GetFeatures(context, this.Contexts))
			{
				var dbContext = feature.Context.Id;

				// Find features by context
				Dictionary<string, DbFeature> byContext;
				if (!this.Features.TryGetValue(dbContext, out byContext))
				{
					byContext = new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(dbContext, byContext);
				}

				byContext.Add(feature.Name, feature);
			}
		}
	}
}