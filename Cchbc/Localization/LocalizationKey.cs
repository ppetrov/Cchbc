using System;

namespace Cchbc.Localization
{
	public sealed class LocalizationKey
	{
		public string Name { get; }

		public LocalizationKey(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}
	}
}