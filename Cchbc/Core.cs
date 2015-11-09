using System;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Features.Db;
using Cchbc.Localization;

namespace Cchbc
{
	public sealed class Core
	{
		public ILogger Logger { get; }
		public QueryHelper QueryHelper { get; }
		public DataCache DataCache { get; } = new DataCache();
		public FeatureManager FeatureManager { get; } = new FeatureManager();
		public LocalizationManager LocalizationManager { get; } = new LocalizationManager();

		public Core(ILogger logger, QueryHelper queryHelper)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.Logger = logger;
			this.QueryHelper = queryHelper;
		}

		public async Task LoadAsync(IDbFeaturesManager dbFeaturesManager)
		{
			await dbFeaturesManager.LoadAsync();

			this.FeatureManager.Initialize(this.Logger, dbFeaturesManager);
			this.FeatureManager.StartDbWriters();

			await this.LoadDataAsync();
		}

		private async Task LoadDataAsync()
		{
			var feature = Feature.StartNew(@"Core app load", nameof(LoadDataAsync));
			try
			{
				await this.DataCache.LoadAsync(this.Logger, this.QueryHelper.ReadDataQueryHelper);
			}
			finally
			{
				this.FeatureManager.Stop(feature);
			}
		}
	}
}