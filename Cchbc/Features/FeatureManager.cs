using System;
using System.Collections.Generic;
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

		public void CreateSchemaAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				FeatureAdapter.CreateSchema(context);
				context.Complete();
			}
		}

		public void DropSchemaAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				FeatureAdapter.DropSchema(context);
				context.Complete();
			}
		}

		public void LoadAsync()
		{
			using (var context = this.ContextCreator.Create())
			{
				this.Contexts = FeatureAdapter.GetContexts(context);
				this.Steps = FeatureAdapter.GetSteps(context);
				this.Features = this.GetFeaturesAsync(context);

				context.Complete();
			}
		}

		public void MarkUsageAsync(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			MarkUsageAsync(feature.Context, feature.Name);
		}

		public void MarkUsageAsync(FeatureData featureData)
		{
			if (featureData == null) throw new ArgumentNullException(nameof(featureData));

			MarkUsageAsync(featureData.Context, featureData.Name);
		}

		public void WriteAsync(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Stop the feature
			feature.Stop();

			var featureSteps = feature.Steps;
			var steps = new FeatureStepData[featureSteps.Count];
			for (var i = 0; i < featureSteps.Count; i++)
			{
				var s = featureSteps[i];
				steps[i] = new FeatureStepData(s.Name, s.Level, s.TimeSpent);
			}
			var featureData = new FeatureData(feature.Context, feature.Name, feature.TimeSpent, steps);

			WriteAsync(featureData, details);
		}

		public void WriteAsync(FeatureData featureData, string details = null)
		{
			if (featureData == null) throw new ArgumentNullException(nameof(featureData));

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = this.SaveAsync(context, featureData.Context, featureData.Name);

				var steps = featureData.Steps;
				var hasSteps = steps.Length > 0;
				var featureEntryId = FeatureAdapter.InsertFeatureEntry(context, featureRow.Id, featureData.TimeSpent, details ?? string.Empty, hasSteps);
				if (hasSteps)
				{
					foreach (var step in steps)
					{
						var name = step.Name;

						DbFeatureStepRow current;
						if (!this.Steps.TryGetValue(name, out current))
						{
							// Inser step
							var newStepId = FeatureAdapter.InsertStep(context, name);

							current = new DbFeatureStepRow(newStepId, name);

							this.Steps.Add(name, current);
						}
					}

					// Inser step entries
					FeatureAdapter.InsertStepEntries(context, featureEntryId, steps, this.Steps);
				}

				context.Complete();
			}
		}

		public void WriteExceptionAsync(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var context = this.ContextCreator.Create())
			{
				var featureRow = this.SaveAsync(context, feature.Context, feature.Name);
				var exceptionId = FeatureAdapter.GetOrCreateException(context, exception.ToString());

				FeatureAdapter.InsertExceptionEntry(context, featureRow.Id, exceptionId);

				context.Complete();
			}
		}

		private void MarkUsageAsync(string featureContext, string name)
		{
			using (var context = this.ContextCreator.Create())
			{
				var featureRow = this.SaveAsync(context, featureContext, name);

				FeatureAdapter.InsertFeatureUsage(context, featureRow.Id);

				context.Complete();
			}
		}

		private DbFeatureRow SaveAsync(ITransactionContext context, string featureContext, string name)
		{
			return this.SaveFeatureAsync(context, this.SaveContextAsync(context, featureContext), name);
		}

		private DbFeatureContextRow SaveContextAsync(ITransactionContext transactionContext, string name)
		{
			DbFeatureContextRow featureContextRow;

			if (!this.Contexts.TryGetValue(name, out featureContextRow))
			{
				// Insert into database
				var newContextId = FeatureAdapter.InsertContext(transactionContext, name);

				featureContextRow = new DbFeatureContextRow(newContextId, name);

				// Insert the new context into the collection
				this.Contexts.Add(name, featureContextRow);
			}

			return featureContextRow;
		}

		private DbFeatureRow SaveFeatureAsync(ITransactionContext transactionContext, DbFeatureContextRow featureContextRow, string name)
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
			var newFeatureId = FeatureAdapter.InsertFeature(transactionContext, name, contextId);

			var feature = new DbFeatureRow(newFeatureId, name, contextId);

			//Insert the new feature into the collection
			features.Add(feature);

			return feature;
		}

		private Dictionary<long, List<DbFeatureRow>> GetFeaturesAsync(ITransactionContext context)
		{
			var featuresByContext = new Dictionary<long, List<DbFeatureRow>>(this.Contexts.Count);

			// Fetch & add new values
			foreach (var feature in FeatureAdapter.GetFeatures(context))
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