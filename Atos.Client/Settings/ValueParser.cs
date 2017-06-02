using System;
using Atos.Client.Logs;

namespace Atos.Client.Settings
{
	public static class ValueParser
	{
		public static int? ParseInt(string input, Action<string, LogLevel> log)
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
			}

			log($@"Unable to parse '{input}' to int", LogLevel.Warn);
			return null;
		}
	}
}