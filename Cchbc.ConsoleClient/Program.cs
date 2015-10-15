using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.ConsoleClient
{
	public sealed class Brand : IDbObject
	{
		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}

		public long Id { get; set; }
		public string Name { get; set; }
	}

	public sealed class DelegateDataReader : IDataReader
	{
		private readonly DbDataReader _r;

		public DelegateDataReader(DbDataReader r)
		{
			if (r == null) throw new ArgumentNullException(nameof(r));

			_r = r;
		}

		public bool IsDbNull(int i)
		{
			return _r.IsDBNull(i);
		}

		public int GetInt32(int i)
		{
			return _r.GetInt32(i);
		}

		public long GetInt64(int i)
		{
			return _r.GetInt64(i);
		}

		public decimal GetDecimal(int i)
		{
			return _r.GetDecimal(i);
		}

		public string GetString(int i)
		{
			return _r.GetString(i);
		}

		public DateTime GetDateTime(int i)
		{
			return _r.GetDateTime(i);
		}

		public bool Read()
		{
			return _r.Read();
		}
	}

	public sealed class SqlReadDataQueryHelper : ReadDataQueryHelper
	{
		private readonly SQLiteConnection _connection;

		public SqlReadDataQueryHelper(string connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			_connection = new SQLiteConnection(connectionString);
		}

		public void Initialize()
		{
			_connection.Open();
		}

		public override async Task<List<T>> ExecuteAsync<T>(ReadQuery<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			var values = new List<T>();

			using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = query.Statement;
				cmd.CommandType = CommandType.Text;
				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SqlParameter(p.Name, p.Value));
				}

				using (var r = await cmd.ExecuteReaderAsync())
				{
					var dr = new DelegateDataReader(r);
					while (dr.Read())
					{
						values.Add(query.Creator(dr));
					}
				}
			}

			return values;
		}

		public override async Task FillAsync<T>(ReadQuery<T> query, Dictionary<long, T> values)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));

			values.Clear();

			using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = query.Statement;
				cmd.CommandType = CommandType.Text;
				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SqlParameter(p.Name, p.Value));
				}

				using (var r = await cmd.ExecuteReaderAsync())
				{
					var dr = new DelegateDataReader(r);
					while (dr.Read())
					{
						var value = query.Creator(dr);
						values.Add(value.Id, value);
					}
				}
			}
		}
	}

	public sealed class SqlModifyDataQueryHelper : ModifyDataQueryHelper
	{
		private readonly SQLiteConnection _connection;

		public SqlModifyDataQueryHelper(string connectionString)
		{
			if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

			_connection = new SQLiteConnection(connectionString);
		}

		public void Initialize()
		{
			_connection.Open();
		}

		public override void Execute(string statement, QueryParameter[] parameters)
		{
			if (statement == null) throw new ArgumentNullException(nameof(statement));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = statement;
				cmd.CommandType = CommandType.Text;
				foreach (var p in parameters)
				{
					cmd.Parameters.Add(new SqlParameter(p.Name, p.Value));
				}
				cmd.ExecuteNonQuery();
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				//var connectionString = @"Server=cwpfsa04;Database=Cchbc;User Id=dev;Password='dev user password'";
				var connectionString = @"Data Source=C:\Users\codem\Desktop\cchbc.sqlite;Version=3;";
				var sqlReadDataQueryHelper = new SqlReadDataQueryHelper(connectionString);
				var sqlModifyDataQueryHelper = new SqlModifyDataQueryHelper(connectionString);
				sqlReadDataQueryHelper.Initialize();
				sqlModifyDataQueryHelper.Initialize();
				var queryHelper = new QueryHelper(sqlReadDataQueryHelper, sqlModifyDataQueryHelper);

				//var brands = new List<Brand>();
				//for (var i = 0; i < 100; i++)
				//{
				//	var query = new ReadQuery<Brand>(@"SELECT ID, NAME FROM BRANDS", r =>
				//	{
				//		var id = 0L;
				//		if (!r.IsDbNull(0))
				//		{
				//			id = r.GetInt64(0);
				//		}
				//		var name = string.Empty;
				//		if (!r.IsDbNull(1))
				//		{
				//			name = r.GetString(1);
				//		}
				//		return new Brand(id, name);
				//	});

				//	brands = queryHelper.Execute(query);
				//}

				var core = new Core(new ConsoleBufferedLogger());

				core.Initialize(queryHelper).Wait();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}

	public abstract class BufferedLogger : ILogger
	{
		protected readonly ConcurrentQueue<string> Buffer = new ConcurrentQueue<string>();

		public string Context { get; }

		protected BufferedLogger(string context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
		}

		public bool IsDebugEnabled { get; protected set; }
		public bool IsInfoEnabled { get; protected set; }
		public bool IsWarnEnabled { get; protected set; }
		public bool IsErrorEnabled { get; protected set; }

		public void Debug(string message)
		{
			if (this.IsDebugEnabled)
			{
				Buffer.Enqueue(message);
			}
		}

		public void Info(string message)
		{
			if (this.IsInfoEnabled)
			{
				Buffer.Enqueue(message);
			}
		}

		public void Warn(string message)
		{
			if (this.IsWarnEnabled)
			{
				Buffer.Enqueue(message);
			}
		}

		public void Error(string message)
		{
			if (this.IsErrorEnabled)
			{
				Buffer.Enqueue(message);
			}
		}
	}

	public sealed class ConsoleBufferedLogger : BufferedLogger
	{
		public ConsoleBufferedLogger() : base("Cchbc Context")
		{
			this.IsInfoEnabled = true;
			this.IsWarnEnabled = true;
			this.IsErrorEnabled = true;

			ThreadPool.QueueUserWorkItem(_ =>
			{
				while (true)
				{
					Flush();
					Thread.Sleep(100);
				}
			});
		}

		public void Flush()
		{
			var local = new StringBuilder();

			string message;
			while (Buffer.TryDequeue(out message))
			{
				local.AppendLine(message);
			}
			if (local.Length > 0)
			{
				Console.Write(local);
			}
		}
	}



	public sealed class LogLevel
	{
		public static readonly LogLevel Trace = new LogLevel(0, @"Trace");
		public static readonly LogLevel Info = new LogLevel(1, @"Info");
		public static readonly LogLevel Warn = new LogLevel(2, @"Warn");
		public static readonly LogLevel Error = new LogLevel(3, @"Error");

		public int Id { get; private set; }
		public string Name { get; private set; }

		public LogLevel(int id, string name)
		{
			if (id < 0) throw new ArgumentNullException(nameof(name));
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}
