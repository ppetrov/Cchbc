using System;
using System.Linq;
using System.Text;
using Cchbc.Db.DDL;
using Cchbc.Db.DML;

namespace Cchbc.Db
{
	public static class ClrGenerator
	{
		private sealed class ClrType
		{
			public static readonly ClrType Long = new ClrType(@"long");
			public static readonly ClrType Decimal = new ClrType(@"decimal");
			public static readonly ClrType String = new ClrType(@"string");
			public static readonly ClrType DateTime = new ClrType(@"DateTime");
			public static readonly ClrType Bytes = new ClrType(@"byte[]");

			public string Name { get; }
			public bool IsReference => this == String || this == Bytes;

			public ClrType(string name)
			{
				if (name == null) throw new ArgumentNullException(nameof(name));

				this.Name = name;
			}
		}

		private sealed class ClrProperty
		{
			public string Name { get; }
			public ClrType Type { get; }
			public string ParameterName { get; }
			public bool IsReference { get; }

			public ClrProperty(string name, ClrType type, string parameterName, bool isReference)
			{
				this.Name = name;
				this.Type = type;
				this.ParameterName = parameterName;
				this.IsReference = isReference;
			}
		}

		public static string Class(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var buffer = new StringBuilder();

			buffer.Append(@"public sealed class ");
			buffer.Append(table.ClassName);
			buffer.AppendLine(@" : IDbObject");

			buffer.AppendLine(@"{");

			var properties = GetProperties(table.Columns);
			AppendProperties(properties, buffer);
			AppendConstructor(table, buffer, properties);

			buffer.AppendLine(@"}");

			return buffer.ToString();
		}

		public static string ReadAdapter(DbTable table)
		{
			if (table == null) throw new ArgumentNullException(nameof(table));

			var template = @"public sealed class {0}Adapter : IReadOnlyAdapter<{0}>
{{
	public ReadQueryHelper QueryHelper {{ get; }}

	public {0}Adapter(ReadQueryHelper queryHelper)
	{{
		if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

		this.QueryHelper = queryHelper;
	}}

	public Task FillAsync(Dictionary<long, {0}> items)
	{{
		if (items == null) throw new ArgumentNullException(nameof(items));

		return this.QueryHelper.ExecuteAsync(new Query<{0}>(@""{1}"", r =>
		{{
{2}
		}}));
	}}
}}";

			var buffer = new StringBuilder();


			var properties = GetProperties(table.Columns);
			var varNames = new string[properties.Length];
			for (var i = 0; i < properties.Length; i++)
			{
				var p = properties[i];
				var type = p.Type;
				var varName = varNames[i] = properties[i].ParameterName;

				if (i > 0)
				{
					buffer.AppendLine();
				}
				// variable declaration
				buffer.Append("\t\t\t");
				buffer.Append(@"var ");
				buffer.Append(varName);
				buffer.Append(@" = ");
				buffer.Append(GetDefaultValue(type));
				buffer.AppendLine(@";");

				// check for NULL
				buffer.Append("\t\t\t");
				buffer.Append(@"if (!r.IsDbNull(");
				buffer.Append(i);
				buffer.AppendLine(@"))");

				// Open brace
				buffer.Append("\t\t\t");
				buffer.AppendLine(@"{");

				// Read the value from the reader and assign to variable

				// TODO : Handle values that need to be mapped via dictionaries !!!!
				buffer.Append("\t\t\t");
				buffer.Append('\t');
				buffer.Append(varName);
				buffer.Append(@" = ");
				buffer.Append(@"r.");
				buffer.Append(GetReaderMethod(type));
				buffer.Append('(');
				buffer.Append(i);
				buffer.Append(')');
				buffer.AppendLine(@";");

				// Close brace
				buffer.Append("\t\t\t");
				buffer.AppendLine(@"}");
			}

			// Create instance & return the value;
			buffer.AppendLine();
			buffer.Append("\t\t\t");
			buffer.Append(@"return new ");
			buffer.Append(table.ClassName);
			buffer.Append('(');
			for (var i = 0; i < varNames.Length; i++)
			{
				if (i > 0)
				{
					buffer.Append(@", ");
				}
				buffer.Append(varNames[i]);
			}
			buffer.Append(')');
			buffer.Append(';');

			return string.Format(template, table.ClassName, QueryCreator.GetSelect(table), buffer);
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

		private static void AppendConstructor(DbTable table, StringBuilder buffer, ClrProperty[] properties)
		{
			buffer.AppendLine();
			buffer.Append('\t');
			buffer.Append(@"public ");
			buffer.Append(table.ClassName);
			buffer.Append('(');
			buffer.Append(string.Join(@", ", properties.Select(p => p.Type.Name + " " + p.ParameterName)));
			buffer.Append(')');
			buffer.AppendLine();
			buffer.Append('\t');
			buffer.AppendLine(@"{");

			var addEmptyLine = false;
			foreach (var p in properties)
			{
				if (p.IsReference)
				{
					addEmptyLine = true;
					var name = p.ParameterName;
					buffer.Append('\t');
					buffer.Append('\t');
					buffer.AppendLine($@"if ({name} == null) throw new ArgumentNullException(nameof({name}));");
				}
			}

			if (addEmptyLine)
			{
				buffer.AppendLine();
			}

			foreach (var p in properties)
			{
				buffer.Append('\t');
				buffer.Append('\t');
				buffer.Append(@"this.");
				buffer.Append(p.Name);
				buffer.Append(@" = ");
				buffer.Append(p.ParameterName);
				buffer.AppendLine(@";");
			}

			buffer.Append('\t');
			buffer.AppendLine(@"}");
		}

		private static void AppendProperties(ClrProperty[] properties, StringBuilder buffer)
		{
			foreach (var p in properties)
			{
				buffer.Append('\t');
				buffer.Append(@"public ");
				buffer.Append(p.Type.Name);
				buffer.Append(@" ");
				buffer.Append(p.Name);
				buffer.AppendLine(p.Name == NameProvider.IdName ? @" { get; set;}" : @" { get; }");
			}
		}

		private static ClrProperty[] GetProperties(DbColumn[] columns)
		{
			var properties = new ClrProperty[columns.Length];

			for (var i = 0; i < columns.Length; i++)
			{
				var column = columns[i];

				ClrProperty clrProperty;
				if (column.DbForeignKey == null)
				{
					var clrType = GetClrType(column.Type);
					clrProperty = new ClrProperty(column.Name, clrType, NameProvider.LowerFirst(column.Name), clrType.IsReference);
				}
				else
				{
					var fkColumn = column.DbForeignKey.Column;
					var name = fkColumn.Substring(0, fkColumn.Length - NameProvider.IdName.Length);
					clrProperty = new ClrProperty(name, new ClrType(name), NameProvider.LowerFirst(name), true);
				}

				properties[i] = clrProperty;
			}

			return properties;
		}

		private static ClrType GetClrType(DbColumnType type)
		{
			if (type == DbColumnType.Integer) return ClrType.Long;
			if (type == DbColumnType.String) return ClrType.String;
			if (type == DbColumnType.Decimal) return ClrType.Decimal;
			if (type == DbColumnType.DateTime) return ClrType.DateTime;
			if (type == DbColumnType.Bytes) return ClrType.Bytes;

			throw new ArgumentOutOfRangeException(nameof(type));
		}
	}
}