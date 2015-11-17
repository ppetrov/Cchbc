using System;
using System.Collections.Generic;
using Cchbc.Db.Clr;
using Cchbc.Db.DDL;

namespace Cchbc.Db
{
	public sealed class Entity
	{
		public ClrClass Class { get; }
		public DbTable Table { get; }
		public DbTable InverseTable { get; }
		public bool IsTableReadOnly { get; }

		public Entity(ClrClass @class, DbTable table, bool isTableReadOnly, DbTable inverseTable = null)
		{
			if (@class == null) throw new ArgumentNullException(nameof(@class));
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.Class = @class;
			this.Table = table;
			this.IsTableReadOnly = isTableReadOnly;
			this.InverseTable = inverseTable;
		}
	}

	public sealed class DbProject
	{
		public DbSchema Schema { get; }
		public string Namespace { get; }

		private Dictionary<string, DbTable> InverseTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> ReadOnlyTables { get; } = new Dictionary<string, DbTable>();

		public DbProject(DbSchema schema, string @namespace)
		{
			if (schema == null) throw new ArgumentNullException(nameof(schema));
			if (@namespace == null) throw new ArgumentNullException(nameof(@namespace));

			this.Schema = schema;
			this.Namespace = @namespace;
		}

		public void AttachInverseTable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var name = table.Name;

			foreach (var t in this.Schema.Tables)
			{
				if (table != t)
				{
					foreach (var column in t.Columns)
					{
						if (column.DbForeignKey != null && column.DbForeignKey.Table == name)
						{
							this.InverseTables.Add(name, t);
							return;
						}
					}
				}
			}

			throw new ArgumentOutOfRangeException(nameof(table));
		}

		public void MarkReadOnly(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.ReadOnlyTables.Add(table.Name, table);
		}

		public Entity[] GenerateEntities()
		{
			var tables = this.Schema.Tables;
			var classes = new Entity[tables.Length];

			for (var i = 0; i < tables.Length; i++)
			{
				var table = tables[i];
				var inverseTable = this.GetInverseTable(table.Name);
				var clrClass = ToClass(table, inverseTable);
				classes[i] = new Entity(clrClass, table, this.ReadOnlyTables.ContainsKey(table.Name), inverseTable);
			}

			return classes;
		}

		public string GenerateClass(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			return ClrCode.Class(entity);
		}

		private DbTable GetInverseTable(string name)
		{
			DbTable table;
			this.InverseTables.TryGetValue(name, out table);
			return table;
		}

		public static ClrClass ToClass(DbTable table, DbTable inverseTable = null)
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

				ClrProperty clrProperty;
				if (column.DbForeignKey == null)
				{
					var clrType = GetClrType(column.Type);
					clrProperty = new ClrProperty(column.Name, clrType, clrType.IsReference);
				}
				else
				{
					var fkColumn = column.DbForeignKey.Column;
					var name = fkColumn.Substring(0, fkColumn.Length - NameProvider.IdName.Length);
					clrProperty = new ClrProperty(name, new ClrType(name, true, true), true);
				}

				properties[i] = clrProperty;
			}

			if (inverseTable != null)
			{
				var name = inverseTable.ClassName;
				properties[properties.Length - 1] = new ClrProperty(inverseTable.Name, new ClrType(@"List<" + name + @">", true, true), true);
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