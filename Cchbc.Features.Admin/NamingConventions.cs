using System;
using System.Text;

namespace Cchbc.Features.Admin
{
    public static class NamingConventions
    {
        public static string ApplyNamingForContext(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return ApplyNaming(input);
        }

        public static string ApplyNamingForFeatures(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));

            return ApplyNaming(input);
        }

        private static string ApplyNaming(string input)
        {
            var value = input;

            if (value.EndsWith(@"LOADED", StringComparison.OrdinalIgnoreCase))
            {
                return @"Load Data";
            }
            if (value.EndsWith(@"SCREEN", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 6);
            }
            if (value.EndsWith(@"TAPPED", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 6);
            }

            var buffer = new StringBuilder(value.Length + 4);

            buffer.Append(value[0]);

            for (var i = 1; i < value.Length; i++)
            {
                var symbol = value[i];
                if (char.IsUpper(symbol))
                {
                    buffer.Append(' ');
                }
                buffer.Append(symbol);
            }

            return buffer.ToString();
        }
    }
}