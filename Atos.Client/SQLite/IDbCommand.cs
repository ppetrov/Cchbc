#if SQLITE
namespace System.SQLite
{
	public interface IDbCommand : IDisposable
	{
		CommandType CommandType { get; set; }
		string CommandText { get; set; }
		IDataParameterCollection Parameters { get; }
		IDataReader ExecuteReader();
		int ExecuteNonQuery();
	}
}
#endif