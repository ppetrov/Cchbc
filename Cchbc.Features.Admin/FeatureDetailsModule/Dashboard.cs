using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.Admin.Providers;
using Cchbc.Logs;

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
			Func<DashboarLoadParams, Task<List<DashboardFeatureByTime>>> slowestFeaturesProvider)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (usersProvider == null) throw new ArgumentNullException(nameof(usersProvider));
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (mostUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(mostUsedFeaturesProvider));
			if (leastUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(leastUsedFeaturesProvider));
			if (slowestFeaturesProvider == null) throw new ArgumentNullException(nameof(slowestFeaturesProvider));

			var dataProvider = new CommonDataProvider();
			await dataProvider.LoadAsync(context);

			var settings = GetDashboardSettings(null, dataProvider.Settings);
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

		private static DashboardSettings GetDashboardSettings(ILog log, Dictionary<string, List<Setting>> settings)
		{
			var context = nameof(Dashboard);

			List<Setting> byContext;
			if (settings.TryGetValue(context, out byContext))
			{
				var maxUsers = default(int?);
				var maxMostUsedFeatures = default(int?);

				foreach (var setting in byContext)
				{
					var name = setting.Name;
					var value = setting.Value;

					if (name.Equals(nameof(DashboardSettings.MaxUsers), StringComparison.OrdinalIgnoreCase))
					{
						maxUsers = ValueParser.ParseInt(value, log);
						break;
					}
					if (name.Equals(nameof(DashboardSettings.MaxMostUsedFeatures), StringComparison.OrdinalIgnoreCase))
					{
						maxMostUsedFeatures = ValueParser.ParseInt(value, log);
						break;
					}
				}

				if (maxUsers == null)
				{
					log.Log($@"Unable to find value for '{nameof(DashboardSettings.MaxUsers)}'", LogLevel.Warn);
				}
				if (maxMostUsedFeatures == null)
				{
					log.Log($@"Unable to find value for '{nameof(DashboardSettings.MaxMostUsedFeatures)}'", LogLevel.Warn);
				}

				return DashboardSettings.Default;
			}

			log.Log($@"Unable to find settings for '{context}'", LogLevel.Warn);

			return DashboardSettings.Default;
		}
	}
}