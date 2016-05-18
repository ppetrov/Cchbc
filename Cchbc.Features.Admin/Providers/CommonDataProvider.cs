using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Admin.Replication;
using Cchbc.Features.Db.Adapters;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.Providers
{
	public sealed class CommonDataProvider
	{
		public Dictionary<long, DbFeatureContextRow> Contexts { get; } = new Dictionary<long, DbFeatureContextRow>();
		public Dictionary<long, DbFeatureRow> Features { get; } = new Dictionary<long, DbFeatureRow>();
		public Dictionary<long, DbFeatureVersionRow> Versions { get; } = new Dictionary<long, DbFeatureVersionRow>();
		public Dictionary<long, DbFeatureUserRow> Users { get; } = new Dictionary<long, DbFeatureUserRow>();
		public Dictionary<string, List<Setting>> Settings { get; } = new Dictionary<string, List<Setting>>();

		public async Task LoadAsync(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			foreach (var row in await DbFeatureAdapter.GetContextsAsync(context))
			{
				this.Contexts.Add(row.Id, new DbFeatureContextRow(row.Id, NamingConventions.ApplyNaming(row.Name)));
			}
			foreach (var row in await DbFeatureAdapter.GetFeaturesAsync(context))
			{
				this.Features.Add(row.Id, new DbFeatureRow(row.Id, NamingConventions.ApplyNaming(row.Name), row.ContextId));
			}
			foreach (var row in await DbFeatureServerAdapter.GetVersionsAsync(context))
			{
				this.Versions.Add(row.Id, row);
			}
			foreach (var row in await DbFeatureServerAdapter.GetUsersAsync(context))
			{
				this.Users.Add(row.Id, row);
			}
		}
	}
}