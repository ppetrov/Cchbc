using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class DbFeatureClientManager
	{
		public Dictionary<string, DbContext> Contexts { get; } = new Dictionary<string, DbContext>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<string, DbFeatureStep> Steps { get; } = new Dictionary<string, DbFeatureStep>(StringComparer.OrdinalIgnoreCase);
		public Dictionary<long, Dictionary<string, DbFeature>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeature>>();

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

		public void Save(ITransactionContext transactionContext, FeatureEntry entry)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var context = this.SaveContext(transactionContext, entry.Context);
			var feature = this.SaveFeature(transactionContext, context, entry.Name);

			var dbFeatureEntry = DbFeatureAdapter.InsertFeatureEntry(transactionContext, feature, entry);
			this.SaveSteps(transactionContext, dbFeatureEntry, entry.Steps);
		}

		public void Save(ITransactionContext transactionContext, FeatureException entry)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var context = this.SaveContext(transactionContext, entry.Context);
			var feature = this.SaveFeature(transactionContext, context, entry.Name);

			DbFeatureAdapter.InsertExceptionEntry(transactionContext, feature, entry);
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
				DbFeatureAdapter.InsertStepEntry(context, featureEntry, this.Steps[step.Name], step);
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