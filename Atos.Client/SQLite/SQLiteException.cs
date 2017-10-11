#if SQLITE
namespace System.SQLite
{
	public sealed class SQLiteException : Exception
	{
		public SQLiteResult SqLiteResult { get; }

		public SQLiteException(SQLiteResult r, string message)
			: base(message)
		{
			SqLiteResult = r;
		}
	}
}
#endif