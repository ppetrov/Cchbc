using System;

namespace iFSA.LoginModule.Objects
{
	public sealed class AppSystem
	{
		public string Name { get; }
		public SystemSource Source { get; }

		public AppSystem(string name, SystemSource source)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.Source = source;
		}
	}
}