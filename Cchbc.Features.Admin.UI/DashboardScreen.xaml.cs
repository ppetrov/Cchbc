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

		private void DashboardScreenLoaded(object sender, RoutedEventArgs e)
		{
			var creator = new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite"));
			this.ViewModel.Load(creator, DashboardDataProvider.GetSettings, DashboardDataProvider.GetVersions, DashboardDataProvider.GetExceptions);
		}
	}
}
