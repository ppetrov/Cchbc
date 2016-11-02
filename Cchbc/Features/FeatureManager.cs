using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private Func<ITransactionContext> ContextCreator { get; set; }
		private Dictionary<string, DbFeatureContextRow> Contexts { get; set; }
		private Dictionary<long, List<DbFeatureRow>> Features { get; set; }

		public static void CreateSchema(Func<ITransactionContext> contextCreator)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			using (var context = contextCreator())
			{
				FeatureAdapter.CreateSchema(context);
				context.Complete();
			}
		}

		public static void DropSchema(Func<ITransactionContext> contextCreator)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			using (var context = contextCreator())
			{
				FeatureAdapter.DropSchema(context);
				context.Complete();
			}
		}

		public void Load(Func<ITransactionContext> contextCreator)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			this.ContextCreator = contextCreator;

			using (var context = this.ContextCreator())
			{
				this.Contexts = FeatureAdapter.GetContexts(context);
				this.Features = this.GetFeatures(context);

				context.Complete();
			}
		}

		public void Write(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Stop the feature
			feature.Stop();

			var featureData = new FeatureData(feature.Context, feature.Name);

			var value = details;
			if (feature.TimeSpent != TimeSpan.Zero)
			{
				value = feature.TimeSpent.ToString(@"c");
			}

			Write(featureData, value);
		}

		public void Write(FeatureData featureData, string details = null)
		{
			if (featureData == null) throw new ArgumentNullException(nameof(featureData));

			using (var context = this.ContextCreator())
			{
				var featureRow = this.Save(context, featureData.Context, featureData.Name);

				FeatureAdapter.InsertFeatureEntry(context, featureRow.Id, details ?? string.Empty);

				context.Complete();
			}
		}

		public void WriteException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var context = this.ContextCreator())
			{
				var featureRow = this.Save(context, feature.Context, feature.Name);
				var exceptionId = FeatureAdapter.GetOrCreateException(context, exception.ToString());

				FeatureAdapter.InsertExceptionEntry(context, featureRow.Id, exceptionId);

				context.Complete();
			}
		}

		private DbFeatureRow Save(ITransactionContext context, string featureContext, string name)
		{
			return this.SaveFeature(context, this.SaveContext(context, featureContext), name);
		}

		private DbFeatureContextRow SaveContext(ITransactionContext transactionContext, string name)
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

		private DbFeatureRow SaveFeature(ITransactionContext transactionContext, DbFeatureContextRow featureContextRow, string name)
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

		private Dictionary<long, List<DbFeatureRow>> GetFeatures(ITransactionContext context)
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