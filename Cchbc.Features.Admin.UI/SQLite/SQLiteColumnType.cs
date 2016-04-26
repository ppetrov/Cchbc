#if SQLITE
namespace System.SQLite
{	
	public enum SQLiteColumnType
	{
		Integer = 1,
		Float = 2,
		Text = 3,
		Blob = 4,
		Null = 5
	}
}

#endif