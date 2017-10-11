#if SQLITE
namespace System.SQLite
{
	public sealed class SQLiteParameter : IDbDataParameter
	{
		private static DateTime _oaZero = new DateTime(1899, 12, 30);

		public static double ToJulianDay(DateTime date)
		{
			return (date - _oaZero).TotalMilliseconds / 24 / 3600 / 1000 + 2415018.5;
		}
		public static DateTime FromJulianDay(double julian)
		{
			return _oaZero.AddMilliseconds((julian - 2415018.5) * 24 * 3600 * 1000);
		}

		public object Value { get; set; }
		public string ParameterName { get; set; }

		public SQLiteParameter(string name, object value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.ParameterName = name;
			this.Value = value;
		}
	}
}
#endif