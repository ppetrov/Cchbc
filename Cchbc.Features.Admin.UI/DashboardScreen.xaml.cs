using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Cchbc.Features.Admin.FeatureDetailsModule;

namespace Cchbc.Features.Admin.UI
{
	public sealed partial class DashboardScreen
	{
		public DashboardViewModel ViewModel { get; } = new DashboardViewModel(new Dashboard(new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite"))));

		public DashboardScreen()
		{
			this.InitializeComponent();
		}

		private void DashboardScreenLoaded(object sender, RoutedEventArgs e)
		{
			this.ViewModel.Load();
		}

		private async void BtnSearchUsersTapped(object sender, TappedRoutedEventArgs e)
		{
			var dialog = new SearchUsersContentDialog(vm =>
			{
				// TODO : !!!
			});
			await dialog.ShowAsync();
		}
	}
}
