using System;

namespace Cchbc.AppBuilder
{
	public static class NameProvider
	{
		public static readonly string IdName = @"Id";

		public static string LowerFirst(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			return char.ToLower(name[0]) + name.Substring(1);
		}
	}
}