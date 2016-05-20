using System;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.DashboardModule
{
	public sealed class DashboardFeatureByCount
	{
		public DbFeatureContextRow Context { get; }
		public DbFeatureRow Feature { get; }
		public int Count { get; }

		public DashboardFeatureByCount(DbFeatureContextRow context, DbFeatureRow feature, int count)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Context = context;
			this.Feature = feature;
			this.Count = count;
		}
	}
}