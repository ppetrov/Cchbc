using System;

namespace Cchbc.Features.Db.Objects
{
	public sealed class DbContextRow
	{
		public readonly long Id;
		public readonly string Name;

		public DbContextRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}