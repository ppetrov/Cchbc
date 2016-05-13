using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureUserModule.Adapters;
using Cchbc.Features.Admin.FeatureUserModule.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureUserModule.Managers
{
	public sealed class UserOverviewManager : IUserOverviewManager
	{
		public UserOverviewAdapter Adapter { get; }

		public UserOverviewManager(UserOverviewAdapter adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public UserOverview[] GetBy(CommonDataProvider dataProvider, ITransactionContext context, IndexedTimePeriod[] timePeriods)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (timePeriods == null) throw new ArgumentNullException(nameof(timePeriods));

			var map = new Dictionary<string, UserOverview>();

			foreach (var indexedTimePeriod in timePeriods)
			{
				foreach (var featureCount in this.Adapter.GetFeaturesBy(dataProvider, context, indexedTimePeriod.RangeTimePeriod))
				{
					var userOverview = GetOrCreateUserOverview(map, featureCount.User, timePeriods);

					userOverview.Features[indexedTimePeriod.Index].Count = featureCount.Value;
				}

				foreach (var featureCount in this.Adapter.GetExceptionsBy(dataProvider, context, indexedTimePeriod.RangeTimePeriod))
				{
					var userOverview = GetOrCreateUserOverview(map, featureCount.User, timePeriods);

					userOverview.Exceptions[indexedTimePeriod.Index].Count = featureCount.Value;
				}
			}

			var overviews = new UserOverview[map.Count];
			map.Values.CopyTo(overviews, 0);

			return overviews;
		}

		private static UserOverview GetOrCreateUserOverview(Dictionary<string, UserOverview> map, string user, IndexedTimePeriod[] timePeriods)
		{
			UserOverview userOverview;

			if (!map.TryGetValue(user, out userOverview))
			{
				userOverview = new UserOverview(user, timePeriods);
				map.Add(user, userOverview);
			}

			return userOverview;
		}
	}
}