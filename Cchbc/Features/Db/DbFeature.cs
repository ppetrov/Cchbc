using System;
using Cchbc.Objects;

namespace Cchbc.Features.Db
{
	public sealed class DbFeature : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }
		public long ContextId { get; }

		public DbFeature(long id, string name, long contextId)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}
}