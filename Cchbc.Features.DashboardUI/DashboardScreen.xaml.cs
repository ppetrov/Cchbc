using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Cchbc.Common;
using Cchbc.Features.DashboardModule;
using Cchbc.Features.DashboardModule.Data;
using Cchbc.Features.DashboardModule.ViewModels;
using Cchbc.Logs;

namespace Cchbc.Features.DashboardUI
{
	public sealed partial class DashboardScreen
	{
		public DashboardViewModel ViewModel { get; } = new DashboardViewModel(new Dashboard());

		public DashboardScreen()
		{
			this.InitializeComponent();
		}

		private async void DashboardScreenLoaded(object sender, RoutedEventArgs e)
		{
			var core = AppContext.Core;
			var feature = Feature.StartNew(@"Dashboard", @"Load");

			try
			{
				using (var coreContext = new CoreContext(core, feature))
				{
					await this.ViewModel.LoadAsync(coreContext,
						DashboardDataProvider.GetSettingsAsync,
						DashboardDataProvider.GetCommonDataAsync,
						DashboardDataProvider.GetUsersAsync,
						DashboardDataProvider.GetVersionsAsync,
						DashboardDataProvider.GetExceptionsAsync, DashboardDataProvider.GetMostUsedFeaturesAsync,
						DashboardDataProvider.GetLeastUsedFeaturesAsync, DashboardDataProvider.GetSlowestFeaturesAsync);
				}
			}
			catch (Exception ex)
			{
				core.Log(ex.ToString(), LogLevel.Error);
				core.FeatureManager.WriteException(feature, ex);
			}
		}

		private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			this.DashboardScreenLoaded(sender, e);
		}
	}
}
