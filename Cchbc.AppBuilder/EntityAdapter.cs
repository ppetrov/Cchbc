using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DDL;
using Cchbc.AppBuilder.DML;

namespace Cchbc.AppBuilder
{
	public static class EntityAdapter
	{
		private static readonly ClrProperty QueryHelperProperty = new ClrProperty(@"QueryHelper", new ClrType(@"QueryHelper", true, false));

		public static string GenerateReadOnly(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));

			var buffer = new StringBuilder(2 * 1024);

			var indentationLevel = 0;
			var entityClassName = entity.Class.Name;
			var adapterClassName = entityClassName + @"Adapter";
			var adapterProperties = GetClassProperties(dictionaryProperties);

			EntityClass.AppendClassDefinition(buffer, adapterClassName, @"IReadOnlyAdapter", entityClassName);

			EntityClass.AppendOpenBrace(buffer, indentationLevel++);

			EntityClass.AppendClassProperties(buffer, adapterProperties, indentationLevel, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClassName, adapterProperties, indentationLevel);
			buffer.AppendLine();

			AppendFillMethod(buffer, entity, indentationLevel);
			buffer.AppendLine();

			AppendCreatorMethod(buffer, entity, dictionaryProperties, indentationLevel);

			EntityClass.AppendCloseBrace(buffer, --indentationLevel);

