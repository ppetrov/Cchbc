using System;
using System.Collections.Generic;

namespace Cchbc.AppBuilder
{
	public class NameProvider
	{
		public static readonly string IdName = @"Id";

		//public static string LowerFirst(string name)
		//{
		//	if (name == null) throw new ArgumentNullException(nameof(name));

		//	return char.ToLower(name[0]) + name.Substring(1);
		//}

		public static string GetPrefix(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (name.Length == 0) throw new ArgumentOutOfRangeException(nameof(name));

			var uppers = new List<char>();

			foreach (var symbol in name)
			{
				if (char.IsUpper(symbol))
				{
					uppers.Add(char.ToLowerInvariant(symbol));
				}
			}

			return new string(uppers.ToArray());
		}
	}
}