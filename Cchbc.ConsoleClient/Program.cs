using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cchbc.App;
using Cchbc.App.ArticlesModule.Data;
using Cchbc.App.ArticlesModule.Helpers;
using Cchbc.App.ArticlesModule.Objects;
using Cchbc.App.ArticlesModule.ViewModel;
using Cchbc.App.OrderModule;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Features.Db;
using Cchbc.Localization;


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

	public class Program
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

					var core = Core.Current;

					// Set logger
					core.Logger = new ConsoleLogger();

					// Create Read query helper
					var sqlReadDataQueryHelper = new SqlReadDataQueryHelper(cn);
					// Create Modify query helper
					var sqlModifyDataQueryHelper = new SqlModifyDataQueryHelper(sqlReadDataQueryHelper, cn);
					// Create General query helper
					var queryHelper = new QueryHelper(sqlReadDataQueryHelper, sqlModifyDataQueryHelper);
					core.QueryHelper = queryHelper;

					var featureManager = new FeatureManager { InspectFeature = InspectFeature() };
					core.FeatureManager = featureManager;
					core.FeatureManager.Initialize(core.Logger, new DbFeaturesManager(new DbFeaturesAdapter(queryHelper)));
					core.FeatureManager.LoadAsync().Wait();
					core.FeatureManager.StartDbWriters();

					var localizationManager = new LocalizationManager();
					core.LocalizationManager = localizationManager;

					// Register helpers
					core.DataCache = new DataCache();
					var cache = core.DataCache;
					cache.AddHelper(new BrandHelper());
					cache.AddHelper(new FlavorHelper());
					cache.AddHelper(new ArticleHelper());
					cache.AddHelper(new OrderTypeHelper());
					cache.AddHelper(new VendorHelper());
					cache.AddHelper(new WholesalerHelper());
					cache.AddHelper(new OrderNoteTypeHelper());

					// Load helpers
					var brandHelper = cache.GetHelper<Brand>();
					brandHelper.LoadAsync(new BrandAdapter(sqlReadDataQueryHelper)).Wait();
					var flavorHelper = cache.GetHelper<Flavor>();
					flavorHelper.LoadAsync(new FlavorAdapter(sqlReadDataQueryHelper)).Wait();
					var articleHelper = cache.GetHelper<Article>();
					articleHelper.LoadAsync(new ArticleAdapter(sqlReadDataQueryHelper, brandHelper.Items, flavorHelper.Items)).Wait();

					cache.GetHelper<OrderType>().LoadAsync(new OrderTypeAdapter());
					cache.GetHelper<Vendor>().LoadAsync(new VendorAdapter());
					cache.GetHelper<Wholesaler>().LoadAsync(new WholesalerAdapter());
					cache.GetHelper<OrderNoteType>().LoadAsync(new OrderNoteTypeAdapter());

					localizationManager.LoadAsync();

					var viewModel = new ArticlesViewModel(core);
					try
					{
						//for (var i = 0; i < 5; i++)
						//{
						//	viewModel.LoadDataAsync().Wait();
						//}

						var manager = new OrderManager(core, new Activity { Id = 1, Outlet = new Outlet { Id = 1, Name = @"Billa" } });

						for (int i = 0; i < 7; i++)
						{
							manager.LoadDataAsync().Wait();
							var items = manager.Assortments;
							Console.WriteLine(items.Count);
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}

					featureManager.StopDbWriters();
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

		private async Task<BrandHelper> LoadBrandsAsync(Feature feature, ReadDataQueryHelper queryHelper)
		{
			var step = feature.AddStep(nameof(LoadBrandsAsync));
			var brandHelper = new BrandHelper();
			await brandHelper.LoadAsync(new BrandAdapter(queryHelper));
			step.Details = brandHelper.Items.Count.ToString();
			return brandHelper;
		}

		private async Task<FlavorHelper> LoadFlavorsAsync(Feature feature, ReadDataQueryHelper queryHelper)
		{
			var step = feature.AddStep(nameof(LoadFlavorsAsync));
			var flavorHelper = new FlavorHelper();
			await flavorHelper.LoadAsync(new FlavorAdapter(queryHelper));
			step.Details = flavorHelper.Items.Count.ToString();
			return flavorHelper;
		}

		private async Task<ArticleHelper> LoadArticlesAsync(Feature feature, ReadDataQueryHelper queryHelper, BrandHelper brandHelper, FlavorHelper flavorHelper)
		{
			var step = feature.AddStep(nameof(LoadArticlesAsync));
			var articleHelper = new ArticleHelper();
			await articleHelper.LoadAsync(new ArticleAdapter(queryHelper, brandHelper.Items, flavorHelper.Items));
			step.Details = articleHelper.Items.Count.ToString();
			return articleHelper;
		}

		private static Action<FeatureEntry> InspectFeature()
		{
			return f =>
			{
				var buffer = new StringBuilder();

				buffer.Append(f.Context);
				buffer.Append('\t');
				buffer.Append(f.Details);
				buffer.Append('\t');
				buffer.AppendLine(f.TimeSpent.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

				if (f.Steps.Any())
				{
					var max = f.Steps.Select(s => s.TimeSpent.TotalMilliseconds).Max();
					var scale = max + (max / 2);

					foreach (var s in f.Steps)
					{
						buffer.Append("  ");
						buffer.Append(s.Name.PadRight(14));
						buffer.Append("  ");
						buffer.Append(s.Details);
						buffer.Append('\t');
						var milliseconds = s.TimeSpent.TotalMilliseconds;
						var tmp = (milliseconds / scale) * 100;
						var graph = new string('-', (int)tmp);
						
                        buffer.Append(milliseconds.ToString(CultureInfo.InvariantCulture).PadRight(8));
						buffer.Append(tmp.ToString(@"F2").PadLeft(5));
						buffer.Append(@"% ");
						buffer.AppendLine(graph);
					}
				}


				var output = buffer.ToString();
				output = output.Replace(@"Async", "");
				output = Regex.Replace(output, @"[A-Z]", m => @" " + m.Value).TrimStart();
				Debug.WriteLine(output);
				Console.WriteLine(output);
			};
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

