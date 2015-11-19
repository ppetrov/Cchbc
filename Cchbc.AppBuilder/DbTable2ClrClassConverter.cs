﻿using System;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder
{
	public static class DbTable2ClrClassConverter
	{
		public static ClrClass Convert(DbTable table, DbTable inverseTable = null)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			return new ClrClass(table.ClassName, GetProperties(table.Columns, inverseTable));
		}

		private static ClrProperty[] GetProperties(DbColumn[] columns, DbTable inverseTable = null)
		{
			var totalColumns = columns.Length;
			var totalProperties = totalColumns;
			if (inverseTable != null)
			{
				totalProperties++;
			}
			var properties = new ClrProperty[totalProperties];

			for (var i = 0; i < totalColumns; i++)
			{
				var column = columns[i];

				var name = column.Name;
				var clrType = GetClrType(column.Type);

				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					name = foreignKey.Table.ClassName;
					clrType = new ClrType(name, true);
				}

				var clrProperty = new ClrProperty(name, clrType);
				properties[i] = clrProperty;
			}

			if (inverseTable != null)
			{
				var name = inverseTable.Name;
				var className = inverseTable.ClassName;
				properties[properties.Length - 1] = new ClrProperty(name, new ClrType(@"List<" + className + @">", true));
			}

			return properties;
		}

		private static ClrType GetClrType(DbColumnType type)
		{
			if (type == DbColumnType.Integer) return ClrType.Long;
			if (type == DbColumnType.String) return ClrType.String;
			if (type == DbColumnType.Decimal) return ClrType.Decimal;
			if (type == DbColumnType.DateTime) return ClrType.DateTime;
			if (type == DbColumnType.Bytes) return ClrType.Bytes;

			throw new ArgumentOutOfRangeException(nameof(type));
		}
	}
}