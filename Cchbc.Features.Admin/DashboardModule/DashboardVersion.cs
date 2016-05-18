using System;
using Cchbc.Features.Admin.Objects;

namespace Cchbc.Features.Admin.DashboardModule
{
	public sealed class DashboardVersion
	{
		public DbFeatureVersionRow Version { get; }
		public int Users { get; }
		public int Exceptions { get; }

		public DashboardVersion(DbFeatureVersionRow version, int users, int exceptions)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Version = version;
			this.Users = users;
			this.Exceptions = exceptions;
		}
	}
}