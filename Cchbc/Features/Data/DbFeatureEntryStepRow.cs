namespace Cchbc.Features.Data
{
	public sealed class DbFeatureEntryStepRow
	{
		public readonly double TimeSpent;
		public readonly long FeatureEntryId;
		public readonly int FeatureStepId;

		public DbFeatureEntryStepRow(double timeSpent, long featureEntryId, int featureStepId)
		{
			this.TimeSpent = timeSpent;
			this.FeatureEntryId = featureEntryId;
			this.FeatureStepId = featureStepId;
		}
	}

	public sealed class FeatureEntryStep
	{
		public double TimeSpent;
		public long FeatureEntryId;
		public int FeatureStepId;
	}
}