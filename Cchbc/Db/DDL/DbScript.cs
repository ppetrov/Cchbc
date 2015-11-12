using System;
using System.Collections.Generic;
using System.Text;

namespace Cchbc.Db.DDL
{
	public static class DbScript
	{
		public static string CreateTable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var buffer = new StringBuilder();

			// Table
			buffer.Append(@"CREATE TABLE ");
			buffer.Append('[');
			buffer.Append(table.Name);
			buffer.Append(']');

			buffer.AppendLine();
			buffer.AppendLine(@"(");

			var columns = table.Columns;
			var definitions = new List<string>(columns.Length * 2);

			// Columns
			foreach (var column in columns)
			{
				definitions.Add(GetColumnDefinition(column));
			}
			// Foreign Keys
			foreach (var column in columns)
			{
				definitions.Add(GetForeignKeyDefinition(column));
			}

			// Remove empty definitions
			definitions.RemoveAll(v => v == string.Empty);

			// Add definitions, skip the last one, it will be added without a ',' at the end
			for (var i = 0; i < definitions.Count - 1; i++)
			{
				var definition = definitions[i];

				buffer.Append('\t');
				buffer.Append(definition);
				buffer.AppendLine(@",");
			}

			// Add the last definition without the ',' at the end
			buffer.Append('\t');
			buffer.Append(definitions[definitions.Count - 1]);
			buffer.AppendLine();

			buffer.AppendLine(@");");

			return buffer.ToString();
		}

		public static string DropTable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			return $@"DROP TABLE [{table.Name}]";
		}

		private static string GetColumnDefinition(DbColumn column)
		{
			var buffer = new StringBuilder();

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

			return buffer.ToString();
		}

		private static string GetForeignKeyDefinition(DbColumn column)
		{
			var foreignKey = column.DbForeignKey;
			if (foreignKey == null)
			{
				return string.Empty;
			}
			return $@"FOREIGN KEY ([{foreignKey.Column}]) REFERENCES [{foreignKey.Table}] ([{NameProvider.IdName}])";
		}
	}
}