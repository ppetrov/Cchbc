using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureEntryRow
	{
		public readonly long Id;
		public readonly string Details;
		public readonly DateTime CreatedAt;
		public readonly int FeatureId;

		public DbFeatureEntryRow(long id, string details, DateTime createdAt, int featureId)
		{
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Id = id;
			this.Details = details;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}