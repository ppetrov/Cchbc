using System;

namespace Cchbc.Features.Admin.Objects
{
	public sealed class DbFeatureVersionRow
	{
		public readonly long Id;
		public readonly string Name;

		public DbFeatureVersionRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}