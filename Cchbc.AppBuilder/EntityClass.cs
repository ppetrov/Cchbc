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
			var cls = entity.Class;
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"sealed");
			buffer.Append(' ');
			buffer.Append(@"class");
			buffer.Append(' ');
			buffer.Append(cls.Name);
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

			// Class properties
			AppendClassProperties(buffer, cls.Properties, readOnly);
			buffer.AppendLine();

			// Class contructor
			AppendClassConstructor(buffer, cls.Name, cls.Properties);

			buffer.Append('}');
			buffer.AppendLine();

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

		public static void AppendClassConstructor(StringBuilder buffer, string className, ClrProperty[] properties)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (className == null) throw new ArgumentNullException(nameof(className));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

			// Constructor definition
			buffer.Append('\t');
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

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			// ArgumentNullException checks for reference types
			foreach (var property in properties)
			{
				if (property.Type.IsReference)
				{
					AppendArgumentNullCheck(buffer, property.Name, 2);
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

			// Assign properties to parameters
			foreach (var property in properties)
			{
				var name = property.Name;

				buffer.Append('\t');
				buffer.Append('\t');
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(name);
				buffer.Append(@" = ");
				BufferHelper.AppendLowerFirst(buffer, name);
				buffer.Append(';');
				buffer.AppendLine();
			}

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();
		}

		public static void AppendArgumentNullCheck(StringBuilder buffer, string name, int indentationLevel = 0)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (name == null) throw new ArgumentNullException(nameof(name));

			AppendIndentation(buffer, indentationLevel);

			buffer.Append(@"if");
			buffer.Append(' ');
			buffer.Append('(');
			BufferHelper.AppendLowerFirst(buffer, name);
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append('=');
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
			BufferHelper.AppendLowerFirst(buffer, name);
			buffer.Append(')');
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();
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

		private static void AppendBrace(StringBuilder buffer, int indentationLevel, char symbol)
		{
			AppendIndentation(buffer, indentationLevel);

			buffer.Append(symbol);
			buffer.AppendLine();
		}

		public static void AppendIndentation(StringBuilder buffer, int indentationLevel)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			for (var i = 0; i < indentationLevel; i++)
			{
				buffer.Append('\t');
			}
		}
	}
}