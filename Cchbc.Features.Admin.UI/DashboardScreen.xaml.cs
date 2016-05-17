using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Cchbc.Features.Admin.FeatureDetailsModule;

namespace Cchbc.Features.Admin.UI
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
			var contextCreator = new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite"));

			try
			{
				using (var ctx = contextCreator.Create())
				{
					await this.ViewModel.LoadAsync(ctx,
						DashboardDataProvider.GetUsersAsync,
						DashboardDataProvider.GetVersionsAsync,
						DashboardDataProvider.GetExceptionsAsync,
						DashboardDataProvider.GetMostUsedFeaturesAsync,
						DashboardDataProvider.GetLeastUsedFeaturesAsync,
						DashboardDataProvider.GetSlowestFeaturesAsync);

					ctx.Complete();
				}
			}
			catch (Exception ex)
			{

			}
		}
	}
}
