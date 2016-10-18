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
			this.ViewModel = new LoginsViewModel(Context.Core, new LoginAdapter());
		}

		private async void LoginsScreenLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				var featureManager = Context.Core.FeatureManager;
				//await featureManager.CreateSchema();
				featureManager.Load(Context.Core.ContextCreator);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}

		private async void CreateActivityTapped(object sender, TappedRoutedEventArgs e)
		{
			var featureManager = Context.Core.FeatureManager;

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
				featureManager.WriteException(feature, ex);
			}
		}

		private async void CopyActivityTapped(object sender, TappedRoutedEventArgs e)
		{
			var r = new Random(0);
			var manager = Context.Core.FeatureManager;
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
