using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder.DML
{
	public static class QueryBuilder
	{
		public static readonly char ParameterPlaceholder = '@';
		public static readonly char ParameterPrefix = 'p';

		private static readonly DbColumn[] IdColumn = { new DbColumn(DbColumn.IdName, DbColumnType.Integer) };

		public static void AppendSelect(StringBuilder buffer, DbTable table)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (table == null) throw new ArgumentNullException(nameof(table));

			buffer.Append(@"SELECT");
			buffer.Append(' ');
			AppendColumns(buffer, table.Columns, AppendColumnName);
			buffer.Append(' ');
			buffer.Append(@"FROM");
			buffer.Append(' ');
			buffer.Append(table.Name);
		}

		public static void AppendSelectJoin(StringBuilder buffer, DbTable a, DbTable b)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (a == null) throw new ArgumentNullException(nameof(a));
			if (b == null) throw new ArgumentNullException(nameof(b));

			var aPrefix = GetPrefix(a.Name);
			var bPrefix = GetPrefix(b.Name);
			if (aPrefix == bPrefix)
			{
				bPrefix = @"_" + bPrefix;
			}

			buffer.Append(@"SELECT");
			buffer.Append(' ');
			var index = 0;
			foreach (var column in a.Columns)
			{
				if (index++ > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				buffer.Append(aPrefix);
				buffer.Append('.');
				buffer.Append(column.Name);
			}
			buffer.Append(',');
			buffer.Append(' ');
			index = 0;
			foreach (var column in b.Columns)
			{
				if (column.DbForeignKey != null && column.DbForeignKey.Table == a)
				{
					continue;
				}
				if (index++ > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				buffer.Append(bPrefix);
				buffer.Append('.');
				buffer.Append(column.Name);
			}
			buffer.Append(' ');
			buffer.Append(@"FROM");
			buffer.Append(' ');
			buffer.Append(a.Name);
			buffer.Append(' ');
			buffer.Append(aPrefix);
			buffer.Append(' ');
			buffer.Append(@"INNER");
			buffer.Append(' ');
			buffer.Append(@"JOIN");
			buffer.Append(' ');
			buffer.Append(b.Name);
			buffer.Append(' ');
			buffer.Append(bPrefix);
			buffer.Append(' ');
			buffer.Append(@"ON");
			buffer.Append(' ');
			buffer.Append(aPrefix);
			buffer.Append('.');

			// Add Primary Key
			foreach (var column in a.Columns)
			{
				if (column.IsPrimaryKey)
				{
					buffer.Append(column.Name);
					break;
				}
			}
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append(bPrefix);
			buffer.Append('.');

			// Add Foreign key to the PK
			foreach (var column in b.Columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null && foreignKey.Table == a)
				{
					buffer.Append(column.Name);
					break;
				}
			}
		}

		public static void AppendInsert(StringBuilder buffer, DbTable table)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (table == null) throw new ArgumentNullException(nameof(table));

			buffer.Append(@"INSERT");
			buffer.Append(' ');
			buffer.Append(@"INTO");
			buffer.Append(' ');
			buffer.Append(table.Name);
			buffer.Append(' ');
			buffer.Append('(');
			AppendColumns(buffer, table.Columns, AppendColumnName, true);
			buffer.Append(')');
			buffer.Append(' ');
			buffer.Append(@"VALUES");
			buffer.Append(' ');
			buffer.Append('(');
			AppendColumns(buffer, table.Columns, AppendParameterName, true);
			buffer.Append(')');
		}

		public static void AppendUpdate(StringBuilder buffer, DbTable table)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (table == null) throw new ArgumentNullException(nameof(table));

			buffer.Append(@"UPDATE");
			buffer.Append(' ');
			buffer.Append(table.Name);
			buffer.Append(' ');
			buffer.Append(@"SET");
			buffer.Append(' ');
			AppendColumns(buffer, table.Columns, AppendColumnParameterAssignment, true);
			buffer.Append(' ');
			buffer.Append(@"WHERE");
			buffer.Append(' ');
			AppendColumns(buffer, IdColumn, AppendColumnParameterAssignment, true);
		}

		public static void AppendDelete(StringBuilder buffer, DbTable table)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (table == null) throw new ArgumentNullException(nameof(table));

			buffer.Append(@"DELETE");
			buffer.Append(' ');
			buffer.Append(@"FROM");
			buffer.Append(' ');
			buffer.Append(table.Name);
			buffer.Append(' ');
			buffer.Append(@"WHERE");
			buffer.Append(' ');
			AppendColumns(buffer, IdColumn, AppendColumnParameterAssignment, true);
		}

		private static void AppendColumnName(StringBuilder buffer, DbColumn column)
		{
			buffer.Append(column.Name);
		}

		private static void AppendParameterName(StringBuilder buffer, DbColumn column)
		{
			buffer.Append(ParameterPlaceholder);
			buffer.Append(ParameterPrefix);
			AppendColumnName(buffer, column);
		}

		private static void AppendColumnParameterAssignment(StringBuilder buffer, DbColumn column)
		{
			AppendColumnName(buffer, column);
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			AppendParameterName(buffer, column);
		}

		private static void AppendColumns(StringBuilder buffer, IEnumerable<DbColumn> columns, Action<StringBuilder, DbColumn> appender, bool ignorePrimaryKey = false)
		{
			var index = 0;
			foreach (var column in columns)
			{
				if (ignorePrimaryKey && column.IsPrimaryKey)
				{
					continue;
				}
				if (index++ > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				appender(buffer, column);
			}
		}

		private static string GetPrefix(string name)
		{
			var uppers = new List<char>();

			foreach (var symbol in name)
			{
				if (char.IsUpper(symbol))
				{
					uppers.Add(char.ToLowerInvariant(symbol));
				}
			}

			return new string(uppers.ToArray());
		}
	}
}
