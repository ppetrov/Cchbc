using System.IO;
using Windows.Storage;
using Cchbc.Features.Admin.FeatureDetailsModule;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.UI
{
	public sealed partial class FeatureDetailsScreen
	{
		public FeatureDetailsViewModel ViewModel { get; }

		public FeatureDetailsScreen()
		{
			this.InitializeComponent();

			var dataProvider = new CommonDataProvider();

			var creator = new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"server.sqlite"));
			using (var ctx = creator.Create())
			{
				dataProvider.Load(ctx);

				ctx.Complete();
			}

			this.ViewModel = new FeatureDetailsViewModel(dataProvider, creator);
		}
	}
}
