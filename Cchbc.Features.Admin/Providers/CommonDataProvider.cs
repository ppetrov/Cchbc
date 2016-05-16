using System;
using System.Collections.Generic;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.Providers
{
	public sealed class CommonDataProvider
	{
		public Dictionary<long, DbContextRow> Contexts { get; } = new Dictionary<long, DbContextRow>();
		public Dictionary<long, DbFeatureRow> Features { get; } = new Dictionary<long, DbFeatureRow>();
		public Dictionary<long, DbFeatureVersionRow> Versions { get; } = new Dictionary<long, DbFeatureVersionRow>();
		public Dictionary<long, DbFeatureUserRow> Users { get; } = new Dictionary<long, DbFeatureUserRow>();
		public Dictionary<string, List<Setting>> Settings { get; } = new Dictionary<string, List<Setting>>();

		public void Load(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Contexts.Clear();
			this.Features.Clear();
			this.Versions.Clear();
			this.Users.Clear();

			foreach (var row in DbFeatureAdapter.GetContexts(context))
			{
				this.Contexts.Add(row.Id, new DbContextRow(row.Id, NamingConventions.ApplyNaming(row.Name)));
			}

			foreach (var row in DbFeatureAdapter.GetFeatures(context))
			{
				this.Features.Add(row.Id, new DbFeatureRow(row.Id, NamingConventions.ApplyNaming(row.Name), row.ContextId));
			}

			foreach (var row in context.Execute(new Query<DbFeatureVersionRow>(@"SELECT ID, NAME FROM FEATURE_VERSIONS", DbFeatureVersionCreator)))
			{
				this.Versions.Add(row.Id, new DbFeatureVersionRow(row.Id, row.Name));
			}

			foreach (var row in context.Execute(new Query<DbFeatureUserRow>(@"SELECT ID, NAME, REPLICATED_AT, VERSION_ID FROM FEATURE_USERS", this.DbFeatureUserRowCreator)))
			{
				this.Users.Add(row.Id, row);
			}
		}

		private DbFeatureUserRow DbFeatureUserRowCreator(IFieldDataReader r)
		{
			return new DbFeatureUserRow(r.GetInt64(0), r.GetString(1), r.GetDateTime(2), r.GetInt64(3));
		}

		private static DbFeatureVersionRow DbFeatureVersionCreator(IFieldDataReader r)
		{
			return new DbFeatureVersionRow(r.GetInt64(0), r.GetString(1));
		}
	}
}