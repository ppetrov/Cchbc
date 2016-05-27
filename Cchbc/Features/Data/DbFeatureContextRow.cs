using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureContextRow
	{
		public readonly long Id;
		public readonly string Name;

		public DbFeatureContextRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}