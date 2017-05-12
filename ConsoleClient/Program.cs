using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cchbc;
using Cchbc.AppBuilder;
using Cchbc.AppBuilder.DDL;
using Cchbc.Archive;
using Cchbc.ConsoleClient;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Features.Data;
using Cchbc.Features.ExceptionsModule;
using Cchbc.Features.Replication;
using Cchbc.Validation;

namespace ConsoleClient
{
	public static class ClientDataReplication
	{
		public static string GetSqliteConnectionString(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			return $@"Data Source = {path}; Version = 3;";
		}

		public static void SimulateDay(string serverDbPath, DateTime date)
		{
			var s = Stopwatch.StartNew();
			var replicationPerDay = 3;

			replicationPerDay = 1;

			var commonUsages = 100;
			var systemUsages = 25;

			var rnd = new Random();

			var versions = new[]
			{
				@"8.28.79.127",
				@"7.74.19.727",
				@"6.22.29.492",
				@"5.96.69.792",
				@"4.11.27.292",
				@"3.85.19.223",
			};

			var users = new List<string>();
			for (var i = 11; i < 1200; i++)
			{
				users.Add(@"BG" + (i.ToString()).PadLeft(6, '0'));
			}

			var replications = new List<Tuple<string, string>>(users.Count * replicationPerDay);
			foreach (var user in users)
			{
				for (var i = 0; i < replicationPerDay; i++)
				{
					replications.Add(Tuple.Create(user, versions[rnd.Next(versions.Length)]));
				}
			}

			ServerData serverData;
			using (var client = new TransactionContextCreator(GetSqliteConnectionString(serverDbPath)).Create())
			{
				serverData = FeatureServerManager.GetServerData(client);
				client.Complete();
			}

			var fromDate = date.AddHours(7);
			var toDate = date.AddHours(19);
			var diff = (int)((toDate - fromDate).TotalSeconds);

			//foreach (var replication in replications)
			//{
			//	var referenceDate = fromDate.AddSeconds(rnd.Next(diff));

			//	var dbFeatureExceptionRows = new List<DbFeatureExceptionRow>
			//	{
			//		new DbFeatureExceptionRow(rnd.Next(1,1024), @"AppSystem.Collections.Generic.KeyNotFoundException: The given key was not present in the dictionary.
			//at AppSystem.Collections.Generic.Dictionary`2.get_Item(TKey key)
			//at AppSystem.SQLite.SQLiteDataReader.GetOrdinal(String name)
			//at SFA.BusinessLogic.DataAccess.OutletManagement.OutletAdapter.OutletSnapCreator(IDataReader r)
			//at SFA.BusinessLogic.DataAccess.Helpers.QueryHelper.ExecuteReader[T](String query, Func`2 creator, IEnumerable`1 parameters, Int32 capacity)
			//at SFA.BusinessLogic.DataAccess.Helpers.QueryHelper.ExecuteReader[T](String query, Func`2 creator, Int32 capacity)
			//at SFA.BusinessLogic.DataAccess.OutletManagement.OutletAdapter.GetAll()
			//at SFA.BusinessLogic.Helpers.DataHelper.Load(OutletAdapter outletAdapter, OutletHierLevelAdapter hierLevelAdapter, TradeChannelsAdapter channelsAdapter, OutletAssignmentAdapter assignmentAdapter, PayerAdapter payerAdapter, OutletAddressAdapter addressAdapter, MarketAttributesAdapter attributesAdapter, List`1 modifiedTables)
			//at SFA.BusinessLogic.Cache.<>c__DisplayClass62_0.<Load>b__32()
			//at SFA.BusinessLogic.Cache.Load(Boolean useDependancies)")
			//	};

			//	var dbFeatureContextRows = new List<DbFeatureContextRow>
			//	{
			//		new DbFeatureContextRow(rnd.Next(1,1024), @"Cache"),
			//		new DbFeatureContextRow(rnd.Next(1024,2024), @"Agenda"),
			//	};
			//	var dbFeatureRows = new List<DbFeatureRow>
			//	{
			//		new DbFeatureRow(rnd.Next(1, 1024), @"Load", dbFeatureContextRows[0].Id),
			//		new DbFeatureRow(rnd.Next(1024, 2024), @"Load", dbFeatureContextRows[1].Id),

			//		new DbFeatureRow(rnd.Next(2024, 3024), @"Close Activity", dbFeatureContextRows[1].Id),
			//		new DbFeatureRow(rnd.Next(3024, 4024), @"Cancel Activity", dbFeatureContextRows[1].Id),
			//		new DbFeatureRow(rnd.Next(4024, 5024), @"Edit Activity", dbFeatureContextRows[1].Id),

			//		new DbFeatureRow(rnd.Next(5024, 6024), @"Synchronize", dbFeatureContextRows[1].Id),
			//		new DbFeatureRow(rnd.Next(6024, 7024), @"View Outlet Details", dbFeatureContextRows[1].Id),
			//	};
			//	var dbFeatureExceptionEntryRows = new List<DbFeatureExceptionEntryRow>
			//	{
			//		new DbFeatureExceptionEntryRow(dbFeatureExceptionRows[0].Id, GetRandomDate(rnd, fromDate, referenceDate), dbFeatureRows[0].Id)
			//	};

			//	var dbFeatureEntryRows = new List<DbFeatureEntryRow>
			//	{
			//		new DbFeatureEntryRow(string.Empty, DateTime.Now, dbFeatureRows[rnd.Next(dbFeatureRows.Count)].Id),
			//		new DbFeatureEntryRow(string.Empty, DateTime.Now, dbFeatureRows[rnd.Next(dbFeatureRows.Count)].Id)
			//	};

			//	var clientData = new ClientData(dbFeatureContextRows, dbFeatureExceptionRows, dbFeatureRows, dbFeatureEntryRows, dbFeatureExceptionEntryRows);

			//	using (var ctx = new TransactionContextCreator(GetSqliteConnectionString(serverDbPath)).Create())
			//	{
			//		FeatureServerManager.Replicate(replication.Item1, replication.Item2, ctx, clientData, serverData);
			//		ctx.Complete();
			//	}
			//}

			s.Stop();
			Console.WriteLine(s.ElapsedMilliseconds);
		}

