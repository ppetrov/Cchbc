using System;
using System.Collections.Generic;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder
{
	public sealed class DbProject
	{
		public DbSchema Schema { get; }

		private Dictionary<string, DbTable> InverseTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> ModifiableTables { get; } = new Dictionary<string, DbTable>();

		public DbProject(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException(nameof(schema));

			this.Schema = schema;
		}

		public void MarkModifiable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.ModifiableTables.Add(table.Name, table);
		}

		public bool IsModifiable(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			return this.ModifiableTables.ContainsKey(table.Name);
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
				entities[i] = CreateEntity(tables[i]);
			}

			return entities;
		}

		public string CreateEntityClass(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			return EntityClass.Generate(entity, !this.ModifiableTables.ContainsKey(entity.Table.Name));
		}

		public string CreateEntityAdapter(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (!this.IsModifiable(entity.Table)) return EntityAdapter.GenerateReadOnly(entity, GetDictionaryProperties(entity));

			// We need Get method for this entity
			var generateGet = true;
			var dictionaryProperties = GetDictionaryProperties(entity);

			var inverseTable = entity.InverseTable;
			if (inverseTable != null)
			{
				// We need Get method for this entity & the inverse table 
				foreach (var dictionaryProperty in GetDictionaryProperties(this.CreateEntity(inverseTable)))
				{
					dictionaryProperties.Add(dictionaryProperty.Key, dictionaryProperty.Value);
				}
			}
			else
			{
				// We don't need a Get method. It will be generated on Inverse table side
				var hasColumnToInverseTable = this.HasColumnToInverseTable(entity);
				if (hasColumnToInverseTable)
				{
					generateGet = false;
				}
			}

			return EntityGenerator.ModifiableAdapter(entity, dictionaryProperties, generateGet);
		}

		private bool HasColumnToInverseTable(Entity entity)
		{
			var entityTable = entity.Table;

			foreach (var column in entityTable.Columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					// Check if the current table isn't an inveverse table for other entity
					DbTable inverseTable;
					this.InverseTables.TryGetValue(foreignKey.Table.Name, out inverseTable);

					if (inverseTable == entityTable)
					{
						return true;
					}
				}
			}

			return false;
		}

		private Entity CreateEntity(DbTable table)
		{
			DbTable inverseTable;
			this.InverseTables.TryGetValue(table.Name, out inverseTable);

			return new Entity(DbTable2ClrClassConverter.Convert(table, inverseTable), table, inverseTable);
		}

		private static Dictionary<ClrType, ClrProperty> GetDictionaryProperties(Entity entity)
		{
			var dictionaryProperties = new Dictionary<ClrType, ClrProperty>();

			foreach (var property in entity.Class.Properties)
			{
				var type = property.Type;
				if (type.IsUserType)
				{
					var foreignKeyTable = FindForeignKeyTable(entity, type.Name);
					if (foreignKeyTable != null)
					{
						dictionaryProperties.Add(type, new ClrProperty(foreignKeyTable.Name, new ClrType(@"Dictionary<long, " + foreignKeyTable.ClassName + @">", true)));
					}
				}
			}

			return dictionaryProperties;
		}

		private static DbTable FindForeignKeyTable(Entity entity, string className)
		{
			foreach (var column in entity.Table.Columns)
			{
				var foreignKey = column.DbForeignKey;
				if (foreignKey != null)
				{
					var foreignKeyTable = foreignKey.Table;
					var fkTableClassName = foreignKeyTable.ClassName;
					if (fkTableClassName == className)
					{
						return foreignKeyTable;
					}
				}
			}

			return null;
		}
	}
}