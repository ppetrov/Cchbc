#if SQLITE
namespace System.SQLite
{
	public class SQLiteException : Exception
	{
		public SQLiteResult SQLiteResult { get; private set; }

		public SQLiteException(SQLiteResult r, string message)
			: base(message)
		{
			SQLiteResult = r;
		}
	}
}
#endif