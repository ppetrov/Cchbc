using System;
using System.Text;

namespace Cchbc.Db.DDL
{
	public static class DbScript
	{
		/// <summary>
		/// Create a CREATE TABLE statement
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		public static string CreateTable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var name = table.Name;
			var columns = table.Columns;
			var buffer = new StringBuilder(32 + name.Length + (64 * columns.Length));

			// Table
			buffer.Append(@"CREATE");
			buffer.Append(' ');
			buffer.Append(@"TABLE");
			buffer.Append(' ');
			buffer.Append('[');
			buffer.Append(name);
			buffer.Append(']');
			buffer.AppendLine();

			buffer.Append('(');
			buffer.AppendLine();

			// Columns
			foreach (var column in columns)
			{
				buffer.Append('\t');
				AppendColumnDefinition(buffer, column);
				buffer.Append(',');
				buffer.AppendLine();
			}
			// Foreign Keys
			foreach (var column in columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					buffer.Append('\t');
					AppendForeignKeyDefinition(buffer, foreignKey);
					buffer.Append(',');
					buffer.AppendLine();
				}
			}

			// Remove the last comma
			buffer.Remove(buffer.Length - (Environment.NewLine.Length + 1), 1);

			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			return buffer.ToString();
		}

		public static string CreateTables(DbTable[] tables)
		{
			if (tables == null) throw new ArgumentNullException(nameof(tables));
			if (tables.Length == 0) throw new ArgumentOutOfRangeException(nameof(tables));
			
			var buffer = new StringBuilder(256 * tables.Length);

			foreach (var table in tables)
			{
				buffer.AppendLine(CreateTable(table));
			}

			return buffer.ToString();
		}

		/// <summary>
		/// Create a DROP TABLE statement
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		public static string DropTable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var name = table.Name;
			var buffer = new StringBuilder(16 + name.Length);

			AppendDropTable(buffer, name);

			return buffer.ToString();
		}

		/// <summary>
		/// Create DROP TABLE script
		/// </summary>
		/// <param name="tables"></param>
		/// <returns></returns>
		public static string DropTables(DbTable[] tables)
		{
			if (tables == null) throw new ArgumentNullException(nameof(tables));
			if (tables.Length == 0) throw new ArgumentOutOfRangeException(nameof(tables));

			var buffer = new StringBuilder(32 * tables.Length);

			foreach (var table in tables)
			{
				AppendDropTable(buffer, table.Name);
				buffer.AppendLine();
			}

			return buffer.ToString();
		}

		private static void AppendDropTable(StringBuilder buffer, string name)
		{
			buffer.Append(@"DROP");
			buffer.Append(' ');
			buffer.Append(@"TABLE");
			buffer.Append(' ');
			buffer.Append('[');
			buffer.Append(name);
			buffer.Append(']');
			buffer.Append(';');
			buffer.AppendLine();
		}

		private static void AppendColumnDefinition(StringBuilder buffer, DbColumn column)
		{
			buffer.Append('[');
			buffer.Append(column.Name);
			buffer.Append(']');

			buffer.Append(' ');
			buffer.Append(column.Type.Name);

			buffer.Append(' ');
			buffer.Append(column.IsNullable ? @"NULL" : @"NOT NULL");

			if (column.IsPrimaryKey)
			{
				buffer.Append(' ');
				buffer.Append(@"PRIMARY KEY AUTOINCREMENT");
			}
		}

		private static void AppendForeignKeyDefinition(StringBuilder buffer, DbForeignKey foreignKey)
		{
			buffer.Append(@"FOREIGN KEY");
			buffer.Append(' ');
			buffer.Append('(');
			buffer.Append('[');
			buffer.Append(foreignKey.Column);
			buffer.Append(']');
			buffer.Append(')');
			buffer.Append(' ');
			buffer.Append(@"REFERENCES");
			buffer.Append(' ');
			buffer.Append('[');
			buffer.Append(foreignKey.Table);
			buffer.Append(']');
			buffer.Append(' ');
			buffer.Append('(');
			buffer.Append('[');
			buffer.Append(NameProvider.IdName);
			buffer.Append(']');
			buffer.Append(')');
		}
	}
}