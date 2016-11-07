using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.Common;
using Cchbc.Objects;

namespace Cchbc.Features.DashboardModule.ViewModels
{
	public sealed class DashboardViewModel : ViewModel
	{
		private Dashboard Dashboard { get; }

		public ObservableCollection<DashboardUserViewModel> Users = new ObservableCollection<DashboardUserViewModel>();
		public ObservableCollection<DashboardVersionViewModel> Versions = new ObservableCollection<DashboardVersionViewModel>();
		public ObservableCollection<DashboardExceptionViewModel> Exceptions = new ObservableCollection<DashboardExceptionViewModel>();
		public ObservableCollection<DashboardFeatureByCountViewModel> MostUsedFeatures = new ObservableCollection<DashboardFeatureByCountViewModel>();
		public ObservableCollection<DashboardFeatureByCountViewModel> LeastUsedFeatures = new ObservableCollection<DashboardFeatureByCountViewModel>();
		public ObservableCollection<DashboardFeatureByTimeViewModel> SlowestFeatures = new ObservableCollection<DashboardFeatureByTimeViewModel>();

		public string UsersCaption { get; }
		public string StatsCaption { get; }
		public string VersionStatsReportCaption { get; }
		public string ExceptionsCaptions { get; }
		public string UsageCaption { get; }

		// TODO : Load from database, Define a class = DashboardStats with this as properties
		public int TotalUsers { get; } = 123;
		public int TotalVersions { get; } = 7;
		public int TotalExceptions { get; } = 138457;
		public int TotalFeatures { get; } = 35;

		public DashboardViewModel(Dashboard dashboard)
		{
			if (dashboard == null) throw new ArgumentNullException(nameof(dashboard));

			this.Dashboard = dashboard;

			// TODO : Load captions !!!! from a dictionary by key ???
			this.UsersCaption = @"Users";
			this.StatsCaption = @"Statistics";
			this.VersionStatsReportCaption = @"By Version statistics";
			this.UsageCaption = @"Most used/Slowest features";
			this.ExceptionsCaptions = @"Exceptions for the last 24 hours";
		}

		public async Task LoadAsync(FeatureContext featureContext, Func<FeatureContext, Task<DashboardSettings>> settingsProvider,
			Func<FeatureContext, Task<DashboardCommonData>> commonDataProvider,
			Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider, Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider,
			Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> mostUsedFeaturesProvider,
			Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> leastUsedFeaturesProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByTime>>> slowestFeaturesProvider)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));
			if (usersProvider == null) throw new ArgumentNullException(nameof(usersProvider));
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (mostUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(mostUsedFeaturesProvider));
			if (leastUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(leastUsedFeaturesProvider));
			if (slowestFeaturesProvider == null) throw new ArgumentNullException(nameof(slowestFeaturesProvider));
			if (settingsProvider == null) throw new ArgumentNullException(nameof(settingsProvider));

			await this.Dashboard.LoadAsync(featureContext, settingsProvider, commonDataProvider, usersProvider, versionsProvider, exceptionsProvider, mostUsedFeaturesProvider, leastUsedFeaturesProvider, slowestFeaturesProvider);

			LoadData(this.Dashboard.Users, this.Users, v => new DashboardUserViewModel(v));
			LoadData(this.Dashboard.Versions, this.Versions, v => new DashboardVersionViewModel(v));
			LoadData(this.Dashboard.Exceptions, this.Exceptions, v => new DashboardExceptionViewModel(v));
			LoadData(this.Dashboard.MostUsedFeatures, this.MostUsedFeatures, u => new DashboardFeatureByCountViewModel(u));
			LoadData(this.Dashboard.LeastUsedFeatures, this.LeastUsedFeatures, u => new DashboardFeatureByCountViewModel(u));
			LoadData(this.Dashboard.SlowestFeatures, this.SlowestFeatures, u => new DashboardFeatureByTimeViewModel(u));
		}

		private static void LoadData<T, TV>(List<T> values, ObservableCollection<TV> viewModels, Func<T, TV> creator)
		{
			viewModels.Clear();
			foreach (var value in values)
			{
				viewModels.Add(creator(value));
			}
		}
	}
}