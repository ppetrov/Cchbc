using System;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureExceptionModule.Adapters;
using Cchbc.Features.Admin.FeatureExceptionModule.Objects;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.FeatureExceptionModule.Managers
{
	public sealed class FeatureExceptionManager
	{
		public ExceptionManagerAdapter Adapter { get; }

		public FeatureExceptionManager(ExceptionManagerAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public FeatureException[] GetBy(ITransactionContext context, TimePeriod timePeriod)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			return this.Adapter.GetBy(context, timePeriod);
		}
	}
}