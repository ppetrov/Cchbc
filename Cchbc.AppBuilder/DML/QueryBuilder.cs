using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder.DML
{
	public static class QueryBuilder
	{
		private static readonly DbColumn[] IdColumn = { new DbColumn(NameProvider.IdName, DbColumnType.Integer) };

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
			buffer.Append('@');
			buffer.Append('p');
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
	}
}
