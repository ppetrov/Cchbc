using System;
using Atos.Common;

namespace Atos.Features.DashboardModule
{
	public sealed class DashboarLoadParams
	{
		public FeatureContext FeatureContext { get; }
		public DashboardSettings Settings { get; }
		public DashboardCommonData Data { get; }

		public DashboarLoadParams(FeatureContext featureContext, DashboardSettings settings, DashboardCommonData data)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));
			if (settings == null) throw new ArgumentNullException(nameof(settings));
			if (data == null) throw new ArgumentNullException(nameof(data));

			this.FeatureContext = featureContext;
			this.Settings = settings;
			this.Data = data;
		}
	}
}