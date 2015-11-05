using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Features.Db;


namespace Cchbc.ConsoleClient
{
	public sealed class DelegateDataReader : IFieldDataReader
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

		public SqlReadDataQueryHelper(SQLiteConnection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
		}

		public override async Task<List<T>> ExecuteAsync<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			var values = new List<T>();

			using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = query.Statement;
				cmd.CommandType = CommandType.Text;
				foreach (var p in query.Parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
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

		public override async Task FillAsync<T>(Query<T> query, Dictionary<long, T> values)
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
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
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

		public SqlModifyDataQueryHelper(ReadDataQueryHelper readDataQueryHelper, SQLiteConnection connection)
			: base(readDataQueryHelper)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
		}

		public override async Task ExecuteAsync(string statement, QueryParameter[] parameters)
		{
			using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = statement;
				cmd.CommandType = CommandType.Text;
				foreach (var p in parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}
				await cmd.ExecuteNonQueryAsync();
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

				using (var cn = new SQLiteConnection(connectionString))
				{
					cn.Open();

					var sqlReadDataQueryHelper = new SqlReadDataQueryHelper(cn);
					var sqlModifyDataQueryHelper = new SqlModifyDataQueryHelper(sqlReadDataQueryHelper, cn);
					var queryHelper = new QueryHelper(sqlReadDataQueryHelper, sqlModifyDataQueryHelper);

					var module = new DbFeaturesManager(new DbFeaturesAdapter(queryHelper));

					var r = new Random();
					var featureSteps = new[]
					{
						new FeatureEntryStep(@"Load Brands",TimeSpan.FromMilliseconds(r.Next(100,500))),
						new FeatureEntryStep(@"Load Flavors",TimeSpan.FromMilliseconds(r.Next(100,500))),
						new FeatureEntryStep(@"Load Articles",TimeSpan.FromMilliseconds(r.Next(100,500))),
						new FeatureEntryStep(@"Display Articles",TimeSpan.FromMilliseconds(r.Next(100,200))),
					};

					var feature = new FeatureEntry(@"View all articles", @"Filter By", @"Coca Cola", TimeSpan.FromMilliseconds(r.Next(500, 1500)), featureSteps);
					try
					{
						//module.CreateSchemaAsync().Wait();
						module.LoadAsync().Wait();
						module.SaveAsync(feature).Wait();
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}



				//using (var featureManager = new FeatureManager(entries =>
				//{
				//	foreach (var entry in entries)
				//	{
				//		var context = entry.Context;
				//		Console.WriteLine(context);
				//		Console.WriteLine(entry.Name + " " + entry.TimeSpent.TotalMilliseconds);
				//		foreach (var step in entry.Steps)
				//		{
				//			Console.WriteLine("\t" + step.Name + ":" + step.TimeSpent.TotalMilliseconds);
				//		}
				//		Console.WriteLine(@"---");
				//	}
				//}))
				//{
				//	using (var cn = new SQLiteConnection(connectionString))
				//	{
				//		cn.Open();

				//		var sqlReadDataQueryHelper = new SqlReadDataQueryHelper(cn);
				//		var sqlModifyDataQueryHelper = new SqlModifyDataQueryHelper(sqlReadDataQueryHelper, cn);
				//		var queryHelper = new QueryHelper(sqlReadDataQueryHelper, sqlModifyDataQueryHelper);

				//		var core = new Core(new ConsoleLogger(), featureManager, queryHelper);
				//		var viewModel = new ArticlesViewModel(core);
				//		viewModel.LoadDataAsync().Wait();
				//		viewModel.LoadDataAsync().Wait();

				//		Console.WriteLine(@"Done");
				//	}
				//}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
	}

	//public abstract class BufferedLogger : ILogger
	//{
	//	protected readonly ConcurrentQueue<string> Buffer = new ConcurrentQueue<string>();

	//	public string Context { get; }

	//	protected BufferedLogger(string context)
	//	{
	//		if (context == null) throw new ArgumentNullException(nameof(context));

	//		this.Context = context;
	//	}

	//	public bool IsDebugEnabled { get; protected set; }
	//	public bool IsInfoEnabled { get; protected set; }
	//	public bool IsWarnEnabled { get; protected set; }
	//	public bool IsErrorEnabled { get; protected set; }

	//	public void Debug(string message)
	//	{
	//		if (this.IsDebugEnabled)
	//		{
	//			Buffer.Enqueue(message);
	//		}
	//	}

	//	public void Info(string message)
	//	{
	//		if (this.IsInfoEnabled)
	//		{
	//			Buffer.Enqueue(message);
	//		}
	//	}

	//	public void Warn(string message)
	//	{
	//		if (this.IsWarnEnabled)
	//		{
	//			Buffer.Enqueue(message);
	//		}
	//	}

	//	public void Error(string message)
	//	{
	//		if (this.IsErrorEnabled)
	//		{
	//			Buffer.Enqueue(message);
	//		}
	//	}
	//}

	public sealed class ConsoleLogger : ILogger
	{
		public bool IsDebugEnabled { get; }
		public bool IsInfoEnabled { get; }
		public bool IsWarnEnabled { get; }
		public bool IsErrorEnabled { get; }
		public void Debug(string message)
		{
			Console.WriteLine(@"Debug:" + message);
		}

		public void Info(string message)
		{
			Console.WriteLine(@"Info:" + message);
		}

		public void Warn(string message)
		{
			Console.WriteLine(@"Warn:" + message);
		}

		public void Error(string message)
		{
			Console.WriteLine(@"Error:" + message);
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
