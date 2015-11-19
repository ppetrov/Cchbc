using System;
using System.Collections.Generic;
using System.Linq;
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
				entities[i] = CreateEntity(tables[i]);
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
			// Don't generate Get in the inversed table - Activities

			var table = entity.Table;
			var readOnly = !this.ModifiableTables.ContainsKey(table.Name);
			if (readOnly)
			{
				return EntityGenerator.ReadOnlyAdapter(entity, GetDictionaryProperties(entity));
			}

			var currentTable = entity.Table;

			// We need only the current entity
			var entities = new[] { entity };

			if (entity.InverseTable != null)
			{
				// If the entity has an inverse table
				// we need both entities
				entities = new[] { entity, this.CreateEntity(entity.InverseTable) };
			}
			else
			{
				foreach (var column in entity.Table.Columns)
				{
					var foreignKey = column.DbForeignKey;
					if (foreignKey != null)
					{
						// Check if the current table isn't an inveverse table for other entity
						DbTable inverseTable;
						this.InverseTables.TryGetValue(foreignKey.Table.Name, out inverseTable);

						if (inverseTable == currentTable)
						{
							entities = Enumerable.Empty<Entity>().ToArray();
							break;
						}
					}
				}
			}


			var dictionaryProperties = new Dictionary<ClrType, ClrProperty>();
			foreach (var e in entities)
			{
				foreach (var typeProperty in GetDictionaryProperties(e))
				{
					if (entity.Class.Name != typeProperty.Key.Name)
					{
						dictionaryProperties.Add(typeProperty.Key, typeProperty.Value);
					}
				}
			}

			return EntityGenerator.ModifiableAdapter(entity, dictionaryProperties, entities.Length > 0);
		}

		private Entity CreateEntity(DbTable table)
		{
			DbTable inverseTable;
			this.InverseTables.TryGetValue(table.Name, out inverseTable);

			var clrClass = DbTable2ClrClassConverter.Convert(table, inverseTable);

			return new Entity(clrClass, table, inverseTable);
		}

		private static Dictionary<ClrType, ClrProperty> GetDictionaryProperties(Entity entity)
		{
			var dictionaryProperties = new Dictionary<ClrType, ClrProperty>();

			foreach (var property in entity.Class.Properties)
			{
				var type = property.Type;
				if (type.IsUserType)
				{
					string propertyType = null;

					var name = type.Name;
					foreach (var column in entity.Table.Columns)
					{
						var foreignKey = column.DbForeignKey;
						if (foreignKey != null)
						{
							var fkTableClassName = foreignKey.Table.ClassName;
							if (fkTableClassName == name)
							{
								propertyType = foreignKey.Table.Name;
								break;
							}
						}
					}

					if (propertyType == null && entity.InverseTable != null)
					{
						var inverseTableClassName = @"List<" + entity.InverseTable.ClassName + @">";
						if (inverseTableClassName == name)
						{
							// Ignore user type(List<T>) from Inverse table
							continue;
						}
					}

					dictionaryProperties.Add(type, new ClrProperty(propertyType, new ClrType($@"Dictionary<long, {name}>", true)));
				}
			}

			return dictionaryProperties;
		}
	}
}