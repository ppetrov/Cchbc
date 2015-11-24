using System;
using System.Collections.Generic;
using System.Text;
using Cchbc.AppBuilder.Clr;
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
			buffer.Append(entity.Class.Name);
			buffer.Append('>');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer);

			EntityClass.AppendClassProperties(buffer, adapterProperties, true, false);
			buffer.AppendLine();

			EntityClass.AppendClassConstructor(buffer, adapterClassName, adapterProperties);
			buffer.AppendLine();

			AppendFillMethod(buffer, entity);
			buffer.AppendLine();

			EntityClass.AppendCreatorMethod(buffer, entity.Class, dictionaryProperties);

			EntityClass.AppendCloseBrace(buffer);

			return buffer.ToString();
		}

		private static void AppendFillMethod(StringBuilder buffer, Entity entity)
		{
			var className = entity.Class.Name;
			var dictionaryName = @"items";
			var funcName = @"selector";

			var level = 1;
			EntityClass.AppendIndentation(buffer, level);
			// Fill method
			buffer.Append(@"public");
			buffer.Append(' ');
			buffer.Append(@"void");
			buffer.Append(' ');
			buffer.Append(@"Fill");
			buffer.Append('(');
			buffer.Append(@"Dictionary");
			buffer.Append('<');
			buffer.Append(@"long");
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(className);
			buffer.Append('>');
			buffer.Append(' ');
			buffer.Append(dictionaryName);
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"Func");
			buffer.Append('<');
			buffer.Append(className);
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(@"long");
			buffer.Append('>');
			buffer.Append(' ');
			buffer.Append(funcName);
			buffer.Append(')');
			buffer.AppendLine();

			EntityClass.AppendOpenBrace(buffer, level++);
			EntityClass.AppendArgumentNullCheck(buffer, dictionaryName, level);
			EntityClass.AppendArgumentNullCheck(buffer, funcName, level);

			buffer.AppendLine();

			// Query
			EntityClass.AppendIndentation(buffer, level);
			buffer.Append(@"var");
			buffer.Append(' ');
			buffer.Append(@"query");
			buffer.Append(' ');
			buffer.Append('=');
			buffer.Append(' ');
			buffer.Append('@');
			buffer.Append('"');
			QueryBuilder.AppendSelect(buffer, entity.Table);
			buffer.Append('"');
			buffer.Append(';');
			buffer.AppendLine();

			buffer.AppendLine();

			// Fill items using the query & creator
			EntityClass.AppendIndentation(buffer, level);
			buffer.Append(@"this");
			buffer.Append('.');
			buffer.Append(@"QueryHelper");
			buffer.Append('.');
			buffer.Append(@"Fill");
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
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(dictionaryName);
			buffer.Append(',');
			buffer.Append(' ');
			buffer.Append(funcName);
			buffer.Append(')');
			buffer.Append(';');
			buffer.AppendLine();

			EntityClass.AppendCloseBrace(buffer, --level);
		}

		
	}
}