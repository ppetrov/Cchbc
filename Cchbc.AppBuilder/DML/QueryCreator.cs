using System;
using System.Text;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder.DML
{
	public static class QueryCreator
	{
		public static StringBuilder GetSelect(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var buffer = new StringBuilder(256);

			buffer.Append(@"SELECT ");

			var columns = table.Columns;
			for (var i = 0; i < columns.Length; i++)
			{
				var column = columns[i];
				if (i > 0)
				{
					buffer.Append(@", ");
				}
				buffer.Append(column.Name);
			}

			buffer.Append(@" FROM ");
			buffer.Append(table.Name);

			return buffer;
		}
	}
}