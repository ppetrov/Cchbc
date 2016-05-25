using System;
using System.Collections.Generic;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.DashboardModule
{
	public sealed class DashboardCommonData
	{
		public Dictionary<long, DbFeatureContextRow> Contexts { get; }
		public Dictionary<long, DbFeatureRow> Features { get; }

		public DashboardCommonData(Dictionary<long, DbFeatureContextRow> contexts, Dictionary<long, DbFeatureRow> features)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (features == null) throw new ArgumentNullException(nameof(features));

			this.Contexts = contexts;
			this.Features = features;
		}
	}
}