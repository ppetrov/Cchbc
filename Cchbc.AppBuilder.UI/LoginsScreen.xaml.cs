﻿using System;
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
			Context.Core.ContextCreator = new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"features.sqlite"));
			this.ViewModel = new LoginsViewModel(Context.Core, new LoginAdapter());
		}

		private async void LoginsScreenLoaded(object sender, RoutedEventArgs e)
		{
			try
			{
				var featureManager = Context.Core.FeatureManager;
				featureManager.ContextCreator = Context.Core.ContextCreator;
				//await featureManager.CreateSchema();
				featureManager.Load();
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
