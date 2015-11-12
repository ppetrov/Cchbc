using System;

namespace Cchbc.Db.DDL
{
	public sealed class DbForeignKey
	{
		public string Table { get; }
		public string Column { get; }

		public DbForeignKey(string table, string column)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));
			if (column == null) throw new ArgumentNullException(nameof(column));

			this.Table = table;
			this.Column = column;
		}
	}
}