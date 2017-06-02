using System;

namespace Atos.Features.Data
{
	public sealed class FeatureRow
	{
		public readonly long Id;
		public readonly string Name;
		public readonly long ContextId;

		public FeatureRow(long id, string name, long contextId)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}
}