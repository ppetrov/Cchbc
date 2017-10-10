#if SQLITE
namespace System.SQLite
{
	public interface IDbConnection
	{
		IDbCommand CreateCommand();

		void Open();
		void Close();
	}
}
#endif