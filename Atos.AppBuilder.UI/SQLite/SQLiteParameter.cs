#if SQLITE
namespace System.SQLite
{
	public class SQLiteParameter : IDbDataParameter
	{
		private static DateTime _OAZero = new DateTime(1899, 12, 30);

		public static double ToJulianDay(DateTime date)
		{
			return (date - _OAZero).TotalMilliseconds / 24 / 3600 / 1000 + 2415018.5;
		}
		public static DateTime FromJulianDay(double julian)
		{
			return _OAZero.AddMilliseconds((julian - 2415018.5) * 24 * 3600 * 1000);
		}

		public object Value { get; set; }
		public string ParameterName { get; set; }

		public SQLiteParameter(string name, object value)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.ParameterName = name;
			this.Value = value;
		}
	}
}
#endif