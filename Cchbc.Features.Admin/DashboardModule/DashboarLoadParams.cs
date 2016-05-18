using System;
using Cchbc.Data;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.DashboardModule
{
	public sealed class DashboarLoadParams
	{
		public ITransactionContext Context { get; }
		public DashboardSettings Settings { get; }
		public CommonDataProvider DataProvider { get; }

		public DashboarLoadParams(ITransactionContext context, DashboardSettings settings, CommonDataProvider dataProvider)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));

			this.Context = context;
			this.Settings = settings;
			this.DataProvider = dataProvider;
		}
	}
}