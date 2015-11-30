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

			var indentationLevel = 0;
			var clrClass = entity.Class;
			var className = clrClass.Name;

			AppendClassDefinition(buffer, className, readOnly ? string.Empty : @"IDbObject", string.Empty);
			AppendOpenBrace(buffer, indentationLevel++);

			AppendClassProperties(buffer, clrClass.Properties, indentationLevel, readOnly);
			buffer.AppendLine();

			AppendClassConstructor(buffer, className, clrClass.Properties, indentationLevel);
			AppendCloseBrace(buffer, --indentationLevel);

			return buffer.ToString();
		}

		public static string GenerateViewModel(Entity entity, bool readOnly)
		{
			if (readOnly)
			{
				var indentationLevel = 0;
				var buffer = new StringBuilder(1024);
				var name = entity.Class.Name;

				var suffix = @"ViewModel";
				AppendClassDefinition(buffer, name + suffix, suffix, name);
				AppendOpenBrace(buffer, indentationLevel++);

				AppendIndentation(buffer, indentationLevel);
				buffer.Append(@"public");
				buffer.Append(' ');
				buffer.Append(name);
				buffer.Append(suffix);
				buffer.Append('(');
				AppendParametersWithType(buffer, new[] { new ClrProperty(name, new ClrType(name, true, false)) });
				buffer.Append(')');
				buffer.Append(' ');
				buffer.Append(':');
				buffer.Append(' ');
				buffer.Append(@"base");
				buffer.Append('(');
				BufferHelper.AppendLowerFirst(buffer, name);
				buffer.Append(')');
				buffer.AppendLine();

				AppendOpenBrace(buffer, indentationLevel++);
				buffer.AppendLine();
				AppendCloseBrace(buffer, --indentationLevel);

				AppendCloseBrace(buffer, --indentationLevel);

				return buffer.ToString();
			}

			throw new NotImplementedException(@"Create ViewModel for Modifiable table");
		}

		public static string GenerateClassModule(Entity entity, bool readOnly)
		{
			if (readOnly)
			{
				var indentationLevel = 0;
				var buffer = new StringBuilder(1024);
				var name = entity.Class.Name;

				var className = entity.Table.Name + @"ReadOnlyModule";
				AppendClassDefinition(buffer, className, @"ReadOnlyModule", name + ", " + name + @"ViewModel");
				AppendOpenBrace(buffer, indentationLevel++);

				AppendIndentation(buffer, indentationLevel++);
				buffer.Append(@"public");
				buffer.Append(' ');
				buffer.Append(className);
				buffer.Append('(');
				buffer.AppendLine();

				AppendIndentation(buffer, indentationLevel);
				buffer.AppendFormat(@"Sorter<{0}ViewModel> sorter,", name);
				buffer.AppendLine();

				AppendIndentation(buffer, indentationLevel);
				buffer.AppendFormat(@"Searcher<{0}ViewModel> searcher,", name);
				buffer.AppendLine();

				AppendIndentation(buffer, indentationLevel);
				buffer.AppendFormat(@"FilterOption<{0}ViewModel>[] filterOptions = null)", name);
				buffer.AppendLine();

				AppendIndentation(buffer, indentationLevel);
				buffer.Append(@": base(sorter, searcher, filterOptions)");
				buffer.AppendLine();

				AppendOpenBrace(buffer, indentationLevel - 1);
				AppendCloseBrace(buffer, --indentationLevel);

				AppendCloseBrace(buffer, --indentationLevel);

				return buffer.ToString();
			}

			throw new NotImplementedException(@"Create ViewModel for Modifiable table");
		}

		public static void AppendClassDefinition(StringBuilder buffer, string className, string baseClass, string baseClassGnericType)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (className == null) throw new ArgumentNullException(nameof(className));
			if (baseClass == null) throw new ArgumentNullException(nameof(baseClass));
			if (baseClassGnericType == null) throw new ArgumentNullException(nameof(baseClassGnericType));

			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"sealed");
			buffer.Append(' ');
			buffer.Append(@"class");
			buffer.Append(' ');
			buffer.Append(className);

			if (baseClass != string.Empty)
			{
				buffer.Append(' ');
				buffer.Append(':');
				buffer.Append(' ');
				buffer.Append(baseClass);

				if (baseClassGnericType != string.Empty)
				{
					buffer.Append('<');
					buffer.Append(baseClassGnericType);
					buffer.Append('>');
				}
			}

			buffer.AppendLine();
		}

		public static void AppendClassProperties(StringBuilder buffer, IEnumerable<ClrProperty> properties, int indentationLevel, bool readOnly, bool publicAccess = true)
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
				AppendIndentation(buffer, indentationLevel);

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

		public static void AppendClassConstructor(StringBuilder buffer, string className, ClrProperty[] properties, int indentationLevel)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (className == null) throw new ArgumentNullException(nameof(className));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

			AppendIndentation(buffer, indentationLevel);
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(className);
			buffer.Append('(');
			AppendParametersWithType(buffer, properties);
			buffer.Append(')');
			buffer.AppendLine();

			AppendOpenBrace(buffer, indentationLevel++);

			var appendEmptyLine = false;
			foreach (var property in properties)
			{
				if (property.Type.IsReference)
				{
					AppendArgumentNullCheck(buffer, property.Name, indentationLevel);
					appendEmptyLine = true;
				}
			}
			if (appendEmptyLine)
			{
				buffer.AppendLine();
			}

			AppendAssignPropertiesToParameters(buffer, properties, indentationLevel);

			AppendCloseBrace(buffer, --indentationLevel);
		}

		public static void AppendArgumentNullCheck(StringBuilder buffer, string argumentName, int indentationLevel)
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

		public static void AppendOpenBrace(StringBuilder buffer, int indentationLevel)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));

			AppendBrace(buffer, indentationLevel, '{');
		}

		public static void AppendCloseBrace(StringBuilder buffer, int indentationLevel)
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

		private static void AppendAssignPropertiesToParameters(StringBuilder buffer, ClrProperty[] properties, int indentationLevel)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

			foreach (var property in properties)
			{
				AppendIndentation(buffer, indentationLevel);

				var name = property.Name;
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

		private static void AppendParametersWithType(StringBuilder buffer, ClrProperty[] properties)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (properties == null) throw new ArgumentNullException(nameof(properties));

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
		}
	}
}