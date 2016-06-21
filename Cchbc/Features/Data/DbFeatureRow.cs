using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureRow
	{
		public readonly int Id;
		public readonly string Name;
		public readonly int ContextId;

		public DbFeatureRow(int id, string name, int contextId)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}
}