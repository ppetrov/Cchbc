namespace Cchbc.Features.Db.Objects
{
	public sealed class DbFeatureEntryStepRow
	{
		public readonly decimal TimeSpent;
		public readonly string Details;
		public long FeatureEntryId;
		public long FeatureStepId;

		public DbFeatureEntryStepRow(decimal timeSpent, string details, long featureEntryId, long featureStepId)
		{
			TimeSpent = timeSpent;
			Details = details;
			FeatureEntryId = featureEntryId;
			FeatureStepId = featureStepId;
		}
	}
}