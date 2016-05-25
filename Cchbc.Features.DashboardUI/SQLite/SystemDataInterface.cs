#if SQLITE
using System.Collections.Generic;

namespace System.SQLite
{
	public interface IDbConnection
	{
		IDbCommand CreateCommand();

		void Open();
		void Close();
	}

	public interface IDbDataParameter
	{
		object Value { get; set; }
		string ParameterName { get; set; }
	}

	public interface IDataReader : IDisposable
	{
		bool Read();
		bool IsDBNull(int i);

		int GetOrdinal(string name);

		DateTime GetDateTime(int i);
		long GetInt64(int i);
		string GetString(int i);
		int GetInt32(int i);
		byte GetByte(int i);
		decimal GetDecimal(int i);
	}

	public interface IDbCommand : IDisposable
	{
		CommandType CommandType { get; set; }
		string CommandText { get; set; }
		IDataParameterCollection Parameters { get; }
		IDataReader ExecuteReader();
		int ExecuteNonQuery();
	}

	public interface IDataParameterCollection : IList<IDbDataParameter>
	{

	}

	public enum CommandType
	{
		Text = 1,
		TableDirect = 512,
		StoredProcedure = 4
	}
}
#endif