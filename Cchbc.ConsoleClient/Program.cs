using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Features.Db;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;


namespace Cchbc.ConsoleClient
{
	public sealed class ConsoleDialog : ModalDialog
	{
		public override Task ShowAsync(string message, DialogType? type = null)
		{
			return Task.FromResult(true);
			//throw new NotImplementedException();
		}
	}

	public sealed class Login : IDbObject
	{
		public long Id { get; set; }
		public string Name { get; }
		public string Password { get; }
		public DateTime CreatedAt { get; }
		public bool IsSystem { get; set; }

		public Login(long id, string name, string password, DateTime createdAt, bool isSystem)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (password == null) throw new ArgumentNullException(nameof(password));

			this.Id = id;
			this.Name = name;
			this.Password = password;
			this.CreatedAt = createdAt;
			this.IsSystem = isSystem;
		}
	}

	public sealed class LoginViewModel : ViewModel<Login>
	{
		public string Name { get; }

		private string _password = string.Empty;
		public string Password
		{
			get { return _password; }
			set { this.SetField(ref _password, value); }
		}

		public string CreatedAt { get; }

		private bool _isSystem;
		public bool IsSystem
		{
			get { return _isSystem; }
			set { this.SetField(ref _isSystem, value); }
		}

		public LoginViewModel(Login login) : base(login)
		{
			if (login == null) throw new ArgumentNullException(nameof(login));

			this.Name = login.Name;
			this.Password = login.Password;
			this.CreatedAt = login.CreatedAt.ToString(@"f");
			this.IsSystem = login.IsSystem;
		}
	}

	public sealed class LoginAdapter : IModifiableAdapter<Login>
	{
		public void Insert(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			//throw new NotImplementedException();
		}

		public void Update(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			//throw new NotImplementedException();
		}

		public void Delete(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			//throw new NotImplementedException();
		}

		public List<Login> GetAll()
		{
			//throw new NotImplementedException();
			return new List<Login>();
		}
	}

	public sealed class LoginModule : Module<Login, LoginViewModel>
	{
		private LoginAdapter Adapter { get; }

		public LoginModule(LoginAdapter adapter, Sorter<LoginViewModel> sorter, Searcher<LoginViewModel> searcher, FilterOption<LoginViewModel>[] filterOptions = null)
			: base(adapter, sorter, searcher, filterOptions)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public List<Login> GetAll()
		{
			return this.Adapter.GetAll();
		}

		public override ValidationResult[] ValidateProperties(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(ValidateProperties));
			return Enumerable.Empty<ValidationResult>().ToArray();
		}

		public override Task<PermissionResult> CanInsertAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanInsertAsync));
			return PermissionResult.Allow;
		}

		public override Task<PermissionResult> CanUpdateAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanUpdateAsync));
			return PermissionResult.Allow;
		}

		public override Task<PermissionResult> CanDeleteAsync(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanDeleteAsync));
			return PermissionResult.Allow;
		}

		public Task<PermissionResult> CanChangePassword(LoginViewModel viewModel, Feature feature)
		{
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			feature.AddStep(nameof(CanChangePassword));
			return PermissionResult.Allow;
		}
	}

	public sealed class LoginsViewModel : ViewModel
	{
		private Core Core { get; }
		private FeatureManager FeatureManager => this.Core.FeatureManager;
		private LoginModule Module { get; }
		private string Context { get; } = nameof(LoginsViewModel);

		public ObservableCollection<LoginViewModel> Logins { get; } = new ObservableCollection<LoginViewModel>();
		public SortOption<LoginViewModel>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<LoginViewModel>[] SearchOptions => this.Module.Searcher.Options;

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetField(ref _textSearch, value);
				this.ApplySearch();
			}
		}

		private SearchOption<LoginViewModel> _searchOption;
		public SearchOption<LoginViewModel> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				this.ApplySearch();
			}
		}

		private SortOption<LoginViewModel> _sortOption;
		public SortOption<LoginViewModel> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				this.ApplySort();
			}
		}

		public LoginsViewModel(Core core)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));

			this.Core = core;
			this.Module = new LoginModule(new LoginAdapter(), new Sorter<LoginViewModel>(new[]
			{
				new SortOption<LoginViewModel>(string.Empty, (x, y) => 0)
			}),
			new Searcher<LoginViewModel>((v, s) => false));

			this.Module.OperationStart += (sender, args) =>
			{
				this.Core.FeatureManager.Start(args.Feature);
			};
			this.Module.OperationEnd += (sender, args) =>
			{
				this.Core.FeatureManager.Stop(args.Feature, args.Exception);
			};

			this.Module.ItemInserted += ModuleOnItemInserted;
			this.Module.ItemUpdated += ModuleOnItemUpdated;
			this.Module.ItemDeleted += ModuleOnItemDeleted;
		}

		public void LoadData()
		{
			var feature = Feature.StartNew(this.Context, nameof(LoadData));

			var models = this.Module.GetAll();
			var viewModels = new LoginViewModel[models.Count];
			var index = 0;
			foreach (var model in models)
			{
				viewModels[index++] = new LoginViewModel(model);
			}
			this.DisplayLogins(viewModels, feature);

			this.FeatureManager.Stop(feature);
		}

		public Task AddAsync(LoginViewModel viewModel, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.InsertAsync(viewModel, dialog, new Feature(this.Context, nameof(AddAsync), string.Empty));
		}

		public Task UpdateAsync(LoginViewModel viewModel, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.UpdateAsync(viewModel, dialog, new Feature(this.Context, nameof(UpdateAsync), string.Empty));
		}

		public Task ChangePasswordAsync(LoginViewModel viewModel, ModalDialog dialog, string newPassword)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (newPassword == null) throw new ArgumentNullException(nameof(newPassword));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var copyViewModel = new LoginViewModel(viewModel.Model) { Password = newPassword };

			return this.Module.ExecuteAsync(copyViewModel, dialog, new Feature(this.Context, nameof(ChangePasswordAsync), string.Empty), this.Module.CanChangePassword, this.ChangePasswordValidated);
		}

		public Task RemoveAsync(LoginViewModel viewModel, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			return this.Module.DeleteAsync(viewModel, dialog, new Feature(this.Context, nameof(RemoveAsync), string.Empty));
		}

		private void ModuleOnItemInserted(object sender, ObjectEventArgs<LoginViewModel> args)
		{
			this.Module.Insert(this.Logins, args.ViewModel, this.TextSearch, this.SearchOption);
		}

		private void ModuleOnItemUpdated(object sender, ObjectEventArgs<LoginViewModel> args)
		{
			this.Module.Update(this.Logins, args.ViewModel, this.TextSearch, this.SearchOption);
		}

		private void ModuleOnItemDeleted(object sender, ObjectEventArgs<LoginViewModel> args)
		{
			this.Module.Delete(this.Logins, args.ViewModel);
		}

		private void DisplayLogins(LoginViewModel[] viewModels, Feature feature)
		{
			feature.AddStep(nameof(DisplayLogins));

			this.Module.SetupViewModels(viewModels);
			this.ApplySearch();
		}

		private void ApplySearch()
		{
			var viewModels = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Logins.Clear();
			foreach (var viewModel in viewModels)
			{
				this.Logins.Add(viewModel);
			}
		}

		private void ApplySort()
		{
			var index = 0;
			foreach (var viewModel in this.Module.Sort(this.Logins, this.SortOption))
			{
				this.Logins[index++] = viewModel;
			}
		}

		private void ChangePasswordValidated(LoginViewModel copyViewModel, FeatureEventArgs args)
		{
			var viewModel = this.Module.FindViewModel(copyViewModel.Model);

			viewModel = null;
			viewModel.Password = copyViewModel.Password;

			this.Module.UpdateValidated(viewModel, args);
		}
	}

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

			var schema = new DbSchema(@"IfsaBuilder", new[]
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

			//var filePath = @"c:\temp\ifsa.ctx";
			//project.Save(filePath);
			//var copy = DbProject.Load(filePath);

			var buffer = new StringBuilder(1024 * 2);

			var s = Stopwatch.StartNew();
			foreach (var entity in project.CreateEntities())
			{
				//
				// Classes
				//
				//var entityClass = project.CreateEntityClass(entity);
				//buffer.AppendLine(entityClass);
				//continue;

				if (!project.IsModifiable(entity.Table))
				{
					buffer.AppendLine(project.CreateTableViewModel(entity));

				}
				continue;

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

			//Console.WriteLine(buffer.ToString());
			File.WriteAllText(@"C:\temp\code.txt", buffer.ToString());

			var prj = new ClrProject();
			prj.Save(@"C:\temp\IfsaBuilder\IfsaBuilder\", project);

			return;
















			var connectionString = @"Data Source=C:\Users\codem\Desktop\cchbc.sqlite;Version=3;";

			using (var cn = new SQLiteConnection(connectionString))
			{
				cn.Open();

				var core = new Core();

				// Set logger
				core.Logger = new ConsoleLogger();

				// Create Read query helper
				var sqlReadDataQueryHelper = new SqlReadQueryHelper(cn);
				// Create Modify query helper
				var sqlModifyDataQueryHelper = new SqlModifyQueryHelper(sqlReadDataQueryHelper, cn);
				// Create General query helper
				var queryHelper = new QueryHelper(sqlReadDataQueryHelper, sqlModifyDataQueryHelper);
				core.QueryHelper = queryHelper;

				var featureManager = new FeatureManager { InspectFeature = InspectFeature };
				core.FeatureManager = featureManager;
				var manager = new DbFeaturesManager(new DbFeaturesAdapter(queryHelper));
				manager.CreateSchema();
				core.FeatureManager.Load(core.Logger, manager);
				core.FeatureManager.StartDbWriters();

				// Register helpers
				core.DataCache = new DataCache();
				var cache = core.DataCache;
				cache.AddHelper(new BrandHelper());
				cache.AddHelper(new FlavorHelper());
				cache.AddHelper(new ArticleHelper());

				try
				{
					File.WriteAllText(@"C:\temp\diagnostics.txt", string.Empty);

					var viewModel = new LoginsViewModel(core);
					viewModel.LoadData();

					var v = new LoginViewModel(new Login(1, @"PPetrov", @"QWE234!", DateTime.Now, true));
					var dialog = new ConsoleDialog();
					viewModel.AddAsync(v, dialog).Wait();
					viewModel.AddAsync(v, dialog).Wait();
					viewModel.ChangePasswordAsync(v, dialog, @"sc1f1r3hack").Wait();
					viewModel.AddAsync(v, dialog).Wait();
					viewModel.ChangePasswordAsync(v, dialog, @"sc1f1r3hackV2").Wait();

					foreach (var login in viewModel.Logins)
					{
						Console.WriteLine(login.Name + " " + v.Password);
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}

				featureManager.StopDbWriters();
			}
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

		private static void InspectFeature(FeatureEntry f)
		{
			var buffer = new StringBuilder();

			var ctxName = f.Context + "(" + f.Name + ")";
			if (f.ExceptionEntry != null)
			{
				ctxName = @"*** " + ctxName;
			}
			buffer.Append(ctxName.PadRight(25));
			buffer.Append(' ');
			buffer.Append(f.Details.PadRight(12));
			buffer.Append(' ');
			buffer.AppendLine(f.TimeSpent.TotalMilliseconds.ToString(CultureInfo.InvariantCulture).PadRight(6));

			if (f.Steps.Any())
			{
				var totalMilliseconds = f.TimeSpent.TotalMilliseconds;
				var remaingTime = totalMilliseconds - (f.Steps.Select(v => v.TimeSpent.TotalMilliseconds).Sum());

				foreach (var s in f.Steps.Concat(new[] { new FeatureEntryStep(@"Other", TimeSpan.FromMilliseconds(remaingTime)) }))
				{
					buffer.Append('\t');

					var value = s.Name.Replace(@"Async", string.Empty);
					value = Regex.Replace(value, @"[A-Z]", m => @" " + m.Value).TrimStart();
					buffer.Append(value.PadRight(24));
					buffer.Append(' ');
					buffer.Append(s.Details.PadRight(4));
					buffer.Append(' ');
					var milliseconds = s.TimeSpent.TotalMilliseconds;
					var tmp = (milliseconds / totalMilliseconds) * 100;
					var graph = new string('-', (int)tmp);

					buffer.Append(milliseconds.ToString(CultureInfo.InvariantCulture).PadRight(8));
					buffer.Append(tmp.ToString(@"F2").PadLeft(5));
					buffer.Append(@"% ");
					buffer.AppendLine(graph);
				}
			}


			buffer.AppendLine();
			var output = buffer.ToString();
			Debug.WriteLine(output);
			Console.WriteLine(output);

			File.AppendAllText(@"C:\temp\diagnostics.txt", output);
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













}
