using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DDL;

namespace Cchbc.AppBuilder
{
	public sealed class DbProject
	{
		[Serializable]
		public sealed class Project
		{
			public DbSchema Schema { get; set; }
			public Dictionary<string, DbTable> InverseTables { get; set; }
			public Dictionary<string, DbTable> ModifiableTables { get; set; }
			public Dictionary<string, DbTable> HiddenTables { get; set; }

			public Project()
			{
			}

			public Project(DbSchema schema, Dictionary<string, DbTable> inverseTables, Dictionary<string, DbTable> modifiableTables, Dictionary<string, DbTable> hiddenTables)
			{
				if (schema == null) throw new ArgumentNullException(nameof(schema));
				if (inverseTables == null) throw new ArgumentNullException(nameof(inverseTables));
				if (modifiableTables == null) throw new ArgumentNullException(nameof(modifiableTables));
				if (hiddenTables == null) throw new ArgumentNullException(nameof(hiddenTables));

				this.Schema = schema;
				this.InverseTables = inverseTables;
				this.ModifiableTables = modifiableTables;
				this.HiddenTables = hiddenTables;
			}
		}

		public DbSchema Schema { get; }
		private Dictionary<string, DbTable> InverseTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> ModifiableTables { get; } = new Dictionary<string, DbTable>();
		private Dictionary<string, DbTable> HiddenTables { get; } = new Dictionary<string, DbTable>();

		public DbProject(DbSchema schema)
		{
			if (schema == null) throw new ArgumentNullException(nameof(schema));

			this.Schema = schema;
		}

		public void Save(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			var project = new Project(this.Schema, this.InverseTables, this.ModifiableTables, this.HiddenTables);

			var formatter = new BinaryFormatter();
			using (var fs = File.OpenWrite(path))
			{
				formatter.Serialize(fs, project);
			}
		}

		public static DbProject Load(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			var formatter = new BinaryFormatter();
			using (var fs = File.OpenRead(path))
			{
				var project = (Project)formatter.Deserialize(fs);

				var dbProject = new DbProject(project.Schema);

				foreach (var pair in project.InverseTables)
				{
					dbProject.InverseTables.Add(pair.Key, pair.Value);
				}
				foreach (var pair in project.ModifiableTables)
				{
					dbProject.ModifiableTables.Add(pair.Key, pair.Value);
				}
				foreach (var pair in project.HiddenTables)
				{
					dbProject.HiddenTables.Add(pair.Key, pair.Value);
				}

				return dbProject;
			}
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
			else
			{
				// We don't need a Get method. It will be generated on Inverse table side
				var hasColumnToInverseTable = this.HasColumnToInverseTable(entity);
				if (hasColumnToInverseTable)
				{
					entities = Enumerable.Empty<Entity>().ToArray();
					dictionaryProperties.Clear();
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

			return EntityClass.GenerateViewModel(entity, !this.IsModifiable(entity.Table));
		}

		public string CreateClassModule(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			
			return EntityClass.GenerateClassModule(entity, !this.IsModifiable(entity.Table));
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

			return new Entity(DbTableConverter.ConvertToClrClass(table, inverseTable), table, inverseTable);
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