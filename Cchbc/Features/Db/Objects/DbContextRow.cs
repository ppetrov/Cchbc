using System;

namespace Cchbc.Features.Db.Objects
{
	public sealed class DbContextRow
	{
		public long Id { get; }
		public string Name { get; }

		public DbContextRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}