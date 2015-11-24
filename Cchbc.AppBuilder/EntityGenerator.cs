using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DDL;
using Cchbc.AppBuilder.DML;

namespace Cchbc.AppBuilder
{
	public static class EntityGenerator
	{
		public static string ModifiableAdapter(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, bool generateGet)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));

			var buffer = new StringBuilder(2 * 1024);

			var className = entity.Class.Name;
			var adapterClass = new ClrClass(className + @"Adapter", GetClassProperties(dictionaryProperties));

			buffer.AppendFormat(@"public sealed class {0}Adapter : IModifiableAdapter<{0}>", className);
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer);

			EntityClass.AppendClassProperties(buffer, adapterClass.Properties, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClass.Name, adapterClass.Properties);
			buffer.AppendLine();

			if (generateGet)
			{
				AppendGetAllMethod(buffer, entity);
				buffer.AppendLine();
			}

			AppendInsertMethod(buffer, entity);
			buffer.AppendLine();

			AppendUpdateMethod(buffer, entity);
			buffer.AppendLine();

			AppendDeleteMethod(buffer, entity);

			if (generateGet)
			{
				buffer.AppendLine();
				EntityClass.AppendCreatorMethod(buffer, entity.Class, dictionaryProperties);
			}

			EntityClass.AppendCloseBrace(buffer);

			return buffer.ToString();
		}

		private static void AppendGetAllMethod(StringBuilder buffer, Entity entity)
		{
			var level = 1;
			var @class = entity.Class;
			var className = @class.Name;

			EntityClass.AppendIndentation(buffer, level);

			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"List");
			buffer.Append('<');
			buffer.Append(entity.Class.Name);
			buffer.Append('>');
			buffer.Append(' ');
			buffer.Append(@"Get");
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
			if (entity.InverseTable == null)
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
			buffer.Append(@"Execute(query, sqlParams);");
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
				buffer.Append(@"GetNewId();");

				buffer.AppendLine();
			}

			EntityClass.AppendCloseBrace(buffer, --level);
		}

		public static ClrProperty[] GetClassProperties(Dictionary<ClrType, ClrProperty> dictionaryProperties)
		{
			var classProperties = new ClrProperty[dictionaryProperties.Count + 1];

			classProperties[0] = new ClrProperty(@"QueryHelper", new ClrType(@"QueryHelper", true));

			var i = 1;
			foreach (var classProperty in dictionaryProperties.Values)
			{
				classProperties[i++] = classProperty;
			}

			return classProperties;
		}
	}
}