using System;
using System.Collections.Generic;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder
{
	public sealed class DbProject
	{
		public DbSchema Schema { get; }

		private Dictionary<string, DbTable> InverseTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> ReadOnlyTables { get; } = new Dictionary<string, DbTable>();

		public DbProject(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException(nameof(schema));

			this.Schema = schema;
		}

		public void AttachInverseTable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			foreach (var t in this.Schema.Tables)
			{
				if (table != t)
				{
					foreach (var column in t.Columns)
					{
						if (column.DbForeignKey != null && column.DbForeignKey.Table == table)
						{
							this.InverseTables.Add(table.Name, t);
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

			var converter = new DbTable2ClrClassConverter();

			for (var i = 0; i < tables.Length; i++)
			{
				var table = tables[i];
				var inverseTable = this.GetInverseTable(table.Name);
				var clrClass = converter.Convert(table, inverseTable);
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
	}
}