﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using Atos.Client.Archive;
using Atos.Client.Data;
using Atos.Client.Features;
using Atos.Client.Features.Data;
using Atos.Client.Validation;
using Atos.ConsoleClient;
using Atos.iFSA.Objects;
using Atos.iFSA;
using Atos.Architecture;

namespace ConsoleClient
{
	public static class ClientDataReplication
	{
		public static string GetSqliteConnectionString(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			return $@"Data Source = {path}; Version = 3;";
		}

		private static DateTime GetRandomDate(Random r, DateTime fromDate, DateTime toDate)
		{
			return fromDate.Add(TimeSpan.FromSeconds(r.Next((int)(toDate - fromDate).TotalSeconds)));
		}
	}


	public sealed class DbVersion
	{
		public static readonly DbVersion Outlet = new DbVersion(@"OUTLET_PICTURES", 1);
		public static readonly DbVersion Activity = new DbVersion(@"COOLER_PLACEMENT_PICTURES", 1);
		public static readonly DbVersion Activation = new DbVersion(@"ACTIVATION_PICTURES", 1);

		public string Name { get; }
		public int Value { get; }

		public DbVersion(string name, int value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
			this.Value = value;
		}

		public bool IsUpgradeNeeded()
		{
			foreach (var version in new[] { Outlet, Activity, Activation })
			{
				if (this.Name.Equals(version.Name, StringComparison.OrdinalIgnoreCase))
				{
					if (this.Value < version.Value)
					{
						return true;
					}
					break;
				}
			}
			return false;
		}
	}

	public class Program
	{
		public static readonly string DbPrefix = @"obppc_db_";




