#if SQLITE
namespace System.SQLite
{
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
}
#endif