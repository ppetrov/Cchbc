#if SQLITE
namespace System.SQLite
{
	public sealed class SQLiteCommand : IDbCommand
	{
		private static readonly IntPtr NegativePointer = new IntPtr(-1);

		public string CommandText { get; set; }
		public CommandType CommandType { get; set; }
		public IDataParameterCollection Parameters { get; }

		private readonly SQLiteConnection _connection;
		private IntPtr _stmt = IntPtr.Zero;

		public SQLiteCommand(SQLiteConnection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;

			this.CommandText = string.Empty;
			this.Parameters = new SQLiteDataParameterCollection();
		}

		public int ExecuteNonQuery()
		{
			_stmt = this.Prepare();

			var result = SQLiteNative.Step(_stmt);
			if (result == SQLiteResult.Done)
			{
				var rows = SQLiteNative.Changes(_connection.Handle);
				return rows;
			}
			if (result == SQLiteResult.Error)
			{
				throw new SQLiteException(result, SQLiteNative.GetErrmsg(_connection.Handle));
			}
			string message;
			try
			{
				message = SQLiteNative.GetErrmsg(_connection.Handle);
			}
			catch
			{
				message = result.ToString();
			}
			throw new SQLiteException(result, message);
		}

		public IDataReader ExecuteReader()
		{
			_stmt = this.Prepare();
			return new SQLiteDataReader(_stmt, _connection.DateTimeAsTicks);
		}

		private IntPtr Prepare()
		{
			var stmt = SQLiteNative.Prepare2(_connection.Handle, CommandText);
			this.BindParameters(stmt);
			return stmt;
		}

		private void BindParameters(IntPtr stmt)
		{
			foreach (var p in this.Parameters)
			{
				var name = p.ParameterName;
				if (!name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
				{
					name = "@" + name;
				}
				var index = SQLiteNative.BindParameterIndex(stmt, name);
				this.BindParameter(stmt, index, p.Value);
			}
		}

		private void BindParameter(IntPtr stmt, int index, object value)
		{
			if (value == null)
			{
				SQLiteNative.BindNull(stmt, index);
				return;
			}
			if (value is Int32)
			{
				SQLiteNative.BindInt(stmt, index, (int)value);
				return;
			}
			var stringValue = value as string;
			if (stringValue != null)
			{
				SQLiteNative.BindText(stmt, index, stringValue, -1, NegativePointer);
				return;
			}
			if (value is bool)
			{
				SQLiteNative.BindInt(stmt, index, (bool)value ? 1 : 0);
				return;
			}
			if (value is Byte || value is UInt16 || value is SByte || value is Int16)
			{
				SQLiteNative.BindInt(stmt, index, Convert.ToInt32(value));
				return;
			}
			if (value is UInt32 || value is Int64)
			{
				SQLiteNative.BindInt64(stmt, index, Convert.ToInt64(value));
				return;
			}
			if (value is Single || value is Double || value is Decimal)
			{
				SQLiteNative.BindDouble(stmt, index, Convert.ToDouble(value));
				return;
			}
			if (value is DateTime)
			{
				var dateTimeValue = (DateTime)value;
				if (_connection.DateTimeAsTicks)
				{
					SQLiteNative.BindDouble(stmt, index, SQLiteParameter.ToJulianDay(dateTimeValue));
				}
				else
				{
					SQLiteNative.BindText(stmt, index, dateTimeValue.ToString("yyyy-MM-dd HH:mm:ss"), -1, NegativePointer);
				}
				return;
			}
			var bytesValue = value as byte[];
			if (bytesValue != null)
			{
				SQLiteNative.BindBlob(stmt, index, bytesValue, bytesValue.Length, NegativePointer);
				return;
			}
			try
			{
				SQLiteNative.BindInt(stmt, index, (int)value);
			}
			catch
			{
				throw new NotSupportedException("Cannot store type: " + value.GetType());
			}
		}

		public void Dispose()
		{
			if (_stmt != IntPtr.Zero)
			{
				SQLiteNative.Finalize(_stmt);

				_stmt = IntPtr.Zero;
			}
		}
	}
}
#endif