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
		private Dictionary<string, DbFeatureContextRow> Contexts { get; set; }
		private Dictionary<string, DbFeatureStepRow> Steps { get; set; }
		private Dictionary<long, Dictionary<string, DbFeatureRow>> Features { get; set; }

		public ITransactionContextCreator ContextCreator { get; set; }

		public async Task CreateSchemaAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				await DbFeatureAdapter.CreateSchemaAsync(context);

				context.Complete();
			}
		}

		public async Task DropSchemaAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				await DbFeatureAdapter.DropSchemaAsync(context);

				context.Complete();
			}
		}

		public async Task LoadAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				this.Contexts = await DbFeatureAdapter.GetContextsMappedByNameAsync(context);
				this.Steps = await DbFeatureAdapter.GetStepsMappedByNameAsync(context);
				this.Features = await this.GetFeaturesAsync(context);

				context.Complete();
			}
		}

		public void Start(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.Start();
		}

		public Feature StartNew(string context, string name)
		{
			var feature = new Feature(context, name);

			feature.Start();

			return feature;
		}

		public async Task StopAsync(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Stop the feature
			feature.Stop();

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = await this.SaveAsync(context, feature);

				var featureEntryId = await DbFeatureAdapter.InsertFeatureEntryAsync(context, featureRow.Id, feature, details ?? string.Empty);
				await this.SaveStepsAsync(context, featureEntryId, feature.Steps);

				context.Complete();
			}
		}

		public async Task LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = this.SaveAsync(context, feature);

				await DbFeatureAdapter.InsertExceptionEntryAsync(context, featureRow.Id, exception);

				context.Complete();
			}
		}

		private async Task<DbFeatureRow> SaveAsync(ITransactionContext context, Feature feature)
		{
			return await this.SaveFeatureAsync(context, await this.SaveContextAsync(context, feature.Context), feature.Name);
		}

		private async Task<DbFeatureContextRow> SaveContextAsync(ITransactionContext transactionContext, string name)
		{
			DbFeatureContextRow featureContextRow;

			if (!this.Contexts.TryGetValue(name, out featureContextRow))
			{
				// Insert into database
				var newContextId = await DbFeatureAdapter.InsertContextAsync(transactionContext, name);

				featureContextRow = new DbFeatureContextRow(newContextId, name);

				// Insert the new context into the collection
				this.Contexts.Add(name, featureContextRow);
			}

			return featureContextRow;
		}

		private async Task<DbFeatureRow> SaveFeatureAsync(ITransactionContext transactionContext, DbFeatureContextRow featureContextRow, string name)
		{
			// Check if the context exists
			DbFeatureRow feature = null;

			var contextId = featureContextRow.Id;
			Dictionary<string, DbFeatureRow> features;
			if (this.Features.TryGetValue(contextId, out features))
			{
				features.TryGetValue(name, out feature);
			}

			if (feature == null)
			{
				// Insert into database
				var newFeatureId = await DbFeatureAdapter.InsertFeatureAsync(transactionContext, name, contextId);

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

		private async Task SaveStepsAsync(ITransactionContext context, long featureEntryId, ICollection<FeatureStep> steps)
		{
			if (steps.Count == 0) return;

			foreach (var step in steps)
			{
				var name = step.Name;

				DbFeatureStepRow current;
				if (!this.Steps.TryGetValue(name, out current))
				{
					// Inser step
					var newStepId = await DbFeatureAdapter.InsertStepAsync(context, name);

					current = new DbFeatureStepRow(newStepId, name);

					this.Steps.Add(name, current);
				}
			}

			// Inser step entries
			foreach (var step in steps)
			{
				await DbFeatureAdapter.InsertStepEntryAsync(context,
					featureEntryId,
					this.Steps[step.Name].Id,
					Convert.ToDecimal(step.TimeSpent.TotalMilliseconds),
					step.Details);
			}
		}

		private async Task<Dictionary<long, Dictionary<string, DbFeatureRow>>> GetFeaturesAsync(ITransactionContext context)
		{
			var featuresByContext = new Dictionary<long, Dictionary<string, DbFeatureRow>>(this.Contexts.Count);

			// Fetch & add new values
			foreach (var feature in await DbFeatureAdapter.GetFeaturesAsync(context))
			{
				var contextId = feature.ContextId;

				// Find features by context
				Dictionary<string, DbFeatureRow> byContext;
				if (!featuresByContext.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, DbFeatureRow>(StringComparer.OrdinalIgnoreCase);
					featuresByContext.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature);
			}

			return featuresByContext;
		}
	}
}