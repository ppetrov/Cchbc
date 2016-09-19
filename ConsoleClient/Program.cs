using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cchbc.App.ArticlesModule.Helpers;
using Cchbc.AppBuilder;
using Cchbc.AppBuilder.DDL;
using Cchbc.Archive;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Features.Data;
using Cchbc.Features.ExceptionsModule;
using Cchbc.Features.Replication;
using Cchbc.Validation;
using Cchbc.Weather;
using ConsoleClient;

namespace Cchbc.ConsoleClient
{
	public sealed class ConsoleDialog : IModalDialog
	{
		public Task<DialogResult> ShowAsync(string message, Feature feature, DialogType? type = null)
		{
			return Task.FromResult(DialogResult.Cancel);
		}
	}

	public sealed class HotPathFinder
	{
		public void FindHotPath()
		{
			// TODO : !!! We need data mine & analyze
		}
	}

	public sealed class ServerProfiler
	{

	}

	public sealed class Profiler
	{
		public void Analyze(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));


			// By version - latest
			// By steps
			// * By date - last day

			// Create report by user on the server
			// PPetrov Top 5 slowest features
			// PPetrov Top 5 most used features
		}
	}

	public sealed class ProfilerReport
	{

	}





	public sealed class TimeEntry
	{
		public DateTime Date { get; }

		public TimeEntry(DateTime date)
		{
			this.Date = date;
		}
	}

	public class Program
	{
		public static string GetSqliteConnectionString(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			return $@"Data Source = {path}; Version = 3;";
		}

		//private static void Display(FeatureReport report)
		//{
		//	Console.WriteLine(@"SlowestFeatures");
		//	foreach (var featureTime in report.SlowestFeatures)
		//	{
		//		Console.WriteLine("\t" + featureTime.Id + " : " + featureTime.Avg + '\t' + featureTime.Count);
		//	}

		//	Console.WriteLine(@"MostUsedFeatures");
		//	foreach (var featureTime in report.MostUsedFeatures)
		//	{
		//		Console.WriteLine("\t" + featureTime.Id + " : " + '\t' + featureTime.Count);
		//	}

		//	Console.WriteLine(@"LeastUsedFeatures");
		//	foreach (var featureTime in report.LeastUsedFeatures)
		//	{
		//		Console.WriteLine("\t" + featureTime.Id + " : " + '\t' + featureTime.Count);
		//	}
		//}

		public static void GenerateDayReport(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));

			var contextCreator = new TransactionContextCreator(GetSqliteConnectionString(path));

			//using (var context = contextCreator.Create())
			//{
			//	var settings = new FeatureReportSettings { SlowestFeatures = 3, MostUsedFeatures = 3, LeastUsedFeatures = 3 };

			//	var s = Stopwatch.StartNew();

			//	List<FeatureReport> reports = null;
			//	for (var i = 0; i < 1; i++)
			//	{
			//		reports = FeatureAnalyzer.GetFeatureReport(context, settings, DateTime.Today);
			//		//reports = new List<FeatureReport>(1) { FeatureAnalyzer.GetFeatureReport(context, settings, DateTime.Today, 2, 1) };
			//	}
			//	Console.WriteLine(s.ElapsedMilliseconds);

			//	foreach (var report in reports)
			//	{
			//		Display(report);
			//		Console.WriteLine();
			//	}

			//	context.Complete();
			//}
		}



		static void Main(string[] args)
		{
			var clientDbPath = @"C:\Users\PetarPetrov\Desktop\features.sqlite";
			var serverDbPath = @"C:\Users\PetarPetrov\Desktop\server.sqlite";

			try
			{
				//GenerateData(clientDbPath);
				//return;

				//WeatherTest();
				//return;

				//CreateSchema(GetSqliteConnectionString(serverDbPath));
				//return;

				var w = Stopwatch.StartNew();
				//GenerateData(clientDbPath);
				//ReplicateData(GetSqliteConnectionString(clientDbPath), GetSqliteConnectionString(serverDbPath));
				//ClearData(clientDbPath);
				//w.Stop();
				//Console.WriteLine(w.ElapsedMilliseconds);
				//return;


				var viewModel = new ExceptionsViewModel(
					new TransactionContextCreator(GetSqliteConnectionString(serverDbPath)),
					new ExceptionsSettings());

				for (var i = 0; i < 100; i++)
				{
					viewModel.Load(
					ExceptionsDataProvider.GetTimePeriods,
					ExceptionsDataProvider.GetVersions,
					ExceptionsDataProvider.GetExceptions,
					ExceptionsDataProvider.GetExceptionsCounts);
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
					Console.WriteLine('\t' + string.Empty + new string(vm.Message.Take(40).ToArray()) + $@"... ({vm.CreatedAt.ToString(@"T")}) " + vm.User.Name + $@"({vm.Version.Name})");
				}
				Console.WriteLine();

				return;
				//SearchSourceCode();
				//return;
				//DisplayHistogram();
				//return;



				GenerateDayReport(serverDbPath);



				//
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return;




			var c = @"Data Source = C:\Users\PetarPetrov\Desktop\ifsa.sqlite; Version = 3;";

			ClientData data;
			using (var client = new TransactionContextCreator(c).Create())
			{
				data = FeatureAdapter.GetDataAsync(client).Result;
				client.Complete();
			}

			data.StepRows.Add(new DbFeatureStepRow(23, @"Apply filter"));

			data.FeatureEntryRows.Add(new DbFeatureEntryRow(17, 123.456, @"#", DateTime.Today.AddDays(-1), -4));


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

			//var core = new Core();
			//core.ContextCreator = new TransactionContextCreator(string.Empty);
			//core.ModalDialog = new ConsoleDialog();
			//var viewModel = new LoginsViewModel(core, new LoginAdapter());
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
			//var cache = core.DataCache;
			//cache.Add(new BrandHelper());
			//cache.Add(new FlavorHelper());
			//cache.Add(new ArticleHelper());

			//try
			//{
			//	File.WriteAllText(@"C:\temp\diagnostics.txt", string.Empty);

			//	//var viewModel = new LoginsViewModel(core, new LoginAdapter());
			//	//viewModel.LoadData();

			//	//var v = new LoginViewModel(new Login(1, @"PPetrov", @"QWE234!", DateTime.Now, true));
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



		private static void ClearData(string path)
		{
			File.Delete(path);
			new FeatureManager { ContextCreator = new TransactionContextCreator(GetSqliteConnectionString(path)) }.CreateSchemaAsync().Wait();
		}

		private static void WeatherTest()
		{
			var forecaAppKey = @"p4ktTTTHknRT9ZbQTROIuwnfw";
			var weathers = ForecaWeather.GetWeatherAsync(new ForecaCityLocation(42.7, 23.32)).Result;

			foreach (var weather in weathers)
			{
				Console.WriteLine(weather);
			}
		}

		private static void ReplicateData(string clientDb, string serverDb)
		{
			Console.WriteLine(@"Load client data");
			var s = Stopwatch.StartNew();

			ClientData data;
			using (var client = new TransactionContextCreator(clientDb).Create())
			{
				data = FeatureAdapter.GetDataAsync(client).Result;
				client.Complete();
			}

			s.Stop();
			Console.WriteLine(s.ElapsedMilliseconds);

			var versions = new[]
			{
				@"8.28.79.127",
				@"7.74.19.727",
				@"6.22.29.492",
				@"5.96.69.792",
				@"4.11.27.292",
				@"3.85.19.223",
			};

			for (var i = 11; i < 120; i++)
			{
				var user = @"BG" + (i.ToString()).PadLeft(6, '0');

				if (_r.Next(0, 10) == 0)
				{
					continue;
				}
				Replicate(serverDb, data, user, versions[_r.Next(versions.Length)]);
			}
		}

		private static void GenerateData(string path)
		{
			var contextCreator = new TransactionContextCreator(GetSqliteConnectionString(path));

			var featureManager = new FeatureManager { ContextCreator = contextCreator };

			if (!File.Exists(path))
			{
				// Create the schema
				featureManager.CreateSchemaAsync().Wait();
			}

			// Load the manager
			featureManager.LoadAsync().Wait();

			// TODO : !!!
			// Generate exceptions
			// Generate more scenarios
			var s = Stopwatch.StartNew();
			var scenarios = new[]
			{
				GetUploadImageFeature(),
				GetDownloadImageFeature(),
				GetLoadImagesAsync(),
				GetDeleteImageAsync(),
				GetSetAsDefaultAsync(),
				GetCreateActivityData(),
			};
			foreach (var data in scenarios)
			{
				//featureManager.MarkUsageAsync(data).Wait();
				//featureManager.WriteAsync(data).Wait();

				break;
			}

			var f = Feature.StartNew(@"Images", @"Upload");
			try
			{
				using (f.NewStep(@"Browse"))
				{

				}
				using (f.NewStep(@"Resize"))
				{
					using (f.NewStep(@"Load client size from db"))
					{
					}
					using (f.NewStep(@"Resize image to client size"))
					{
					}
				}
				using (f.NewStep(@"Compress"))
				{
					using (f.NewStep(@"Compress as JPG"))
					{
					}
					using (f.NewStep(@"Compress as PNG"))
					{
					}
					using (f.NewStep(@"Choose the smallest"))
					{
					}
					using (f.NewStep(@"Load max image size allowed from db"))
					{
					}
					using (f.NewStep(@"Verify against max size allowed"))
					{
					}
				}

				DisplayFeature(f);

				throw new Exception(@"Unable to display feature");

				featureManager.WriteAsync(f).Wait();
			}
			catch (Exception ex)
			{
				featureManager.WriteExceptionAsync(f, ex).Wait();
			}

			s.Stop();

			Console.WriteLine(s.ElapsedMilliseconds);
		}

		private static FeatureData GetUploadImageFeature()
		{
			var steps = new[]
			{
				new FeatureStepData(@"BrowseImageAsync", 1, GetTime(5, 17)),
				new FeatureStepData(@"AdjustImageAsync", 1, GetTime(50, 117)),
				new FeatureStepData(@"CompressImageAsync", 1, GetTime(11, 33)),
				new FeatureStepData(@"ResizeImageAsync", 1, GetTime(68, 98)),
				new FeatureStepData(@"CheckSize", 1, GetTime(1, 2)),
				new FeatureStepData(@"ConfirmSize", 1, GetTime(23, 54)),
				new FeatureStepData(@"UploadDataAsync", 1, GetTime(231, 541)),
			};
			return new FeatureData(@"Images", "UploadImageAsync",
				TimeSpan.FromMilliseconds(steps.Select(t => t.TimeSpent.TotalMilliseconds).Sum() + _r.Next(17, 23)), steps);
		}

		private static FeatureData GetDownloadImageFeature()
		{
			var steps = new[]
			{
				new FeatureStepData(@"DownloadImageDataFromService", 1, GetTime(100, 200)),
				new FeatureStepData(@"SaveImageToDb", 1, GetTime(7, 23)),
			};
			return new FeatureData(@"Images", "DownloadImageAsync",
				TimeSpan.FromMilliseconds(steps.Select(t => t.TimeSpent.TotalMilliseconds).Sum() + _r.Next(17, 23)), steps);
		}

		private static FeatureData GetLoadImagesAsync()
		{
			var steps = new[]
			{
				new FeatureStepData(@"LoadImagesFromDb", 1, GetTime(250, 350)),
			};
			return new FeatureData(@"Images", "LoadImagesAsync",
				TimeSpan.FromMilliseconds(steps.Select(t => t.TimeSpent.TotalMilliseconds).Sum() + _r.Next(17, 23)), steps);
		}

		private static FeatureData GetDeleteImageAsync()
		{
			var steps = new[]
			{
				new FeatureStepData(@"ConfirmDelete", 1, GetTime(1, 20)),
				new FeatureStepData(@"CheckDeleteDefault", 1, GetTime(1, 20)),
				new FeatureStepData(@"DeleteImageFromService", 1, GetTime(375, 951)),
				new FeatureStepData(@"DeleteImageFromDb", 1, GetTime(15, 50)),
			};
			return new FeatureData(@"Images", "DeleteImageAsync",
				TimeSpan.FromMilliseconds(steps.Select(t => t.TimeSpent.TotalMilliseconds).Sum() + _r.Next(17, 23)), steps);
		}

		private static FeatureData GetSetAsDefaultAsync()
		{
			var steps = new[]
			{
				new FeatureStepData(@"CheckDefault", 1, GetTime(5, 17)),
				new FeatureStepData(@"SetAsDefaultFromService", 1, GetTime(564, 700)),
				new FeatureStepData(@"UpdateImageFromDb", 1, GetTime(20, 30)),
			};
			return new FeatureData(@"Images", "SetAsDefaultAsync",
				TimeSpan.FromMilliseconds(steps.Select(t => t.TimeSpent.TotalMilliseconds).Sum() + _r.Next(17, 23)), steps);
		}

		private static FeatureData GetCreateActivityData()
		{
			var steps = new[]
			{
				new FeatureStepData(@"ValidateActiveDay", 1, GetTime(20, 30)),
				new FeatureStepData(@"ValidateOldDays", 1, GetTime(20, 30)),
				new FeatureStepData(@"ValidateOutletAssignment", 1, GetTime(10, 20)),
				new FeatureStepData(@"ValidateActivityTypesByOutlet", 1, GetTime(100, 300)),
				new FeatureStepData(@"CreateVisit", 1, GetTime(50, 220)),
				new FeatureStepData(@"CreateVisitActivity", 1, GetTime(50, 220)),
				new FeatureStepData(@"Create", 1, GetTime(50, 220)),
			};
			return new FeatureData(@"Images", "SetAsDefaultAsync",
				TimeSpan.FromMilliseconds(steps.Select(t => t.TimeSpent.TotalMilliseconds).Sum() + _r.Next(17, 23)), steps);
		}


		private static TimeSpan GetTime(int min, int max)
		{
			return TimeSpan.FromTicks((long)((_r.Next(min, max) + _r.NextDouble()) * 10000));
		}

		static Random _r = new Random();




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

		private static void DisplayFeature(Feature feature)
		{
			Console.WriteLine();
			Console.WriteLine(feature.Name.PadRight(45) + feature.TimeSpent.TotalMilliseconds.ToString(@"F3") + @"ms");

			var max = feature.Steps.Select(v => v.TimeSpent.TotalMilliseconds).Max();
			var percent = max / 100;

			foreach (var step in feature.Steps)
			{
				var offset = string.Empty;
				for (int i = 0; i < step.Level; i++)
				{
					offset += @"   ";
				}

				var value = offset + step.Name;
				var ms = step.TimeSpent.TotalMilliseconds;
				var times = (int)(ms / percent) / 2;
				Console.WriteLine(value.PadRight(45) + ms.ToString(@"F3") + " ms " + new string('-', times));
			}

			Console.WriteLine();
		}

		private static void DisplayHistogram()
		{
			// Histohram by hour
			var hours = new int[24];

			// Create & initialize with random data
			var r = new Random(0);
			var entries = new TimeEntry[1024];
			for (var i = 0; i < entries.Length; i++)
			{
				entries[i] =
					new TimeEntry(DateTime.Today.AddSeconds(r.Next(0, Convert.ToInt32(TimeSpan.FromDays(1).TotalSeconds))));
			}

			// Create the histogram
			foreach (var e in entries)
			{
				hours[e.Date.Hour]++;
			}

			// Display the data as bar chart
			var bars = new List<string>();

			var min = hours.Min() - 1;
			var limit = hours.Max() - min + 3;
			foreach (var hour in hours)
			{
				bars.Add(new string(' ', limit));

				var cnt = hour - min;
				var line = new string('*', cnt).PadRight(limit, ' ');
				bars.Add(line);

				bars.Add(new string(' ', limit));
			}
			var lines = new List<string>();
			for (var i = 0; i < limit; i++)
			{
				var buffer = new StringBuilder(bars.Count);

				foreach (var bar in bars)
				{
					buffer.Append(bar[i]);
				}

				lines.Add(buffer.ToString());
			}

			for (var i = lines.Count - 1; i >= 0; i--)
			{
				Console.WriteLine(lines[i]);
			}

			return;
		}

		private static void SearchDirectory()
		{
			var inDir = @"C:\Cchbc\PhoenixClient\iOS\SFA.iOS7";

			foreach (var file in Directory.GetFiles(inDir, @"*.*", SearchOption.AllDirectories))
			{
				var name = Path.GetFileName(file);
				if (name.EndsWith(@".xib", StringComparison.OrdinalIgnoreCase) ||
					name.EndsWith(@".dll", StringComparison.OrdinalIgnoreCase) ||
					name.EndsWith(@".png", StringComparison.OrdinalIgnoreCase))
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
			using (feature.NewStep(nameof(CreateActivity)))
			{
				var visit = CreateVisit(feature);
				if (visit == null) return;

				InsertActivity(feature, visit, activity);
			}
		}

		private static void InsertActivity(Feature feature, string visit, string activity)
		{
			using (feature.NewStep(nameof(InsertActivity)))
			{
				var copy = activity + @"+" + visit;
			}
		}

		private static DateTime? SelectDateFromDialog(Feature feature)
		{
			using (feature.NewStep(nameof(SelectDateFromDialog)))
			{
				// TODO : !!! Get from the UI
				return DateTime.Today.AddDays(1);
			}
		}

		private static List<object> GetDays(Feature feature, DateTime value)
		{
			using (feature.NewStep(nameof(GetDays)))
			{
				Thread.Sleep(5);
				return new List<object>();
			}
		}

		private static bool IsDayActive(Feature feature, List<object> days)
		{
			using (feature.NewStep(nameof(IsDayActive)))
			{
				//Thread.Sleep(100);
				// TODO : Query the db
				return true;
			}
		}

		private static bool IsDayInThePast(Feature feature, List<object> days)
		{
			using (feature.NewStep(nameof(IsDayInThePast)))
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

			using (f.NewStep(nameof(CreateVisit)))
			{
				var visit = GetVisit(f);
				if (visit != null) return visit;

				return InsertVisit(f);
			}
		}

		private static PermissionResult CanCreateActivityForOutlet(Feature f)
		{
			using (f.NewStep(nameof(CanCreateActivityForOutlet)))
			{
				using (f.NewStep(@"IsOutletAssignmentValid"))
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
				using (f.NewStep(@"IsREDActivitiesAllowed"))
				{
					// TODO : !!!
					return PermissionResult.Allow.Result;
				}
			}
		}

		private static string InsertVisit(Feature f)
		{
			using (f.NewStep(nameof(InsertVisit)))
			{
				// TODO : !!!
				return @"new visit";
			}
		}

		private static string GetVisit(Feature f)
		{
			using (f.NewStep(nameof(GetVisit)))
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
					FeatureServerManager.DropSchemaAsync(serverContext);
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
				FeatureServerManager.CreateSchemaAsync(serverContext);
				serverContext.Complete();

				Console.WriteLine(@"Schema created");
			}
		}

		private static void Replicate(string serverDb, ClientData data, string user, string version)
		{
			//Console.WriteLine($@"Replicate '{user}' & {version}");

			var s = Stopwatch.StartNew();
			using (var server = new TransactionContextCreator(serverDb).Create())
			{
				FeatureServerManager.ReplicateAsync(user, version, server, data).Wait();
				server.Complete();
			}

			s.Stop();
			Console.WriteLine(s.ElapsedMilliseconds);
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
				var adapter = !project.IsModifiable(entity.Table)
					? project.CreateEntityAdapter(entity)
					: project.CreateEntityAdapter(entity);
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
				DbColumn.DateTime(@"Created_At"),
				DbColumn.ForeignKey(activityNoteTypes),
				DbColumn.ForeignKey(activities),
			});

			var schema = new DbSchema(@"Phoenix", new[]
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
				DbColumn.String(@"Name"),
				DbColumn.String(@"Description"),
				DbColumn.ForeignKey(users)
			});

			var posts = DbTable.Create(@"Posts", new[]
			{
				DbColumn.String(@"Title"),
				DbColumn.String(@"Contents"),
				DbColumn.DateTime(@"CreationDate"),
				DbColumn.ForeignKey(blogs),
			});

			var comments = DbTable.Create(@"Comments", new[]
			{
				DbColumn.String(@"Contents"),
				DbColumn.DateTime(@"CreationDate"),
				DbColumn.ForeignKey(users),
				DbColumn.ForeignKey(posts),
			});

			var schema = new DbSchema(@"WordPress", new[]
			{
				users,
				blogs,
				posts,
				comments,
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
using System.SQLite;
#else
using System.Data;
#endif";

			var sqliteUsingFlag = @"#if SQLITE
using System.SQLite;
#else
using System.Data;
#endif";

			winrtUsingFlag = @"NETFX_CORE";

			var sourceFiles = Directory.GetFiles(@"C:\Cchbc\PhoenixClient\Phoenix SFA SAP\SFA 5.5\SFA.BusinessLogic", @"*.cs",
				SearchOption.AllDirectories);
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

		private async Task<BrandHelper> LoadBrandsAsync(Feature feature)
		{
			var brandHelper = new BrandHelper();

			using (feature.NewStep(nameof(LoadBrandsAsync)))
			{
				// TODO : Load the brand helper
			}

			return brandHelper;
		}

		private async Task<FlavorHelper> LoadFlavorsAsync(Feature feature)
		{
			var flavorHelper = new FlavorHelper();

			using (feature.NewStep(nameof(LoadFlavorsAsync)))
			{
				// TODO : Load the flavor helper
			}

			return flavorHelper;
		}

		private async Task<ArticleHelper> LoadArticlesAsync(Feature feature, BrandHelper brandHelper, FlavorHelper flavorHelper)
		{
			var articleHelper = new ArticleHelper();

			using (var step = feature.NewStep(nameof(LoadArticlesAsync)))
			{
				// TODO : Load the articles helper
			}

			return articleHelper;
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















}

