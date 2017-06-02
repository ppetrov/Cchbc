using System;

namespace Atos.Features.DashboardModule
{
	public sealed class DashboardUser
	{
		public long Id { get; }
		public string Name { get; }
		public DateTime ReplicatedAt { get; }
		public string Version { get; }

		public DashboardUser(long id, string name, DateTime replicatedAt, string version)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Id = id;
			this.Name = name;
			this.ReplicatedAt = replicatedAt;
			this.Version = version;
		}
	}
}