using System;

namespace Atos.Client.Features.Data
{
	public sealed class FeatureContextRow
	{
		public readonly long Id;
		public readonly string Name;

		public FeatureContextRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}