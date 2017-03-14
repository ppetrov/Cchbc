using System;

namespace Cchbc.Localization
{
	public sealed class LocalizationKey
	{
		public string Context { get; }		
		public string Name  { get; }

		public LocalizationKey(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Context = context;
			this.Name = name;
		}
	}
}