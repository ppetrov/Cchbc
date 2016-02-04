using System;

namespace Cchbc.AppBuilder.DDL
{
	public sealed class DbTable
	{
		public string Name { get; } = string.Empty;
		public DbColumn[] Columns { get; }
		public string ClassName { get; } = string.Empty;

		public DbTable()
		{
		}

		public DbTable(string name, DbColumn[] columns, string className = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (columns == null) throw new ArgumentNullException(nameof(columns));
			if (columns.Length == 0) throw new ArgumentOutOfRangeException(nameof(columns));

			this.Name = name;
			this.Columns = columns;
			this.ClassName = className ?? name.Substring(0, name.Length - 1);
		}

		public static DbTable Create(string name, DbColumn[] columns, string className = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (columns == null) throw new ArgumentNullException(nameof(columns));
			if (columns.Length == 0) throw new ArgumentOutOfRangeException(nameof(columns));

			var withPromaryKeyColumns = new DbColumn[columns.Length + 1];

			// Add primary key
			withPromaryKeyColumns[0] = DbColumn.PrimaryKey();

			// Add other columns
			Array.Copy(columns, 0, withPromaryKeyColumns, 1, columns.Length);

			return new DbTable(name, withPromaryKeyColumns, className);
		}
	}
}