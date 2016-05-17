using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.Admin.Providers;


namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class Dashboard
	{
		public ObservableCollection<DashboardUserViewModel> Users { get; } = new ObservableCollection<DashboardUserViewModel>();
		public ObservableCollection<DashboardVersionViewModel> Versions { get; } = new ObservableCollection<DashboardVersionViewModel>();
		public ObservableCollection<DashboardExceptionViewModel> Exceptions { get; } = new ObservableCollection<DashboardExceptionViewModel>();
		public ObservableCollection<DashboardFeatureByCountViewModel> MostUsedFeatures { get; } = new ObservableCollection<DashboardFeatureByCountViewModel>();
		public ObservableCollection<DashboardFeatureByCountViewModel> LeastUsedFeatures { get; } = new ObservableCollection<DashboardFeatureByCountViewModel>();
		public ObservableCollection<DashboardFeatureByTimeViewModel> SlowestFeatures { get; } = new ObservableCollection<DashboardFeatureByTimeViewModel>();

		public async Task LoadAsync(ITransactionContext context,
			Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider,
			Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider,
			Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider,
			Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> mostUsedFeaturesProvider,
			Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> leastUsedFeaturesProvider,
			Func<DashboarLoadParams, Task<List<DashboardFeatureByTime>>> slowestFeaturesProvider
			)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (usersProvider == null) throw new ArgumentNullException(nameof(usersProvider));
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (mostUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(mostUsedFeaturesProvider));
			if (leastUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(leastUsedFeaturesProvider));
			if (slowestFeaturesProvider == null) throw new ArgumentNullException(nameof(slowestFeaturesProvider));

			// Load common data - users, version, settings, features
			var dataProvider = new CommonDataProvider();
			await dataProvider.LoadAsync(context);

			var settings = GetDashboardSettings(dataProvider.Settings);
			var loadParams = new DashboarLoadParams(context, settings, dataProvider);

			var tasks = new[]
			{
				this.LoadUsersAsync(loadParams, usersProvider),
				this.LoadVersionsAsync(loadParams, versionsProvider),
				this.LoadExceptionsAsync(loadParams, exceptionsProvider),
				this.LoadMostUsedFeaturesAsync(loadParams, mostUsedFeaturesProvider),
				this.LoadLeastUsedFeaturesAsync(loadParams, leastUsedFeaturesProvider),
				this.LoadSlowestUsedFeaturesAsync(loadParams, slowestFeaturesProvider),
			};

			await Task.WhenAll(tasks);
		}

		private static DashboardSettings GetDashboardSettings(Dictionary<string, List<Setting>> settings)
		{
			var dashboardSettings = new DashboardSettings();

			List<Setting> byContext;
			if (settings.TryGetValue(nameof(Dashboard), out byContext))
			{
				foreach (var setting in byContext)
				{
					//TODO : !!!
					//if (setting.Name.Equals(nameof(DashboardSettings.NumberOfUsersToDisplay), StringComparison.OrdinalIgnoreCase))
					//{
					//	settings.NumberOfUsersToDisplay = ValueParser.ParseInt(setting.Value) ?? 10;
					//	break;
					//}
				}
			}

			return dashboardSettings;
		}

		private async Task LoadUsersAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider)
		{
			var users = await usersProvider(loadParams);

			this.Users.Clear();
			foreach (var user in users)
			{
				this.Users.Add(new DashboardUserViewModel(user));
			}
		}

		private async Task LoadVersionsAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider)
		{
			var versions = await versionsProvider(loadParams);

			this.Versions.Clear();
			foreach (var version in versions)
			{
				this.Versions.Add(new DashboardVersionViewModel(version));
			}
		}

		private async Task LoadExceptionsAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider)
		{
			var exceptions = await exceptionsProvider(loadParams);

			this.Exceptions.Clear();
			foreach (var exception in exceptions)
			{
				this.Exceptions.Add(new DashboardExceptionViewModel(exception));
			}
		}

		private async Task LoadMostUsedFeaturesAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> featuresProvider)
		{
			var features = await featuresProvider(loadParams);

			this.MostUsedFeatures.Clear();
			foreach (var feature in features)
			{
				this.MostUsedFeatures.Add(new DashboardFeatureByCountViewModel(feature));
			}
		}

		private async Task LoadLeastUsedFeaturesAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> featuresProvider)
		{
			var features = await featuresProvider(loadParams);

			this.LeastUsedFeatures.Clear();
			foreach (var feature in features)
			{
				this.LeastUsedFeatures.Add(new DashboardFeatureByCountViewModel(feature));
			}
		}

		private async Task LoadSlowestUsedFeaturesAsync(DashboarLoadParams loadParams, Func<DashboarLoadParams, Task<List<DashboardFeatureByTime>>> featuresProvider)
		{
			var features = await featuresProvider(loadParams);

			this.SlowestFeatures.Clear();
			foreach (var feature in features)
			{
				this.SlowestFeatures.Add(new DashboardFeatureByTimeViewModel(feature));
			}
		}
	}

}