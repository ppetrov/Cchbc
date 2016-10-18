using System;
using Cchbc.Data;
using Cchbc.Features;

namespace Cchbc.Common
{
	public sealed class CoreContext : IDisposable
	{
		public Core Core { get; }
		public Feature Feature { get; }
		public ITransactionContext DbContext { get; }

		public CoreContext(Core core, Feature feature)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Core = core;
			this.Feature = feature;
			this.DbContext = core.ContextCreator.Create();
		}

		public void Dispose()
		{
			this.DbContext.Complete();
		}
	}
}