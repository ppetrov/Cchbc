using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features;

namespace Cchbc
{
	public sealed class Core
	{
		public ILogger Logger { get; }
		public FeatureManager FeatureManager { get; }
		public QueryHelper QueryHelper { get; }
		public DataCache DataCache { get; } = new DataCache();

		public Core(ILogger logger, FeatureManager featureManager, QueryHelper queryHelper)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (featureManager == null) throw new ArgumentNullException(nameof(featureManager));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.Logger = logger;
			this.FeatureManager = featureManager;
			this.QueryHelper = queryHelper;
		}

		public async Task LoadDataAsync()
		{
			var s = Stopwatch.StartNew();
			try
			{
				await this.DataCache.LoadAsync(this.Logger, this.QueryHelper.ReadDataQueryHelper);
			}
			finally
			{
				this.Logger.Info($@"{nameof(LoadDataAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}
	}
}