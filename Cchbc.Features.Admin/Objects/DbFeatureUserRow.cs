using System;

namespace Cchbc.Features.Admin.Objects
{
	public sealed class DbFeatureUserRow
	{
		public long Id { get; }
		public string Name { get; }
		public long VersionId { get; }
		public DateTime ReplicatedAt { get; }

		public DbFeatureUserRow(long id, string name, long versionId, DateTime replicatedAt)
		{
			this.Id = id;
			this.Name = name;
			this.VersionId = versionId;
			this.ReplicatedAt = replicatedAt;
		}
	}
}