		private static DateTime GetRandomDate(Random r, DateTime fromDate, DateTime toDate)
		{
			return fromDate.Add(TimeSpan.FromSeconds(r.Next((int)(toDate - fromDate).TotalSeconds)));
		}
	}




	public class Program
	{
		public static readonly string DbPrefix = @"obppc_db_";

		public static string GetCountryCode(string url)
		{
			if (url == null) throw new ArgumentNullException(nameof(url));

			var value = url.Trim();
			var start = value.LastIndexOf('/');
			if (start >= 0)
			{
				return value.Substring(start + 1);
			}

			return string.Empty;
		}


		public sealed class Person
		{
			public Person(string firstName, string secondName)
			{
				if (firstName == null) throw new ArgumentNullException(nameof(firstName));
				if (secondName == null) throw new ArgumentNullException(nameof(secondName));
				this.FirstName = firstName;
				this.SecondName = secondName;
			}

			public string FirstName { get; }
			public string SecondName { get; }

			public void Method()
			{
				var buffer = new StringBuilder();

				for (int i = 0; i < 100; i++)
				{
					buffer.AppendLine(string.Empty);
				}

				var result = buffer.ToString();
			}
		}

		public sealed class MapEntry
		{
			public string Name { get; }
			public int Sequence { get; set; }

			public MapEntry(string name, int sequence)
			{
				if (name == null) throw new ArgumentNullException(nameof(name));
				this.Name = name;
				this.Sequence = sequence;
			}
		}

		public static void Reorder(MapEntry[] items, int sourceIndex, int destinationIndex)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (sourceIndex == destinationIndex) throw new ArgumentOutOfRangeException();

			var sourceItem = items[sourceIndex];
			var desctinationItem = items[destinationIndex];
			var startOffset = 0;
			var endOffset = 0;
			Func<int, int> modifier;
			if (sourceIndex < destinationIndex)
			{
				startOffset++;
				modifier = s => s - 1;
			}
			else
			{
				endOffset--;
				modifier = s => s + 1;
			}

			Action<MapEntry> dbUpdate = e => Console.WriteLine(@"Db Update => " + e.Name + " : " + e.Sequence);

			// Copy the sequence from the destination item
			sourceItem.Sequence = desctinationItem.Sequence;

			var start = Math.Min(sourceIndex, destinationIndex) + startOffset;
			var end = Math.Max(sourceIndex, destinationIndex) + endOffset;

			for (var i = start; i <= end; i++)
			{
				// Check if we can index in the collection
				var isValid = 0 <= i && i < items.Length;
				if (isValid)
				{
					var item = items[i];

					item.Sequence = modifier(item.Sequence);

					dbUpdate(item);
				}
			}

			dbUpdate(sourceItem);
		}

