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
		public static string GenerateReadOnly(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));

			var buffer = new StringBuilder(2 * 1024);

			var adapterClassName = entity.Class.Name + @"Adapter";
			var adapterProperties = GetClassProperties(dictionaryProperties);

			AppendClassDefinition(entity, buffer, adapterClassName, @"IReadOnlyAdapter");

			EntityClass.AppendOpenBrace(buffer);

			EntityClass.AppendClassProperties(buffer, adapterProperties, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClassName, adapterProperties);
			buffer.AppendLine();

			AppendFillMethod(buffer, entity);
			buffer.AppendLine();

			EntityClass.AppendCreatorMethod(buffer, entity, dictionaryProperties, new[] { entity });

			EntityClass.AppendCloseBrace(buffer);

			return buffer.ToString();
		}

		public static string GenerateModifiable(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, Entity[] readerEntities)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));
			if (readerEntities == null) throw new ArgumentNullException(nameof(readerEntities));

			var buffer = new StringBuilder(4 * 1024);

			var entityClass = entity.Class;
			var adapterClassName = entityClass.Name + @"Adapter";
			var adapterProperties = GetClassProperties(dictionaryProperties);

			AppendClassDefinition(entity, buffer, adapterClassName, @"IModifiableAdapter");

			EntityClass.AppendOpenBrace(buffer);

			EntityClass.AppendClassProperties(buffer, adapterProperties, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClassName, adapterProperties);
			buffer.AppendLine();

			if (readerEntities.Length > 0)
			{
				AppendGetAllMethod(buffer, entity, readerEntities);
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
				EntityClass.AppendCreatorMethod(buffer, entity, dictionaryProperties, readerEntities);
			}

			EntityClass.AppendCloseBrace(buffer);

			return buffer.ToString();
		}

		private static void AppendClassDefinition(Entity entity, StringBuilder buffer, string adapterClassName, string baseClass)
		{
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"sealed");
			buffer.Append(' ');
			buffer.Append(@"class");
			buffer.Append(' ');
			buffer.Append(adapterClassName);
			buffer.Append(' ');
			buffer.Append(':');
			buffer.Append(' ');
			buffer.Append(baseClass);
			buffer.Append('<');
			buffer.Append(entity.Class.Name);
			buffer.Append('>');
			buffer.AppendLine();
		}

		private static void AppendFillMethod(StringBuilder buffer, Entity entity)
		{
			var className = entity.Class.Name;
			var dictionaryName = @"items";
			var funcName = @"selector";

			var level = 1;
			EntityClass.AppendIndentation(buffer, level);
			// Fill method
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

			EntityClass.AppendOpenBrace(buffer, level++);
			EntityClass.AppendArgumentNullCheck(buffer, dictionaryName, level);
			EntityClass.AppendArgumentNullCheck(buffer, funcName, level);

			buffer.AppendLine();

			// Query
			EntityClass.AppendIndentation(buffer, level);
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

			// Fill items using the query & creator
			EntityClass.AppendIndentation(buffer, level);
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

			EntityClass.AppendCloseBrace(buffer, --level);
		}

		private static void AppendGetAllMethod(StringBuilder buffer, Entity entity, Entity[] readerEntities)
		{
			var level = 1;
			var className = entity.Class.Name;

			EntityClass.AppendIndentation(buffer, level);

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

			EntityClass.AppendOpenBrace(buffer, level++);

			EntityClass.AppendIndentation(buffer, level);
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
				EntityClass.AppendIndentation(buffer, level);
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
				EntityClass.AppendIndentation(buffer, level);
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

				EntityClass.AppendIndentation(buffer, level);
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

				EntityClass.AppendIndentation(buffer, level);
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

			EntityClass.AppendCloseBrace(buffer, --level);
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
			buffer.Append(@"void");
			buffer.Append(' ');
			buffer.Append(methodName);
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

			for (var index = 0; index < entity.Table.Columns.Length; index++)
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
					buffer.Append(NameProvider.IdName);
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
				buffer.Append(NameProvider.IdName);
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

			EntityClass.AppendCloseBrace(buffer, --level);
		}

		private static ClrProperty[] GetClassProperties(Dictionary<ClrType, ClrProperty> dictionaryProperties)
		{
			var classProperties = new ClrProperty[dictionaryProperties.Count + 1];

			classProperties[0] = new ClrProperty(@"QueryHelper", new ClrType(@"QueryHelper", true, false));

			var i = 1;
			foreach (var classProperty in dictionaryProperties.Values)
			{
				classProperties[i++] = classProperty;
			}

			return classProperties;
		}
	}
}