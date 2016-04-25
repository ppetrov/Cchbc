using System;
using Cchbc.Data;
using Cchbc.Features.Admin.Providers;
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

		public FeatureException[] GetBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			return this.Adapter.GetBy(provider, context, timePeriod);
		}
	}
}