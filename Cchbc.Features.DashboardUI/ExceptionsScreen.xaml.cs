using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
using Windows.UI.Xaml;
using Cchbc.Features.ExceptionsModule;
using Cchbc.Logs;

namespace Cchbc.Features.DashboardUI
{
	public sealed partial class ExceptionsScreen
	{
		public ExceptionsViewModel ViewModel { get; } = new ExceptionsViewModel(new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite")).Create, ExceptionsSettings.Default);

		public ExceptionsScreen()
		{
			this.InitializeComponent();
			this.DataContext = this.ViewModel;
		}

		private void ExceptionsScreen_OnLoaded(object sender, RoutedEventArgs e)
		{
			var core = AppContextObsolete.MainContext;
			var feature = new Feature(@"Exceptions", @"Load");
			try
			{
				var s = Stopwatch.StartNew();

				try
				{
					FeatureManager.CreateSchema(core.DbContextCreator);
				}
				catch { }

				core.FeatureManager.Load(core.DbContextCreator);

				core.FeatureManager.Save(feature, string.Empty);
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
				core.Log(ex.ToString(), LogLevel.Error);
				core.FeatureManager.Save(feature, ex);
			}
		}
	}
}
