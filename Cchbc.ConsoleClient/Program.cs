using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cchbc.App.ArticlesModule.Helpers;
using Cchbc.AppBuilder;
using Cchbc.AppBuilder.DDL;
using Cchbc.AppBuilder.DML;
using Cchbc.Data;
using Cchbc.Features;


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

	public sealed class SqlReadQueryHelper : ReadQueryHelper
	{
		private readonly SQLiteConnection _connection;

		public SqlReadQueryHelper(SQLiteConnection connection)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
		}

		public override List<T> Execute<T>(Query<T> query)
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

				using (var r = cmd.ExecuteReader())
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

		public override void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector)
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

				using (var r = cmd.ExecuteReader())
				{
					var dr = new DelegateDataReader(r);
					while (dr.Read())
					{
						var value = query.Creator(dr);
						values.Add(selector(value), value);
					}
				}
			}
		}

		public override void Fill<T>(string query, Dictionary<long, T> values, Action<IFieldDataReader, Dictionary<long, T>> filler, QueryParameter[] parameters)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (filler == null) throw new ArgumentNullException(nameof(filler));
			if (parameters == null) throw new ArgumentNullException(nameof(parameters));

			values.Clear();

			using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = query;
				cmd.CommandType = CommandType.Text;
				foreach (var p in parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}

				using (var r = cmd.ExecuteReader())
				{
					var dr = new DelegateDataReader(r);
					while (dr.Read())
					{
						filler(dr, values);
					}
				}
			}
		}
	}

	public sealed class SqlModifyQueryHelper : ModifyQueryHelper
	{
		private readonly SQLiteConnection _connection;

		public SqlModifyQueryHelper(ReadQueryHelper readQueryHelper, SQLiteConnection connection)
			: base(readQueryHelper)
		{
			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_connection = connection;
		}

		public override int Execute(string statement, QueryParameter[] parameters)
		{
			using (var cmd = _connection.CreateCommand())
			{
				cmd.CommandText = statement;
				cmd.CommandType = CommandType.Text;
				foreach (var p in parameters)
				{
					cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
				}
				return cmd.ExecuteNonQuery();
			}
		}
	}


































	public class Program
	{
		static void Main(string[] args)
		{
			var outlets = DbTable.Create(@"Outlets", new[]
			{
				DbColumn.String(@"Name"),
			});
			var visits = DbTable.Create(@"Visits", new[]
			{
				DbColumn.ForeignKey(outlets),
				DbColumn.DateTime(@"Date"),
			});
			var activityTypes = DbTable.Create(@"ActivityTypes", new[]
			{
				DbColumn.String(@"Name"),
			});
			var activities = DbTable.Create(@"Activities", new[]
			{
				DbColumn.DateTime(@"Date"),
				DbColumn.ForeignKey(activityTypes),
				DbColumn.ForeignKey(visits),
			}, @"Activity");
			var brands = DbTable.Create(@"Brands", new[]
			{
				DbColumn.String(@"Name"),
			});
			var flavors = DbTable.Create(@"Flavors", new[]
			{
				DbColumn.String(@"Name"),
			});
			var articles = DbTable.Create(@"Articles", new[]
			{
				DbColumn.String(@"Name"),
				DbColumn.ForeignKey(brands),
				DbColumn.ForeignKey(flavors),
			});
			var activityNoteTypes = DbTable.Create(@"ActivityNoteTypes", new[]
			{
				DbColumn.String(@"Name"),
			});
			var activityNotes = DbTable.Create(@"ActivityNotes", new[]
			{
				DbColumn.String(@"Contents"),
				DbColumn.DateTime(@"CreatedAt"),
				DbColumn.ForeignKey(activityNoteTypes),
				DbColumn.ForeignKey(activities),
			});

			var schema = new DbSchema(@"ifsa", new[]
			{
				outlets,
				visits,
				activityTypes,
				activities,
				brands,
				flavors,
				articles,
				activityNoteTypes,
				activityNotes
			});

			var project = new DbProject(schema);

			// Mark tables as Modifiable, all tables are ReadOnly by default
			project.MarkModifiable(visits);
			project.MarkModifiable(activities);
			project.MarkModifiable(activityNotes);

			// Attach Inverse tables
			project.AttachInverseTable(visits);

			var filePath = @"c:\temp\ifsa.ctx";
			project.Save(filePath);
			var copy = DbProject.Load(filePath);

			var buffer = new StringBuilder(1024 * 2);

			var s = Stopwatch.StartNew();
			foreach (var entity in project.CreateEntities())
			{
				//
				// Classes
				//
				var entityClass = project.CreateEntityClass(entity);
				buffer.AppendLine(entityClass);
				//continue;

				//
				// Read Only adapters
				if (!project.IsModifiable(entity.Table))
				{
					var adapter = project.CreateEntityAdapter(entity);
					buffer.AppendLine(adapter);
				}
				//continue;

				//
				// Modifiable adapters
				//
				if (project.IsModifiable(entity.Table))
				{
					var adapter = project.CreateEntityAdapter(entity);
					buffer.AppendLine(adapter);
				}
			}
			s.Stop();
			Console.WriteLine(s.ElapsedMilliseconds);

			Console.WriteLine(buffer.ToString());
			File.WriteAllText(@"C:\temp\code.txt", buffer.ToString());



			return;

















			//var connectionString = @"Server=cwpfsa04;Database=Cchbc;User Id=dev;Password='dev user password'";


			//using (var cn = new SQLiteConnection(connectionString))
			//{
			//	cn.Open();

			//	var core = Core.Current;

			//	// Set logger
			//	core.Logger = new ConsoleLogger();

			//	// Create Read query helper
			//	var sqlReadDataQueryHelper = new SqlReadQueryHelper(cn);
			//	// Create Modify query helper
			//	var sqlModifyDataQueryHelper = new SqlModifyQueryHelper(sqlReadDataQueryHelper, cn);
			//	// Create General query helper
			//	var queryHelper = new QueryHelper(sqlReadDataQueryHelper, sqlModifyDataQueryHelper);
			//	core.QueryHelper = queryHelper;

			//	var featureManager = new FeatureManager { InspectFeature = InspectFeature() };
			//	core.FeatureManager = featureManager;
			//	core.FeatureManager.Initialize(core.Logger, new DbFeaturesManager(new DbFeaturesAdapter(queryHelper)));
			//	core.FeatureManager.LoadAsync().Wait();
			//	core.FeatureManager.StartDbWriters();

			//	var localizationManager = new LocalizationManager();
			//	core.LocalizationManager = localizationManager;

			//	// Register helpers
			//	core.DataCache = new DataCache();
			//	var cache = core.DataCache;
			//	cache.AddHelper(new BrandHelper());
			//	cache.AddHelper(new FlavorHelper());
			//	cache.AddHelper(new ArticleHelper());
			//	cache.AddHelper(new OrderTypeHelper());
			//	cache.AddHelper(new VendorHelper());
			//	cache.AddHelper(new WholesalerHelper());
			//	cache.AddHelper(new OrderNoteTypeHelper());

			//	// Load helpers
			//	var brandHelper = cache.GetHelper<Brand>();
			//	brandHelper.LoadAsync(new BrandAdapter(sqlReadDataQueryHelper)).Wait();
			//	var flavorHelper = cache.GetHelper<Flavor>();
			//	flavorHelper.LoadAsync(new FlavorAdapter(sqlReadDataQueryHelper)).Wait();
			//	var articleHelper = cache.GetHelper<Article>();
			//	articleHelper.LoadAsync(new ArticleAdapter(sqlReadDataQueryHelper, brandHelper.Items, flavorHelper.Items)).Wait();

			//	cache.GetHelper<OrderType>().LoadAsync(new OrderTypeAdapter());
			//	cache.GetHelper<Vendor>().LoadAsync(new VendorAdapter());
			//	cache.GetHelper<Wholesaler>().LoadAsync(new WholesalerAdapter());
			//	cache.GetHelper<OrderNoteType>().LoadAsync(new OrderNoteTypeAdapter());

			//	localizationManager.LoadAsync();

			//	var viewModel = new ArticlesViewModel(core);
			//	try
			//	{
			//		//for (var i = 0; i < 5; i++)
			//		//{
			//		//	viewModel.LoadDataAsync().Wait();
			//		//}

			//		var manager = new OrderManager(core, new Activity { Id = 1, Outlet = new Outlet { Id = 1, Name = @"Billa" } });

			//		for (int i = 0; i < 7; i++)
			//		{
			//			manager.LoadDataAsync().Wait();
			//			var items = manager.Assortments;
			//			Console.WriteLine(items.Count);
			//		}
			//	}
			//	catch (Exception e)
			//	{
			//		Console.WriteLine(e);
			//	}

			//	featureManager.StopDbWriters();
			//}



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

		private async Task<BrandHelper> LoadBrandsAsync(Feature feature, ReadQueryHelper queryHelper)
		{
			var step = feature.AddStep(nameof(LoadBrandsAsync));
			var brandHelper = new BrandHelper();
			//await brandHelper.LoadAsync(new BrandAdapter(queryHelper), v => v.Id);
			step.Details = brandHelper.Items.Count.ToString();
			return brandHelper;
		}

		private async Task<FlavorHelper> LoadFlavorsAsync(Feature feature, ReadQueryHelper queryHelper)
		{
			var step = feature.AddStep(nameof(LoadFlavorsAsync));
			var flavorHelper = new FlavorHelper();
			//await flavorHelper.LoadAsync(new FlavorAdapter(queryHelper), v => v.Id);
			step.Details = flavorHelper.Items.Count.ToString();
			return flavorHelper;
		}

		private async Task<ArticleHelper> LoadArticlesAsync(Feature feature, ReadQueryHelper queryHelper, BrandHelper brandHelper, FlavorHelper flavorHelper)
		{
			var step = feature.AddStep(nameof(LoadArticlesAsync));
			var articleHelper = new ArticleHelper();
			//await articleHelper.LoadAsync(new ArticleAdapter(queryHelper, brandHelper.Items, flavorHelper.Items));
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

