using System;
using Cchbc.Data;
using Cchbc.Features.Db.Managers;

namespace Cchbc.Features
{
	public sealed class FeatureManager
	{
		private DbFeatureClientManager DbClientClientManager { get; } = new DbFeatureClientManager();

		private ITransactionContextCreator ContextCreator { get; set; }

		public void CreateSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureClientManager.CreateSchema(context);
		}

		public void DropSchema(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			DbFeatureClientManager.DropSchema(context);
		}

		public void Load(ITransactionContextCreator contextCreator)
		{
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			this.ContextCreator = contextCreator;

			using (var context = this.ContextCreator.Create())
			{
				this.DbClientClientManager.Load(context);

				context.Complete();
			}
		}

		public void Start(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.Start();
		}

		public void Stop(Feature feature, string details = null)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			// Stop the feature
			feature.Stop();

			using (var context = this.ContextCreator.Create())
			{
				this.DbClientClientManager.Save(context, feature, details ?? string.Empty);

				context.Complete();
			}
		}

		public void LogException(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			using (var context = this.ContextCreator.Create())
			{
				this.DbClientClientManager.Save(context, new FeatureException(feature.Context, feature.Name, exception));

				context.Complete();
			}
		}

		public Feature StartNew(string context, string name)
		{
			var feature = new Feature(context, name);

			feature.Start();

			return feature;
		}
	}
}