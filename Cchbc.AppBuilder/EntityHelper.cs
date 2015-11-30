using System;
using System.Text;

namespace Cchbc.AppBuilder
{
	public static class EntityHelper
	{
		public static string Generate(Entity entity)
		{
			if (entity == null) throw new ArgumentNullException(nameof(entity));

			var buffer = new StringBuilder(256);

			var name = entity.Class.Name;
			EntityClass.AppendClassDefinition(buffer, name + @"Helper", @"Helper", name);
			EntityClass.AppendOpenBrace(buffer, 0);
			EntityClass.AppendCloseBrace(buffer, 0);

			return buffer.ToString();
		}
	}
}