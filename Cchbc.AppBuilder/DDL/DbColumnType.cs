using System;

namespace Cchbc.AppBuilder.DDL
{
	public sealed class DbColumnType
	{
		public readonly static DbColumnType Integer = new DbColumnType(@"INTEGER");
		public readonly static DbColumnType String = new DbColumnType(@"TEXT");
		public readonly static DbColumnType Decimal = new DbColumnType(@"DECIMAL");
		public readonly static DbColumnType DateTime = new DbColumnType(@"DATETIME");
		public readonly static DbColumnType Bytes = new DbColumnType(@"BLOB");

		public string Name { get; } = string.Empty;

		public DbColumnType()
		{
		}

		public DbColumnType(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}
	}
}