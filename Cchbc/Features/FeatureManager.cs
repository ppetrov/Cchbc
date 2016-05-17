using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private Dictionary<string, DbContextRow> Contexts { get; } = new Dictionary<string, DbContextRow>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<string, DbFeatureStepRow> Steps { get; } = new Dictionary<string, DbFeatureStepRow>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<long, Dictionary<string, DbFeatureRow>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeatureRow>>();

		public ITransactionContextCreator ContextCreator { get; set; }

		public void CreateSchema()
		{
			using (var context = this.ContextCreator.Create())
			{
				DbFeatureAdapter.CreateSchema(context);

				context.Complete();
			}
		}

		public void DropSchema()
		{
			using (var context = this.ContextCreator.Create())
			{
				DbFeatureAdapter.DropSchema(context);

				context.Complete();
			}
		}

		public async Task LoadAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				await this.LoadContextsAsync(context);
				await this.LoadStepsAsync(context);
				await this.LoadFeaturesAsync(context);

				context.Complete();
			}
		}

		public void Start(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.Start();
		}

		public void Stop(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Stop the feature
			feature.Stop();

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = this.Save(context, feature);

				var featureEntryId = DbFeatureAdapter.InsertFeatureEntry(context, featureRow.Id, feature, details ?? string.Empty);
				this.SaveSteps(context, featureEntryId, feature.Steps);

				context.Complete();
			}
		}

		public void LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = this.Save(context, feature);

				DbFeatureAdapter.InsertExceptionEntry(context, featureRow.Id, exception);

				context.Complete();
			}
		}

		public Feature StartNew(string context, string name)
		{
			var feature = new Feature(context, name);

			feature.Start();

			return feature;
		}

		private DbFeatureRow Save(ITransactionContext context, Feature feature)
		{
			return this.SaveFeature(context, this.SaveContext(context, feature.Context), feature.Name);
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
			if (steps.Count == 0) return;

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
				DbFeatureAdapter.InsertStepEntry(context, featureEntryId, this.Steps[step.Name].Id,
					Convert.ToDecimal(step.TimeSpent.TotalMilliseconds), step.Details);
			}
		}

		private async Task LoadContextsAsync(ITransactionContext transactionContext)
		{
			// Clear contexts from old values
			this.Contexts.Clear();

			// Fetch & add new values
			foreach (var context in await DbFeatureAdapter.GetContexts(transactionContext))
			{
				this.Contexts.Add(context.Name, context);
			}
		}

		private async Task LoadStepsAsync(ITransactionContext context)
		{
			// Clear steps from old values
			this.Steps.Clear();

			// Fetch & add new values
			foreach (var step in await DbFeatureAdapter.GetSteps(context))
			{
				this.Steps.Add(step.Name, step);
			}
		}

		private async Task LoadFeaturesAsync(ITransactionContext context)
		{
			// Clear steps from old values
			this.Features.Clear();

			// Fetch & add new values
			foreach (var feature in await DbFeatureAdapter.GetFeatures(context))
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