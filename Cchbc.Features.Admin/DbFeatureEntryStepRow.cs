namespace Cchbc.Features.Admin
{
    public sealed class DbFeatureEntryStepRow
    {
        public readonly decimal TimeSpent;
        public readonly string Details;
        public readonly long FeatureEntryId;
        public readonly long FeatureStepId;

        public DbFeatureEntryStepRow(decimal timeSpent, string details, long featureEntryId, long featureStepId)
        {
            this.TimeSpent = timeSpent;
            this.Details = details;
            this.FeatureEntryId = featureEntryId;
            this.FeatureStepId = featureStepId;
        }
    }
}