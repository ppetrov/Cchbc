using System;
using Cchbc.Logs;

namespace Cchbc.Common
{
	public static class ValueParser
	{
		public static int? ParseInt(string input, ILog log)
		{
			if (input == null) throw new ArgumentNullException(nameof(input));

			var value = input.Trim();
			if (value != string.Empty)
			{
				int number;
				if (int.TryParse(value, out number))
				{
					return number;
				}
				log.Log($@"Unable to parse '{input}'", LogLevel.Warn);
			}

			return null;
		}
	}
}