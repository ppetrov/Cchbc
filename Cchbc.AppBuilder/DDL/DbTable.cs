using System;
using System.Collections.Generic;

namespace Cchbc.AppBuilder.DDL
{
	public sealed class DbTable
	{
		public string Name { get; }
		public List<DbColumn> Columns { get; }
		public string ClassName { get; }

		public DbTable(string name, List<DbColumn> columns, string className = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (columns == null) throw new ArgumentNullException(nameof(columns));
			if (columns.Count == 0) throw new ArgumentOutOfRangeException(nameof(columns));

			this.Name = name;
			this.Columns = new List<DbColumn>(columns);
			this.ClassName = className ?? name.Substring(0, name.Length - 1);
		}

		public static DbTable Create(string name, DbColumn[] columns, string className = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (columns == null) throw new ArgumentNullException(nameof(columns));
			if (columns.Length == 0) throw new ArgumentOutOfRangeException(nameof(columns));

			var hasPrimaryKey = false;
			foreach (var column in columns)
			{
				if (column.IsPrimaryKey)
				{
					hasPrimaryKey = true;
					break;
				}
			}

			var tableColumns = new List<DbColumn>(columns.Length + 1);
			if (!hasPrimaryKey)
			{
				tableColumns.Add(DbColumn.PrimaryKey());
			}
			tableColumns.AddRange(columns);

			return new DbTable(name, tableColumns, className);
		}
	}
}