		public static void Main(string[] args)
		{
			var freshDbPath = @"C:\Users\PetarPetrov\Desktop\data.sqlite";
			var clientDbPath = @"C:\Users\PetarPetrov\Desktop\features.sqlite";
			var serverDbPath = @"C:\Users\PetarPetrov\Desktop\server.sqlite";

			try
			{

				var pth = @"C:\Users\PetarPetrov\AppData\Local\Packages\FSAforWindows8_g87ygcespgq1j\LocalState\ifsa.sqlite";
				var data = File.ReadAllBytes(pth);
				var dbString = Convert.ToBase64String(data);
				var l = dbString.Length;
				//var rules = new[]
				//{
				//	new SourceCodeRule(@"Class must be sealed or abstract", cts =>
				//	{
				//		return cts.IndexOf(@"public class", 0, StringComparison.OrdinalIgnoreCase) >= 0;
				//	}),

				//	new SourceCodeRule(@"No structs", cts =>
				//	{
				//		return cts.IndexOf(@" struct ", 0, StringComparison.OrdinalIgnoreCase) >= 0;
				//	}),

				//	new SourceCodeRule(@"Interface must start with an 'I'", cts =>
				//	{
				//		var flag = @"public interface ";
				//		var index = cts.IndexOf(flag, StringComparison.OrdinalIgnoreCase);
				//		if (index >= 0)
				//		{
				//			var name = cts.Substring(index + flag.Length);
				//			return !name.StartsWith(@"I", StringComparison.OrdinalIgnoreCase);
				//		}
				//		return false;
				//	}),

				//	new SourceCodeRule(@"Only one class/enum/interface per file", cts =>
				//	{
				//		var definitions = 0;

				//		foreach (var flag in new[]
				//		{
				//			@"public sealed class ",
				//			@"public interface ",
				//			@"public enum ",
				//		})
				//		{
				//			definitions += Count(cts, flag);
				//			if (definitions > 1)
				//			{
				//				return true;
				//			}
				//		}

				//		return definitions > 1;
				//	}),
				//};


				var sw = Stopwatch.StartNew();

				var projectFile = @"C:\Sources\Atos\Atos.iFSA\Atos.iFSA.csproj";
				projectFile = @"C:\Sources\Atos\Atos.Client\Atos.Client.csproj";
				//projectFile = @"C:\Cchbc\PhoenixClient\Metro\iFSA Metro 8.1\iFSA Metro\iFSA.csproj";

				var rules = SourceCodeRules.General;
				var sourceProject = GetSourceProject(projectFile);

				sourceProject.Apply(rules);


				sw.Stop();
				Console.WriteLine(sw.ElapsedMilliseconds);

				foreach (var rule in rules)
				{
					Console.WriteLine(@" - " + rule.Name);
					foreach (var violation in rule.Violations)
					{
						Console.WriteLine("\t- " + violation.Filename);
					}
					Console.WriteLine();
				}

				return;

				//var dataUser = default(User);

				//var c = Convert.ToBase64String(File.ReadAllBytes(@"C:\Users\PetarPetrov\Desktop\burn.jpg"));
				//Console.WriteLine(c);
				//var provider = UserDataProvider.GetUsers(null);
				//var h = DataHelper.GetTradeChannel(null, null, null);


				//public static TradeChannel GetTradeChannel(IDbContext context, DataCache cache, Outlet outlet)
				//{
				//	if (context == null) throw new ArgumentNullException(nameof(context));
				//	if (cache == null) throw new ArgumentNullException(nameof(cache));
				//	if (outlet == null) throw new ArgumentNullException(nameof(outlet));

				//	TradeChannel result;
				//	cache.GetValues<TradeChannel>(context).TryGetValue(-1, out result);

				//	return result ?? TradeChannel.Empty;
				//}

				//public static SubTradeChannel GetSubTradeChannel(IDbContext context, DataCache cache, Outlet outlet)
				//{
				//	if (context == null) throw new ArgumentNullException(nameof(context));
				//	if (cache == null) throw new ArgumentNullException(nameof(cache));
				//	if (outlet == null) throw new ArgumentNullException(nameof(outlet));

				//	SubTradeChannel result;
				//	cache.GetValues<SubTradeChannel>(context).TryGetValue(-1, out result);

				//	return result ?? SubTradeChannel.Empty;
				//}

				//FeatureDataReplicaSimulation.Replicate();

				//Console.WriteLine(ExtractContractField(@"comment"));
				//Console.WriteLine(ExtractContractField(@"h_t_outlets_name2"));
				//Console.WriteLine(ExtractContractField(@"h_atp_tax1_no"));
				//Console.WriteLine(ExtractContractField(@"contact_persons_2"));

				//Console.WriteLine(data);
				//foreach (var f in Directory.GetFiles(@"C:\Users\PetarPetrov\Desktop\HR_Refresher log"))
				//{
				//	if (f.IndexOf(".20170531", StringComparison.OrdinalIgnoreCase) < 0)
				//	{
				//		continue;
				//	}
				//	var contents = File.ReadAllText(f);
				//	var start = contents.IndexOf(@"Activation(", StringComparison.OrdinalIgnoreCase);
				//	if (start >= 0)
				//	{
				//		Console.WriteLine(f);
				//		//var end = contents.IndexOf(@"Alcohol Licenses", start, StringComparison.OrdinalIgnoreCase);
				//		//if (end >= 0)
				//		//{
				//		//	var val = contents.Substring(start, end - start);
				//		//}
				//	}
				//}
				//FeatureDataReplicaSimulation.Replicate();
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


				//var viewModel =
				//	new ExceptionsViewModel(new TransactionContextCreator(GetSqliteConnectionString(serverDbPath)).Create,
				//		ExceptionsSettings.Default);

				for (var i = 0; i < 100; i++)
				{
					//viewModel.Load(ExceptionsDataProvider.GetTimePeriods, ExceptionsDataProvider.GetVersions,
					//	ExceptionsDataProvider.GetExceptions, ExceptionsDataProvider.GetExceptionsCounts);
				}


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

		private static SourceCodeProject GetSourceProject(string projectFilePath)
		{
			var files = new List<SourceCodeFile>();

			var rootNamespace = string.Empty;
			var projectDirectory = Path.GetDirectoryName(projectFilePath);

			foreach (var line in File.ReadAllLines(projectFilePath))
			{
				var value = line.Trim();
				var flag = @"<RootNamespace>";
				var index = value.IndexOf(flag, StringComparison.OrdinalIgnoreCase);
				if (index >= 0)
				{
					var start = index + flag.Length;
					var end = value.IndexOf(@"<", start + 1, StringComparison.OrdinalIgnoreCase);
					rootNamespace = value.Substring(start, end - start);
				}
				if (value.StartsWith(@"<Compile Include=", StringComparison.OrdinalIgnoreCase))
				{
					var startIndex = value.IndexOf('"') + 1;
					var endIndex = value.LastIndexOf('"');
					var filename = value.Substring(startIndex, endIndex - startIndex);
					var filePath = Path.Combine(projectDirectory, filename);
					var contents = File.ReadAllText(filePath);

					files.Add(new SourceCodeFile(rootNamespace, filename, contents));
				}
			}

			return new SourceCodeProject(projectFilePath, rootNamespace, files);
		}

		private static int Count(string cts, string flag)
		{
			var classes = 0;

			var index = 0;
			while (index >= 0)
			{
				index = cts.IndexOf(flag, index, StringComparison.OrdinalIgnoreCase);
				if (index < 0) break;
				index++;
				classes++;
			}

			return classes;
		}

		private static void ApplyRules(string filename, string[] lines)
		{
			//All classes must be sealed or abstract
			foreach (var line in lines)
			{
				var value = line.Trim();
				if (value.StartsWith(@"public class"))
				{
					Console.WriteLine(@"ERROR:" + filename);
				}
			}
		}

		public static TResult[] Process<TItem, TResult>(List<TItem> items, Func<TItem, TResult> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var sharedResults = new ConcurrentQueue<TResult>();
			var sharedItems = new ConcurrentQueue<TItem>(items);

			using (var syncEvent = new CountdownEvent(Math.Min(items.Count, 8)))
			{
				var parameters = new object[] { syncEvent, sharedItems, sharedResults, selector };

				for (var i = 0; i < syncEvent.InitialCount; i++)
				{
					ThreadPool.QueueUserWorkItem(_ =>
					{
						var args = _ as object[];
						var e = args[0] as CountdownEvent;
						var localItems = args[1] as ConcurrentQueue<TItem>;
						var localResults = args[2] as ConcurrentQueue<TResult>;
						var localSelector = args[3] as Func<TItem, TResult>;
						try
						{
							TItem item;
							while (localItems.TryDequeue(out item))
							{
								localResults.Enqueue(localSelector(item));
							}
						}
						finally
						{
							e.Signal();
						}
					}, parameters);
				}
				syncEvent.Wait();
			}

			return sharedResults.ToArray();
		}

		private static DbVersion[] GetVersions()
		{
			throw new NotImplementedException();
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

		public static string GetSqliteConnectionString(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			return $@"Data Source = {path}; Version = 3;";
		}

		private static void ClearData(string path)
		{
			File.Delete(path);
			Atos.Client.Features.FeatureManager.CreateSchema(new TransactionContextCreator(GetSqliteConnectionString(path)).Create);
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

		private static void GenerateData(string path)
		{
			var contextCreator = new TransactionContextCreator(GetSqliteConnectionString(path));

			var featureManager = new Atos.Client.Features.FeatureManager(() => contextCreator.Create());

			if (!File.Exists(path))
			{
				// Create the schema
				Atos.Client.Features.FeatureManager.CreateSchema(contextCreator.Create);
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

			var f = new Feature(@"Images", @"Upload");
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
			foreach (var f in Directory.GetFiles(@"C:\Atos\PhoenixClient\iOS\SFA.iOS7\", @"*.*", SearchOption.AllDirectories))
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
			var inDir = @"C:\Atos\PhoenixClient\iOS\SFA.iOS7";

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
			//var creator = new TransactionContextCreator(connectionString);
			//try
			//{
			//	using (var serverContext = creator.Create())
			//	{
			//		FeatureManager.DropSchema(serverContext);
			//		serverContext.Complete();

			//		Console.WriteLine(@"Drop schema");
			//	}
			//}
			//catch (Exception e)
			//{
			//	Console.WriteLine(e);
			//}

			//using (var serverContext = creator.Create())
			//{
			//	FeatureManager.CreateSchema(serverContext);
			//	serverContext.Complete();

			//	Console.WriteLine(@"Schema created");
			//}
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

			var sourceFiles = Directory.GetFiles(@"C:\Atos\PhoenixClient\Phoenix SFA SAP\SFA 5.5\SFA.BusinessLogic", @"*.cs", SearchOption.AllDirectories);
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

			var lines = File.ReadAllLines(@"C:\Atos\PhoenixClient\Phoenix SFA SAP\SFA 5.5\SFA.BusinessLogic\SFA.BusinessLogic.csproj");
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