		public static void Main(string[] args)
		{
			var freshDbPath = @"C:\Users\PetarPetrov\Desktop\data.sqlite";
			var clientDbPath = @"C:\Users\PetarPetrov\Desktop\features.sqlite";
			var serverDbPath = @"C:\Users\PetarPetrov\Desktop\server.sqlite";

			try
			{
				return;

				var imgAsString = File.ReadAllBytes(@"C:\Users\PetarPetrov\Desktop\signature.jpg");
				//imgAsString = File.ReadAllBytes(@"C:\Users\PetarPetrov\Desktop\signature.jpg");
				var b64 = Convert.ToBase64String(imgAsString);
				Console.WriteLine(b64.Length);




				using (var client = new TransactionContextCreator(GetSqliteConnectionString(@"C:\Users\PetarPetrov\Desktop\BG000956.sqlite")).Create())
				{
					foreach (var name in client.Execute(new Query<string>(@"SELECT name FROM sqlite_master WHERE type='table' order by name", dr => dr.GetString(0))))
					{
						var localQuery = @"select count(*) from " + name + " where rec_status <> 1";
						try
						{
							var v = client.Execute(new Query<int>(localQuery, r => r.GetInt32(0))).Single();
							if (v != 0)
							{
								Console.WriteLine(name + " " + v);
							}
						}
						catch
						{
							Console.WriteLine(name);
						}
					}

					client.Complete();
				}


				return;

				//var addViewModel = new AddActivityViewModel(new ActivityCreator(), new ConsoleDialog());
				//using (var dbContext = new TransactionContextCreator(string.Empty).Create())
				//{
				//	var modelData = new AddActivityViewModelData(dbContext);
				//	modelData.Load();

				//	addViewModel.Load(modelData);

				//	dbContext.Complete();
				//}

				//var context = new AppContext((msg, level) => { Console.WriteLine(level + @":" + msg); }, () => new DbContext(GetSqliteConnectionString(freshDbPath)), new ConsoleDialog());

				//var module = new AppModule(context);
				//module.Init();
				//module.Load();

				//return;

				//if (!File.Exists(serverDbPath))
				//{
				//	CreateSchema(GetSqliteConnectionString(serverDbPath));
				//}
				//foreach (var date in new[]
				//{
				//	//DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-9), DateTime.Today.AddDays(-8), DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-6), DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-4), DateTime.Today.AddDays(-3), DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1), DateTime.Today,
				//	DateTime.Today.AddDays(-10)
				//})
				//{
				//	ClientDataReplication.SimulateDay(serverDbPath, date);
				//}

				//return;


				return;

				//var fm = new FeatureManager();

				//fm.Load(null);

				//return;

				//GenerateData(clientDbPath);
				//return;

				//WeatherTest();
				//return;

				if (!File.Exists(serverDbPath))
				{
					CreateSchema(GetSqliteConnectionString(serverDbPath));
				}

				var w = Stopwatch.StartNew();
				GenerateData(clientDbPath);

				Console.WriteLine(@"Load client data");
				var s = Stopwatch.StartNew();

				//ClientData clientData;
				//using (var client = new TransactionContextCreator(GetSqliteConnectionString(clientDbPath)).Create())
				//{
				//	clientData = FeatureAdapter.GetData(client);
				//	clientData.FeatureEntryRows.Clear();
				//	client.Complete();
				//}

				File.Delete(clientDbPath);

				s.Stop();
				Console.WriteLine(s.ElapsedMilliseconds);

				ServerData serverData;
				using (var client = new TransactionContextCreator(GetSqliteConnectionString(serverDbPath)).Create())
				{
					serverData = FeatureServerManager.GetServerData(client);
					client.Complete();
				}

				//while (true)
				//{
				//	s.Restart();
				//	Replicate(GetSqliteConnectionString(serverDbPath), clientData, serverData);
				//	s.Stop();
				//	Console.WriteLine(s.ElapsedMilliseconds);
				//	//Console.ReadLine();
				//}

				w.Stop();
				Console.WriteLine(w.ElapsedMilliseconds);
				return;


				var viewModel =
					new ExceptionsViewModel(new TransactionContextCreator(GetSqliteConnectionString(serverDbPath)).Create,
						ExceptionsSettings.Default);

				for (var i = 0; i < 100; i++)
				{
					viewModel.Load(ExceptionsDataProvider.GetTimePeriods, ExceptionsDataProvider.GetVersions,
						ExceptionsDataProvider.GetExceptions, ExceptionsDataProvider.GetExceptionsCounts);
				}

				w.Stop();
				Console.WriteLine(w.ElapsedMilliseconds);

				Console.WriteLine(@"Time Periods");
				foreach (var v in viewModel.TimePeriods)
				{
					Console.WriteLine('\t' + string.Empty + v.Name);
				}
				Console.WriteLine();

				Console.WriteLine(@"Versions");
				foreach (var v in viewModel.Versions)
				{
					Console.WriteLine('\t' + string.Empty + v.Name);
				}
				Console.WriteLine();

				Console.WriteLine(@"Exceptions");
				foreach (var vm in viewModel.LatestExceptions)
				{
					Console.WriteLine('\t' + string.Empty + new string(vm.Message.Take(40).ToArray()) +
									  $@"... ({vm.CreatedAt.ToString(@"T")}) " + vm.User.Name + $@"({vm.Version.Name})");
				}
				Console.WriteLine();

				return;
				//SearchSourceCode();
				//return;
				//DisplayHistogram();
				//return;


				//GenerateDayReport(serverDbPath);


				//
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return;


			//var c = @"Data Source = C:\Users\PetarPetrov\Desktop\ifsa.sqlite; Version = 3;";

			//ClientData data;
			//using (var client = new TransactionContextCreator(c).Create())
			//{
			//	data = FeatureAdapter.GetData(client);
			//	client.Complete();
			//}


			//data.FeatureEntryRows.Add(new DbFeatureEntryRow(@"#", DateTime.Today.AddDays(-1), -4));


			//var s = Stopwatch.StartNew();
			//var result = ClientDataPacker.Pack(data);

			//s.Stop();
			//Console.WriteLine(s.ElapsedMilliseconds);

			//s.Restart();
			//var back = ClientDataPacker.Unpack(result);
			//s.Stop();
			//Console.WriteLine(s.ElapsedMilliseconds);
			//Console.WriteLine(back);

			//Console.WriteLine();
			//Console.WriteLine();
			//Console.WriteLine();

			//Console.WriteLine(result.Length);


			return;


			//GenerateProject(PhoenixModel(), @"C:\temp\IfsaBuilder\IfsaBuilder\Phoenix");

			//GenerateProject(WordpressModel(), @"C:\temp\IfsaBuilder\IfsaBuilder\Wordpress");

			//var prj = new ClrProject();
			//prj.Save(@"C:\temp\IfsaBuilder\IfsaBuilder\", project);

			//var AppContext = new AppContext();
			//AppContext.DbContextCreator = new TransactionContextCreator(string.Empty);
			//AppContext.ModalDialog = new ConsoleDialog();
			//var viewModel = new LoginsViewModel(AppContext, new LoginAdapter());
			//try
			//{
			//	viewModel.InsertAsync(new Login(1, @"PPetrov", @"Password", DateTime.Now, false)).Wait();

			//	Console.WriteLine(@"Done");
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex);
			//}

			//return;


			//// Register helpers
			//var cache = AppContext.DataCache;
			//cache.Add(new BrandHelper());
			//cache.Add(new FlavorHelper());
			//cache.Add(new ArticleHelper());

			//try
			//{
			//	File.WriteAllText(@"C:\temp\diagnostics.txt", string.Empty);

			//	//var viewModel = new LoginsViewModel(AppContext, new LoginAdapter());
			//	//viewModel.LoadData();

			//	//var v = new ViewModel(new Login(1, @"PPetrov", @"QWE234!", DateTime.Now, true));
			//	//var dialog = new ConsoleDialog();
			//	//viewModel.AddAsync(v, dialog).Wait();
			//	//viewModel.AddAsync(v, dialog).Wait();
			//	//viewModel.ChangePasswordAsync(v, dialog, @"sc1f1r3hack").Wait();
			//	//viewModel.AddAsync(v, dialog).Wait();
			//	//viewModel.ChangePasswordAsync(v, dialog, @"sc1f1r3hackV2").Wait();

			//	//foreach (var login in viewModel.Logins)
			//	//{
			//	//	Console.WriteLine(login.Name + " " + v.Password);
			//	//}
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine(e);
			//}
		}

