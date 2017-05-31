using System;

namespace Cchbc.Features
{
	public sealed class Feature
	{
		public static readonly Feature None = new Feature(string.Empty, string.Empty);

		public string Context { get; }
		public string Name { get; }

		public static Feature StartNew(string context, string name)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (name == null) throw new ArgumentNullException(nameof(name));

			return new Feature(context, name);
		}

		private Feature(string context, string name)
		{
			this.Context = context;
			this.Name = name;
		}
	}
}