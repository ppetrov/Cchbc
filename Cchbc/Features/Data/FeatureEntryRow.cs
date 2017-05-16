using System;

namespace Cchbc.Features.Data
{
	public sealed class FeatureEntryRow
	{
		public readonly long FeatureId;
		public readonly string Details;
		public readonly DateTime CreatedAt;
		public readonly double TimeSpent;

		public FeatureEntryRow(long featureId, string details, DateTime createdAt, double timeSpent)
		{
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.FeatureId = featureId;
			this.Details = details;
			this.CreatedAt = createdAt;
			this.TimeSpent = timeSpent;
		}
	}
}