			return buffer.ToString();
		}

		public static string GenerateModifiable(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, Entity[] readerEntities)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));
			if (readerEntities == null) throw new ArgumentNullException(nameof(readerEntities));

			var buffer = new StringBuilder(4 * 1024);

			var indentationLevel = 0;
			var entityClass = entity.Class;
			var entityClassName = entityClass.Name;
			var adapterClassName = entityClassName + @"Adapter";
			var adapterProperties = GetClassProperties(dictionaryProperties);

			EntityClass.AppendClassDefinition(buffer, adapterClassName, @"IModifiableAdapter", entityClassName);

			EntityClass.AppendOpenBrace(buffer, indentationLevel++);

			EntityClass.AppendClassProperties(buffer, adapterProperties, indentationLevel, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClassName, adapterProperties, indentationLevel);
			buffer.AppendLine();

			if (readerEntities.Length > 0)
			{
				AppendGetAllMethod(buffer, entity, readerEntities, indentationLevel);
				buffer.AppendLine();
			}

			AppendInsertMethod(buffer, entity);
			buffer.AppendLine();

			AppendUpdateMethod(buffer, entity);
			buffer.AppendLine();

			AppendDeleteMethod(buffer, entity);

			if (readerEntities.Length > 0)
			{
				buffer.AppendLine();
				if (readerEntities.Length == 1)
				{
					AppendCreatorMethod(buffer, entity, dictionaryProperties, indentationLevel);
				}
				else
				{
					AppendCreatorMethod(buffer, entity, dictionaryProperties, indentationLevel, readerEntities);
				}
			}

			EntityClass.AppendCloseBrace(buffer, --indentationLevel);

			return buffer.ToString();
		}

		private static ClrProperty[] GetClassProperties(Dictionary<ClrType, ClrProperty> dictionaryProperties)
		{
			var classProperties = new ClrProperty[dictionaryProperties.Count + 1];

			classProperties[0] = QueryHelperProperty;

			var i = 1;
			foreach (var classProperty in dictionaryProperties.Values)
			{
				classProperties[i++] = classProperty;
			}

			return classProperties;
		}

		private static void AppendFillMethod(StringBuilder buffer, Entity entity, int indentationLevel)
		{
			var className = entity.Class.Name;
			const string dictionaryName = @"items";
			const string funcName = @"selector";

			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"void");
			buffer.Append(' ');
			buffer.Append(@"Fill");
			buffer.Append('(');
			buffer.Append(@"Dictionary");
			buffer.Append('<');
			buffer.Append(@"long");
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(className);
			buffer.Append('>');
			buffer.Append(' ');
			buffer.Append(dictionaryName);
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"Func");
			buffer.Append('<');
			buffer.Append(className);
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"long");
			buffer.Append('>');
			buffer.Append(' ');
			buffer.Append(funcName);
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, indentationLevel++);
			EntityClass.AppendArgumentNullCheck(buffer, dictionaryName, indentationLevel);
			EntityClass.AppendArgumentNullCheck(buffer, funcName, indentationLevel);

			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"var");
			buffer.Append(' ');
			buffer.Append(@"query");
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append('@');
			buffer.Append('"');
			QueryBuilder.AppendSelect(buffer, entity.Table);
			buffer.Append('"');
			buffer.Append(';');
			buffer.AppendLine();

			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"this");
			buffer.Append('.');
			buffer.Append(@"QueryHelper");
			buffer.Append('.');
			buffer.Append(@"Fill");
			buffer.Append('(');
			buffer.Append(@"new");
			buffer.Append(' ');
			buffer.Append(@"Query");
			buffer.Append('<');
			buffer.Append(className);
			buffer.Append('>');
			buffer.Append('(');
			buffer.Append(@"query");
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"this");
			buffer.Append('.');
			buffer.Append(className);
			buffer.Append(@"Creator");
			buffer.Append(')');
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(dictionaryName);
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(funcName);
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendCloseBrace(buffer, --indentationLevel);
		}

		private static void AppendCreatorMethod(StringBuilder buffer, Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, int indentationLevel)
		{
			var entityClass = entity.Class;
			var entityClassName = entityClass.Name;
			var properties = entityClass.Properties;

			EntityClass.AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"private");
			buffer.Append(' ');
			buffer.Append(entityClassName);
			buffer.Append(' ');
			buffer.Append(entityClassName);
			buffer.Append(@"Creator");
			buffer.Append('(');
			buffer.Append(@"IFieldDataReader");
			buffer.Append(' ');
			buffer.Append('r');
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, indentationLevel++);

			for (var index = 0; index < properties.Length; index++)
			{
				var property = properties[index];

				// Primary Key isn't checked for NULL
				if (property.Name.Equals(DbColumn.IdName))
				{
					AppendAssignValue(buffer, property, index, indentationLevel);
				}
				else
				{
					AppendVariableDeclaration(buffer, property.Name, property.Type, string.Empty, indentationLevel);
					AppendCheckForValue(buffer, index, indentationLevel);
					EntityClass.AppendOpenBrace(buffer, indentationLevel++);
					AppendAssignValue(buffer, property.Name, string.Empty, property.Type, index, dictionaryProperties, indentationLevel);
					EntityClass.AppendCloseBrace(buffer, --indentationLevel);
				}
			}

			buffer.AppendLine();
			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"return");
			buffer.Append(' ');
			AppendCreateNewInstance(buffer, entityClassName, properties);
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendCloseBrace(buffer, --indentationLevel);
		}

		private static void AppendCreatorMethod(StringBuilder buffer, Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, int indentationLevel, Entity[] readerEntities)
		{
			var masterClass = entity.Class;
			var masterClassName = masterClass.Name;
			var masterProperties = masterClass.Properties;

			EntityClass.AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"private");
			buffer.Append(' ');
			buffer.Append(@"void");
			buffer.Append(' ');
			buffer.Append(masterClassName);
			buffer.Append(@"Creator");
			buffer.Append('(');
			buffer.Append(@"IFieldDataReader");
			buffer.Append(' ');
			buffer.Append('r');
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"Dictionary");
			buffer.Append('<');
			buffer.Append(@"long");
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(masterClassName);
			buffer.Append('>');
			buffer.Append(' ');
			BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, indentationLevel++);

			// Read PrimaryKey
			for (var index = 0; index < masterProperties.Length; index++)
			{
				var property = masterProperties[index];

				// Primary Key isn't checked for NULL
				if (property.Name.Equals(DbColumn.IdName))
				{
					AppendAssignValue(buffer, property, index, indentationLevel);
					break;
				}
			}

			buffer.AppendLine();
			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(masterClassName);
			buffer.Append(' ');
			BufferHelper.AppendLowerFirst(buffer, masterClassName);
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"if");
			buffer.Append(' ');
			buffer.Append('(');
			buffer.Append('!');
			BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
			buffer.Append('.');
			buffer.Append(@"TryGetValue");
			buffer.Append('(');
			BufferHelper.AppendLowerFirst(buffer, DbColumn.IdName);
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"out");
			buffer.Append(' ');
			BufferHelper.AppendLowerFirst(buffer, masterClassName);
			buffer.Append(')');
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, indentationLevel++);

			for (var index = 0; index < masterProperties.Length; index++)
			{
				var property = masterProperties[index];
				if (property.Name.Equals(DbColumn.IdName))
				{
					continue;
				}
				if (property.Type.IsCollection)
				{
					continue;
				}
				AppendVariableDeclaration(buffer, property.Name, property.Type, string.Empty, indentationLevel);
				AppendCheckForValue(buffer, index, indentationLevel);
				EntityClass.AppendOpenBrace(buffer, indentationLevel++);
				AppendAssignValue(buffer, property.Name, string.Empty, property.Type, index, dictionaryProperties, indentationLevel);
				EntityClass.AppendCloseBrace(buffer, --indentationLevel);
			}

			EntityClass.AppendIndentation(buffer, indentationLevel);
			BufferHelper.AppendLowerFirst(buffer, masterClassName);
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			AppendCreateNewInstance(buffer, masterClassName, masterProperties);
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, indentationLevel);
			BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
			buffer.Append('.');
			buffer.Append(@"Add");
			buffer.Append('(');
			BufferHelper.AppendLowerFirst(buffer, DbColumn.IdName);
			buffer.Append(',');
			buffer.Append(' ');
			BufferHelper.AppendLowerFirst(buffer, masterClassName);
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendCloseBrace(buffer, --indentationLevel);

			var offset = entity.Table.Columns.Count;
			var detailEntity = readerEntities[1];
			var detailClass = detailEntity.Class;
			var detailClassName = detailClass.Name;
			var detailProperties = detailClass.Properties;

			buffer.AppendLine();
			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.AppendLine(@"// Ignore NULL entities caused by the LEFT JOIN");
			AppendCheckForDbNull(buffer, offset, indentationLevel);
			EntityClass.AppendIndentation(buffer, indentationLevel + 1);
			buffer.AppendLine(@"return;");
			buffer.AppendLine();

			for (var i = 0; i < detailProperties.Length; i++)
			{
				var index = i + offset;
				var property = detailProperties[i];
				if (property.Name == masterClassName)
				{
					continue;
				}
				var propertyName = property.Name;
				AppendVariableDeclaration(buffer, propertyName, property.Type, detailClassName, indentationLevel);
				AppendCheckForValue(buffer, index, indentationLevel);
				EntityClass.AppendOpenBrace(buffer, indentationLevel++);
				AppendAssignValue(buffer, propertyName, detailClassName, property.Type, index, dictionaryProperties, indentationLevel);
				EntityClass.AppendCloseBrace(buffer, --indentationLevel);
			}

			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"var");
			buffer.Append(' ');
			BufferHelper.AppendLowerFirst(buffer, detailClassName);
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');

			buffer.Append(@"new");
			buffer.Append(' ');
			buffer.Append(detailClassName);
			buffer.Append('(');

			for (var i = 0; i < detailProperties.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				var property = detailProperties[i];
				var type = property.Type;
				if (type.IsCollection)
				{
					buffer.Append(@"new");
					buffer.Append(' ');
					buffer.Append(type.Name);
					buffer.Append('(');
					buffer.Append(')');
				}
				else
				{
					if (property.Name == masterClassName)
					{
						BufferHelper.AppendLowerFirst(buffer, masterClassName);
					}
					else
					{
						BufferHelper.AppendLowerFirst(buffer, detailClassName);
						buffer.Append(property.Name);
					}
				}
			}

			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, indentationLevel);
			BufferHelper.AppendLowerFirst(buffer, masterClassName);
			buffer.Append('.');
			buffer.Append(detailEntity.Table.Name);
			buffer.Append('.');
			buffer.Append(@"Add");
			buffer.Append('(');
			BufferHelper.AppendLowerFirst(buffer, detailClassName);
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendCloseBrace(buffer, --indentationLevel);
		}

		private static void AppendGetAllMethod(StringBuilder buffer, Entity entity, Entity[] readerEntities, int indentationLevel)
		{
			var className = entity.Class.Name;

			EntityClass.AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"List");
			buffer.Append('<');
			buffer.Append(className);
			buffer.Append('>');
			buffer.Append(' ');
			buffer.Append(@"GetAll");
			buffer.Append('(');
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, indentationLevel++);

			EntityClass.AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"var");
			buffer.Append(' ');
			buffer.Append(@"query");
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append('@');
			buffer.Append('"');
			if (readerEntities.Length == 1)
			{
				QueryBuilder.AppendSelect(buffer, entity.Table);
			}
			else
			{
				QueryBuilder.AppendSelectJoin(buffer, entity.Table, entity.InverseTable);
			}

			buffer.Append('"');
			buffer.Append(';');
			buffer.AppendLine();
			buffer.AppendLine();

			if (readerEntities.Length == 1)
			{
				EntityClass.AppendIndentation(buffer, indentationLevel);
				buffer.Append(@"return");
				buffer.Append(' ');
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(@"QueryHelper");
				buffer.Append('.');
				buffer.Append(@"Execute");
				buffer.Append('(');
				buffer.Append(@"new");
				buffer.Append(' ');
				buffer.Append(@"Query");
				buffer.Append('<');
				buffer.Append(className);
				buffer.Append('>');
				buffer.Append('(');
				buffer.Append(@"query");
				buffer.Append(',');
				buffer.Append(' ');
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(className);
				buffer.Append(@"Creator");
				buffer.Append(')');
				buffer.Append(')');
				buffer.Append(';');
				buffer.AppendLine();
			}
			else
			{
				EntityClass.AppendIndentation(buffer, indentationLevel);
				buffer.Append(@"var");
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
				buffer.Append(' ');
				buffer.Append('=');
				buffer.Append(' ');
				buffer.Append(@"new");
				buffer.Append(' ');
				buffer.Append(@"Dictionary");
				buffer.Append('<');
				buffer.Append(@"long");
				buffer.Append(',');
				buffer.Append(' ');
				buffer.Append(className);
				buffer.Append('>');
				buffer.Append('(');
				buffer.Append(')');
				buffer.Append(';');
				buffer.AppendLine();

				EntityClass.AppendIndentation(buffer, indentationLevel);
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(@"QueryHelper");
				buffer.Append('.');
				buffer.Append(@"Fill");
				buffer.Append('(');
				buffer.Append(@"query");
				buffer.Append(',');
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
				buffer.Append(',');
				buffer.Append(' ');
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(className);
				buffer.Append(@"Creator");
				buffer.Append(')');
				buffer.Append(';');
				buffer.AppendLine();

				EntityClass.AppendIndentation(buffer, indentationLevel);
				buffer.Append(@"return");
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
				buffer.Append('.');
				buffer.Append(@"Values");
				buffer.Append('.');
				buffer.Append(@"ToList");
				buffer.Append('(');
				buffer.Append(')');
				buffer.Append(';');
				buffer.AppendLine();
			}

			EntityClass.AppendCloseBrace(buffer, --indentationLevel);
		}

		private static void AppendInsertMethod(StringBuilder buffer, Entity entity)
		{
			AppendMethod(buffer, entity, @"Insert", QueryBuilder.AppendInsert, c => !c.IsPrimaryKey, true);
		}

		private static void AppendUpdateMethod(StringBuilder buffer, Entity entity)
		{
			AppendMethod(buffer, entity, @"Update", QueryBuilder.AppendUpdate, c => true, false);
		}

		private static void AppendDeleteMethod(StringBuilder buffer, Entity entity)
		{
			AppendMethod(buffer, entity, @"Delete", QueryBuilder.AppendDelete, c => c.IsPrimaryKey, false);
		}

		private static void AppendMethod(StringBuilder buffer, Entity entity, string methodName, Action<StringBuilder, DbTable> queryAppender, Func<DbColumn, bool> columnMatcher, bool getNewId)
		{
			var className = entity.Class.Name;

			var level = 1;
			EntityClass.AppendIndentation(buffer, level);
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"Task");
			buffer.Append(' ');
			buffer.Append(methodName);
			buffer.Append(@"Async");
			buffer.Append('(');
			buffer.Append(className);
			buffer.Append(' ');
			buffer.Append(@"item");
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, level++);
			EntityClass.AppendArgumentNullCheck(buffer, @"item", level);

			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, level);
			buffer.Append(@"var");
			buffer.Append(' ');
			buffer.Append(@"sqlParams");
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append(@"new");
			buffer.Append('[');
			buffer.Append(']');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, level++);

			for (var index = 0; index < entity.Table.Columns.Count; index++)
			{
				var column = entity.Table.Columns[index];
				if (!columnMatcher(column))
				{
					continue;
				}

				EntityClass.AppendIndentation(buffer, level);
				buffer.Append(@"new");
				buffer.Append(' ');
				buffer.Append(@"QueryParameter");
				buffer.Append('(');
				buffer.Append('@');
				buffer.Append('"');
				buffer.Append(QueryBuilder.ParameterPlaceholder);
				buffer.Append(QueryBuilder.ParameterPrefix);
				buffer.Append(column.Name);
				buffer.Append('"');
				buffer.Append(',');
				buffer.Append(' ');
				buffer.Append(@"item");
				buffer.Append('.');
				var property = entity.Class.Properties[index];
				buffer.Append(property.Name);
				if (property.Type.IsUserType)
				{
					buffer.Append('.');
					buffer.Append(DbColumn.IdName);
				}
				buffer.Append(')');
				buffer.Append(',');
				buffer.AppendLine();
			}

			EntityClass.AppendIndentation(buffer, --level);

			buffer.Append('}');
			buffer.Append(';');
			buffer.AppendLine();

			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, level);
			buffer.Append(@"var");
			buffer.Append(' ');
			buffer.Append(@"query");
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append('@');
			buffer.Append('"');
			queryAppender(buffer, entity.Table);
			buffer.Append('"');
			buffer.Append(';');
			buffer.AppendLine();

			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, level);
			buffer.Append(@"this");
			buffer.Append('.');
			buffer.Append(@"QueryHelper");
			buffer.Append('.');
			buffer.Append(@"Execute");
			buffer.Append('(');
			buffer.Append(@"query");
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"sqlParams");
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			if (getNewId)
			{
				buffer.AppendLine();
				EntityClass.AppendIndentation(buffer, level);
				buffer.Append(@"item");
				buffer.Append('.');
				buffer.Append(DbColumn.IdName);
				buffer.Append(' ');
				buffer.Append('=');
				buffer.Append(' ');
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(@"QueryHelper");
				buffer.Append('.');
				buffer.Append(@"GetNewId");
				buffer.Append('(');
				buffer.Append(')');
				buffer.Append(';');
				buffer.AppendLine();
			}

			EntityClass.AppendIndentation(buffer, level);
			buffer.AppendLine();

			EntityClass.AppendIndentation(buffer, level);
			buffer.AppendLine(@"return Task.FromResult(true);");

			EntityClass.AppendCloseBrace(buffer, --level);
		}

		private static void AppendCreateNewInstance(StringBuilder buffer, string className, ClrProperty[] properties)
		{
			buffer.Append(@"new");
			buffer.Append(' ');
			buffer.Append(className);
			buffer.Append('(');

			for (var i = 0; i < properties.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				var property = properties[i];
				var type = property.Type;
				if (type.IsCollection)
				{
					buffer.Append(@"new");
					buffer.Append(' ');
					buffer.Append(type.Name);
					buffer.Append('(');
					buffer.Append(')');
				}
				else
				{
					BufferHelper.AppendLowerFirst(buffer, property.Name);
				}
			}

			buffer.Append(')');
		}

		private static void AppendVariableDeclaration(StringBuilder buffer, string propertyName, ClrType type, string propertyPrefix, int indentationLevel)
		{
			EntityClass.AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"var");
			buffer.Append(' ');
			if (propertyPrefix != string.Empty)
			{
				BufferHelper.AppendLowerFirst(buffer, propertyPrefix);
				buffer.Append(propertyName);
			}
			else
			{
				BufferHelper.AppendLowerFirst(buffer, propertyName);
			}
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append(GetDefaultValue(type));
			buffer.Append(';');
			buffer.AppendLine();
		}

		private static void AppendCheckForValue(StringBuilder buffer, int index, int indentationLevel)
		{
			AppendReaderCheckValue(buffer, index, indentationLevel, @"!");
		}

		private static void AppendCheckForDbNull(StringBuilder buffer, int index, int indentationLevel)
		{
			AppendReaderCheckValue(buffer, index, indentationLevel, string.Empty);
		}

		private static void AppendReaderCheckValue(StringBuilder buffer, int index, int indentationLevel, string condition)
		{
			EntityClass.AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"if");
			buffer.Append(' ');
			buffer.Append('(');
			buffer.Append(condition);
			buffer.Append('r');
			buffer.Append('.');
			buffer.Append(@"IsDbNull");
			buffer.Append('(');
			buffer.Append(index);
			buffer.Append(')');
			buffer.Append(')');
			buffer.AppendLine();
		}

		private static void AppendAssignValue(StringBuilder buffer, ClrProperty property, int index, int indentationLevel)
		{
			EntityClass.AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"var");
			buffer.Append(' ');
			BufferHelper.AppendLowerFirst(buffer, property.Name);
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			AppendReadValue(buffer, GetReaderMethod(property.Type), index);
			buffer.Append(';');
			buffer.AppendLine();
		}

		private static void AppendAssignValue(StringBuilder buffer, string propertyName, string prefix, ClrType type, int index, Dictionary<ClrType, ClrProperty> dictionaries, int indentationLevel)
		{
			var readerMethod = GetReaderMethod(type);

			EntityClass.AppendIndentation(buffer, indentationLevel);
			if (prefix != string.Empty)
			{
				BufferHelper.AppendLowerFirst(buffer, prefix);
				buffer.Append(propertyName);
			}
			else
			{
				BufferHelper.AppendLowerFirst(buffer, propertyName);
			}
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			if (type.IsUserType)
			{
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(dictionaries[type].Name);
				buffer.Append('[');
				AppendReadValue(buffer, readerMethod, index);
				buffer.Append(']');
			}
			else
			{
				AppendReadValue(buffer, readerMethod, index);
			}
			buffer.Append(';');
			buffer.AppendLine();
		}

		private static void AppendReadValue(StringBuilder buffer, string readerMethod, int index)
		{
			buffer.Append('r');
			buffer.Append('.');
			buffer.Append(readerMethod);
			buffer.Append('(');
			buffer.Append(index);
			buffer.Append(')');
		}

		private static string GetReaderMethod(ClrType type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			if (type == ClrType.Long) return @"GetInt64";
			if (type == ClrType.String) return @"GetString";
			if (type == ClrType.Decimal) return @"GetDecimal";
			if (type == ClrType.DateTime) return @"GetDateTime";
			if (type == ClrType.Bytes) return @"GetBytes";

			return @"GetInt64";
		}

		private static string GetDefaultValue(ClrType type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			if (type == ClrType.Long) return @"0L";
			if (type == ClrType.String) return @"string.Empty";
			if (type == ClrType.Decimal) return @"0M";
			if (type == ClrType.DateTime) return @"DateTime.MinValue";
			if (type == ClrType.Bytes) return @"default(byte[])";

			return $@"default({type.Name})";
		}
	}
}