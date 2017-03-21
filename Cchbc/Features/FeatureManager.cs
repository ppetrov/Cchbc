using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Data;

namespace Cchbc.Features
{
	public interface IFeatureManager
	{
		void Load();
		void Save(Feature feature, string details = null);
		void Save(Feature feature, Exception exception);
	}

	public sealed class FeatureManager : IFeatureManager
	{
		private Func<IDbContext> DbContextCreator { get; }
		private Dictionary<string, DbFeatureContextRow> Contexts { get; set; }
		private Dictionary<long, List<DbFeatureRow>> Features { get; set; }

		public static void CreateSchema(Func<IDbContext> dbContextCreator)
		{
			if (dbContextCreator == null) throw new ArgumentNullException(nameof(dbContextCreator));

			using (var context = dbContextCreator())
			{
				FeatureAdapter.CreateSchema(context);
				context.Complete();
			}
		}

		public static void DropSchema(Func<IDbContext> dbContextCreator)
		{
			if (dbContextCreator == null) throw new ArgumentNullException(nameof(dbContextCreator));

			using (var context = dbContextCreator())
			{
				FeatureAdapter.DropSchema(context);
				context.Complete();
			}
		}

		public FeatureManager(Func<IDbContext> dbContextCreator)
		{
			if (dbContextCreator == null) throw new ArgumentNullException(nameof(dbContextCreator));

			this.DbContextCreator = dbContextCreator;
		}

		public void Load()
		{
			using (var context = this.DbContextCreator())
			{
				this.Contexts = FeatureAdapter.GetContexts(context);
				this.Features = this.GetFeatures(context);

				context.Complete();
			}
		}

		public void Save(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Stop the feature
			feature.Stop();

			var currentDetails = details;
			if (feature.TimeSpent != TimeSpan.Zero)
			{
				currentDetails = feature.TimeSpent.ToString(@"c");
			}

			using (var context = this.DbContextCreator())
			{
				var featureRow = this.Save(context, feature.Context, feature.Name);

				FeatureAdapter.InsertFeatureEntry(context, featureRow.Id, currentDetails ?? string.Empty);

				context.Complete();
			}
		}

		public void Save(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var context = this.DbContextCreator())
			{
				var featureRow = this.Save(context, feature.Context, feature.Name);
				var exceptionId = FeatureAdapter.GetOrCreateException(context, exception.ToString());

				FeatureAdapter.InsertExceptionEntry(context, featureRow.Id, exceptionId);

				context.Complete();
			}
		}

		private DbFeatureRow Save(IDbContext context, string featureContext, string name)
		{
			return this.SaveFeature(context, this.SaveContext(context, featureContext), name);
		}

		private DbFeatureContextRow SaveContext(IDbContext dbContext, string name)
		{
			DbFeatureContextRow featureContextRow;

			if (!this.Contexts.TryGetValue(name, out featureContextRow))
			{
				// Insert into database
				var newContextId = FeatureAdapter.InsertContext(dbContext, name);

				featureContextRow = new DbFeatureContextRow(newContextId, name);

				// Insert the new context into the collection
				this.Contexts.Add(name, featureContextRow);
			}

			return featureContextRow;
		}

		private DbFeatureRow SaveFeature(IDbContext dbContext, DbFeatureContextRow featureContextRow, string name)
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
			var newFeatureId = FeatureAdapter.InsertFeature(dbContext, name, contextId);

			var feature = new DbFeatureRow(newFeatureId, name, contextId);

			//Insert the new feature into the collection
			features.Add(feature);

			return feature;
		}

		private Dictionary<long, List<DbFeatureRow>> GetFeatures(IDbContext context)
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