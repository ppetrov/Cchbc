using System;

namespace Cchbc.Features
{
	public abstract class DbEntry
	{
		public string Context { get; }
		public string Name { get; }

		protected DbEntry(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Context = context;
			this.Name = name;
		}
	}
}