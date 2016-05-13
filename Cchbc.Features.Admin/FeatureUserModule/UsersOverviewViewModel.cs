using System;
using System.Collections.ObjectModel;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureUserModule.Managers;
using Cchbc.Features.Admin.FeatureUserModule.Objects;
using Cchbc.Features.Admin.FeatureUserModule.ViewModels;
using Cchbc.Features.Admin.Helpers;
using Cchbc.Features.Admin.Providers;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureUserModule
{
	public sealed class UsersOverviewViewModel : ViewModel
	{
		private ITransactionContextCreator Provider { get; }
		private CommonDataProvider DataProvider { get; }
		private IUserOverviewManager UserOverviewManager { get; }

		public ObservableCollection<UserOverviewViewModel> Users { get; } = new ObservableCollection<UserOverviewViewModel>();

		public UsersOverviewViewModel(ITransactionContextCreator provider, CommonDataProvider dataProvider, IUserOverviewManager userOverviewManager)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (userOverviewManager == null) throw new ArgumentNullException(nameof(userOverviewManager));

			this.Provider = provider;
			this.DataProvider = dataProvider;
			this.UserOverviewManager = userOverviewManager;
		}

		public void Load()
		{
			var timePeriods = TimePeriodHelper.GetStandardPeriods();

			UserOverview[] userOverviews;

			using (var ctx = this.Provider.Create())
			{
				var periods = new IndexedTimePeriod[timePeriods.Length];
				for (var i = 0; i < timePeriods.Length; i++)
				{
					periods[i] = new IndexedTimePeriod(i, timePeriods[i].RangeTimePeriod);
				}

				userOverviews = this.UserOverviewManager.GetBy(this.DataProvider, ctx, periods);

				ctx.Complete();
			}

			this.Users.Clear();
			foreach (var overview in userOverviews)
			{
				this.Users.Add(new UserOverviewViewModel(overview, timePeriods));
			}
		}
	}
}