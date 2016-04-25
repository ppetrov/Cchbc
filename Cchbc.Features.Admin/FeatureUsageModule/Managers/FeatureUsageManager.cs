using System;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureUsageModule.Adapters;
using Cchbc.Features.Admin.FeatureUsageModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureUsageModule.Managers
{
	public sealed class FeatureUsageManager
	{
		public FeatureUsageAdapter Adapter { get; }

		public FeatureUsageManager(FeatureUsageAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public FeatureUsage[] GetBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			return this.Adapter.GetBy(context, timePeriod, provider.Contexts, provider.Features);
		}
	}
}