using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Cchbc.Features.ExceptionsModule;

namespace Cchbc.Features.DashboardUI
{
	public sealed partial class ExceptionsScreen
	{
		public ExceptionsViewModel ViewModel { get; } = new ExceptionsViewModel(new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite")), ExceptionsSettings.Default);

		public ExceptionsScreen()
		{
			this.InitializeComponent();
			this.DataContext = this.ViewModel;
		}

		private void ExceptionsScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				var s = Stopwatch.StartNew();
				this.ViewModel.Load(
					ExceptionsDataProvider.GetTimePeriods,
					ExceptionsDataProvider.GetVersions,
					ExceptionsDataProvider.GetExceptions,
					ExceptionsDataProvider.GetExceptionsCounts);
				s.Stop();
				Debug.WriteLine(s.ElapsedMilliseconds);
			}
			catch (Exception ex)
			{

			}
		}
	}
}
