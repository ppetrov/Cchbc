using System;
using Cchbc.Data;
using Cchbc.Features;

namespace Cchbc.Common
{
	public sealed class FeatureContext : IDisposable
	{
		public AppContext AppContext { get; }
		public Feature Feature { get; }
		public IDbContext DbContext { get; }

		public FeatureContext(AppContext appContext, Feature feature)
		{
			if (appContext == null) throw new ArgumentNullException(nameof(appContext));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.AppContext = appContext;
			this.Feature = feature;
			this.DbContext = appContext.DbContextCreator();
		}

		public void Dispose()
		{
			this.DbContext.Complete();
		}
	}
}