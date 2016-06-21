using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureExceptionEntryRow
	{
		public readonly long Id;
		public readonly DateTime CreatedAt;
		public readonly int FeatureId;

		public DbFeatureExceptionEntryRow(long id, DateTime createdAt, int featureId)
		{
			this.Id = id;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}