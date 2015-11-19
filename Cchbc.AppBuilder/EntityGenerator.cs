using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.Clr;
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
			buffer.AppendLine(@class.Name);

			buffer.Append('{');
			buffer.AppendLine();

			AppendClassProperties(buffer, @class.Properties, readOnly);
			buffer.AppendLine();
			AppendClassConstructor(buffer, @class);

			buffer.Append('}');
			buffer.AppendLine();

			return buffer.ToString();
		}

		public static string CreateEntityAdapter(Entity entity, NameProvider nameProvider, bool readOnly, bool includeDictionaries)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			if (readOnly)
			{
				return ReadOnlyAdapter(entity);
			}
			return ModifiableAdapter(entity, nameProvider, includeDictionaries);
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

		private static string ReadOnlyAdapter(Entity entity)
		{
			var buffer = new StringBuilder(2 * 1024);

			var className = entity.Class.Name;
			buffer.AppendFormat(@"public sealed class {0}Adapter : IReadOnlyAdapter<{0}>", className);
			buffer.AppendLine();

			buffer.Append('{');
			buffer.AppendLine();

			var dictionaryProperties = GetDictionaryProperties(entity);

			var i = 0;
			var classProperties = new ClrProperty[dictionaryProperties.Count + 1];
			classProperties[i++] = new ClrProperty(@"QueryHelper", new ClrType(@"ReadQueryHelper", true));
			foreach (var classProperty in dictionaryProperties.Values)
			{
				classProperties[i++] = classProperty;
			}

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

		private static string ModifiableAdapter(Entity entity, NameProvider nameProvider, bool includeDictionaries)
		{
			var buffer = new StringBuilder(2 * 1024);

			var className = entity.Class.Name;
			buffer.AppendFormat(@"public sealed class {0}Adapter : IModifiableAdapter<{0}>", className);
			buffer.AppendLine();

			buffer.Append('{');
			buffer.AppendLine();

			var classProperties = new[]
			{
				new ClrProperty(@"QueryHelper", new ClrType(@"QueryHelper", true))
			};

			if (includeDictionaries)
			{
				var properties = new List<ClrProperty>(classProperties);
				properties.AddRange(GetDictionaryProperties(entity).Values);

				classProperties = properties.ToArray();
			}

			var adapterClass = new ClrClass(className + @"Adapter", classProperties);

			AppendClassProperties(buffer, adapterClass.Properties, true, false);
			buffer.AppendLine();

			AppendClassConstructor(buffer, adapterClass);
			buffer.AppendLine();

			// TODO : !!!
			//AppendGetAllMethod(buffer, entity);
			//buffer.AppendLine();

			AppendInsertMethod(buffer, entity);
			buffer.AppendLine();
			AppendUpdateMethod(buffer, entity);
			buffer.AppendLine();
			AppendDeleteMethod(buffer, entity);
			buffer.AppendLine();

			buffer.Append('}');
			buffer.AppendLine();

			return buffer.ToString();
		}

		//private static void AppendGetAllMethod(StringBuilder buffer, Entity entity)
		//{
		//	buffer.Append('\t');
		//	buffer.AppendLine(@"//GET ALL !!!");
		//}

		private static void AppendInsertMethod(StringBuilder buffer, Entity entity)
		{
			var className = entity.Class.Name;

			buffer.Append('\t');
			buffer.Append(@"public Task ");
			buffer.AppendFormat(@"InsertAsync({0} item)", className);
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
			buffer.Append(@"throw new NotImplementedException();");
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();
		}

		private static void AppendUpdateMethod(StringBuilder buffer, Entity entity)
		{
			var className = entity.Class.Name;

			buffer.Append('\t');
			buffer.Append(@"public Task ");
			buffer.AppendFormat(@"UpdateAsync({0} item)", className);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"throw new NotImplementedException();");
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();
		}

		private static void AppendDeleteMethod(StringBuilder buffer, Entity entity)
		{
			var className = entity.Class.Name;

			buffer.Append('\t');
			buffer.Append(@"public Task ");
			buffer.AppendFormat(@"DeleteAsync({0} item)", className);
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('\t');
			buffer.Append(@"throw new NotImplementedException();");
			buffer.AppendLine();

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
			buffer.AppendFormat(@"public Task FillAsync(Dictionary<long, {0}> items, Func<{0}, long> selector)", @class.Name);
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
			buffer.Append(@"return this.QueryHelper.FillAsync(new Query<");
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

				// Close brace
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