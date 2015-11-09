using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Exceptions;
using Cchbc.Features;

namespace Cchbc
{
	public sealed class Core
	{
		public ILogger Logger { get; }
		public FeatureManager FeatureManager { get; set; } = new FeatureManager(entries => { });
		public ExceptionManager ExceptionManager { get; set; } = new ExceptionManager(entries => { });
		public QueryHelper QueryHelper { get; }
		public DataCache DataCache { get; } = new DataCache();

		public Core(ILogger logger, QueryHelper queryHelper)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.Logger = logger;
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