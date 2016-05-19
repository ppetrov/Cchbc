using System;
using Cchbc.Common;

namespace Cchbc.Features.Admin.DashboardModule
{
	public sealed class DashboarLoadParams
	{
		public CoreContext CoreContext { get; }
		public DashboardSettings Settings { get; }
		public DashboardCommonData Data { get; }

		public DashboarLoadParams(CoreContext coreContext, DashboardSettings settings, DashboardCommonData data)
		{
			if (coreContext == null) throw new ArgumentNullException(nameof(coreContext));
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (data == null) throw new ArgumentNullException(nameof(data));

			this.CoreContext = coreContext;
			this.Settings = settings;
			this.Data = data;
		}
	}
}