using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private Dictionary<string, DbFeatureContextRow> Contexts { get; set; }
		private Dictionary<string, DbFeatureStepRow> Steps { get; set; }
		private Dictionary<long, List<DbFeatureRow>> Features { get; set; }

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
		}

		public async Task MarkUsageAsync(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			await MarkUsageAsync(feature.Context, feature.Name);
		}

		public async Task MarkUsageAsync(FeatureData featureData)
		{
			if (featureData == null) throw new ArgumentNullException(nameof(featureData));

			await MarkUsageAsync(featureData.Context, featureData.Name);
		}

		public Task WriteAsync(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Stop the feature
			feature.Stop();

			var featureSteps = feature.Steps;
			var steps = new FeatureStepData[featureSteps.Count];
			for (var i = 0; i < featureSteps.Count; i++)
			{
				var s = featureSteps[i];
				steps[i] = new FeatureStepData(s.Name, s.TimeSpent);
			}
			var featureData = new FeatureData(feature.Context, feature.Name, feature.TimeSpent, steps);

			return WriteAsync(featureData, details);
		}

		public async Task WriteAsync(FeatureData featureData, string details = null)
		{
			if (featureData == null) throw new ArgumentNullException(nameof(featureData));

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = await this.SaveAsync(context, featureData.Context, featureData.Name);

				var steps = featureData.Steps;
				var hasSteps = steps.Length > 0;
				var featureEntryId = await FeatureAdapter.InsertFeatureEntryAsync(context, featureRow.Id, featureData.TimeSpent, details ?? string.Empty, hasSteps);
				if (hasSteps)
				{
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
					await FeatureAdapter.InsertStepEntriesAsync(context, this.GetFeatureEntryStep(featureEntryId, steps));
				}

				context.Complete();
			}
		}

		public async Task WriteExceptionAsync(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = await this.SaveAsync(context, feature.Context, feature.Name);
				var exceptionId = await FeatureAdapter.GetOrCreateExceptionAsync(context, exception.ToString());

				await FeatureAdapter.InsertExceptionEntryAsync(context, featureRow.Id, exceptionId);

				context.Complete();
			}
		}

		private async Task MarkUsageAsync(string featureContext, string name)
		{
			using (var context = this.ContextCreator.Create())
			{
				var featureRow = await this.SaveAsync(context, featureContext, name);

				await FeatureAdapter.InsertFeatureUsageAsync(context, featureRow.Id);

				context.Complete();
			}
		}

		private async Task<DbFeatureRow> SaveAsync(ITransactionContext context, string featureContext, string name)
		{
			return await this.SaveFeatureAsync(context, await this.SaveContextAsync(context, featureContext), name);
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
			var contextId = featureContextRow.Id;
			List<DbFeatureRow> features;
			if (this.Features.TryGetValue(contextId, out features))
			{
				foreach (var featureRow in features)
				{
					if (featureRow.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
					{
						return featureRow;
					}
				}
			}
			else
			{
				// Create feature collection for this context
				features = new List<DbFeatureRow>();
				this.Features.Add(contextId, features);
			}

			// Insert into database
			var newFeatureId = await FeatureAdapter.InsertFeatureAsync(transactionContext, name, contextId);

			var feature = new DbFeatureRow(newFeatureId, name, contextId);

			//Insert the new feature into the collection
			features.Add(feature);

			return feature;
		}

		private IEnumerable<FeatureEntryStep> GetFeatureEntryStep(long featureEntryId, IEnumerable<FeatureStepData> steps)
		{
			var common = new FeatureEntryStep();

			foreach (var step in steps)
			{
				common.TimeSpent = step.TimeSpent.TotalMilliseconds;
				common.FeatureEntryId = featureEntryId;
				common.FeatureStepId = this.Steps[step.Name].Id;

				yield return common;
			}
		}

		private async Task<Dictionary<long, List<DbFeatureRow>>> GetFeaturesAsync(ITransactionContext context)
		{
			var featuresByContext = new Dictionary<long, List<DbFeatureRow>>(this.Contexts.Count);

			// Fetch & add new values
			foreach (var feature in await FeatureAdapter.GetFeaturesAsync(context))
			{
				var contextId = feature.ContextId;

				// Find features by context
				List<DbFeatureRow> byContext;
				if (!featuresByContext.TryGetValue(contextId, out byContext))
				{
					byContext = new List<DbFeatureRow>();
					featuresByContext.Add(contextId, byContext);
				}

				byContext.Add(feature);
			}

			return featuresByContext;
		}
	}
}