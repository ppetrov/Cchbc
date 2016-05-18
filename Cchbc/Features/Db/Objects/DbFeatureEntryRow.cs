using System;

namespace Cchbc.Features.Db.Objects
{
	public sealed class DbFeatureEntryRow
	{
		public readonly long Id;
		public readonly decimal TimeSpent;
		public readonly string Details;
		public readonly DateTime CreatedAt;
		public readonly long FeatureId;

		public DbFeatureEntryRow(long id, decimal timeSpent, string details, DateTime createdAt, long featureId)
		{
			this.Id = id;
			this.TimeSpent = timeSpent;
			this.Details = details;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}