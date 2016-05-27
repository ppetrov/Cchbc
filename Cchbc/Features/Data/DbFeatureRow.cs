using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureRow
	{
		public readonly long Id;
		public readonly string Name;
		public readonly long ContextId;

		public DbFeatureRow(long id, string name, long contextId)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}
}