		private static string GetFormatted()
		{
			var name = @"Coca Cola (UC)";
			var headers = new[] { @"Header 1", @"Header 2", @"Header 3", @"Day" };
			var subHeaders = new[] { @"Sub 1", @"Sub 2", @"S 3", @"Planned" };
			var values = new[] { "123.23", "23.17", "0", @"13" };

			var title = name + @" : ";
			var emptyTitle = new string(' ', title.Length);
			var lines = new string[3];
			lines[0] = title;
			for (var i = 1; i < lines.Length; i++)
			{
				lines[i] = emptyTitle;
			}
			for (var i = 0; i < headers.Length; i++)
			{
				var column = new[] { headers[i], subHeaders[i], values[i] };
				var width = column.Select(c => c.Length).Max();
				// Center captions
				for (var j = 0; j < lines.Length - 1; j++)
				{
					var value = column[j];
					var half = (int)Math.Floor((width - value.Length) / 2.0M);
					var space = new string(' ', half);
					var padded = (space + value + space).PadLeft(width);
					lines[j] = lines[j] + @" " + padded;
				}
				// Align right values
				for (var j = lines.Length - 1; j < lines.Length; j++)
				{
					lines[j] = lines[j] + @" " + column[j].PadLeft(width);
				}
			}

			return string.Join(Environment.NewLine, lines);
		}

		private static void Display(MapEntry[] entries)
		{
			Array.Sort(entries, (x, y) => x.Sequence.CompareTo(y.Sequence));

			foreach (var e in entries)
			{
				if (e.Name.EndsWith(e.Sequence.ToString()))
				{
					continue;
				}
				Console.WriteLine(e.Name + " : " + e.Sequence);
			}
			Console.WriteLine();
		}

		public static string GetSqliteConnectionString(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			return $@"Data Source = {path}; Version = 3;";
		}

		private static void ClearData(string path)
		{
			File.Delete(path);
			FeatureManager.CreateSchema(new TransactionContextCreator(GetSqliteConnectionString(path)).Create);
		}

		private static void ReplicateData(string clientDb, string serverDb)
		{
			Console.WriteLine(@"Load client data");
			var s = Stopwatch.StartNew();

			ClientData data;
			//using (var client = new TransactionContextCreator(clientDb).Create())
			//{
			//	data = FeatureAdapter.GetData(client);
			//	client.Complete();
			//}

			s.Stop();
			Console.WriteLine(s.ElapsedMilliseconds);

			//Replicate(serverDb, data, null);
		}

		private static void Replicate(string serverDb, ClientData data, ServerData serverData)
		{
			var versions = new[]
			{
				@"8.28.79.127", @"7.74.19.727", @"6.22.29.492", @"5.96.69.792", @"4.11.27.292", @"3.85.19.223",
			};

			using (var ctx = new TransactionContextCreator(serverDb).Create())
			{
				for (var i = 11; i < 120; i++)
				{
					var user = @"BG" + (i.ToString()).PadLeft(6, '0');

					if (_r.Next(0, 10) == 0)
					{
						continue;
					}

					//Replicate(serverDb, data, user, versions[_r.Next(versions.Length)], serverData);
					var version = versions[_r.Next(versions.Length)];

					//Replicate(serverDb, data, user, version, serverData);

					FeatureServerManager.Replicate(user, version, ctx, data, serverData);
				}
				ctx.Complete();
			}
		}

