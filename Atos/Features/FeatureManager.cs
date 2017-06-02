using System;
using System.Collections.Generic;
using Atos.Data;
using Atos.Features.Data;

namespace Atos.Features
{
	public interface IFeatureManager
	{
		void Save(Feature feature, string details = null);
		void Save(Feature feature, Exception exception);
	}

	public sealed class FeatureManager : IFeatureManager
	{
		private Func<IDbContext> DbContextCreator { get; }
		private Dictionary<string, FeatureContextRow> Contexts { get; set; }
		private Dictionary<long, List<FeatureRow>> Features { get; set; }

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

		public void Save(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			using (var dbContext = this.DbContextCreator())
			{
				this.Load(dbContext);

				var featureRow = this.SaveFeature(dbContext, this.SaveContext(dbContext, feature.Context), feature.Name);

				FeatureAdapter.InsertFeatureEntry(dbContext, featureRow.Id, details ?? string.Empty);

				dbContext.Complete();
			}
		}

		public void Save(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var dbContext = this.DbContextCreator())
			{
				this.Load(dbContext);

				var featureRow = this.SaveFeature(dbContext, this.SaveContext(dbContext, feature.Context), feature.Name);

				var exceptionId = FeatureAdapter.GetOrCreateException(dbContext, exception.ToString());
				FeatureAdapter.InsertExceptionEntry(dbContext, exceptionId, featureRow.Id);

				dbContext.Complete();
			}
		}

		public byte[] GetClientDataBytes()
		{
			using (var dbContext = this.DbContextCreator())
			{
				this.Load(dbContext);

				var data = new ClientData();

				data.Add(this.Contexts);
				data.Add(FeatureAdapter.GetExceptions(dbContext));
				data.Add(this.Features);
				data.Add(FeatureAdapter.GetFeatureEntries(dbContext));
				data.Add(FeatureAdapter.GetExceptionEntries(dbContext));

				dbContext.Complete();

				return data.GetBytes();
			}
		}

		private void Load(IDbContext context)
		{
			if (this.Contexts == null)
			{
				this.Contexts = FeatureAdapter.GetContexts(context);
				this.Features = FeatureAdapter.GetFeatures(context);
			}
		}

		private FeatureContextRow SaveContext(IDbContext dbContext, string contextName)
		{
			FeatureContextRow context;

			if (!this.Contexts.TryGetValue(contextName, out context))
			{
				// Insert into database
				var newContextId = FeatureAdapter.InsertContext(dbContext, contextName);

				context = new FeatureContextRow(newContextId, contextName);

				// Insert the new context into the cache
				this.Contexts.Add(contextName, context);
			}

			return context;
		}

		private FeatureRow SaveFeature(IDbContext dbContext, FeatureContextRow context, string featureName)
		{
			var contextId = context.Id;

			List<FeatureRow> features;
			if (this.Features.TryGetValue(contextId, out features))
			{
				foreach (var f in features)
				{
					if (f.Name.Equals(featureName, StringComparison.OrdinalIgnoreCase))
					{
						return f;
					}
				}
			}
			else
			{
				// Create feature collection for this context
				features = new List<FeatureRow>();
				this.Features.Add(contextId, features);
			}

			// Insert into database
			var newFeatureId = FeatureAdapter.InsertFeature(dbContext, featureName, contextId);

			var feature = new FeatureRow(newFeatureId, featureName, contextId);

			//Insert the new feature into the cache
			features.Add(feature);

			return feature;
		}
	}
}