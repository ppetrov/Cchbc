using System;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureUserModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureUserModule.Adapters
{
	public sealed class UserOverviewAdapter
	{
		private sealed class UserFeatureCountRow
		{
			public readonly long UserId;
			public readonly int Count;

			public UserFeatureCountRow(long userId, int count)
			{
				this.UserId = userId;
				this.Count = count;
			}
		}

		public UserFeatureCount[] GetFeaturesBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			var query = @"
			SELECT E.USER_ID,
					COUNT(*)
			FROM FEATURE_ENTRIES E
			WHERE @FROMDATE <= E.CREATED_AT
				AND E.CREATED_AT < @TODATE
			GROUP BY E.USER_ID";

			return GetBy(provider, context, query, timePeriod);
		}

		public UserFeatureCount[] GetExceptionsBy(CommonDataProvider provider, ITransactionContext context, TimePeriod timePeriod)
		{
			if (provider == null) throw new ArgumentNullException(nameof(provider));
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (timePeriod == null) throw new ArgumentNullException(nameof(timePeriod));

			var query = @"
			SELECT E.USER_ID,
					COUNT(*)
			FROM FEATURE_EXCEPTIONS E
			WHERE @FROMDATE <= E.CREATED_AT
				AND E.CREATED_AT < @TODATE
			GROUP BY E.USER_ID";

			return GetBy(provider, context, query, timePeriod);
		}

		private UserFeatureCount[] GetBy(CommonDataProvider provider, ITransactionContext context, string query, TimePeriod timePeriod)
		{
			var users = provider.Users;

			var sqlParams = new[]
			{
				new QueryParameter(@"@FROMDATE", timePeriod.FromDate),
				new QueryParameter(@"@TODATE", timePeriod.ToDate),
			};

			var rows = context.Execute(new Query<UserFeatureCountRow>(query, this.UserFeatureCountRowCreator, sqlParams));

			var userFeatureCounts = new UserFeatureCount[rows.Count];

			for (var i = 0; i < rows.Count; i++)
			{
				var row = rows[i];
				userFeatureCounts[i] = new UserFeatureCount(users[row.UserId].Name, row.Count);
			}

			return userFeatureCounts;
		}

		private UserFeatureCountRow UserFeatureCountRowCreator(IFieldDataReader r)
		{
			return new UserFeatureCountRow(r.GetInt64(0), r.GetInt32(1));
		}
	}
}
