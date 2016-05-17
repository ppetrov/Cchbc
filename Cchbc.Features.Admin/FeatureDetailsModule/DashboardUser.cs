using System;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class DashboardUser
	{
		public long Id { get; }
		public string Name { get; }
		public string Version { get; }
		public DateTime ReplicatedAt { get; }

		public DashboardUser(long id, string name, string version, DateTime replicatedAt)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Id = id;
			this.Name = name;
			this.Version = version;
			this.ReplicatedAt = replicatedAt;
		}
	}
}