		private static void GenerateData(string path)
		{
			var contextCreator = new TransactionContextCreator(GetSqliteConnectionString(path));

			var featureManager = new FeatureManager(() => contextCreator.Create());

			if (!File.Exists(path))
			{
				// Create the schema
				FeatureManager.CreateSchema(contextCreator.Create);
			}

			// TODO : !!!
			// Generate exceptions
			// Generate more scenarios
			var s = Stopwatch.StartNew();
			//var scenarios = new[]
			//{
			//	GetUploadImageFeature(),
			//	GetDownloadImageFeature(),
			//	GetLoadImagesAsync(),
			//	GetDeleteImageAsync(),
			//	GetSetAsDefaultAsync(),
			//	GetCreateActivityData(),
			//};
			//foreach (var data in scenarios)
			//{
			//	featureManager.MarkUsageAsync(data);
			//	//featureManager.Write(data);
			//}

			var f = Feature.StartNew(@"Images", @"Upload");
			try
			{
				throw new Exception(@"Unable to display feature");

				featureManager.Save(f);
			}
			catch (Exception ex)
			{
				featureManager.Save(f, ex);
			}

			s.Stop();

			Console.WriteLine(s.ElapsedMilliseconds);
		}


		private static TimeSpan GetTime(int min, int max)
		{
			return TimeSpan.FromTicks((long)((_r.Next(min, max) + _r.NextDouble()) * 10000));
		}

		private static Random _r = new Random();


