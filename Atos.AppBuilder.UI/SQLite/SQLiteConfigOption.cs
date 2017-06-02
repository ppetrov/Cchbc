#if SQLITE
namespace System.SQLite
{
	public enum SQLiteConfigOption
	{
		SingleThread = 1,
		MultiThread = 2,
		Serialized = 3
	}
}
#endif