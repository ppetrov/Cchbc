using System;
using Cchbc.Data;
using Cchbc.Features.ExceptionsModule.Objects;

namespace Cchbc.Features.ExceptionsModule
{
	public sealed class ExceptionsDataLoadParams
	{
		public ITransactionContext Context { get; }
		public int MaxEntries { get; }
		public FeatureVersion Version { get; set; }
		public TimePeriod TimePeriod { get; set; }

		public ExceptionsDataLoadParams(ITransactionContext context, int maxEntries)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
			this.MaxEntries = maxEntries;
		}
	}
}