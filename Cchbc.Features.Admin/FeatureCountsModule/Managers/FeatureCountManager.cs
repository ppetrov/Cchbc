using System;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureCountsModule.Adapters;
using Cchbc.Features.Admin.FeatureCountsModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureCountsModule.Managers
{
	public sealed class FeatureCountManager : IFeatureCountManager
    {
		public FeatureCountAdapter Adapter { get; }

		public FeatureCountManager(FeatureCountAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public FeatureCount[] GetBy(CommonDataProvider provider, ITransactionContext context, RangeTimePeriod rangeTimePeriod)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (rangeTimePeriod == null) throw new ArgumentNullException(nameof(rangeTimePeriod));

			return this.Adapter.GetBy(provider, context, rangeTimePeriod);
		}
	}
}