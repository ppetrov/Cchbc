namespace Cchbc.Features.Data
{
	public sealed class DbFeatureEntryStepRow
	{
		public readonly double TimeSpent;
		public readonly int Level;
		public readonly long FeatureEntryId;
		public readonly int FeatureStepId;

		public DbFeatureEntryStepRow(double timeSpent, int level, long featureEntryId, int featureStepId)
		{
			TimeSpent = timeSpent;
			Level = level;
			FeatureEntryId = featureEntryId;
			FeatureStepId = featureStepId;
		}
	}
}