		private static void SearchSourceCode()
		{
			foreach (var f in Directory.GetFiles(@"C:\Cchbc\PhoenixClient\iOS\SFA.iOS7\", @"*.*", SearchOption.AllDirectories))
			{
				var name = Path.GetFileName(f);
				if (!name.EndsWith(@".cs", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				var contents = File.ReadAllText(f);
				if (contents.IndexOf(@"UpdateDetailsInfo", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					Console.WriteLine(f);
				}
			}
			return;
		}

		private static void SearchDirectory()
		{
			var inDir = @"C:\Cchbc\PhoenixClient\iOS\SFA.iOS7";

			foreach (var file in Directory.GetFiles(inDir, @"*.*", SearchOption.AllDirectories))
			{
				var name = Path.GetFileName(file);
				if (name.EndsWith(@".xib", StringComparison.OrdinalIgnoreCase) || name.EndsWith(@".dll", StringComparison.OrdinalIgnoreCase) || name.EndsWith(@".png", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				var contents = File.ReadAllText(file);
				if (contents.IndexOf(@"RedActivityIndexManager", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					Console.WriteLine(file);
				}
			}
		}

		private static void CopyActivity(Feature feature, string activity)
		{
			var date = SelectDateFromDialog(feature);
			if (date == null) return;

			var days = GetDays(feature, date.Value);

			var isDayActive = IsDayActive(feature, days);
			if (!isDayActive) return;

			var isDayInThePast = IsDayInThePast(feature, days);
			if (isDayInThePast) return;

			var copyActivity = activity + @"*";
			CreateActivity(feature, activity);
		}

		private static void CreateActivity(Feature feature, string activity)
		{
			{
				var visit = CreateVisit(feature);
				if (visit == null) return;

				InsertActivity(feature, visit, activity);
			}
		}

		private static void InsertActivity(Feature feature, string visit, string activity)
		{
			{
				var copy = activity + @"+" + visit;
			}
		}

		private static DateTime? SelectDateFromDialog(Feature feature)
		{
			{
				// TODO : !!! Get from the UI
				return DateTime.Today.AddDays(1);
			}
		}

		private static List<object> GetDays(Feature feature, DateTime value)
		{
			{
				Thread.Sleep(5);
				return new List<object>();
			}
		}

		private static bool IsDayActive(Feature feature, List<object> days)
		{
			{
				//Thread.Sleep(100);
				// TODO : Query the db
				return true;
			}
		}

		private static bool IsDayInThePast(Feature feature, List<object> days)
		{
			{
				// TODO : Query the db
				//Thread.Sleep(100);
				return false;
			}
		}


		private static string CreateVisit(Feature f)
		{
			var canCreateActivityForOutlet = CanCreateActivityForOutlet(f);
			if (canCreateActivityForOutlet.Type != PermissionType.Allow) return null;

			{
				var visit = GetVisit(f);
				if (visit != null) return visit;

				return InsertVisit(f);
			}
		}

		private static PermissionResult CanCreateActivityForOutlet(Feature f)
		{
			{
				{
					var assignment = "From the for outlet";
					if (assignment == null)
					{
						// TODO : Display message
						return PermissionResult.Deny("No outlet assignment");
					}
					var hasAssignment = assignment.Length > 0;
					if (!hasAssignment)
					{
						// TODO : Display message
						return PermissionResult.Deny("Invalid assignment");
					}
				}
				//Is R E D Activities Allowed
				{
					// TODO : !!!
					return PermissionResult.Allow;
				}
			}
		}

		private static string InsertVisit(Feature f)
		{
			{
				// TODO : !!!
				return @"new visit";
			}
		}

		private static string GetVisit(Feature f)
		{
			{
				// TODO : !!!
				return null;
				return "Get from db";
			}
		}

		private static void CreateSchema(string connectionString)
		{
			var creator = new TransactionContextCreator(connectionString);
			try
			{
				using (var serverContext = creator.Create())
				{
					FeatureServerManager.DropSchema(serverContext);
					serverContext.Complete();

					Console.WriteLine(@"Drop schema");
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			using (var serverContext = creator.Create())
			{
				FeatureServerManager.CreateSchema(serverContext);
				serverContext.Complete();

				Console.WriteLine(@"Schema created");
			}
		}

		private static void Replicate(string serverDb, ClientData data, string user, string version, ServerData serverData)
		{
			using (var server = new TransactionContextCreator(serverDb).Create())
			{
				FeatureServerManager.Replicate(user, version, server, data, serverData);
				server.Complete();
			}
		}

		public static void Unpack(byte[] input)
		{
			using (var ms = new MemoryStream(input))
			{
				//var id = 12L;
				//var name = "P";
				var buffer = BitConverter.GetBytes(0L);
				ms.Read(buffer, 0, buffer.Length);

				Console.WriteLine(BitConverter.ToInt64(buffer, 0));

				var len = 0;
				buffer = BitConverter.GetBytes(len);
				ms.Read(buffer, 0, buffer.Length);

				len = BitConverter.ToInt32(buffer, 0);
				Console.WriteLine(len);

				buffer = new byte[len * 2];
				ms.Read(buffer, 0, buffer.Length);

				Console.WriteLine(Encoding.Unicode.GetString(buffer));
			}
		}

		private static void GenerateProject(DbProject project, string directoryPath)
		{
			var buffer = new StringBuilder(1024 * 80);

			foreach (var entity in project.CreateEntities())
			{
				// Classes
				var entityClass = project.CreateEntityClass(entity);
				buffer.AppendLine(entityClass);
				buffer.AppendLine(EntityClass.GenerateClassViewModel(entity, !project.IsModifiable(entity.Table)));
				buffer.AppendLine(project.CreateTableViewModel(entity));

				if (!project.IsModifiable(entity.Table))
				{
					buffer.AppendLine(project.CreateTableViewModel(entity));
				}

				// Read Only adapters
				var adapter = !project.IsModifiable(entity.Table) ? project.CreateEntityAdapter(entity) : project.CreateEntityAdapter(entity);
				buffer.AppendLine(adapter);
			}

			var clrProject = new ClrProject();
			clrProject.WriteAllText = File.WriteAllText;
			clrProject.CreateDirectory = path =>
			{
				if (Directory.Exists(path))
				{
					Directory.Delete(path, true);
				}
				Directory.CreateDirectory(path);
			};
			clrProject.Save(directoryPath, project);

			var tmp = DbScript.CreateTables(project.Schema.Tables);
			Console.WriteLine(tmp);
		}

		private static DbProject PhoenixModel()
		{
			var outlets = DbTable.Create(@"Outlets", new[]
			{
				DbColumn.String(@"Name"),
			});
			var visits = DbTable.Create(@"Visits", new[]
			{
				DbColumn.ForeignKey(outlets), DbColumn.DateTime(@"Date"),
			});
			var activityTypes = DbTable.Create(@"ActivityTypes", new[]
			{
				DbColumn.String(@"Name"),
			});
			var activities = DbTable.Create(@"Activities", new[]
			{
				DbColumn.DateTime(@"Date"), DbColumn.ForeignKey(activityTypes), DbColumn.ForeignKey(visits),
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
				DbColumn.String(@"Name"), DbColumn.ForeignKey(brands), DbColumn.ForeignKey(flavors),
			});
			var activityNoteTypes = DbTable.Create(@"ActivityNoteTypes", new[]
			{
				DbColumn.String(@"Name"),
			});
			var activityNotes = DbTable.Create(@"ActivityNotes", new[]
			{
				DbColumn.String(@"Contents"), DbColumn.DateTime(@"Created_At"), DbColumn.ForeignKey(activityNoteTypes), DbColumn.ForeignKey(activities),
			});

			var schema = new DbSchema(@"Phoenix", new[]
			{
				outlets, visits, activityTypes, activities, brands, flavors, articles, activityNoteTypes, activityNotes
			});

			var project = new DbProject(schema);

			// Mark tables as Modifiable, all tables are ReadOnly by default
			project.MarkModifiable(visits);
			project.MarkModifiable(activities);
			project.MarkModifiable(activityNotes);

			// Attach Inverse tables
			project.AttachInverseTable(visits);

			// Hidden tables
			project.MarkHidden(brands);
			project.MarkHidden(flavors);
			return project;
		}

		private static DbProject WordpressModel()
		{
			var users = DbTable.Create(@"Users", new[]
			{
				DbColumn.String(@"Name"),
			});

			var blogs = DbTable.Create(@"Blogs", new[]
			{
				DbColumn.String(@"Name"), DbColumn.String(@"Description"), DbColumn.ForeignKey(users)
			});

			var posts = DbTable.Create(@"Posts", new[]
			{
				DbColumn.String(@"Title"), DbColumn.String(@"Contents"), DbColumn.DateTime(@"CreationDate"), DbColumn.ForeignKey(blogs),
			});

			var comments = DbTable.Create(@"Comments", new[]
			{
				DbColumn.String(@"Contents"), DbColumn.DateTime(@"CreationDate"), DbColumn.ForeignKey(users), DbColumn.ForeignKey(posts),
			});

			var schema = new DbSchema(@"WordPress", new[]
			{
				users, blogs, posts, comments,
			});
			var project = new DbProject(schema);

			project.AttachInverseTable(posts);

			project.MarkModifiable(blogs);
			project.MarkModifiable(posts);
			project.MarkModifiable(comments);
			return project;
		}

		private static void Zip()
		{
			var archive = new ZipArchive();

			foreach (var file in Directory.GetFiles(@"C:\Logs", @"*.*", SearchOption.AllDirectories))
			{
				Console.WriteLine("Compressing file " + file);
				using (var fs = File.OpenRead(file))
				{
					archive.AddFileAsync(file, fs, CancellationToken.None).Wait();
				}
			}

			Console.WriteLine(@"Save the file");
			using (var fs = File.OpenWrite(@"C:\temp\archive.dat"))
			{
				archive.SaveAsync(fs, CancellationToken.None).Wait();
			}


			using (var fs = File.OpenRead(@"C:\temp\archive.dat"))
			{
				archive.LoadAsync(fs, CancellationToken.None).Wait();
			}


			Console.WriteLine("Completed");
		}

		private static void ExtractArchive(byte[] data, string cTempTmp)
		{
			using (var ms = new MemoryStream(data))
			{
				using (var zip = new GZipStream(ms, CompressionMode.Decompress, true))
				{
					var buffer = new byte[8 * 1024];

					//// Add the header buffer to zip buffer
					//var bytes = Encoding.Unicode.GetBytes(header.ToString());
					//zip.Write(bytes, 0, bytes.Length);

					//// Reset the position of the data stream
					//data.Position = 0;

					//// Copy the data buffer to zip buffer
					//while ((readBytes = data.Read(buffer, 0, buffer.Length)) != 0)
					//{
					//	zip.Write(buffer, 0, readBytes);
					//}
				}
			}
		}

		private static void Display(int[,] m)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Console.Write(m[i, j]);
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}

		private static void DisplayNew(int[,] m)
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					Console.Write(m[i, j]);
				}
				Console.WriteLine();
			}
			Console.WriteLine();
		}

		private static void Floors()
		{
			var n = 0;
			foreach (var line in File.ReadAllLines(@"C:\temp\input.txt"))
			{
				var s = @"qjhvhtzxzqqjkmpb";
				//s = @"uurcxstgmygtbstg";


				s = line;


				for (var i = 0; i < s.Length - 2; i++)
				{
					var a = s[i];
					var c = s[i + 2];
					if (a == c)
					{
						//n++;
						Console.WriteLine(line);

						var pairs = new Dictionary<string, int>();
						for (var j = 0; j < s.Length; j += 2)
						{
							var x = s[j];
							var y = s[j + 1];
							var key = x + string.Empty + y;

							int value;
							if (!pairs.TryGetValue(key, out value))
							{
								pairs.Add(key, 1);
							}
							else
							{
								pairs[key] = value + 1;
							}
						}

						break;
					}
				}
			}

			Console.WriteLine(n);
		}

		private static void ToggleSet(int[,] grid, int startX, int startY, int endX, int endY, int value)
		{
			for (var i = startX; i <= endX; i++)
			{
				for (var j = startY; j <= endY; j++)
				{
					grid[i, j] += value;
					if (grid[i, j] < 0)
					{
						grid[i, j] = 0;
					}
				}
			}
		}

		private static void ToggleInvert(bool[,] grid, int startX, int startY, int endX, int endY)
		{
			for (var i = startX; i <= endX; i++)
			{
				for (var j = startY; j <= endY; j++)
				{
					grid[i, j] = !grid[i, j];
				}
			}
		}

		private static void Toggle(int[,] grid, int startX, int startY, int endX, int endY, int value)
		{
			for (var i = startX; i < endX; i++)
			{
				for (var j = startY; j < endY; j++)
				{
					grid[i, j] += value;
				}
			}
		}

		public static IEnumerable<char> ReadString(IEnumerable<char> input)
		{
			var cnt = 1;
			char? previous = null;

			foreach (var s in input)
			{
				if (!previous.HasValue)
				{
					previous = s;
					continue;
				}
				if (s != previous.Value)
				{
					yield return (char)(48 + cnt);
					yield return previous.Value;

					previous = s;
					cnt = 1;
					continue;
				}
				cnt++;
			}

			yield return (char)(48 + cnt);
			yield return previous.Value;
		}

		private static int CountFlag(string value, string flag)
		{
			var count = 0;
			var offset = 0;
			var index = value.IndexOf(flag, offset, StringComparison.OrdinalIgnoreCase);
			while (index >= 0)
			{
				count++;
				offset = index + 1;
				index = value.IndexOf(flag, offset, StringComparison.OrdinalIgnoreCase);
			}
			return count;
		}

		private static void FixAdapterClasses()
		{
			var winrtUsingFlag = @"#if NETFX_CORE
using AppSystem.SQLite;
#else
using AppSystem.Data;
#endif";

			var sqliteUsingFlag = @"#if SQLITE
using AppSystem.SQLite;
#else
using AppSystem.Data;
#endif";

			winrtUsingFlag = @"NETFX_CORE";

			var sourceFiles = Directory.GetFiles(@"C:\Cchbc\PhoenixClient\Phoenix SFA SAP\SFA 5.5\SFA.BusinessLogic", @"*.cs", SearchOption.AllDirectories);
			foreach (var file in sourceFiles)
			{
				var contents = File.ReadAllText(file);
				if (contents.IndexOf(winrtUsingFlag, StringComparison.Ordinal) >= 0)
				{
					Console.WriteLine(file);
					//File.WriteAllText(file, contents.Replace(winrtUsingFlag, sqliteUsingFlag));
				}
			}

			//throw new NotImplementedException();
		}

		private static void GenerateIncludes()
		{
			var basePath = @"..\..\..\Phoenix SFA SAP\SFA 5.5\SFA.BusinessLogic\";

			var buffer = new StringBuilder();

			var lines = File.ReadAllLines(@"C:\Cchbc\PhoenixClient\Phoenix SFA SAP\SFA 5.5\SFA.BusinessLogic\SFA.BusinessLogic.csproj");
			foreach (var line in lines)
			{
				var value = line.Trim();
				if (value.StartsWith(@"<Compile Include=") && value.EndsWith(@"/>"))
				{
					//<Compile Include="ApplicationParameters\ClientParameterProvider.cs" />
					var start = value.IndexOf('"') + 1;
					var end = value.IndexOf('"', start);
					var path = value.Substring(start, end - start);
					//Console.WriteLine(path);

					//<Compile Include="..\..\..\Phoenix SFA SAP\SFA 5.5\SFA.BusinessLogic\ApplicationParameters\ClientParameterProvider.cs">
					//  <Link>ApplicationParameters\ClientParameterProvider.cs</Link>
					//</Compile>

					buffer.Append(@"<Compile Include=");
					buffer.Append('"');
					buffer.Append(basePath);
					buffer.Append(path);
					buffer.Append('"');
					buffer.Append('>');
					buffer.AppendLine();

					buffer.Append('\t');
					buffer.Append(@"<Link>");
					buffer.Append(path);
					buffer.Append(@"</Link>");
					buffer.AppendLine();

					buffer.AppendLine(@"</Compile>");

					//Console.WriteLine(value);
				}
			}

			var tnp = buffer.ToString();
			File.WriteAllText(@"C:\temp\includes.txt", tnp);
			//Console.WriteLine(tnp);
		}

		private static void InspectFeature(Feature f)
		{
			//var buffer = new StringBuilder();

			//var ctxName = feature.Context + "(" + feature.Name + ")";
			//buffer.Append(ctxName.PadRight(25));
			//buffer.Append(' ');
			//buffer.Append(feature.Details.PadRight(12));
			//buffer.Append(' ');
			//buffer.AppendLine(feature.TimeSpent.TotalMilliseconds.ToString(CultureInfo.InvariantCulture).PadRight(6));

			//if (feature.Steps.Any())
			//{
			//	var totalMilliseconds = feature.TimeSpent.TotalMilliseconds;
			//	var remaingTime = totalMilliseconds - (feature.Steps.Select(v => v.TimeSpent.TotalMilliseconds).Sum());

			//	foreach (var s in feature.Steps.Concat(new[] { new FeatureEntryStep(@"Other", TimeSpan.FromMilliseconds(remaingTime), string.Empty) }))
			//	{
			//		buffer.Append('\t');

			//		var value = s.Name.Replace(@"Async", string.Empty);
			//		value = Regex.Replace(value, @"[A-Z]", m => @" " + m.Value).TrimStart();
			//		buffer.Append(value.PadRight(24));
			//		buffer.Append(' ');
			//		buffer.Append(s.Details.PadRight(4));
			//		buffer.Append(' ');
			//		var milliseconds = s.TimeSpent.TotalMilliseconds;
			//		var tmp = (milliseconds / totalMilliseconds) * 100;
			//		var graph = new string('-', (int)tmp);

			//		buffer.Append(milliseconds.ToString(CultureInfo.InvariantCulture).PadRight(8));
			//		buffer.Append(tmp.ToString(@"F2").PadLeft(5));
			//		buffer.Append(@"% ");
			//		buffer.AppendLine(graph);
			//	}
			//}


			//buffer.AppendLine();
			//var output = buffer.ToString();
			//Debug.WriteLine(output);
			//Console.WriteLine(output);

			//File.AppendAllText(@"C:\temp\diagnostics.txt", output);
		}
	}


}

