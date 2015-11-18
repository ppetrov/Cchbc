using System;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder
{
	public sealed class Entity
	{
		public ClrClass Class { get; }
		public DbTable Table { get; }
		public DbTable InverseTable { get; }

		public Entity(ClrClass @class, DbTable table, DbTable inverseTable = null)
		{
			if (@class == null) throw new ArgumentNullException(nameof(@class));
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.Class = @class;
			this.Table = table;
			this.InverseTable = inverseTable;
		}
	}
}