using System;

namespace Atos.AppBuilder.DDL
{
	public sealed class DbForeignKey
	{
		public DbTable Table { get; }

		public DbForeignKey(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.Table = table;
		}
	}
}