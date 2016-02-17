using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
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
	public sealed class ConsoleDialog : IModalDialog
	{
		public Task<DialogResult> ShowAsync(string message, Feature feature, DialogType? type = null)
		{
			return Task.FromResult(DialogResult.Cancel);
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
		public Task InsertAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			//throw new NotImplementedException();
			return Task.FromResult(true);
		}

		public Task UpdateAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			//throw new NotImplementedException();
			return Task.FromResult(true);
		}

		public Task DeleteAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			//throw new NotImplementedException();
			return Task.FromResult(true);
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

		public LoginsViewModel(Core core, LoginAdapter adapter)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Core = core;
			this.Module = new LoginModule(adapter, new Sorter<LoginViewModel>(new[]
			{
				new SortOption<LoginViewModel>(string.Empty, (x, y) => 0)
			}),
			new Searcher<LoginViewModel>((v, s) => false));

			this.Module.OperationStart += (sender, args) =>
			{
				this.FeatureManager.Start(args.Feature);
			};
			this.Module.OperationEnd += (sender, args) =>
			{
				this.FeatureManager.Stop(args.Feature);
			};

			this.Module.ItemInserted += ModuleOnItemInserted;
			this.Module.ItemUpdated += ModuleOnItemUpdated;
			this.Module.ItemDeleted += ModuleOnItemDeleted;
		}

		public void LoadData()
		{
			var feature = this.FeatureManager.StartNew(this.Context, nameof(LoadData));

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

		public async Task AddAsync(LoginViewModel viewModel, IModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var feature = this.FeatureManager.StartNew(this.Context, nameof(AddAsync));
			try
			{
				await this.Module.InsertAsync(viewModel, dialog, feature);
			}
			catch (Exception ex)
			{
				this.FeatureManager.LogException(feature, ex);
			}
		}

		public async Task UpdateAsync(LoginViewModel viewModel, IModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var feature = this.FeatureManager.StartNew(this.Context, nameof(UpdateAsync));
			try
			{
				await this.Module.UpdateAsync(viewModel, dialog, feature);
			}
			catch (Exception ex)
			{
				this.FeatureManager.LogException(feature, ex);
			}
		}

		public async Task ChangePasswordAsync(LoginViewModel viewModel, IModalDialog dialog, string newPassword)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (newPassword == null) throw new ArgumentNullException(nameof(newPassword));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var copyViewModel = new LoginViewModel(viewModel.Model) { Password = newPassword };

			var feature = new Feature(this.Context, nameof(this.ChangePasswordAsync));
			try
			{
				await this.Module.ExecuteAsync(copyViewModel, dialog, feature, this.Module.CanChangePassword, this.ChangePasswordValidatedAsync);
			}
			catch (Exception ex)
			{
				this.FeatureManager.LogException(feature, ex);
			}
		}

		public async Task DeleteAsync(LoginViewModel viewModel, IModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

			var feature = new Feature(this.Context, nameof(DeleteAsync));
			try
			{
				await this.Module.DeleteAsync(viewModel, dialog, feature);
			}
			catch (Exception ex)
			{
				this.FeatureManager.LogException(feature, ex);
			}
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

		private async Task ChangePasswordValidatedAsync(LoginViewModel copyViewModel, FeatureEventArgs args)
		{
			var viewModel = this.Module.FindViewModel(copyViewModel.Model);

			viewModel.Password = copyViewModel.Password;

			await this.Module.UpdateValidatedAsync(viewModel, args);
		}
	}

	public sealed class SqlLiteDelegateDataReader : IFieldDataReader
	{
		private readonly DbDataReader _r;

		public SqlLiteDelegateDataReader(DbDataReader r)
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

	public sealed class SqlLiteReadQueryHelper : ReadQueryHelper
	{
		private readonly string _cnString;

		public SqlLiteReadQueryHelper(string cnString)
		{
			if (cnString == null) throw new ArgumentNullException(nameof(cnString));

			_cnString = cnString;
		}

		public override List<T> Execute<T>(Query<T> query)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));

			var values = new List<T>();

			using (var cn = new SQLiteConnection(_cnString))
			{
				cn.Open();
				using (var cmd = cn.CreateCommand())
				{
					cmd.CommandText = query.Statement;
					cmd.CommandType = CommandType.Text;
					foreach (var p in query.Parameters)
					{
						cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
					}

					using (var r = cmd.ExecuteReader())
					{
						var dr = new SqlLiteDelegateDataReader(r);
						while (dr.Read())
						{
							values.Add(query.Creator(dr));
						}
					}
				}
			}

			return values;
		}

		public override void Fill<T>(Query<T> query, Dictionary<long, T> values, Func<T, long> selector)
		{
			if (query == null) throw new ArgumentNullException(nameof(query));
			if (values == null) throw new ArgumentNullException(nameof(values));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			values.Clear();

			using (var cn = new SQLiteConnection(_cnString))
			{
				cn.Open();
				using (var cmd = cn.CreateCommand())
				{
					cmd.CommandText = query.Statement;
					cmd.CommandType = CommandType.Text;
					foreach (var p in query.Parameters)
					{
						cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
					}

					using (var r = cmd.ExecuteReader())
					{
						var dr = new SqlLiteDelegateDataReader(r);
						while (dr.Read())
						{
							var value = query.Creator(dr);
							values.Add(selector(value), value);
						}
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

			using (var cn = new SQLiteConnection(_cnString))
			{
				cn.Open();
				using (var cmd = cn.CreateCommand())
				{
					cmd.CommandText = query;
					cmd.CommandType = CommandType.Text;
					foreach (var p in parameters)
					{
						cmd.Parameters.Add(new SQLiteParameter(p.Name, p.Value));
					}

					using (var r = cmd.ExecuteReader())
					{
						var dr = new SqlLiteDelegateDataReader(r);
						while (dr.Read())
						{
							filler(dr, values);
						}
					}
				}
			}
		}
	}

	public sealed class SqlLiteModifyQueryHelper : ModifyQueryHelper
	{
		private readonly string _cnString;

		public SqlLiteModifyQueryHelper(string cnString)
		{
			if (cnString == null) throw new ArgumentNullException(nameof(cnString));

			_cnString = cnString;
		}

		public override int Execute(string statement, QueryParameter[] parameters)
		{
			using (var cn = new SQLiteConnection(_cnString))
			{
				cn.Open();
				using (var cmd = cn.CreateCommand())
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
	}






























	public class Program
	{
		static void Main(string[] args)
		{
			//// Blog, posts, comments, users

			//var users = DbTable.Create(@"Users", new[]
			//{
			//	DbColumn.String(@"Name"),
			//});

			//var blogs = DbTable.Create(@"Blogs", new[]
			//{
			//	DbColumn.String(@"Name"),
			//	DbColumn.String(@"Description"),
			//	DbColumn.ForeignKey(users)
			//});

			//var posts = DbTable.Create(@"Posts", new[]
			//{
			//	DbColumn.String(@"Title"),
			//	DbColumn.String(@"Contents"),
			//	DbColumn.DateTime(@"CreationDate"),
			//	DbColumn.ForeignKey(blogs),
			//});

			//var comments = DbTable.Create(@"Comments", new[]
			//{
			//	DbColumn.String(@"Contents"),
			//	DbColumn.DateTime(@"CreationDate"),
			//	DbColumn.ForeignKey(users),
			//	DbColumn.ForeignKey(posts),
			//});

			//var schema = new DbSchema(@"WordPress", new[]
			//{
			//	users,
			//	blogs,
			//	posts,
			//	comments,
			//});


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

			//var filePath = @"c:\temp\ifsa.ctx";
			//project.Save(filePath);
			//var copy = DbProject.Load(filePath);


			//var project = new DbProject(schema);

			//project.AttachInverseTable(posts);

			//project.MarkModifiable(blogs);
			//project.MarkModifiable(posts);
			//project.MarkModifiable(comments);			

			var buffer = new StringBuilder(1024 * 2);

			var s = Stopwatch.StartNew();
			foreach (var entity in project.CreateEntities())
			{
				//
				// Classes
				//
				var entityClass = project.CreateEntityClass(entity);
				buffer.AppendLine(entityClass);
				buffer.AppendLine(EntityClass.GenerateClassViewModel(entity, !project.IsModifiable(entity.Table)));
				buffer.AppendLine(project.CreateTableViewModel(entity));


				if (!project.IsModifiable(entity.Table))
				{
					buffer.AppendLine(project.CreateTableViewModel(entity));
				}
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

			//Console.WriteLine(buffer.ToString());
			//File.WriteAllText(@"C:\temp\code.txt", buffer.ToString());

			var p = new ClrProject();
			p.WriteAllText = File.WriteAllText;
			p.CreateDirectory = path =>
			{
				if (Directory.Exists(path))
				{
					Directory.Delete(path, true);
				}
				Directory.CreateDirectory(path);
			};
			p.Save(@"C:\temp\IfsaBuilder\IfsaBuilder\Phoenix", project);

			//var prj = new ClrProject();
			//prj.Save(@"C:\temp\IfsaBuilder\IfsaBuilder\", project);

			//Console.WriteLine(DbScript.CreateTable(users));
			//Console.WriteLine(DbScript.CreateTable(blogs));
			//Console.WriteLine(DbScript.CreateTable(posts));
			//Console.WriteLine(DbScript.CreateTable(comments));

			//return;


			var connectionString = @"Data Source=C:\Users\codem\Desktop\cchbc.sqlite;Version=3;";
			var core = new Core();
			core.FeatureManager.Init(new DbFeaturesManager(new DbFeaturesAdapter(new QueryHelper())));




			// Register helpers
			var cache = core.DataCache;
			cache.Add(new BrandHelper());
			cache.Add(new FlavorHelper());
			cache.Add(new ArticleHelper());

			try
			{
				File.WriteAllText(@"C:\temp\diagnostics.txt", string.Empty);

				var viewModel = new LoginsViewModel(core, new LoginAdapter());
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
			finally
			{
				core.FeatureManager.Flush();
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

		static HashSet<string> idents = new HashSet<string>();
		static List<string> _links = new List<string>();

		public sealed class BinaryGate
		{
			public bool Applied { get; set; }
			public string Left { get; set; }
			public string Right { get; set; }
			public string Function { get; set; }
			public string Result { get; set; }

			public BinaryGate(string left, string right, string function, string result)
			{
				Left = left;
				Right = right;
				Function = function;
				Result = result;
			}

			public SignalGage CreateSignal()
			{
				if (this.Function == @"LSHIFT")
				{
					var v = ushort.Parse(this.Left) << ushort.Parse(this.Right);
					return new SignalGage(this.Result, v);
				}
				if (this.Function == @"RSHIFT")
				{
					var v = ushort.Parse(this.Left) >> ushort.Parse(this.Right);
					return new SignalGage(this.Result, v);
				}
				if (this.Function == @"OR")
				{
					var v = ushort.Parse(this.Left) | ushort.Parse(this.Right);
					return new SignalGage(this.Result, v);
				}
				if (this.Function == @"AND")
				{
					var v = ushort.Parse(this.Left) & ushort.Parse(this.Right);
					return new SignalGage(this.Result, v);
				}
				throw new NotImplementedException();
			}
		}

		public sealed class UnaryGage
		{
			public bool Applied { get; set; }
			public string Input { get; set; }
			public string Function { get; set; }
			public string Result { get; set; }

			public UnaryGage(string input, string function, string result)
			{
				Input = input;
				Function = function;
				Result = result;
			}

			public SignalGage CreateSignal()
			{
				if (this.Function == @"NOT")
				{
					ushort n = 0;
					ushort.TryParse(this.Input, out n);

					return new SignalGage(this.Result, 65535 - n);
				}
				return null;
			}
		}

		public sealed class SignalGage
		{
			public string Input { get; }
			public int Value { get; }

			public SignalGage(string input, int value)
			{
				Input = input;
				Value = value;
			}
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
			buffer.Append(ctxName.PadRight(25));
			buffer.Append(' ');
			buffer.Append(f.Details.PadRight(12));
			buffer.Append(' ');
			buffer.AppendLine(f.TimeSpent.TotalMilliseconds.ToString(CultureInfo.InvariantCulture).PadRight(6));

			if (f.Steps.Any())
			{
				var totalMilliseconds = f.TimeSpent.TotalMilliseconds;
				var remaingTime = totalMilliseconds - (f.Steps.Select(v => v.TimeSpent.TotalMilliseconds).Sum());

				foreach (var s in f.Steps.Concat(new[] { new FeatureEntryStep(@"Other", TimeSpan.FromMilliseconds(remaingTime), string.Empty) }))
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















}
