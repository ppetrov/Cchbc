using System;
using Cchbc.Features.Data;

namespace Cchbc.Features.DashboardModule
{
	public sealed class DashboardFeatureByCount
	{
		public FeatureContextRow Context { get; }
		public FeatureRow Feature { get; }
		public int Count { get; }

		public DashboardFeatureByCount(FeatureContextRow context, FeatureRow feature, int count)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Context = context;
			this.Feature = feature;
			this.Count = count;
		}
	}
}