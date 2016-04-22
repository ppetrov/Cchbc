using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.Adapters
{
	public static class CommonAdapter
	{
		private static Dictionary<long, DbFeatureRow> Features { get; set; }
		private static Dictionary<long, DbContextRow> Contexts { get; set; }

		public static Dictionary<long, DbContextRow> GetContexts(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			if (Contexts == null)
			{
				Contexts = new Dictionary<long, DbContextRow>();

				foreach (var row in DbFeatureAdapter.GetContexts(context))
				{
					Contexts.Add(row.Id, row);
				}
			}

			return Contexts;
		}

		public static Dictionary<long, DbFeatureRow> GetFeatures(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			if (Features == null)
			{
				Features = new Dictionary<long, DbFeatureRow>();

				foreach (var row in DbFeatureAdapter.GetFeatures(context))
				{
					Features.Add(row.Id, row);
				}
			}

			return Features;
		}

		public static Dictionary<long, string> GetUserNames(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var rows = context.Execute(new Query<DbFeatureUserRow>(@"SELECT ID, NAME FROM FEATURE_USERS", DbFeatureUserRowCreator));

			var userNames = new Dictionary<long, string>(rows.Count);

			foreach (var row in rows)
			{
				userNames.Add(row.Id, row.Name);
			}

			return userNames;
		}

		private static DbFeatureUserRow DbFeatureUserRowCreator(IFieldDataReader r)
		{
			return new DbFeatureUserRow(r.GetInt64(0), r.GetString(1));
		}

		public static void Clear()
		{
			Contexts = null;
			Features = null;
		}
	}
}