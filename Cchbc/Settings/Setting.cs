using System;

namespace Cchbc.Settings
{
	public sealed class Setting
	{
		public string Context { get; }
		public string Name { get; }
		public string Value { get; }

		public Setting(string context, string name, string value)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (value == null) throw new ArgumentNullException(nameof(value));

			this.Context = context;
			this.Name = name;
			this.Value = value;
		}
	}
}