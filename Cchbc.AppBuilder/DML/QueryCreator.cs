using System;
using System.Text;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder.DML
{
	public static class QueryCreator
	{
		public static void AppendSelect(StringBuilder buffer, DbTable table)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (table == null) throw new ArgumentNullException(nameof(table));

			buffer.Append(@"SELECT");
			buffer.Append(' ');

			var columns = table.Columns;
			for (var i = 0; i < columns.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				buffer.Append(columns[i].Name);
			}

			buffer.Append(' ');
			buffer.Append(@"FROM");
			buffer.Append(' ');
			buffer.Append(table.Name);
		}

		public static StringBuilder GetSelect(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var buffer = new StringBuilder(256);

			AppendSelect(buffer, table);

			return buffer;
		}
	}
}