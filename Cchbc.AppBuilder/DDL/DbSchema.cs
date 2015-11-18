using System;

namespace Cchbc.AppBuilder.DDL
{
	public sealed class DbSchema
	{
		public string Name { get; }
		public DbTable[] Tables { get; }

		public DbSchema(string name, DbTable[] tables)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (tables == null) throw new ArgumentNullException(nameof(tables));
			if (tables.Length == 0) throw new ArgumentOutOfRangeException(nameof(tables));

			this.Name = name;
			this.Tables = tables;
		}
	}
}