namespace Cchbc.Features.Data
{
	public sealed class DbFeatureEntryStepRow
	{
		public readonly double TimeSpent;
		public readonly long FeatureEntryId;
		public readonly long FeatureStepId;

		public DbFeatureEntryStepRow(double timeSpent, long featureEntryId, long featureStepId)
		{
			this.TimeSpent = timeSpent;
			this.FeatureEntryId = featureEntryId;
			this.FeatureStepId = featureStepId;
		}
	}
}