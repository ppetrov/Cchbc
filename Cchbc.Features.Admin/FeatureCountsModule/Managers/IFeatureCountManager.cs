using Cchbc.Data;
using Cchbc.Features.Admin.FeatureCountsModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureCountsModule.Managers
{
	public interface IFeatureCountManager
	{
		FeatureCount[] GetBy(CommonDataProvider provider, ITransactionContext context, RangeTimePeriod rangeTimePeriod);
	}
}