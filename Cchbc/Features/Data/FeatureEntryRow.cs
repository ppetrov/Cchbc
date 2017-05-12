using System;

namespace Cchbc.Features.Data
{
	public sealed class FeatureEntryRow
	{
		public readonly long FeatureId;
		public readonly string Details;
		public readonly DateTime CreatedAt;

		public FeatureEntryRow(long featureId, DateTime createdAt, string details)
		{
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.FeatureId = featureId;
			this.Details = details;
			this.CreatedAt = createdAt;
		}
	}
}