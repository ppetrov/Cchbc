using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureEntryRow
	{
		public readonly string Details;
		public readonly DateTime CreatedAt;
		public readonly int FeatureId;

		public DbFeatureEntryRow(string details, DateTime createdAt, int featureId)
		{
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Details = details;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}