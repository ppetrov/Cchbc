#if SQLITE
using System.Collections.Generic;

namespace System.SQLite
{
	public sealed class SQLiteDataReader : IDataReader
	{
		private readonly IntPtr _stmt;
		private readonly bool _useTicks;

		private SQLiteColumnType[] _columnTypes;
		private Dictionary<string, int> _nameIndex;

		public SQLiteDataReader(IntPtr stmt, bool useTicks)
		{
			_stmt = stmt;
			_useTicks = useTicks;
		}

		private void LoadMetaData()
		{
			if (_columnTypes == null)
			{
				_columnTypes = new SQLiteColumnType[SQLiteNative.ColumnCount(_stmt)];
			}
			var totalColumns = _columnTypes.Length;
			for (var i = 0; i < totalColumns; i++)
			{
				_columnTypes[i] = SQLiteNative.ColumnType(_stmt, i);
			}

			if (_nameIndex == null)
			{
				_nameIndex = new Dictionary<string, int>(totalColumns);

				for (var i = 0; i < totalColumns; i++)
				{
					_nameIndex.Add(SQLiteNative.ColumnName16(_stmt, i), i);
				}
			}
		}

		public bool Read()
		{
			var hasRows = SQLiteNative.Step(_stmt) == SQLiteResult.Row;
			if (hasRows)
			{
				this.LoadMetaData();
			}

			return hasRows;
		}

		public bool IsDBNull(int i)
		{
			return _columnTypes[i] == SQLiteColumnType.Null;
		}

		public int GetOrdinal(string name)
		{
			return _nameIndex[name];
		}

		public DateTime GetDateTime(int i)
		{
			if (_useTicks)
			{
				return SQLiteParameter.FromJulianDay(SQLiteNative.ColumnDouble(_stmt, i));
			}
			return DateTime.Parse(SQLiteNative.ColumnString(_stmt, i));
		}

		public long GetInt64(int i)
		{
			return SQLiteNative.ColumnInt64(_stmt, i);
		}

		public string GetString(int i)
		{
			return SQLiteNative.ColumnString(_stmt, i);
		}

		public int GetInt32(int i)
		{
			return SQLiteNative.ColumnInt(_stmt, i);
		}

		public byte GetByte(int i)
		{
			return Convert.ToByte(SQLiteNative.ColumnInt(_stmt, i));
		}

		public decimal GetDecimal(int i)
		{
			return Convert.ToDecimal(SQLiteNative.ColumnDouble(_stmt, i));
		}

		public void Dispose()
		{
		}
	}
}
#endif