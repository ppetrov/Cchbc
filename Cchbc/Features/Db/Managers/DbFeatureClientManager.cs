using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Db.Managers
{
	public sealed class DbFeatureClientManager
	{
		public Dictionary<string, DbContextRow> Contexts { get; } = new Dictionary<string, DbContextRow>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, DbFeatureStepRow> Steps { get; } = new Dictionary<string, DbFeatureStepRow>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<long, Dictionary<string, DbFeatureRow>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeatureRow>>();

		public static void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureAdapter.CreateClientSchema(context);
		}

		public static void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureAdapter.DropClientSchema(context);
		}

		public void Load(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.LoadContexts(context);
			this.LoadSteps(context);
			this.LoadFeatures(context);
		}

		public void Save(ITransactionContext transactionContext, Feature feature, string details)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (details == null) throw new ArgumentNullException(nameof(details));

			var contextRow = this.SaveContext(transactionContext, feature.Context);
			var featureRow = this.SaveFeature(transactionContext, contextRow, feature.Name);

			var featureEntryId = DbFeatureAdapter.InsertFeatureEntry(transactionContext, featureRow.Id, details, feature);
			this.SaveSteps(transactionContext, featureEntryId, feature.Steps);
		}

		public void Save(ITransactionContext transactionContext, Feature feature, Exception exception)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			var contextRow = this.SaveContext(transactionContext, feature.Context);
			var featureRow = this.SaveFeature(transactionContext, contextRow, feature.Name);

			DbFeatureAdapter.InsertExceptionEntry(transactionContext, featureRow.Id, exception);
		}

		private DbContextRow SaveContext(ITransactionContext transactionContext, string name)
		{
			DbContextRow contextRow;

			if (!this.Contexts.TryGetValue(name, out contextRow))
			{
				// Insert into database
				var newContextId = DbFeatureAdapter.InsertContext(transactionContext, name);

				contextRow = new DbContextRow(newContextId, name);

				// Insert the new context into the collection
				this.Contexts.Add(name, contextRow);
			}

			return contextRow;
		}

		private DbFeatureRow SaveFeature(ITransactionContext transactionContext, DbContextRow contextRow, string name)
		{
			// Check if the context exists
			DbFeatureRow feature = null;

			var contextId = contextRow.Id;
			Dictionary<string, DbFeatureRow> features;
			if (this.Features.TryGetValue(contextId, out features))
			{
				features.TryGetValue(name, out feature);
			}

			if (feature == null)
			{
				// Insert into database
				var newFeatureId = DbFeatureAdapter.InsertFeature(transactionContext, name, contextId);

				feature = new DbFeatureRow(newFeatureId, name, contextId);

				// Insert the new feature into the collection
				if (!this.Features.TryGetValue(contextId, out features))
				{
					features = new Dictionary<string, DbFeatureRow>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(contextId, features);
				}
				features.Add(name, feature);
			}
			return feature;
		}

		private void SaveSteps(ITransactionContext context, long featureEntryId, ICollection<FeatureStep> steps)
		{
			foreach (var step in steps)
			{
				var name = step.Name;

				DbFeatureStepRow current;
				if (!this.Steps.TryGetValue(name, out current))
				{
					// Inser step
					var newStepId = DbFeatureAdapter.InsertStep(context, name);

					current = new DbFeatureStepRow(newStepId, name);

					this.Steps.Add(name, current);
				}
			}

			// Inser step entries
			foreach (var step in steps)
			{
				DbFeatureAdapter.InsertStepEntry(context, featureEntryId, this.Steps[step.Name].Id, Convert.ToDecimal(step.TimeSpent.TotalMilliseconds), step.Details);
			}
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
			foreach (var feature in DbFeatureAdapter.GetFeatures(context))
			{
				var contextId = feature.ContextId;

				// Find features by context
				Dictionary<string, DbFeatureRow> byContext;
				if (!this.Features.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, DbFeatureRow>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature);
			}
		}
	}
}