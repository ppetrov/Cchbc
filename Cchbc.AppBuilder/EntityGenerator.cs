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
		public static string CreateEntityClass(Entity entity, bool readOnly)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			var buffer = new StringBuilder(1024);

			var @class = entity.Class;

			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"sealed");
			buffer.Append(' ');
			buffer.Append(@"class");
			buffer.Append(' ');
			buffer.Append(@class.Name);
			if (!readOnly)
			{
				buffer.Append(' ');
				buffer.Append(':');
				buffer.Append(' ');
				buffer.Append(@"IDbObject");
			}
			buffer.AppendLine();

			buffer.Append('{');
			buffer.AppendLine();

			AppendClassProperties(buffer, @class.Properties, readOnly);
			buffer.AppendLine();
			AppendClassConstructor(buffer, @class);

			buffer.Append('}');
			buffer.AppendLine();

			return buffer.ToString();
		}

		private static void AppendClassProperties(StringBuilder buffer, ClrProperty[] properties, bool readOnly, bool publicAccess = true)
		{
			var propertyAccess = @"get;";
			if (!readOnly)
			{
				propertyAccess = @"get; set;";
			}
			var accessModifier = @"public";
			if (!publicAccess)
			{
				accessModifier = @"private";
			}
			foreach (var property in properties)
			{
				buffer.Append('\t');
				buffer.Append(accessModifier);
				buffer.Append(' ');
				buffer.Append(property.Type.Name);
				buffer.Append(' ');
				buffer.Append(property.Name);
				buffer.Append(' ');
				buffer.Append('{');
				buffer.Append(' ');
				buffer.Append(propertyAccess);
				buffer.Append(' ');
				buffer.Append('}');
				buffer.AppendLine();
			}
		}

		private static void AppendClassConstructor(StringBuilder buffer, ClrClass @class)
		{
			var properties = @class.Properties;

			buffer.Append('\t');
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@class.Name);
			buffer.Append('(');

			var parameterNames = new string[properties.Length];
			for (var i = 0; i < properties.Length; i++)
			{
				parameterNames[i] = NameProvider.LowerFirst(properties[i].Name);
			}

			for (var i = 0; i < properties.Length; i++)
			{
				var propertyType = properties[i].Type.Name;
				var parameterName = parameterNames[i];

				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}

				buffer.Append(propertyType);
				buffer.Append(' ');
				buffer.Append(parameterName);
			}

			buffer.Append(')');
			buffer.AppendLine();
			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			var addEmptyLine = false;
			for (var i = 0; i < properties.Length; i++)
			{
				var property = properties[i];
				if (property.Type.IsReference)
				{
					addEmptyLine = true;

					var name = parameterNames[i];
					buffer.Append('\t');
					buffer.Append('\t');
					buffer.AppendFormat(@"if ({0} == null) throw new ArgumentNullException(nameof({0}));", name);
					buffer.AppendLine();
				}
			}

			// Add empty line if we have argument null checks
			if (addEmptyLine)
			{
				buffer.AppendLine();
			}

			// Assign properties to parameters
			for (int i = 0; i < properties.Length; i++)
			{
				var propertyName = properties[i].Name;
				var paramterName = parameterNames[i];

				buffer.Append('\t');
				buffer.Append('\t');
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(propertyName);
				buffer.Append(@" = ");
				buffer.Append(paramterName);
				buffer.Append(';');
				buffer.AppendLine();
			}

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();
		}

		public static string ReadOnlyAdapter(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));

			var buffer = new StringBuilder(2 * 1024);

			var className = entity.Class.Name;
			buffer.AppendFormat(@"public sealed class {0}Adapter : IReadOnlyAdapter<{0}>", className);
			buffer.AppendLine();

			buffer.Append('{');
			buffer.AppendLine();

			var classProperties = GetClassProperties(dictionaryProperties);

			var adapterClass = new ClrClass(className + @"Adapter", classProperties);

			AppendClassProperties(buffer, adapterClass.Properties, true, false);
			buffer.AppendLine();

			AppendClassConstructor(buffer, adapterClass);
			buffer.AppendLine();

			AppendFillMethod(buffer, entity, dictionaryProperties);
			buffer.AppendLine();

			buffer.Append('}');
			buffer.AppendLine();

			return buffer.ToString();
		}

		public static string ModifiableAdapter(Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, bool generateGetMethod)
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

			AppendClassProperties(buffer, adapterClass.Properties, true, false);
			buffer.AppendLine();

			AppendClassConstructor(buffer, adapterClass);
			buffer.AppendLine();

			if (generateGetMethod)
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
				parameters[i] = NameProvider.LowerFirst(properties[i].Name);
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
			QueryBuilder.AppendSelect(buffer, entity.Table);
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
				buffer.Append(GetDefaultValue(type));
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
					;
					//buffer.Append(dictionaries[type].Name);
					buffer.Append(tmp);
					buffer.Append('[');
				}
				buffer.Append(@"r.");
				buffer.Append(GetReaderMethod(type));
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

		private static void AppendFillMethod(StringBuilder buffer, Entity entity, Dictionary<ClrType, ClrProperty> dictionaries)
		{
			var @class = entity.Class;
			var properties = @class.Properties;
			var parameters = new string[properties.Length];
			for (var i = 0; i < properties.Length; i++)
			{
				parameters[i] = NameProvider.LowerFirst(properties[i].Name);
			}

			buffer.Append('\t');
			buffer.AppendFormat(@"public void Fill(Dictionary<long, {0}> items, Func<{0}, long> selector)", @class.Name);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"if (items == null) throw new ArgumentNullException(nameof(items));");
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"if (selector == null) throw new ArgumentNullException(nameof(selector));");
			buffer.AppendLine();

			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"this");
			buffer.Append('.');
			buffer.Append(@"QueryHelper");
			buffer.Append('.');
			buffer.Append(@"Fill(new Query<");
			buffer.Append(@class.Name);
			buffer.Append(">(@");
			buffer.Append('"');
			QueryBuilder.AppendSelect(buffer, entity.Table);
			buffer.Append('"');
			buffer.Append(@", r =>");
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
				buffer.Append(GetDefaultValue(type));
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
					buffer.Append(dictionaries[type].Name);
					buffer.Append('[');
				}
				buffer.Append(@"r.");
				buffer.Append(GetReaderMethod(type));
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
			buffer.Append(@"), items, selector);");
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('}');
		}

		private static ClrProperty[] GetClassProperties(Dictionary<ClrType, ClrProperty> dictionaryProperties)
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

		private static string GetReaderMethod(ClrType type)
		{
			if (type == ClrType.Long) return @"GetInt64";
			if (type == ClrType.String) return @"GetString";
			if (type == ClrType.Decimal) return @"GetDecimal";
			if (type == ClrType.DateTime) return @"GetDateTime";
			if (type == ClrType.Bytes) return @"GetBytes";

			return @"GetInt64";
		}

		private static string GetDefaultValue(ClrType type)
		{
			if (type == ClrType.Long) return @"0L";
			if (type == ClrType.String) return @"string.Empty";
			if (type == ClrType.Decimal) return @"0M";
			if (type == ClrType.DateTime) return @"DateTime.MinValue";
			if (type == ClrType.Bytes) return @"default(byte[])";

			return $@"default({type.Name})";
		}
	}
}