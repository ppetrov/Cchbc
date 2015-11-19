using System;
using System.Collections.Generic;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder
{
	public sealed class DbProject
	{
		public DbSchema Schema { get; }

		private Dictionary<string, DbTable> InverseTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> ModifiableTables { get; } = new Dictionary<string, DbTable>();
		private NameProvider NameProvider { get; } = new NameProvider();

		public DbProject(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException(nameof(schema));

			this.Schema = schema;
		}

		public void DefineModifiable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.ModifiableTables.Add(table.Name, table);
		}

		public bool IsModifiable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			return this.ModifiableTables.ContainsKey(table.Name);
		}

		public bool IsReadOnly(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			return !this.IsModifiable(table);
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

		public Entity[] CreateEntities()
		{
			var tables = this.Schema.Tables;
			var entities = new Entity[tables.Length];

			for (var i = 0; i < tables.Length; i++)
			{
				var table = tables[i];

				DbTable inverseTable;
				this.InverseTables.TryGetValue(table.Name, out inverseTable);
				var clrClass = DbTable2ClrClassConverter.Convert(table, inverseTable);
				entities[i] = new Entity(clrClass, table, inverseTable);
			}

			return entities;
		}

		public string CreateEntityClass(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			return EntityGenerator.CreateEntityClass(entity, !this.ModifiableTables.ContainsKey(entity.Table.Name));
		}

		public string CreateEntityAdapter(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			// TODO : !!!
			// Remove all dictionaries from inversed table - Activities
			// Copy all dictionaries from inverse table Activities to Visit
			// Don't generate Get in the inversed table - Activities

			//DbTable inverseTable;
			//this.InverseTables.TryGetValue("", out inverseTable);

			var includeDictionaries = false;
			return EntityGenerator.CreateEntityAdapter(entity, this.NameProvider, !this.ModifiableTables.ContainsKey(entity.Table.Name), includeDictionaries);
		}
	}
}