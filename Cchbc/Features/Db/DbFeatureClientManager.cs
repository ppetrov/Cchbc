using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class DbFeatureClientManager : DbFeatureManager
	{
		public static void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureClientAdapter.CreateSchema(context);
		}

		public static void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureClientAdapter.DropSchema(context);
		}

		public List<FeatureEntryRow> GetFeatureEntries(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return DbFeatureClientAdapter.GetFeatureEntries(context);
		}

		public List<FeatureEntryStepRow> GetFeatureEntrySteps(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			return DbFeatureClientAdapter.GetFeatureEntrySteps(context);
		}

		public void Save(ITransactionContext transactionContext, FeatureEntry entry)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var context = this.SaveContext(transactionContext, entry.Context);
			var feature = this.SaveFeature(transactionContext, context, entry.Name);

			var dbFeatureEntry = DbFeatureClientAdapter.InsertFeatureEntry(transactionContext, feature, entry);
			this.SaveSteps(transactionContext, dbFeatureEntry, entry.Steps);
		}

		public void Save(ITransactionContext transactionContext, FeatureException entry)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var context = this.SaveContext(transactionContext, entry.Context);
			var feature = this.SaveFeature(transactionContext, context, entry.Name);

			DbFeatureClientAdapter.InsertExceptionEntry(transactionContext, feature, entry);
		}

		private DbContext SaveContext(ITransactionContext transactionContext, string name)
		{
			DbContext context;

			if (!this.Contexts.TryGetValue(name, out context))
			{
				// Insert into database
				var newContextId = DbFeatureAdapter.InsertContext(transactionContext, name);

				context = new DbContext(newContextId, name);

				// Insert the new context into the collection
				this.Contexts.Add(name, context);
			}

			return context;
		}

		private DbFeature SaveFeature(ITransactionContext transactionContext, DbContext context, string name)
		{
			// Check if the context exists
			DbFeature feature = null;

			var contextId = context.Id;
			Dictionary<string, DbFeature> features;
			if (this.Features.TryGetValue(contextId, out features))
			{
				features.TryGetValue(name, out feature);
			}

			if (feature == null)
			{
				// Insert into database
				var newFeatureId = DbFeatureAdapter.InsertFeature(transactionContext, name, contextId);

				feature = new DbFeature(newFeatureId, name, context);

				// Insert the new feature into the collection
				if (!this.Features.TryGetValue(contextId, out features))
				{
					features = new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(contextId, features);
				}
				features.Add(name, feature);
			}
			return feature;
		}

		private void SaveSteps(ITransactionContext context, DbFeatureEntry featureEntry, ICollection<FeatureEntryStep> entrySteps)
		{
			if (entrySteps.Count == 0) return;

			foreach (var step in entrySteps)
			{
				var name = step.Name;

				DbFeatureStep current;
				if (!this.Steps.TryGetValue(name, out current))
				{
					// Inser step
					var newStepId = DbFeatureAdapter.InsertStep(context, name);

					current = new DbFeatureStep(newStepId, name);

					this.Steps.Add(name, current);
				}
			}

			// Inser step entries
			foreach (var step in entrySteps)
			{
				DbFeatureClientAdapter.InsertStepEntry(context, featureEntry, this.Steps[step.Name], step);
			}
		}
	}
}