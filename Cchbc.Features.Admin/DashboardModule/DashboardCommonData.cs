using System;
using System.Collections.Generic;
using Cchbc.Features.Admin.Objects;
using Cchbc.Features.Db.Objects;

namespace Cchbc.Features.Admin.DashboardModule
{
	public sealed class DashboardCommonData
	{
		public Dictionary<long, DbFeatureUserRow> Users { get; }
		public Dictionary<long, DbFeatureVersionRow> Versions { get; }
		public Dictionary<long, DbFeatureContextRow> Contexts { get; }
		public Dictionary<long, DbFeatureRow> Features { get; }

		public DashboardCommonData(Dictionary<long, DbFeatureUserRow> users, Dictionary<long, DbFeatureVersionRow> versions, Dictionary<long, DbFeatureContextRow> contexts, Dictionary<long, DbFeatureRow> features)
		{
			if (users == null) throw new ArgumentNullException(nameof(users));
			if (versions == null) throw new ArgumentNullException(nameof(versions));
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (features == null) throw new ArgumentNullException(nameof(features));

			this.Users = users;
			this.Versions = versions;
			this.Contexts = contexts;
			this.Features = features;
		}
	}
}