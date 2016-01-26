using System;
using System.Collections.Generic;

namespace Cchbc.Features.Db
{
	public sealed class DbFeaturesManager
	{
		private static readonly object SyncLock = new object();

		private DbFeaturesAdapter Adapter { get; }
		private Dictionary<string, DbContext> Contexts { get; } = new Dictionary<string, DbContext>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<string, DbFeatureStep> Steps { get; } = new Dictionary<string, DbFeatureStep>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<long, Dictionary<string, DbFeature>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeature>>();

		public DbFeaturesManager(DbFeaturesAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public void CreateSchema()
		{
			this.Adapter.CreateSchema();
		}

		public void DropSchema()
		{
			this.Adapter.DropSchema();
		}

		public void Load()
		{
			this.LoadContexts();
			this.LoadSteps();
			this.LoadFeatures();
		}

		public void Save(FeatureEntry featureEntry)
		{
			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));

			lock (SyncLock)
			{
				var context = this.SaveContext(featureEntry.Context);
				var feature = this.SaveFeature(context, featureEntry.Name);
				var dbFeatureEntry = this.Adapter.InsertFeatureEntry(feature, featureEntry);
				this.SaveSteps(dbFeatureEntry, featureEntry.Steps);
			}
		}

		public void Save(ExceptionEntry exceptionEntry)
		{
			if (exceptionEntry == null) throw new ArgumentNullException(nameof(exceptionEntry));

			lock (SyncLock)
			{
				var context = this.SaveContext(exceptionEntry.Context);
				var feature = this.SaveFeature(context, exceptionEntry.Name);
				this.Adapter.InsertExceptionEntry(feature, exceptionEntry);
			}
		}

		private void LoadContexts()
		{
			// Clear contexts from old values
			this.Contexts.Clear();

			// Fetch & add new values
			foreach (var context in this.Adapter.GetContexts())
			{
				this.Contexts.Add(context.Name, context);
			}
		}

		private void LoadSteps()
		{
			// Clear steps from old values
			this.Steps.Clear();

			// Fetch & add new values
			foreach (var step in this.Adapter.GetSteps())
			{
				this.Steps.Add(step.Name, step);
			}
		}

		private void LoadFeatures()
		{
			// Clear steps from old values
			this.Features.Clear();

			// Fetch & add new values
			foreach (var feature in this.Adapter.GetFeatures())
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

		private DbContext SaveContext(string name)
		{
			DbContext context;

			if (!this.Contexts.TryGetValue(name, out context))
			{
				// Insert into database
				context = this.Adapter.InsertContext(name);

				// Insert the new context into the collection
				this.Contexts.Add(name, context);
			}

			return context;
		}

		private DbFeature SaveFeature(DbContext context, string name)
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
				feature = this.Adapter.InsertFeature(context, name);

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

		private void SaveSteps(DbFeatureEntry featureEntry, FeatureEntryStep[] entrySteps)
		{
			foreach (var step in entrySteps)
			{
				var name = step.Name;

				DbFeatureStep current;
				if (!this.Steps.TryGetValue(name, out current))
				{
					// Inser step
					current = this.Adapter.InsertStep(name);
					this.Steps.Add(name, current);
				}
			}

			// Inser step entries
			foreach (var step in entrySteps)
			{
				this.Adapter.InsertStepEntry(featureEntry, this.Steps[step.Name], step);
			}
		}
	}
}