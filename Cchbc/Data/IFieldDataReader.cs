using System;

namespace Cchbc.Data
{
	public interface IFieldDataReader
	{
		bool Read();
		bool IsDbNull(int i);
		int GetInt32(int i);
		long GetInt64(int i);
		decimal GetDecimal(int i);
		string GetString(int i);
		DateTime GetDateTime(int i);
	}
}