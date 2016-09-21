using System;
using System.Diagnostics;
using System.IO;
using Windows.Storage;
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
			var contextCreator = new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite"));
			try
			{
				Action<string, LogLevel> log = (msg, level) =>
				{
					// Easy to port to a file
					// or db !!!
					Debug.WriteLine(level + ":" + msg);
				};

				var fm = new FeatureManager { ContextCreator = contextCreator };
				fm.LoadAsync();

				var feature = Feature.StartNew(@"Dashboard", @"Load");

				var w = Stopwatch.StartNew();
				using (var ctx = contextCreator.Create())
				{
					var coreContext = new CoreContext(ctx, log, feature);

					await this.ViewModel.LoadAsync(coreContext,
						DashboardDataProvider.GetSettingsAsync,
						DashboardDataProvider.GetCommonDataAsync,
						DashboardDataProvider.GetUsersAsync,
						DashboardDataProvider.GetVersionsAsync,
						DashboardDataProvider.GetExceptionsAsync, DashboardDataProvider.GetMostUsedFeaturesAsync,
						DashboardDataProvider.GetLeastUsedFeaturesAsync, DashboardDataProvider.GetSlowestFeaturesAsync);

					ctx.Complete();
				}
				w.Stop();
				feature.Stop();

				log(string.Empty + w.Elapsed.TotalMilliseconds, LogLevel.Info);

				foreach (var s in feature.Steps)
				{
					log(@" - " + s.Name + @" [" + @"N/А" + @"]" + @": " + s.TimeSpent.TotalMilliseconds + " ms", LogLevel.Info);
				}
			}
			catch (Exception ex)
			{

			}
		}

		private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
		{
			this.DashboardScreenLoaded(sender, e);
		}
	}
}
