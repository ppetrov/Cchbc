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
			var adapterProperties = EntityGenerator.GetClassProperties(dictionaryProperties);

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
			buffer.Append(@"IReadOnlyAdapter");
			buffer.Append('<');
			buffer.Append(adapterClassName);
			buffer.Append('>');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer);

			EntityClass.AppendClassProperties(buffer, adapterProperties, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClassName, adapterProperties);
			buffer.AppendLine();

			AppendFillMethod(buffer, entity);
			buffer.AppendLine();

			AppendCreatorMethod(buffer, dictionaryProperties, entity.Class);
			buffer.AppendLine();

			EntityClass.AppendCloseBrace(buffer);

			return buffer.ToString();
		}

		private static void AppendFillMethod(StringBuilder buffer, Entity entity)
		{
			var className = entity.Class.Name;

			buffer.Append('\t');
			buffer.AppendFormat(@"public void Fill(Dictionary<long, {0}> items, Func<{0}, long> selector)", className);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			EntityClass.AppendArgumentNullCheck(buffer, @"items", 2);
			EntityClass.AppendArgumentNullCheck(buffer, @"selector", 2);

			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"var query = @");
			buffer.Append('"');
			QueryBuilder.AppendSelect(buffer, entity.Table);
			buffer.Append('"');
			buffer.Append(';');
			buffer.AppendLine();
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.AppendFormat(@"this.QueryHelper.Fill(new Query<{0}>(query, {0}Creator), items, selector);", className);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();
		}

		private static void AppendCreatorMethod(StringBuilder buffer, Dictionary<ClrType, ClrProperty> dictionaries, ClrClass @class, int level = 1)
		{
			EntityClass.AppendIndentation(buffer, level);

			buffer.Append(@"private");
			buffer.Append(' ');
			buffer.Append(@"static");
			buffer.Append(' ');
			buffer.Append(@class.Name);
			buffer.Append(' ');
			buffer.Append(@class.Name);
			buffer.Append(@"Creator");
			buffer.Append('(');
			buffer.Append(@"IFieldDataReader");
			buffer.Append(' ');
			buffer.Append('r');
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, level++);

			var properties = @class.Properties;
			for (var i = 0; i < properties.Length; i++)
			{
				if (i > 0)
				{
					buffer.AppendLine();
				}

				var property = properties[i];
				var type = property.Type;

				// Variable declaration
				EntityClass.AppendIndentation(buffer, level);
				buffer.Append(@"var");
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, property.Name);
				buffer.Append(' ');
				buffer.Append('=');
				buffer.Append(' ');
				buffer.Append(TypeHelper.GetDefaultValue(type));
				buffer.Append(';');
				buffer.AppendLine();

				// Check for NULL
				EntityClass.AppendIndentation(buffer, level);
				buffer.Append(@"if (!r.IsDbNull(");
				buffer.Append(i);
				buffer.Append(')');
				buffer.Append(')');
				buffer.AppendLine();

				EntityClass.AppendIndentation(buffer, level);
				buffer.AppendLine(@"{");

				// Read the value from the reader and assign to variable
				buffer.Append("\t\t");
				buffer.Append('\t');
				BufferHelper.AppendLowerFirst(buffer, property.Name);
				buffer.Append(@" = ");
				if (type.IsUserType)
				{
					buffer.Append(@"this");
					buffer.Append('.');
					buffer.Append(dictionaries[type].Name);
					buffer.Append('[');
				}
				buffer.Append(@"r.");
				buffer.Append(TypeHelper.GetReaderMethod(type));
				buffer.Append('(');
				buffer.Append(i);
				buffer.Append(')');
				if (type.IsUserType)
				{
					buffer.Append(']');
				}
				buffer.Append(';');
				buffer.AppendLine();

				buffer.Append("\t\t");
				buffer.AppendLine(@"}");
			}

			// Create instance & return the value;
			buffer.AppendLine();
			buffer.Append("\t\t");
			buffer.Append(@"return");
			buffer.Append(' ');
			buffer.Append(@"new");
			buffer.Append(' ');
			buffer.Append(@class.Name);
			buffer.Append('(');

			for (var i = 0; i < properties.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				BufferHelper.AppendLowerFirst(buffer, properties[i].Name);
			}
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendCloseBrace(buffer, --level);
		}
	}

	public static class EntityGenerator
	{
		public static string ModifiableAdapter(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, Entity[] entities)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));

			var buffer = new StringBuilder(2 * 1024);

			var className = entity.Class.Name;
			buffer.AppendFormat(@"public sealed class {0}Adapter : IModifiableAdapter<{0}>", className);
			buffer.AppendLine();

			buffer.Append('{');
			buffer.AppendLine();

			var classProperties = GetClassProperties(dictionaryProperties);

			var adapterClass = new ClrClass(className + @"Adapter", classProperties);

			EntityClass.AppendClassProperties(buffer, adapterClass.Properties, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClass.Name, adapterClass.Properties);
			buffer.AppendLine();

			if (entities.Length > 0)
			{
				AppendGetAllMethod(buffer, entity, dictionaryProperties);
				buffer.AppendLine();
			}

			AppendInsertMethod(buffer, entity);
			buffer.AppendLine();
			AppendUpdateMethod(buffer, entity);
			buffer.AppendLine();
			AppendDeleteMethod(buffer, entity);

			buffer.Append('}');
			buffer.AppendLine();

			return buffer.ToString();
		}

		private static void AppendGetAllMethod(StringBuilder buffer, Entity entity, Dictionary<ClrType, ClrProperty> dictionaries)
		{
			var @class = entity.Class;
			var properties = @class.Properties;
			var parameters = new string[properties.Length];
			for (var i = 0; i < properties.Length; i++)
			{
				parameters[i] = @"_" + properties[i].Name;
			}

			buffer.Append('\t');
			buffer.AppendFormat(@"public List<{0}> Get()", @class.Name);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"var query = @");
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

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.AppendFormat(@"return this.QueryHelper.Execute(new Query<{0}>(query, r =>", @class.Name);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			for (var i = 0; i < properties.Length; i++)
			{
				var p = properties[i];

				var type = p.Type;
				var name = parameters[i];

				if (i > 0)
				{
					buffer.AppendLine();
				}
				// Variable declaration
				buffer.Append("\t\t\t");
				buffer.Append(@"var");
				buffer.Append(' ');
				buffer.Append(name);
				buffer.Append(@" = ");
				buffer.Append(TypeHelper.GetDefaultValue(type));
				buffer.Append(';');
				buffer.AppendLine();

				// Check for NULL
				buffer.Append("\t\t\t");
				buffer.Append(@"if (!r.IsDbNull(");
				buffer.Append(i);
				buffer.Append(')');
				buffer.Append(')');
				buffer.AppendLine();

				buffer.Append("\t\t\t");
				buffer.AppendLine(@"{");

				// Read the value from the reader and assign to variable
				buffer.Append("\t\t\t");
				buffer.Append('\t');
				buffer.Append(name);
				buffer.Append(@" = ");
				if (type.IsUserType)
				{
					buffer.Append(@"this");
					buffer.Append('.');

					var tmp = "_lookup";
					ClrProperty t;
					if (dictionaries.TryGetValue(type, out t))
					{
						tmp = t.Name;
					}
					//buffer.Append(dictionaries[type].Name);
					buffer.Append(tmp);
					buffer.Append('[');
				}
				buffer.Append(@"r.");
				buffer.Append(TypeHelper.GetReaderMethod(type));
				buffer.Append('(');
				buffer.Append(i);
				buffer.Append(')');
				if (type.IsUserType)
				{
					buffer.Append(']');
				}
				buffer.Append(';');
				buffer.AppendLine();

				buffer.Append("\t\t\t");
				buffer.AppendLine(@"}");
			}

			// Create instance & return the value;
			buffer.AppendLine();
			buffer.Append("\t\t\t");
			buffer.Append(@"return");
			buffer.Append(' ');
			buffer.Append(@"new");
			buffer.Append(' ');
			buffer.Append(@class.Name);
			buffer.Append('(');
			for (var i = 0; i < parameters.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				buffer.Append(parameters[i]);
			}
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append('}');
			buffer.Append(@"));");
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('}');

			buffer.AppendLine();
		}

		private static void AppendInsertMethod(StringBuilder buffer, Entity entity)
		{
			AppendMethod(buffer, entity, @"Insert", QueryBuilder.AppendInsert, c => !c.IsPrimaryKey, true);
		}

		private static void AppendUpdateMethod(StringBuilder buffer, Entity entity)
		{
			AppendMethod(buffer, entity, @"Update", QueryBuilder.AppendUpdate, c => true);
		}

		private static void AppendDeleteMethod(StringBuilder buffer, Entity entity)
		{
			AppendMethod(buffer, entity, @"Delete", QueryBuilder.AppendDelete, c => c.IsPrimaryKey);
		}

		private static void AppendMethod(StringBuilder buffer, Entity entity, string methodName, Action<StringBuilder, DbTable> queryAppender, Func<DbColumn, bool> columnMatcher, bool getNewId = false)
		{
			var className = entity.Class.Name;

			buffer.Append('\t');
			buffer.Append(@"public void ");
			buffer.Append(methodName);
			buffer.AppendFormat(@"({0} item)", className);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.AppendLine(@"if (item == null) throw new ArgumentNullException(nameof(item));");
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.AppendLine(@"var sqlParams = new[]");

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			for (var index = 0; index < entity.Table.Columns.Length; index++)
			{
				var column = entity.Table.Columns[index];
				if (!columnMatcher(column))
				{
					continue;
				}

				buffer.Append('\t');
				buffer.Append('\t');
				buffer.Append('\t');
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

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append('}');
			buffer.Append(';');
			buffer.AppendLine();

			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"var query = @");
			buffer.Append('"');
			queryAppender(buffer, entity.Table);
			buffer.Append('"');
			buffer.Append(';');
			buffer.AppendLine();

			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"this.QueryHelper.Execute(query, sqlParams);");
			buffer.AppendLine();

			if (getNewId)
			{
				buffer.AppendLine();
				buffer.Append('\t');
				buffer.Append('\t');
				buffer.Append(@"item.Id = this.QueryHelper.GetNewId();");
				buffer.AppendLine();
			}

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();
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