using System;
using System.Collections.Generic;
using Atos.Features.Data;

namespace Atos.Features.DashboardModule
{
	public sealed class DashboardCommonData
	{
		public Dictionary<long, FeatureContextRow> Contexts { get; }
		public Dictionary<long, FeatureRow> Features { get; }

		public DashboardCommonData(Dictionary<long, FeatureContextRow> contexts, Dictionary<long, FeatureRow> features)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (features == null) throw new ArgumentNullException(nameof(features));

			this.Contexts = contexts;
			this.Features = features;
		}
	}
}