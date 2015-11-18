using System;

namespace Cchbc.AppBuilder.Clr
{
	public sealed class ClrClass
	{
		public string Name { get; }
		public ClrProperty[] Properties { get; }

		public ClrClass(string name, ClrProperty[] properties)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (properties == null) throw new ArgumentNullException(nameof(properties));
			if (properties.Length == 0) throw new ArgumentOutOfRangeException(nameof(properties));

			this.Name = name;
			this.Properties = properties;
		}
	}
}