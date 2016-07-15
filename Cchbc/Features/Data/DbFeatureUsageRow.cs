using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureUsageRow
	{
		public readonly DateTime CreatedAt;
		public readonly int FeatureId;

		public DbFeatureUsageRow(DateTime createdAt, int featureId)
		{
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}