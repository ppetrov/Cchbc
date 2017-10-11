#if SQLITE
namespace System.SQLite
{
	public sealed class SQLiteConnection : IDbConnection, IDisposable
	{
		public IntPtr Handle { get; private set; }
		public string DatabasePath { get; }
		public bool DateTimeAsTicks { get; }

		public SQLiteConnection(string databasePath)
			: this(databasePath, false)
		{
		}

		public SQLiteConnection(string databasePath, bool storeDateTimeAsTicks)
		{
			if (databasePath == null) throw new ArgumentNullException(nameof(databasePath));

			this.DatabasePath = databasePath;
			this.DateTimeAsTicks = storeDateTimeAsTicks;
		}

		public void Open()
		{
			IntPtr handle;
			var result = SQLiteNative.Open(this.DatabasePath, out handle, 2 | 4, IntPtr.Zero);
			if (result != SQLiteResult.OK)
			{
				throw new SQLiteException(result, string.Format("Could not open database file: {0} ({1})", this.DatabasePath, result));
			}
			this.Handle = handle;
		}

		public void Close()
		{
			if (this.Handle != IntPtr.Zero)
			{
				try
				{
					var result = SQLiteNative.Close(this.Handle);
					if (result != SQLiteResult.OK)
					{
						throw new SQLiteException(result, SQLiteNative.GetErrmsg(this.Handle));
					}
				}
				finally
				{
					this.Handle = IntPtr.Zero;
				}
			}
		}

		public IDbCommand CreateCommand()
		{
			return new SQLiteCommand(this);
		}

		public void Dispose()
		{
			this.Close();
		}
	}
}
#endif