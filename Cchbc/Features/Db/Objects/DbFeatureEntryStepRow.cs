namespace Cchbc.Features.Db.Objects
{
	public sealed class DbFeatureEntryStepRow
	{
		public readonly decimal TimeSpent;
		public readonly string Details;
		public readonly long FeatureEntryId;
		public readonly long FeatureStepId;

		public DbFeatureEntryStepRow(decimal timeSpent, string details, long featureEntryId, long featureStepId)
		{
			TimeSpent = timeSpent;
			Details = details;
			FeatureEntryId = featureEntryId;
			FeatureStepId = featureStepId;
		}
	}
}