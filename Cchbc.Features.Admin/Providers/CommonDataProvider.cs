using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.Providers
{
	public sealed class CommonDataProvider
	{
		public Dictionary<long, DbContextRow> Contexts { get; } = new Dictionary<long, DbContextRow>();
		public Dictionary<long, DbFeatureRow> Features { get; } = new Dictionary<long, DbFeatureRow>();
		public Dictionary<long, DbFeatureUserRow> Users { get; } = new Dictionary<long, DbFeatureUserRow>();

		public void Load(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Contexts.Clear();
			this.Features.Clear();
			this.Users.Clear();

			foreach (var row in DbFeatureAdapter.GetContexts(context))
			{
				this.Contexts.Add(row.Id, row);
			}

			foreach (var row in DbFeatureAdapter.GetFeatures(context))
			{
				this.Features.Add(row.Id, row);
			}

			foreach (var row in context.Execute(new Query<DbFeatureUserRow>(@"SELECT ID, NAME FROM FEATURE_USERS", DbFeatureUserRowCreator)))
			{
				this.Users.Add(row.Id, row);
			}
		}

		private static DbFeatureUserRow DbFeatureUserRowCreator(IFieldDataReader r)
		{
			return new DbFeatureUserRow(r.GetInt64(0), r.GetString(1));
		}
	}
}