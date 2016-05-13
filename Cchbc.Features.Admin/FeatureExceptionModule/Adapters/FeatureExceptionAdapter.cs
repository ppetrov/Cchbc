using System;
using Cchbc.Data;
using Cchbc.Features.Admin.FeatureExceptionModule.Objects;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Providers;

namespace Cchbc.Features.Admin.FeatureExceptionModule.Adapters
{
	public sealed class FeatureExceptionAdapter
	{
		private sealed class DbServerExceptionRow
		{
			public readonly string Message;
			public readonly string StackTrace;
			public readonly DateTime CreatedAt;
			public readonly long FeatureId;
			public readonly long UserId;

			public DbServerExceptionRow(string message, string stackTrace, DateTime createdAt, long featureId, long userId)
			{
				if (message == null) throw new ArgumentNullException(nameof(message));
				if (stackTrace == null) throw new ArgumentNullException(nameof(stackTrace));

				this.Message = message;
				this.StackTrace = stackTrace;
				this.CreatedAt = createdAt;
				this.FeatureId = featureId;
				this.UserId = userId;
			}
		}

		public FeatureException[] GetBy(CommonDataProvider provider, ITransactionContext context, RangeTimePeriod rangeTimePeriod)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (rangeTimePeriod == null) throw new ArgumentNullException(nameof(rangeTimePeriod));

			var sqlParams = new[]
			{
				new QueryParameter(@"@FROMDATE", rangeTimePeriod.FromDate),
				new QueryParameter(@"@TODATE", rangeTimePeriod.ToDate),
			};

			var query = new Query<DbServerExceptionRow>(@"
			SELECT MESSAGE,
					STACKTRACE,
					CREATED_AT,
					FEATURE_ID,
					USER_ID
			FROM FEATURE_EXCEPTIONS
			WHERE @FROMDATE <= CREATED_AT
				AND CREATED_AT < @TODATE
			ORDER BY CREATED_AT DESC", this.DbServerExceptionRowCreator, sqlParams);

			var rows = context.Execute(query);

			var exceptions = new FeatureException[rows.Count];

		    var contexts = provider.Contexts;
		    var features = provider.Features;
		    var users = provider.Users;

			for (var i = 0; i < rows.Count; i++)
			{
				var row = rows[i];
				var feature = features[row.FeatureId];
				var contextName = contexts[feature.ContextId].Name;
				var userName = users[row.UserId].Name;

				exceptions[i] = new FeatureException(contextName, feature.Name, userName, row.Message, row.StackTrace, row.CreatedAt);
			}

			return exceptions;
		}

		private DbServerExceptionRow DbServerExceptionRowCreator(IFieldDataReader r)
		{
			return new DbServerExceptionRow(r.GetString(0), r.GetString(1), r.GetDateTime(2), r.GetInt64(3), r.GetInt64(4));
		}
	}
}