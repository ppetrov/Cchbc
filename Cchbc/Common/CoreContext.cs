using System;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Logs;

namespace Cchbc.Common
{
	public sealed class CoreContext
	{
		public ITransactionContext DbContext { get; }
		public Action<string, LogLevel> Log { get; }
		public Feature Feature { get; }

		public CoreContext(ITransactionContext dbContext, Action<string, LogLevel> log, Feature feature)
		{
			if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
			if (log == null) throw new ArgumentNullException(nameof(log));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.DbContext = dbContext;
			this.Log = log;
			this.Feature = feature;
		}
	}
}