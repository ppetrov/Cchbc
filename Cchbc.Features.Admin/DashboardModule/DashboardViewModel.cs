using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Logs;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.DashboardModule
{
	public sealed class DashboardViewModel : ViewModel
	{
		private Dashboard Dashboard { get; }

		public ObservableCollection<DashboardUserViewModel> Users => this.Dashboard.Users;
		public ObservableCollection<DashboardVersionViewModel> Versions => this.Dashboard.Versions;
		public ObservableCollection<DashboardExceptionViewModel> Exceptions => this.Dashboard.Exceptions;
		public ObservableCollection<DashboardFeatureByCountViewModel> MostUsedFeatures => this.Dashboard.MostUsedFeatures;
		public ObservableCollection<DashboardFeatureByTimeViewModel> SlowestFeatures => this.Dashboard.SlowestFeatures;

		public string UsersCaption { get; }
		public string StatsCaption { get; }
		public string VersionStatsReportCaption { get; }
		public string ExceptionsCaptions { get; }
		public string UsageCaption { get; }

		// TODO : Load from database
		public int TotalUsers { get; } = 123;
		public int TotalVersions { get; } = 7;
		public int TotalExceptions { get; } = 138457;
		public int TotalFeatures { get; } = 35;

		public DashboardViewModel(Dashboard dashboard)
		{
			if (dashboard == null) throw new ArgumentNullException(nameof(dashboard));

			this.Dashboard = dashboard;

			// TODO : Load captions !!!!
			this.UsersCaption = @"Users";
			this.StatsCaption = @"Statistics";
			this.VersionStatsReportCaption = @"By Version statistics";
			this.UsageCaption = @"Most used/Slowest features";
			this.ExceptionsCaptions = @"Exceptions for the last 24 hours";
		}

		public Task LoadAsync(ITransactionContext context, Action<string, LogLevel> log, Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider, Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider, Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> mostUsedFeaturesProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> leastUsedFeaturesProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByTime>>> slowestFeaturesProvider)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (log == null) throw new ArgumentNullException(nameof(log));
			if (usersProvider == null) throw new ArgumentNullException(nameof(usersProvider));
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (mostUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(mostUsedFeaturesProvider));
			if (leastUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(leastUsedFeaturesProvider));
			if (slowestFeaturesProvider == null) throw new ArgumentNullException(nameof(slowestFeaturesProvider));

			return this.Dashboard.LoadAsync(context, log, usersProvider, versionsProvider, exceptionsProvider, mostUsedFeaturesProvider, leastUsedFeaturesProvider, slowestFeaturesProvider);
		}
	}
}