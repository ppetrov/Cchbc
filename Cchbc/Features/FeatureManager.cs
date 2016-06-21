using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private bool _isLoaded;

		private Dictionary<string, DbFeatureContextRow> Contexts { get; set; }
		private Dictionary<string, DbFeatureStepRow> Steps { get; set; }
		private Dictionary<long, Dictionary<string, DbFeatureRow>> Features { get; set; }

		public ITransactionContextCreator ContextCreator { get; set; }

		public async Task CreateSchemaAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				await FeatureAdapter.CreateSchemaAsync(context);

				context.Complete();
			}
		}

		public async Task DropSchemaAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				await FeatureAdapter.DropSchemaAsync(context);

				context.Complete();
			}
		}

		public async Task LoadAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				this.Contexts = await FeatureAdapter.GetContextsAsync(context);
				this.Steps = await FeatureAdapter.GetStepsAsync(context);
				this.Features = await this.GetFeaturesAsync(context);

				context.Complete();
			}

			_isLoaded = true;
		}

		public async Task StopAsync(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.ValidateIsLoaded();

			// Stop the feature
			feature.Stop();

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = await this.SaveAsync(context, feature);

				var featureEntryId = await FeatureAdapter.InsertFeatureEntryAsync(context, featureRow.Id, feature.TimeSpent, details ?? string.Empty);
				await this.SaveStepsAsync(context, featureEntryId, feature.Steps);

				context.Complete();
			}
		}

		public async Task LogExceptionAsync(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.ValidateIsLoaded();

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = await this.SaveAsync(context, feature);
				var exceptionId = await FeatureAdapter.GetOrCreateExceptionAsync(context, exception.ToString());

				await FeatureAdapter.InsertExceptionEntryAsync(context, featureRow.Id, exceptionId);

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
				var newContextId = await FeatureAdapter.InsertContextAsync(transactionContext, name);

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
				var newFeatureId = await FeatureAdapter.InsertFeatureAsync(transactionContext, name, contextId);

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
					var newStepId = await FeatureAdapter.InsertStepAsync(context, name);

					current = new DbFeatureStepRow(newStepId, name);

					this.Steps.Add(name, current);
				}
			}

			// Inser step entries
			foreach (var step in steps)
			{
				await FeatureAdapter.InsertStepEntryAsync(context,
					featureEntryId,
					this.Steps[step.Name].Id,
					step.TimeSpent.TotalMilliseconds);
			}
		}

		private async Task<Dictionary<long, Dictionary<string, DbFeatureRow>>> GetFeaturesAsync(ITransactionContext context)
		{
			var featuresByContext = new Dictionary<long, Dictionary<string, DbFeatureRow>>(this.Contexts.Count);

			// Fetch & add new values
			foreach (var feature in await FeatureAdapter.GetFeaturesAsync(context))
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

		private void ValidateIsLoaded()
		{
			if (!_isLoaded)
				throw new InvalidOperationException(nameof(FeatureManager) + " isn't loaded. Call " + nameof(LoadAsync) + " first.");
		}
	}
}