using System;
using Atos.Client.Data;
using Atos.Client.Features;

namespace Atos.Client
{
	public sealed class FeatureContext : IDisposable
	{
		public MainContext MainContext { get; }
		public Feature Feature { get; }
		public IDbContext DbContext { get; }

		public FeatureContext(MainContext mainContext, Feature feature)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.MainContext = mainContext;
			this.Feature = feature;
			this.DbContext = mainContext.DbContextCreator();
		}

		public void Complete()
		{
			this.DbContext.Complete();
		}

		public void Dispose()
		{
			this.DbContext.Dispose();
		}
	}
}