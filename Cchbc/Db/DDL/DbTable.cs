using System;

namespace Cchbc.Db.DDL
{
	public sealed class DbTable
	{
		public string Name { get; }
		public DbColumn[] Columns { get; }
		public string ClassName { get; }

		public DbTable(string name, DbColumn[] columns, string className = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (columns == null) throw new ArgumentNullException(nameof(columns));
			if (columns.Length == 0) throw new ArgumentOutOfRangeException(nameof(columns));

			this.Name = name;
			this.Columns = columns;
			this.ClassName = className ?? this.Name.Substring(0, this.Name.Length - 1);
		}
	}
}