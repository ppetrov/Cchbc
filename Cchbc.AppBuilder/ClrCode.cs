using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.Clr;
using Cchbc.AppBuilder.DML;

namespace Cchbc.AppBuilder
{
	public static class ClrCode
	{
		private sealed class ClrField
		{
			public string Type { get; }
			public string ParameterName { get; }
			public ClrType UserType { get; }

			public ClrField(string type, string name, ClrType userType = null)
			{
				if (name == null) throw new ArgumentNullException(nameof(name));
				if (type == null) throw new ArgumentNullException(nameof(type));

				this.Type = type;
				this.ParameterName = NameProvider.LowerFirst(name);
				this.UserType = userType;
			}
		}

		public static string Class(Entity entity)
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

			AppendProperties(buffer, @class.Properties);
			buffer.AppendLine();
			AppendConstructor(buffer, @class);

			buffer.Append('}');
			buffer.AppendLine();

			return buffer.ToString();
		}

		public static string ReadOnlyAdapter(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			var buffer = new StringBuilder(2 * 1024);

			buffer.Append(@"public sealed class ");
			buffer.Append(entity.Class.Name);
			buffer.Append(@"Adapter : IReadOnlyAdapter<");
			buffer.Append(entity.Class.Name);
			buffer.Append('>');
			buffer.AppendLine();

			buffer.Append('{');
			buffer.AppendLine();

			var fields = new List<ClrField> { new ClrField(@"ReadQueryHelper", @"queryHelper"), };
			foreach (var p in entity.Class.Properties)
			{
				var type = p.Type;
				if (type.IsUserType)
				{
					fields.Add(new ClrField($@"Dictionary<long, {type.Name}>", type.Name + @"s", type));
				}
			}

			// Add fields
			foreach (var f in fields)
			{
				buffer.Append('\t');
				buffer.Append(@"private readonly ");
				buffer.Append(f.Type);
				buffer.Append(@" _");
				buffer.Append(f.ParameterName);
				buffer.Append(';');
				buffer.AppendLine();
			}

			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append(@"public ");
			buffer.Append(entity.Class.Name);
			buffer.Append(@"Adapter");
			buffer.Append('(');
			for (var i = 0; i < fields.Count; i++)
			{
				var f = fields[i];
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				buffer.Append(f.Type);
				buffer.Append(' ');
				buffer.Append(f.ParameterName);
			}
			buffer.Append(')');
			buffer.AppendLine();

			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			// Add ArgumentNullException checks
			foreach (var f in fields)
			{
				buffer.Append('\t');
				buffer.Append('\t');
				buffer.AppendFormat(@"if ({0} == null) throw new ArgumentNullException(nameof({0}));", f.ParameterName);
				buffer.AppendLine();
			}

			// Add separator
			buffer.AppendLine();

			// Add fields assignments
			foreach (var f in fields)
			{
				buffer.Append('\t');
				buffer.Append('\t');
				buffer.Append('_');
				buffer.Append(f.ParameterName);
				buffer.Append(@" = ");
				buffer.Append(f.ParameterName);
				buffer.Append(';');
				buffer.AppendLine();
			}

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();

			buffer.Append(AppendFillMethod(entity, fields));
			buffer.AppendLine();

			buffer.Append('}');
			buffer.AppendLine();

			return buffer.ToString();
		}

		private static string AppendFillMethod(Entity entity, List<ClrField> fields)
		{
			var buffer = new StringBuilder(512);

			var @class = entity.Class;
			var properties = @class.Properties;

			for (var i = 0; i < properties.Length; i++)
			{
				var p = properties[i];

				var type = p.Type;
				var name = p.ParameterName;

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
					foreach (var f in fields)
					{
						if (f.UserType == type)
						{
							buffer.Append('_');
							buffer.Append(f.ParameterName);
							buffer.Append('[');
							break;
						}
					}
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
			for (var i = 0; i < properties.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				buffer.Append(properties[i].ParameterName);
			}
			buffer.Append(')');
			buffer.Append(';');

			var template = @"
	public Task FillAsync(Dictionary<long, {0}> items, Func<{0}, long> selector)
	{{
		if (items == null) throw new ArgumentNullException(nameof(items));
		if (selector == null) throw new ArgumentNullException(nameof(selector));

		return _queryHelper.FillAsync(new Query<{0}>(@""{1}"", r =>
		{{
{2}
		}}), items, selector);
	}}";

			return string.Format(template, @class.Name, QueryCreator.GetSelect(entity.Table), buffer);
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

		private static void AppendProperties(StringBuilder buffer, ClrProperty[] properties)
		{
			foreach (var property in properties)
			{
				buffer.Append('\t');
				buffer.Append(@"public");
				buffer.Append(' ');
				buffer.Append(property.Type.Name);
				buffer.Append(' ');
				buffer.Append(property.Name);
				buffer.Append(' ');
				buffer.Append('{');
				buffer.Append(' ');
				buffer.Append(@"get");
				buffer.Append(';');
				buffer.Append(' ');
				buffer.Append('}');
				buffer.AppendLine();
			}
		}

		private static void AppendConstructor(StringBuilder buffer, ClrClass @class)
		{
			var properties = @class.Properties;

			buffer.Append('\t');
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@class.Name);
			buffer.Append('(');
			for (var i = 0; i < properties.Length; i++)
			{
				var p = properties[i];
				if (i > 0)
				{
					buffer.Append(',');
					buffer.Append(' ');
				}
				buffer.Append(p.Type.Name);
				buffer.Append(' ');
				buffer.Append(p.ParameterName);
			}
			buffer.Append(')');
			buffer.AppendLine();
			buffer.Append('\t');
			buffer.Append('{');
			buffer.AppendLine();

			var addEmptyLine = false;
			foreach (var p in properties)
			{
				if (p.Type.IsReference)
				{
					addEmptyLine = true;

					var name = p.ParameterName;

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
			foreach (var property in properties)
			{
				buffer.Append('\t');
				buffer.Append('\t');
				buffer.Append(@"this");
				buffer.Append('.');
				buffer.Append(property.Name);
				buffer.Append(@" = ");
				buffer.Append(property.ParameterName);
				buffer.Append(';');
				buffer.AppendLine();
			}

			buffer.Append('\t');
			buffer.Append('}');
			buffer.AppendLine();
		}
	}
}