using System;
using System.Text;

namespace Atos.AppBuilder
{
	public static class BufferHelper
	{
		public static void AppendLowerFirst(StringBuilder buffer, string value)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			if (value == null) throw new ArgumentNullException(nameof(value));

			// Append the value
			buffer.Append(value);

			// Force the first letter to be in lower case
			buffer[buffer.Length - value.Length] = char.ToLowerInvariant(value[0]);
		}
	}
}