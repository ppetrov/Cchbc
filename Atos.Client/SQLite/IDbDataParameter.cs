#if SQLITE
namespace System.SQLite
{
	public interface IDbDataParameter
	{
		object Value { get; set; }
		string ParameterName { get; set; }
	}
}
#endif