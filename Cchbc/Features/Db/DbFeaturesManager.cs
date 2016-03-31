using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Features.Db
{
	public sealed class DbFeaturesManager
	{
		private Dictionary<string, DbContext> Contexts { get; } = new Dictionary<string, DbContext>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<string, DbFeatureStep> Steps { get; } = new Dictionary<string, DbFeatureStep>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<long, Dictionary<string, DbFeature>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeature>>();

		public void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			new DbFeaturesAdapter().CreateSchema(context);
		}

		public void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			new DbFeaturesAdapter().DropSchema(context);
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

			var adapter = new DbFeaturesAdapter();
			var dbFeatureEntry = adapter.InsertFeatureEntry(transactionContext, feature, entry);
			this.SaveSteps(transactionContext, dbFeatureEntry, entry.Steps);
		}

		public void Save(ITransactionContext transactionContext, ExceptionEntry entry)
		{
			if (transactionContext == null) throw new ArgumentNullException(nameof(transactionContext));
			if (entry == null) throw new ArgumentNullException(nameof(entry));

			var context = this.SaveContext(transactionContext, entry.Context);
			var feature = this.SaveFeature(transactionContext, context, entry.Name);

			var adapter = new DbFeaturesAdapter();
			adapter.InsertExceptionEntry(transactionContext, feature, entry);
		}

		private void LoadContexts(ITransactionContext transactionContext)
		{
			// Clear contexts from old values
			this.Contexts.Clear();

			// Fetch & add new values
			foreach (var context in new DbFeaturesAdapter().GetContexts(transactionContext))
			{
				this.Contexts.Add(context.Name, context);
			}
		}

		private void LoadSteps(ITransactionContext context)
		{
			// Clear steps from old values
			this.Steps.Clear();

			// Fetch & add new values
			foreach (var step in new DbFeaturesAdapter().GetSteps(context))
			{
				this.Steps.Add(step.Name, step);
			}
		}

		private void LoadFeatures(ITransactionContext context)
		{
			// Clear steps from old values
			this.Features.Clear();

			// Fetch & add new values
			foreach (var feature in new DbFeaturesAdapter().GetFeatures(context))
			{
				var contextId = feature.ContextId;

				// Find features by context
				Dictionary<string, DbFeature> byContext;
				if (!this.Features.TryGetValue(contextId, out byContext))
				{
					byContext = new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase);
					this.Features.Add(contextId, byContext);
				}

				byContext.Add(feature.Name, feature);
			}
		}

		private DbContext SaveContext(ITransactionContext transactionContext, string name)
		{
			DbContext context;

			if (!this.Contexts.TryGetValue(name, out context))
			{
				// Insert into database
				context = new DbFeaturesAdapter().InsertContext(transactionContext, name);

				// Insert the new context into the collection
				this.Contexts.Add(name, context);
			}

			return context;
		}

		private DbFeature SaveFeature(ITransactionContext transactionContext, DbContext context, string name)
		{
			var contextId = context.Id;

			// Check if the context exists
			DbFeature feature = null;

			Dictionary<string, DbFeature> features;
			if (this.Features.TryGetValue(contextId, out features))
			{
				features.TryGetValue(name, out feature);
			}

			if (feature == null)
			{
				// Insert into database
				feature = new DbFeaturesAdapter().InsertFeature(transactionContext, context, name);

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

		private void SaveSteps(ITransactionContext context, DbFeatureEntry featureEntry, FeatureEntryStep[] entrySteps)
		{
			var adapter = new DbFeaturesAdapter();

			foreach (var step in entrySteps)
			{
				var name = step.Name;

				DbFeatureStep current;
				if (!this.Steps.TryGetValue(name, out current))
				{
					// Inser step
					current = adapter.InsertStep(context, name);
					this.Steps.Add(name, current);
				}
			}

			// Inser step entries
			foreach (var step in entrySteps)
			{
				adapter.InsertStepEntry(context, featureEntry, this.Steps[step.Name], step);
			}
		}
	}
}