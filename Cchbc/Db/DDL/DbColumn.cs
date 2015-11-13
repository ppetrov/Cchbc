﻿using System;

namespace Cchbc.Db.DDL
{
	public sealed class DbColumn
	{
		public string Name { get; }
		public DbColumnType Type { get; }
		public bool IsNullable { get; }
		public bool IsPrimaryKey { get; }
		public DbForeignKey DbForeignKey { get; }

		public DbColumn(string name, DbColumnType type)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (type == null) throw new ArgumentNullException(nameof(type));

			this.Name = name;
			this.Type = type;
		}

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
			return new DbColumn(NameProvider.IdName, DbColumnType.Integer, false, true);
		}

		public static DbColumn ForeignKey(DbTable table, bool isNullable = false)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var name = table.ClassName + NameProvider.IdName;
			return new DbColumn(name, DbColumnType.Integer, isNullable, false, new DbForeignKey(table.Name, name));
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