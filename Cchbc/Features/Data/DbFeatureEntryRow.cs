using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureEntryRow
	{
		public readonly long Id;
		public readonly double TimeSpent;
		public readonly string Details;
		public readonly DateTime CreatedAt;
		public readonly int FeatureId;

		public DbFeatureEntryRow(long id, double timeSpent, string details, DateTime createdAt, int featureId)
		{
			this.Id = id;
			this.TimeSpent = timeSpent;
			this.Details = details;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}