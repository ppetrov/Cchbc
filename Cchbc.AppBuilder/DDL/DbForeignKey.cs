using System;

namespace Cchbc.AppBuilder.DDL
{
	[Serializable]
	public sealed class DbForeignKey
	{
		public DbTable Table { get; }

		public DbForeignKey()
		{
		}

		public DbForeignKey(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.Table = table;
		}
	}
}