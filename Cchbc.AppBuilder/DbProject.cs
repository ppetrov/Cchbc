using System;
using System.Collections.Generic;
using Atos.AppBuilder.Clr;
using Atos.AppBuilder.DDL;

namespace Atos.AppBuilder
{
	public sealed class DbProject
	{
		public DbSchema Schema { get; }

		private Dictionary<string, DbTable> InverseTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> ModifiableTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> HiddenTables { get; } = new Dictionary<string, DbTable>();

		public DbProject(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException(nameof(schema));

			this.Schema = schema;
		}

		public void MarkHidden(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			this.HiddenTables.Add(table.Name, table);
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

		public bool IsHidden(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			return this.HiddenTables.ContainsKey(table.Name);
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

		public Entity CreateEntity(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			DbTable inverseTable;
			this.InverseTables.TryGetValue(table.Name, out inverseTable);

			return new Entity(DbTableConverter.ConvertToClrClass(table, inverseTable), table, inverseTable);
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

			return EntityClass.Generate(entity, !this.IsModifiable(entity.Table));
		}

		public string CreateEntityAdapter(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (!this.IsModifiable(entity.Table)) return EntityAdapter.GenerateReadOnly(entity, GetDictionaryProperties(entity));

			// We need Get method for this entity
			var entities = new[] { entity };
			var dictionaryProperties = GetDictionaryProperties(entity);

			var inverseTable = entity.InverseTable;
			if (inverseTable != null)
			{
				var className = entity.Class.Name;
				var inverseTableEntity = this.CreateEntity(inverseTable);

				entities = new[] { entity, inverseTableEntity };

				// We need Get method for this entity & the inverse table 
				foreach (var dictionaryProperty in GetDictionaryProperties(inverseTableEntity))
				{
					var type = dictionaryProperty.Key;
					if (type.Name != className)
					{
						dictionaryProperties.Add(type, dictionaryProperty.Value);
					}
				}
			}

			return EntityAdapter.GenerateModifiable(entity, dictionaryProperties, entities);
		}

		public string CreateEntityHelper(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (!this.IsModifiable(entity.Table)) return EntityHelper.Generate(entity);

			return string.Empty;
		}

		public string CreateClassViewModel(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			return EntityClass.GenerateClassViewModel(entity, !this.IsModifiable(entity.Table));
		}

		public string CreateTableViewModel(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			return EntityClass.GenerateTableViewModel(entity, !this.IsModifiable(entity.Table));
		}

		public string CreateClassModule(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			return EntityClass.GenerateClassModule(entity, !this.IsModifiable(entity.Table));
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
						dictionaryProperties.Add(type, new ClrProperty(foreignKeyTable.Name, new ClrType(@"Dictionary<long, " + foreignKeyTable.ClassName + @">", true, true)));
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