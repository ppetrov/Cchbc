using System;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Cchbc.Features.ExceptionsModule;

namespace Cchbc.Features.DashboardUI
{
	public sealed partial class ExceptionsScreen
	{
		public ExceptionsViewModel ViewModel { get; } = new ExceptionsViewModel(new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite")), new ExceptionsSettings());

		public ExceptionsScreen()
		{
			this.InitializeComponent();
			this.DataContext = this.ViewModel;
		}

		private void ExceptionsScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				this.ViewModel.Load(
					ExceptionsDataProvider.GetTimePeriods,
					ExceptionsDataProvider.GetVersions,
					ExceptionsDataProvider.GetExceptions,
					ExceptionsDataProvider.GetExceptionsCounts);
			}
			catch (Exception ex)
			{

			}
		}
	}
}
