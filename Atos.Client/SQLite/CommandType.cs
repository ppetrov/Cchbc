#if SQLITE
namespace System.SQLite
{
	public enum CommandType
	{
		Text = 1,
		TableDirect = 512,
		StoredProcedure = 4
	}
}
#endif