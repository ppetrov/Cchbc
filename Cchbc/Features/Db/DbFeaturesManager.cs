using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cchbc.Features.Db
{
	public sealed class DbFeaturesManager : IDbFeaturesManager
	{
		private DbFeaturesAdapter Adapter { get; }
		private Dictionary<string, DbContext> Contexts { get; } = new Dictionary<string, DbContext>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<string, DbFeatureStep> Steps { get; } = new Dictionary<string, DbFeatureStep>(StringComparer.OrdinalIgnoreCase);
		private Dictionary<long, Dictionary<string, DbFeature>> Features { get; } = new Dictionary<long, Dictionary<string, DbFeature>>();

		public DbFeaturesManager(DbFeaturesAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public Task CreateSchemaAsync()
		{
			return this.Adapter.CreateSchemaAsync();
		}

		public Task LoadAsync()
		{
			var contextTesk = this.LoadContextsAsync();
			var stepsTask = this.LoadStepsAsync();
			var featuresTask = this.LoadFeaturesAsync();

			return Task.WhenAll(contextTesk, stepsTask, featuresTask);
		}

		public async Task SaveAsync(FeatureEntry featureEntry)
		{
			if (featureEntry == null) throw new ArgumentNullException(nameof(featureEntry));

			var context = await this.SaveContextAsync(featureEntry.Context);
			var feature = await this.SaveFeatureAsync(context, featureEntry.Name);
			var dbFeatureEntry = await this.Adapter.InsertFeatureEntryAsync(feature, featureEntry);
			await this.SaveStepsAsync(dbFeatureEntry, featureEntry.Steps);
		}

		public async Task SaveAsync(ExceptionEntry exceptionEntry)
		{
			if (exceptionEntry == null) throw new ArgumentNullException(nameof(exceptionEntry));

			var context = await this.SaveContextAsync(exceptionEntry.Context);
			var feature = await this.SaveFeatureAsync(context, exceptionEntry.Name);
			await this.Adapter.InsertExceptionEntryAsync(feature, exceptionEntry);
		}

		private async Task LoadContextsAsync()
		{
			// Clear contexts from old values
			this.Contexts.Clear();

			// Fetch & add new values
			foreach (var context in await this.Adapter.GetContextsAsync())
			{
				this.Contexts.Add(context.Name, context);
			}
		}

		private async Task LoadStepsAsync()
		{
			// Clear steps from old values
			this.Steps.Clear();

			// Fetch & add new values
			foreach (var step in await this.Adapter.GetStepsAsync())
			{
				this.Steps.Add(step.Name, step);
			}
		}

		private async Task LoadFeaturesAsync()
		{
			// Clear steps from old values
			this.Features.Clear();

			// Fetch & add new values
			foreach (var feature in await this.Adapter.GetFeaturesAsync())
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

		private async Task<DbContext> SaveContextAsync(string name)
		{
			DbContext context;

			if (!this.Contexts.TryGetValue(name, out context))
			{
				// Insert into database
				context = await this.Adapter.InsertContextAsync(name);

				// Insert the new context into the collection
				this.Contexts.Add(name, context);
			}

			return context;
		}

		private async Task<DbFeature> SaveFeatureAsync(DbContext context, string name)
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
				feature = await this.Adapter.InsertFeatureAsync(context, name);

				// Insert the new feature into the collection
				this.Features.Add(contextId,
					new Dictionary<string, DbFeature>(StringComparer.OrdinalIgnoreCase) { { feature.Name, feature } });
			}
			return feature;
		}

		private async Task SaveStepsAsync(DbFeatureEntry featureEntry, FeatureEntryStep[] entrySteps)
		{
			foreach (var step in entrySteps)
			{
				var name = step.Name;

				DbFeatureStep current;
				if (!this.Steps.TryGetValue(name, out current))
				{
					// Inser step
					current = await this.Adapter.InsertStepAsync(name);
					this.Steps.Add(name, current);
				}
			}

			// Inser step entries
			foreach (var step in entrySteps)
			{
				await this.Adapter.InsertStepEntryAsync(featureEntry, this.Steps[step.Name], step);
			}
		}
	}
}