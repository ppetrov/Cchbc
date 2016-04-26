using System;
using System.Collections.ObjectModel;
using Cchbc.Features.Admin.FeatureUserModule.Objects;
using Cchbc.Features.Admin.ViewModels;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureUserModule.ViewModels
{
	public sealed class UserOverviewViewModel : ViewModel
	{
		public string Name { get; }
		public ObservableCollection<FeatureCountByPeriodViewModel> Features { get; } = new ObservableCollection<FeatureCountByPeriodViewModel>();
		public ObservableCollection<FeatureCountByPeriodViewModel> Exceptions { get; } = new ObservableCollection<FeatureCountByPeriodViewModel>();

		public UserOverviewViewModel(UserOverview userOverview, TimePeriodViewModel[] timePeriods)
		{
			if (userOverview == null) throw new ArgumentNullException(nameof(userOverview));
			if (timePeriods == null) throw new ArgumentNullException(nameof(timePeriods));

			this.Name = userOverview.Name;

			foreach (var byPeriod in userOverview.Features)
			{
				this.Features.Add(new FeatureCountByPeriodViewModel(timePeriods[byPeriod.IndexedTimePeriod.Index], byPeriod.Count));
			}

			foreach (var byPeriod in userOverview.Exceptions)
			{
				this.Exceptions.Add(new FeatureCountByPeriodViewModel(timePeriods[byPeriod.IndexedTimePeriod.Index], byPeriod.Count));
			}
		}
	}
}