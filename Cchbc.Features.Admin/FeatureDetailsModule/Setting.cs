using System;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class Setting
	{
		public string Name { get; }
		public string Value { get; }

		public Setting(string name, string value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (value == null) throw new ArgumentNullException(nameof(value));

			this.Name = name;
			this.Value = value;
		}
	}
}