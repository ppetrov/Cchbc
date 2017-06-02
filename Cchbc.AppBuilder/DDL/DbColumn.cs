using System;

namespace Atos.AppBuilder.DDL
{
	public sealed class DbColumn
	{
		public static readonly string IdName = @"Id";

		public string Name { get; set; }
		public DbColumnType Type { get; set; }
		public bool IsNullable { get; set; }
		public bool IsPrimaryKey { get; set; }
		public DbForeignKey DbForeignKey { get; set; }

		public DbColumn(string name, DbColumnType type, bool isNullable = false, bool isPrimaryKey = false, DbForeignKey dbForeignKey = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (type == null) throw new ArgumentNullException(nameof(type));

			this.Name = name;
			this.Type = type;
			this.IsNullable = isNullable;
			this.IsPrimaryKey = isPrimaryKey;
			this.DbForeignKey = dbForeignKey;
		}

		public static DbColumn PrimaryKey()
		{
			return new DbColumn(IdName, DbColumnType.Integer, false, true);
		}

		public static DbColumn ForeignKey(DbTable table, bool isNullable = false)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			return new DbColumn(table.ClassName + IdName, DbColumnType.Integer, isNullable, false, new DbForeignKey(table));
		}

		public static DbColumn Integer(string name, bool isNullable = false)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new DbColumn(name, DbColumnType.Integer, isNullable);
		}

		public static DbColumn String(string name, bool isNullable = false)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new DbColumn(name, DbColumnType.String, isNullable);
		}

		public static DbColumn DateTime(string name, bool isNullable = false)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new DbColumn(name, DbColumnType.DateTime, isNullable);
		}

		public static DbColumn Decimal(string name, bool isNullable = false)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new DbColumn(name, DbColumnType.Decimal, isNullable);
		}
	}
}