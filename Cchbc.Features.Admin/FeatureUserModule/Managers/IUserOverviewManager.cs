using Cchbc.Data;
using Cchbc.Features.Admin.FeatureUserModule.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureUserModule.Managers
{
	public interface IUserOverviewManager
	{
		UserOverview[] GetBy(CommonDataProvider dataProvider, ITransactionContext context, IndexedTimePeriod[] timePeriods);
	}
}