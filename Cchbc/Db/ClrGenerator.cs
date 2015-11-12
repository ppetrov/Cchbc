using System;
using System.Linq;
using System.Text;
using Cchbc.Db.DDL;

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

			private ClrType(string name)
			{
				if (name == null) throw new ArgumentNullException(nameof(name));

				this.Name = name;
			}
		}

		private sealed class ClrProperty
		{
			public string Name { get; }
			public string Type { get; }
			public string ParameterName { get; }
			public bool IsReference { get; }

			public ClrProperty(string name, string type, string parameterName, bool isReference)
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

			//ReadQueryHelper
			// TODO : !!!
			return "";
		}

		private static void AppendConstructor(DbTable table, StringBuilder buffer, ClrProperty[] properties)
		{
			buffer.AppendLine();
			buffer.Append('\t');
			buffer.Append(@"public ");
			buffer.Append(table.ClassName);
			buffer.Append('(');
			buffer.Append(string.Join(@", ", properties.Select(p => p.Type + " " + p.ParameterName)));
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
				buffer.Append(p.Type);
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
					clrProperty = new ClrProperty(column.Name, clrType.Name, LowerFirst(column.Name), clrType.IsReference);
				}
				else
				{
					var fkColumn = column.DbForeignKey.Column;
					var name = fkColumn.Substring(0, fkColumn.Length - NameProvider.IdName.Length);
					clrProperty = new ClrProperty(name, name, LowerFirst(name), true);
				}

				properties[i] = clrProperty;
			}

			return properties;
		}

		private static string LowerFirst(string name)
		{
			return char.ToLower(name[0]) + name.Substring(1);
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