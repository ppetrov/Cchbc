using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Common;

namespace Cchbc.Features.DashboardModule
{
	public sealed class Dashboard
	{
		public List<DashboardUser> Users { get; private set; }
		public List<DashboardVersion> Versions { get; private set; }
		public List<DashboardException> Exceptions { get; private set; }
		public List<DashboardFeatureByCount> MostUsedFeatures { get; private set; }
		public List<DashboardFeatureByCount> LeastUsedFeatures { get; private set; }
		public List<DashboardFeatureByTime> SlowestFeatures { get; private set; }

		public async Task LoadAsync(FeatureContext featureContext,
			Func<FeatureContext, Task<DashboardSettings>> settingsProvider,
			Func<FeatureContext, Task<DashboardCommonData>> commonDataProvider,
			Func<DashboarLoadParams, Task<List<DashboardUser>>> usersProvider, Func<DashboarLoadParams, Task<List<DashboardVersion>>> versionsProvider,
			Func<DashboarLoadParams, Task<List<DashboardException>>> exceptionsProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> mostUsedFeaturesProvider,
			Func<DashboarLoadParams, Task<List<DashboardFeatureByCount>>> leastUsedFeaturesProvider, Func<DashboarLoadParams, Task<List<DashboardFeatureByTime>>> slowestFeaturesProvider)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));
			if (settingsProvider == null) throw new ArgumentNullException(nameof(settingsProvider));
			if (commonDataProvider == null) throw new ArgumentNullException(nameof(commonDataProvider));
			if (usersProvider == null) throw new ArgumentNullException(nameof(usersProvider));
			if (versionsProvider == null) throw new ArgumentNullException(nameof(versionsProvider));
			if (exceptionsProvider == null) throw new ArgumentNullException(nameof(exceptionsProvider));
			if (mostUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(mostUsedFeaturesProvider));
			if (leastUsedFeaturesProvider == null) throw new ArgumentNullException(nameof(leastUsedFeaturesProvider));
			if (slowestFeaturesProvider == null) throw new ArgumentNullException(nameof(slowestFeaturesProvider));

			var settings = await settingsProvider(featureContext);
			var dataProvider = await commonDataProvider(featureContext);
			var loadParams = new DashboarLoadParams(featureContext, settings, dataProvider);

			this.Users = await usersProvider(loadParams);
			this.Versions = await versionsProvider(loadParams);
			this.Exceptions = await exceptionsProvider(loadParams);
			this.MostUsedFeatures = await mostUsedFeaturesProvider(loadParams);
			this.LeastUsedFeatures = await leastUsedFeaturesProvider(loadParams);
			this.SlowestFeatures = await slowestFeaturesProvider(loadParams);
		}
	}
}