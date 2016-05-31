using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureEntryStepRow
	{
		public readonly double TimeSpent;
		public readonly string Details;
		public readonly long FeatureEntryId;
		public readonly long FeatureStepId;

		public DbFeatureEntryStepRow(double timeSpent, string details, long featureEntryId, long featureStepId)
		{
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.TimeSpent = timeSpent;
			this.Details = details;
			this.FeatureEntryId = featureEntryId;
			this.FeatureStepId = featureStepId;
		}
	}
}