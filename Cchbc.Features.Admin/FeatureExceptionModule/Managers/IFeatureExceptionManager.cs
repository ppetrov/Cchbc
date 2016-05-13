using Cchbc.Data;
using Cchbc.Features.Admin.FeatureExceptionModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureExceptionModule.Managers
{
	public interface IFeatureExceptionManager
	{
		FeatureException[] GetBy(CommonDataProvider provider, ITransactionContext context, RangeTimePeriod rangeTimePeriod);
	}
}