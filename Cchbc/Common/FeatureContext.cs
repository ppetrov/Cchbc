using System;
using Cchbc.Data;
using Cchbc.Features;

namespace Cchbc.Common
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

		public void Dispose()
		{
			this.DbContext.Complete();
		}
	}
}