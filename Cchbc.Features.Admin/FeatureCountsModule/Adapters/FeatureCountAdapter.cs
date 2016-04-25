using System;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureCountsModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureCountsModule.Adapters
{
    public sealed class FeatureCountAdapter
    {
        private sealed class FeatureUsageRow
        {
            public readonly long ContextId;
            public readonly long FeatureId;
            public readonly int Count;

            public FeatureUsageRow(long contextId, long featureId, int count)
            {
                this.ContextId = contextId;
                this.FeatureId = featureId;
                this.Count = count;
            }
        }

        public FeatureCount[] GetBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

            var sqlParams = new[]
            {
                new QueryParameter(@"@FROMDATE", timePeriod.FromDate),
                new QueryParameter(@"@TODATE", timePeriod.ToDate),
            };

            var query = new Query<FeatureUsageRow>(@"
			SELECT
						C.ID,
						F.ID,
						COUNT(E.ID)
			FROM
						FEATURE_CONTEXTS    C
						INNER JOIN FEATURES F
						ON        C.ID = F.CONTEXT_ID
						INNER JOIN FEATURE_ENTRIES E
						ON        F.ID = E.FEATURE_ID
			WHERE
						@FROMDATE < E.CREATED_AT AND E.CREATED_AT < @TODATE
			GROUP BY
						C.ID,
						F.ID
			ORDER BY
						COUNT(E.ID) DESC", FeatureUsageRowCreator, sqlParams);

            var rows = context.Execute(query);

            var contexts = provider.Contexts;
            var features = provider.Features;
            var featureUsages = new FeatureCount[rows.Count];

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                featureUsages[i] = new FeatureCount(contexts[row.ContextId].Name, features[row.FeatureId].Name, row.Count);
            }

            return featureUsages;
        }

        private static FeatureUsageRow FeatureUsageRowCreator(IFieldDataReader r)
        {
            return new FeatureUsageRow(r.GetInt64(0), r.GetInt64(1), r.GetInt32(2));
        }
    }
}