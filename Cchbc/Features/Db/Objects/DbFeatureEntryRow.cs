using System;

namespace Cchbc.Features.Db.Objects
{
	public sealed class DbFeatureEntryRow
	{
		public readonly long Id;
		public readonly decimal TimeSpent;
		public readonly string Details;
		public readonly DateTime CreatedAt;
		public long FeatureId;

		public DbFeatureEntryRow(long id, decimal timeSpent, string details, DateTime createdAt, long featureId)
		{
			Id = id;
			TimeSpent = timeSpent;
			Details = details;
			CreatedAt = createdAt;
			FeatureId = featureId;
		}
	}
}