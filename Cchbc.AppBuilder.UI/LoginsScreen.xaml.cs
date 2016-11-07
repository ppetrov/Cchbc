using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Cchbc.Features;
using LoginModule.Adapter;
using LoginModule.ViewModels;

namespace Cchbc.AppBuilder.UI
{
	public sealed partial class LoginsScreen
	{
		public LoginsViewModel ViewModel { get; }

		public LoginsScreen()
		{
			this.InitializeComponent();
			this.ViewModel = new LoginsViewModel(MainContext.AppContext, new LoginAdapter());
		}

		private async void LoginsScreenLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				var featureManager = MainContext.AppContext.FeatureManager;
				//await featureManager.CreateSchema();
				featureManager.Load(MainContext.AppContext.DbContextCreator);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		private async void CreateActivityTapped(object sender, TappedRoutedEventArgs e)
		{
			var featureManager = MainContext.AppContext.FeatureManager;

			var feature = Feature.StartNew(@"Agenda", @"CreateActivity");
			try
			{

				{
					var r = new Random(0);
					await Task.Delay(r.Next(20));

					{
						await Task.Delay(r.Next(20));

						{
							await Task.Delay(r.Next(20));

							{
								await Task.Delay(r.Next(20));

								{
									await Task.Delay(r.Next(20));
								}
							}
						}
					}
				}

				featureManager.Write(feature);
			}
			catch (Exception ex)
			{
				featureManager.Write(feature, ex);
			}
		}

		private async void CopyActivityTapped(object sender, TappedRoutedEventArgs e)
		{
			var r = new Random(0);
			var manager = MainContext.AppContext.FeatureManager;
			var f = Feature.StartNew(@"Agenda", @"CopyActivity");
			try
			{

				{
					await Task.Delay(r.Next(20));
				}
				{
					await Task.Delay(r.Next(20));
				}
				{
					await Task.Delay(r.Next(20));
				}
				{
					await Task.Delay(r.Next(20));

					{
						await Task.Delay(r.Next(20));
					}
				}
			}
			finally
			{
				manager.Write(f);
			}
		}
	}
}
