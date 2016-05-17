using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

		public async Task LoadAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Contexts.Clear();
			foreach (var row in await DbFeatureAdapter.GetContexts(context))
			{
				this.Contexts.Add(row.Id, new DbContextRow(row.Id, NamingConventions.ApplyNaming(row.Name)));
			}

			this.Features.Clear();
			foreach (var row in await DbFeatureAdapter.GetFeatures(context))
			{
				this.Features.Add(row.Id, new DbFeatureRow(row.Id, NamingConventions.ApplyNaming(row.Name), row.ContextId));
			}

			this.Versions.Clear();
			foreach (var row in context.Execute(new Query<DbFeatureVersionRow>(@"SELECT ID, NAME FROM FEATURE_VERSIONS", DbFeatureVersionCreator)))
			{
				this.Versions.Add(row.Id, new DbFeatureVersionRow(row.Id, row.Name));
			}

			this.Users.Clear();
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