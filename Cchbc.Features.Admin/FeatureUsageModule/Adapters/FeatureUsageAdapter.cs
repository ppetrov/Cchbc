using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureUsageModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.FeatureUsageModule.Adapters
{
	public sealed class FeatureUsageAdapter
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

		public FeatureUsage[] GetBy(ITransactionContext context, TimePeriod timePeriod, Dictionary<long, DbContextRow> contexts, Dictionary<long, DbFeatureRow> features)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (features == null) throw new ArgumentNullException(nameof(features));

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
						@FROMDATE < E.CREATEDAT AND E.CREATEDAT < @TODATE
			GROUP BY
						C.ID,
						F.ID
			ORDER BY
						COUNT(E.ID) DESC", FeatureUsageRowCreator, sqlParams);

			var rows = context.Execute(query);

			var featureUsages = new FeatureUsage[rows.Count];

			for (var i = 0; i < rows.Count; i++)
			{
				var row = rows[i];
				featureUsages[i] = new FeatureUsage(contexts[row.ContextId].Name, features[row.FeatureId].Name, row.Count);
			}

			return featureUsages;
		}

		private static FeatureUsageRow FeatureUsageRowCreator(IFieldDataReader r)
		{
			return new FeatureUsageRow(r.GetInt64(0), r.GetInt64(1), r.GetInt32(2));
		}
	}
}