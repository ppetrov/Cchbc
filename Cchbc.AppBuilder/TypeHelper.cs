using System;
using Cchbc.AppBuilder.Clr;

namespace Cchbc.AppBuilder
{
	public static class TypeHelper
	{
		public static string GetReaderMethod(ClrType type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			if (type == ClrType.Long) return @"GetInt64";
			if (type == ClrType.String) return @"GetString";
			if (type == ClrType.Decimal) return @"GetDecimal";
			if (type == ClrType.DateTime) return @"GetDateTime";
			if (type == ClrType.Bytes) return @"GetBytes";

			return @"GetInt64";
		}

		public static string GetDefaultValue(ClrType type)
		{
			if (type == null) throw new ArgumentNullException(nameof(type));

			if (type == ClrType.Long) return @"0L";
			if (type == ClrType.String) return @"string.Empty";
			if (type == ClrType.Decimal) return @"0M";
			if (type == ClrType.DateTime) return @"DateTime.MinValue";
			if (type == ClrType.Bytes) return @"default(byte[])";

			return $@"default({type.Name})";
		}
	}
}