using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Cchbc.App.ArticlesModule.Helpers;
using Cchbc.AppBuilder;
using Cchbc.AppBuilder.DDL;
using Cchbc.Archive;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Features.Db;
using Cchbc.Features.Db.Managers;
using LoginModule.Adapter;
using LoginModule.Objects;
using LoginModule.ViewModels;


namespace Cchbc.ConsoleClient
{
	public sealed class ConsoleDialog : IModalDialog
	{
		public Task<DialogResult> ShowAsync(string message, Feature feature, DialogType? type = null)
		{
			return Task.FromResult(DialogResult.Cancel);
		}
	}

	public class Program
	{
		static void Main(string[] args)
		{
			var serverDb = @"Data Source = C:\Users\codem\Desktop\server.sqlite; Version = 3;";
			var clientDb = @"Data Source = C:\Users\codem\Desktop\ppetrov.sqlite; Version = 3;";

			var si = clientDb.LastIndexOf('\\') + 1;
			var ei = clientDb.IndexOf('.', si);
			var userName = clientDb.Substring(si, ei - si);
			try
			{
				//DropSchema(serverDb);
				//CreateSchema(serverDb);

				using (var server = new TransactionContextCreator(serverDb).Create())
				{
					using (var client = new TransactionContextCreator(clientDb).Create())
					{
						var w = Stopwatch.StartNew();
						
						DbFeatureServerManager.Replicate(server, client, @"ppetrov");

						client.Complete();
						server.Complete();
						w.Stop();
						Console.WriteLine(w.ElapsedMilliseconds);
					}
				}



				//var client = GetClient(clientDb);

				//using (var serverContext = new TransactionContextCreator(serverDb).Create())
				//{
				//	var server = new DbFeatureServerManager();
				//	server.Load(serverContext);

				//	List<FeatureEntryRow> rows;
				//	List<FeatureEntryStepRow> rows2;
				//	using (var context = new TransactionContextCreator(clientDb).Create())
				//	{
				//		rows = client.GetFeatureEntries(context);
				//		rows2 = client.GetFeatureEntrySteps(context);
				//	}

				//	server.SaveClientData(serverContext, userName, client, rows, rows2);
				//	serverContext.Complete();

				//	Console.WriteLine(@"Done");
				//}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return;



			//GenerateProject(PhoenixModel(), @"C:\temp\IfsaBuilder\IfsaBuilder\Phoenix");

			//GenerateProject(WordpressModel(), @"C:\temp\IfsaBuilder\IfsaBuilder\Wordpress");

			//var prj = new ClrProject();
			//prj.Save(@"C:\temp\IfsaBuilder\IfsaBuilder\", project);

			var core = new Core();
			core.ContextCreator = new TransactionContextCreator(string.Empty);
			core.ModalDialog = new ConsoleDialog();
			var viewModel = new LoginsViewModel(core, new LoginAdapter());
			try
			{
				viewModel.InsertAsync(new Login(1, @"PPetrov", @"Password", DateTime.Now, false)).Wait();

				Console.WriteLine(@"Done");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			return;








			// Register helpers
			var cache = core.DataCache;
			cache.Add(new BrandHelper());
			cache.Add(new FlavorHelper());
			cache.Add(new ArticleHelper());

			try
			{
				File.WriteAllText(@"C:\temp\diagnostics.txt", string.Empty);

				//var viewModel = new LoginsViewModel(core, new LoginAdapter());
				//viewModel.LoadData();

				//var v = new LoginViewModel(new Login(1, @"PPetrov", @"QWE234!", DateTime.Now, true));
				//var dialog = new ConsoleDialog();
				//viewModel.AddAsync(v, dialog).Wait();
				//viewModel.AddAsync(v, dialog).Wait();
				//viewModel.ChangePasswordAsync(v, dialog, @"sc1f1r3hack").Wait();
				//viewModel.AddAsync(v, dialog).Wait();
				//viewModel.ChangePasswordAsync(v, dialog, @"sc1f1r3hackV2").Wait();

				//foreach (var login in viewModel.Logins)
				//{
				//	Console.WriteLine(login.Name + " " + v.Password);
				//}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private static void CreateSchema(string serverDb)
		{
			using (var serverContext = new TransactionContextCreator(serverDb).Create())
			{
				DbFeatureServerManager.CreateSchema(serverContext);
				serverContext.Complete();

				Console.WriteLine(@"Create schema");
			}
		}

		private static void DropSchema(string serverDb)
		{
			using (var serverContext = new TransactionContextCreator(serverDb).Create())
			{
				try
				{
					DbFeatureServerManager.DropSchema(serverContext);
					serverContext.Complete();
				}
				catch
				{
				}
				Console.WriteLine(@"Drop schema");
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
				DbColumn.DateTime(@"CreatedAt"),
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
			var step = feature.AddStep(nameof(LoadBrandsAsync));
			var brandHelper = new BrandHelper();
			step.Details = brandHelper.Items.Count.ToString();
			return brandHelper;
		}

		private async Task<FlavorHelper> LoadFlavorsAsync(Feature feature)
		{
			var step = feature.AddStep(nameof(LoadFlavorsAsync));
			var flavorHelper = new FlavorHelper();
			step.Details = flavorHelper.Items.Count.ToString();
			return flavorHelper;
		}

		private async Task<ArticleHelper> LoadArticlesAsync(Feature feature, BrandHelper brandHelper, FlavorHelper flavorHelper)
		{
			var step = feature.AddStep(nameof(LoadArticlesAsync));
			var articleHelper = new ArticleHelper();
			step.Details = articleHelper.Items.Count.ToString();
			return articleHelper;
		}

		private static void InspectFeature(Feature f)
		{
			//var buffer = new StringBuilder();

			//var ctxName = f.Context + "(" + f.Name + ")";
			//buffer.Append(ctxName.PadRight(25));
			//buffer.Append(' ');
			//buffer.Append(f.Details.PadRight(12));
			//buffer.Append(' ');
			//buffer.AppendLine(f.TimeSpent.TotalMilliseconds.ToString(CultureInfo.InvariantCulture).PadRight(6));

			//if (f.Steps.Any())
			//{
			//	var totalMilliseconds = f.TimeSpent.TotalMilliseconds;
			//	var remaingTime = totalMilliseconds - (f.Steps.Select(v => v.TimeSpent.TotalMilliseconds).Sum());

			//	foreach (var s in f.Steps.Concat(new[] { new FeatureEntryStep(@"Other", TimeSpan.FromMilliseconds(remaingTime), string.Empty) }))
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
