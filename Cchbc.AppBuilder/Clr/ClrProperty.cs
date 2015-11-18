using System;

namespace Cchbc.AppBuilder.Clr
{
	public sealed class ClrProperty
	{
		public string Name { get; }
		public ClrType Type { get; }

		public ClrProperty(string name, ClrType type)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (type == null) throw new ArgumentNullException(nameof(type));

			this.Name = name;
			this.Type = type;
		}
	}
}