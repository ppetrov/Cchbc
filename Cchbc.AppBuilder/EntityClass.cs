using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.Clr;

namespace Cchbc.AppBuilder
{
	public static class EntityClass
	{
		public static string Generate(Entity entity, bool readOnly)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			var buffer = new StringBuilder(1024);

			// Class definition
			var className = entity.Class.Name;
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"sealed");
			buffer.Append(' ');
			buffer.Append(@"class");
			buffer.Append(' ');
			buffer.Append(className);
			if (!readOnly)
			{
				buffer.Append(' ');
				buffer.Append(':');
				buffer.Append(' ');
				buffer.Append(@"IDbObject");
			}
			buffer.AppendLine();

			AppendOpenBrace(buffer);

			AppendClassProperties(buffer, entity.Class.Properties, readOnly);
			buffer.AppendLine();
			AppendClassConstructor(buffer, className, entity.Class.Properties);

			AppendCloseBrace(buffer);

			return buffer.ToString();
		}

		public static void AppendClassProperties(StringBuilder buffer, IEnumerable<ClrProperty> properties, bool readOnly, bool publicAccess = true)
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
				AppendIndentation(buffer, 1);
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

		public static void AppendClassConstructor(StringBuilder buffer, string className, ClrProperty[] properties)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (className == null) throw new ArgumentNullException(nameof(className));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

			// Constructor definition
			var level = 1;
			AppendIndentation(buffer, level);
			buffer.Append(@"public");
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
				var propertyType = property.Type.Name;
				buffer.Append(propertyType);
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, property.Name);
			}

			buffer.Append(')');
			buffer.AppendLine();

			AppendOpenBrace(buffer, level++);

			// ArgumentNullException checks for reference types
			foreach (var property in properties)
			{
				if (property.Type.IsReference)
				{
					AppendArgumentNullCheck(buffer, property.Name, level);
				}
			}

			// Add empty line if we have ArgumentNullException checks
			foreach (var property in properties)
			{
				if (property.Type.IsReference)
				{
					buffer.AppendLine();
					break;
				}
			}

			AssignPropertiesToParameters(buffer, properties, level);

			AppendCloseBrace(buffer, --level);
		}

		private static void AssignPropertiesToParameters(StringBuilder buffer, ClrProperty[] properties, int indentationLevel)
		{
			foreach (var property in properties)
			{
				var name = property.Name;

				AppendIndentation(buffer, indentationLevel);
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(name);
				buffer.Append(' ');
				buffer.Append('=');
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, name);
				buffer.Append(';');
				buffer.AppendLine();
			}
		}

		public static void AppendCreateNewInstance(StringBuilder buffer, string className, ClrProperty[] properties)
		{
			if (className == null) throw new ArgumentNullException(nameof(className));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

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

		public static void AppendVariableDeclaration(StringBuilder buffer, ClrProperty property, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (property == null) throw new ArgumentNullException(nameof(property));

			AppendVariableDeclaration(buffer, property.Name, string.Empty, property.Type, indentationLevel);
		}

		public static void AppendVariableDeclaration(StringBuilder buffer, string propertyName, string prefix, ClrType type, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (prefix == null) throw new ArgumentNullException(nameof(prefix));
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			if (type == null) throw new ArgumentNullException(nameof(type));

			AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"var");
			buffer.Append(' ');
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
			buffer.Append(TypeHelper.GetDefaultValue(type));
			buffer.Append(';');
			buffer.AppendLine();
		}

		public static void AppendArgumentNullCheck(StringBuilder buffer, string argumentName, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (argumentName == null) throw new ArgumentNullException(nameof(argumentName));

			AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"if");
			buffer.Append(' ');
			buffer.Append('(');
			BufferHelper.AppendLowerFirst(buffer, argumentName);
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append(@"null");
			buffer.Append(')');
			buffer.Append(' ');
			buffer.Append(@"throw");
			buffer.Append(' ');
			buffer.Append(@"new");
			buffer.Append(' ');
			buffer.Append(@"ArgumentNullException");
			buffer.Append('(');
			buffer.Append(@"nameof");
			buffer.Append('(');
			BufferHelper.AppendLowerFirst(buffer, argumentName);
			buffer.Append(')');
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();
		}

		public static void AppendCheckForDbNull(StringBuilder buffer, int index, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"if");
			buffer.Append(' ');
			buffer.Append('(');
			buffer.Append('!');
			buffer.Append('r');
			buffer.Append('.');
			buffer.Append(@"IsDbNull");
			buffer.Append('(');
			buffer.Append(index);
			buffer.Append(')');
			buffer.Append(')');
			buffer.AppendLine();
		}

		public static void AppendAssignValue(StringBuilder buffer, ClrProperty property, int index, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (property == null) throw new ArgumentNullException(nameof(property));

			AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"var");
			buffer.Append(' ');
			BufferHelper.AppendLowerFirst(buffer, property.Name);
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			AppendReadValue(buffer, TypeHelper.GetReaderMethod(ClrType.Long), index);
			buffer.Append(';');
			buffer.AppendLine();
		}

		public static void AppendAssignValue(StringBuilder buffer, ClrProperty property, int index, Dictionary<ClrType, ClrProperty> dictionaries, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (property == null) throw new ArgumentNullException(nameof(property));
			if (dictionaries == null) throw new ArgumentNullException(nameof(dictionaries));

			AppendAssignValue(buffer, property.Name, string.Empty, property.Type, index, dictionaries, indentationLevel);
		}

		public static void AppendAssignValue(StringBuilder buffer, string propertyName, string prefix, ClrType type, int index, Dictionary<ClrType, ClrProperty> dictionaries, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
			if (prefix == null) throw new ArgumentNullException(nameof(prefix));
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (dictionaries == null) throw new ArgumentNullException(nameof(dictionaries));

			var readerMethod = TypeHelper.GetReaderMethod(type);

			AppendIndentation(buffer, indentationLevel);
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

				string v = null;
				ClrProperty t;
				if (dictionaries.TryGetValue(type, out t))
				{
					v = t.Name;
				}

				buffer.Append(v ?? @"N/A");
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

		public static void AppendCreatorMethod(StringBuilder buffer, Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties, Entity[] readerEntities)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (entity == null) throw new ArgumentNullException(nameof(entity));
			if (dictionaryProperties == null) throw new ArgumentNullException(nameof(dictionaryProperties));
			if (readerEntities == null) throw new ArgumentNullException(nameof(readerEntities));

			if (readerEntities.Length == 1)
			{
				AppendCreatorMethod(buffer, entity, dictionaryProperties);
			}
			else
			{
				var level = 1;
				var masterClass = entity.Class;
				var masterClassName = masterClass.Name;
				var masterProperties = masterClass.Properties;

				AppendIndentation(buffer, level);

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

				AppendOpenBrace(buffer, level++);

				// Read PrimaryKey
				for (var index = 0; index < masterProperties.Length; index++)
				{
					var property = masterProperties[index];
					if (!property.Name.Equals(NameProvider.IdName))
					{
						continue;
					}
					AppendAssignValue(buffer, property, index, level);
				}

				buffer.AppendLine();
				AppendIndentation(buffer, level);
				buffer.Append(masterClassName);
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, masterClassName);
				buffer.Append(';');
				buffer.AppendLine();

				AppendIndentation(buffer, level);
				buffer.Append(@"if");
				buffer.Append(' ');
				buffer.Append('(');
				buffer.Append('!');
				BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
				buffer.Append('.');
				buffer.Append(@"TryGetValue");
				buffer.Append('(');
				BufferHelper.AppendLowerFirst(buffer, NameProvider.IdName);
				buffer.Append(',');
				buffer.Append(' ');
				buffer.Append(@"out");
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, masterClassName);
				buffer.Append(')');
				buffer.Append(')');
				buffer.AppendLine();

				AppendOpenBrace(buffer, level++);

				for (var index = 0; index < masterProperties.Length; index++)
				{
					var property = masterProperties[index];
					if (property.Name.Equals(NameProvider.IdName))
					{
						continue;
					}
					if (property.Type.IsCollection)
					{
						continue;
					}
					AppendVariableDeclaration(buffer, property, level);
					AppendCheckForDbNull(buffer, index, level);
					AppendOpenBrace(buffer, level);
					AppendAssignValue(buffer, property, index, dictionaryProperties, level + 1);
					AppendCloseBrace(buffer, level);
				}

				AppendIndentation(buffer, level);
				BufferHelper.AppendLowerFirst(buffer, masterClassName);
				buffer.Append(' ');
				buffer.Append('=');
				buffer.Append(' ');
				AppendCreateNewInstance(buffer, masterClassName, masterProperties);
				buffer.Append(';');
				buffer.AppendLine();

				AppendIndentation(buffer, level);
				BufferHelper.AppendLowerFirst(buffer, entity.Table.Name);
				buffer.Append('.');
				buffer.Append(@"Add");
				buffer.Append('(');
				BufferHelper.AppendLowerFirst(buffer, NameProvider.IdName);
				buffer.Append(',');
				buffer.Append(' ');
				BufferHelper.AppendLowerFirst(buffer, masterClassName);
				buffer.Append(')');
				buffer.Append(';');
				buffer.AppendLine();

				AppendCloseBrace(buffer, --level);

				var offset = entity.Table.Columns.Length;
				var detailEntity = readerEntities[1];
				var detailClass = detailEntity.Class;
				var detailClassName = detailClass.Name;
				var detailProperties = detailClass.Properties;

				for (var i = 0; i < detailProperties.Length; i++)
				{
					var index = i + offset;
					var property = detailProperties[i];
					if (property.Name == masterClassName)
					{
						continue;
					}
					var propertyName = property.Name;
					AppendVariableDeclaration(buffer, propertyName, detailClassName, property.Type, level);
					AppendCheckForDbNull(buffer, index, level);
					AppendOpenBrace(buffer, level);
					AppendAssignValue(buffer, propertyName, detailClassName, property.Type, index, dictionaryProperties, level + 1);
					AppendCloseBrace(buffer, level);
				}

				buffer.AppendLine();

				AppendIndentation(buffer, level);
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

				AppendIndentation(buffer, level);
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

				AppendCloseBrace(buffer, --level);
			}
		}

		private static void AppendCreatorMethod(StringBuilder buffer, Entity entity, Dictionary<ClrType, ClrProperty> dictionaryProperties)
		{
			var level = 1;
			var className = entity.Class.Name;
			var properties = entity.Class.Properties;

			AppendIndentation(buffer, level);

			buffer.Append(@"private");
			buffer.Append(' ');
			buffer.Append(className);
			buffer.Append(' ');
			buffer.Append(className);
			buffer.Append(@"Creator");
			buffer.Append('(');
			buffer.Append(@"IFieldDataReader");
			buffer.Append(' ');
			buffer.Append('r');
			buffer.Append(')');
			buffer.AppendLine();

			AppendOpenBrace(buffer, level++);

			// Read properties
			for (var index = 0; index < properties.Length; index++)
			{
				var property = properties[index];

				if (property.Name.Equals(NameProvider.IdName))
				{
					AppendAssignValue(buffer, property, index, level);
				}
				else
				{
					AppendVariableDeclaration(buffer, property, level);
					AppendCheckForDbNull(buffer, index, level);
					AppendOpenBrace(buffer, level);
					AppendAssignValue(buffer, property, index, dictionaryProperties, level + 1);
					AppendCloseBrace(buffer, level);
				}
			}

			// Create instance & return the value;
			buffer.AppendLine();
			AppendIndentation(buffer, level);
			buffer.Append(@"return");
			buffer.Append(' ');
			AppendCreateNewInstance(buffer, className, properties);
			buffer.Append(';');
			buffer.AppendLine();

			AppendCloseBrace(buffer, --level);
		}

		public static void AppendOpenBrace(StringBuilder buffer, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			AppendBrace(buffer, indentationLevel, '{');
		}

		public static void AppendCloseBrace(StringBuilder buffer, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			AppendBrace(buffer, indentationLevel, '}');
		}

		public static void AppendIndentation(StringBuilder buffer, int indentationLevel)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			for (var i = 0; i < indentationLevel; i++)
			{
				buffer.Append('\t');
			}
		}

		private static void AppendBrace(StringBuilder buffer, int indentationLevel, char symbol)
		{
			AppendIndentation(buffer, indentationLevel);

			buffer.Append(symbol);
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
	}
}