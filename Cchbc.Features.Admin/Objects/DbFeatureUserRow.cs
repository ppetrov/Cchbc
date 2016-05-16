using System;

namespace Cchbc.Features.Admin.Objects
{
	public sealed class DbFeatureUserRow
	{
		public long Id { get; }
		public string Name { get; }		
		public DateTime ReplicatedAt { get; }
		public long VersionId { get; }

		public DbFeatureUserRow(long id, string name, DateTime replicatedAt, long versionId)
		{
			this.Id = id;
			this.Name = name;
			this.VersionId = versionId;
			this.ReplicatedAt = replicatedAt;
		}
	}
}