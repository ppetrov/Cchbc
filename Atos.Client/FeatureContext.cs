using System;
using System.Collections.Generic;
using Atos.Client.Data;
using Atos.Client.Features;

namespace Atos.Client
{
	public sealed class FeatureContext : IDisposable
	{
		public MainContext MainContext { get; }
		private Feature Feature { get; }
		private IDbContext DbContext { get; }

		public FeatureContext(MainContext mainContext, Feature feature)
		{
			if (mainContext == null) throw new ArgumentNullException(nameof(mainContext));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.MainContext = mainContext;
			this.Feature = feature;
			this.DbContext = mainContext.DbContextCreator();
		}

		public int Execute(Query query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.DbContext.Execute(query);
		}

		public IEnumerable<T> Execute<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			return this.DbContext.Execute(query);
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