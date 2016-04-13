using System;
using System.Windows;
using Cchbc.Features;
using Cchbc.Features.Db;

namespace DataCollector.UI
{
	public partial class MainWindow
	{
		private static readonly string CnString = @"Data Source = C:\Users\codem\Desktop\iandonov.sqlite; Version = 3;";
		//private static readonly string CnString = @"Data Source = C:\Users\codem\Desktop\ppetrov.sqlite; Version = 3;";
		private static readonly Random Rnd = new Random();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void CreateButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			using (var context = new TransactionContextCreator(CnString).Create())
			{
				try
				{
					DbFeatureClientManager.CreateSchema(context);
					context.Complete();
				}
				catch (Exception ex)
				{
				}
			}
		}

		private void AgendaButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var feature = new FeatureEntry(@"Agenda", @"Load", string.Empty, TimeSpan.FromMilliseconds(Rnd.Next(250, 750)), new FeatureEntryStep[0]);

			Save(feature);
		}

		private void AgendaFilterButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var feature = new FeatureEntry(@"Agenda", @"Filter", string.Empty, TimeSpan.FromMilliseconds(Rnd.Next(250, 750)),
				new[]
				{
					new FeatureEntryStep(@"Find active activities", TimeSpan.FromMilliseconds(Rnd.Next(100, 200)), string.Empty),
					new FeatureEntryStep(@"Find orders", TimeSpan.FromMilliseconds(Rnd.Next(100, 200)), string.Empty),
					new FeatureEntryStep(@"Find promo quantities", TimeSpan.FromMilliseconds(Rnd.Next(100, 200)), string.Empty),
				});
			Save(feature);
		}

		private void AgendaSortButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var feature = new FeatureEntry(@"Agenda", @"Sort", string.Empty, TimeSpan.FromMilliseconds(Rnd.Next(250, 750)), new[]
				{
					new FeatureEntryStep(@"Find sort expression", TimeSpan.FromMilliseconds(Rnd.Next(100, 200)), string.Empty),
					new FeatureEntryStep(@"Apply sort", TimeSpan.FromMilliseconds(Rnd.Next(100, 200)), string.Empty),
					new FeatureEntryStep(@"Invert results", TimeSpan.FromMilliseconds(Rnd.Next(100, 200)), string.Empty),
				});
			Save(feature);
		}

		private void OutletsButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var feature = new FeatureEntry(@"Outlets", @"Load", string.Empty, TimeSpan.FromMilliseconds(Rnd.Next(250, 750)), new FeatureEntryStep[0]);
			Save(feature);
		}

		private void SettingsButtonBase_OnClick(object sender, RoutedEventArgs e)
		{
			var feature = new FeatureEntry(@"Settings", @"Load", string.Empty, TimeSpan.FromMilliseconds(Rnd.Next(250, 750)), new FeatureEntryStep[0]);
			Save(feature);
		}

		private static void Save(FeatureEntry feature)
		{
			using (var context = new TransactionContextCreator(CnString).Create())
			{
				var manager = new DbFeatureClientManager();
				manager.Load(context);
				manager.Save(context, feature);

				context.Complete();
			}
		}
	}
}
