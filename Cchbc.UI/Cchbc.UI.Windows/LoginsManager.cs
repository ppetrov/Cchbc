using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc.UI.Comments
{
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

	public sealed class LoginViewItem : ViewItem<Login>
	{
		public string Name => this.Item.Name;
		public string Password => this.Item.Password;
		public string CreatedAt => this.Item.CreatedAt.ToString(@"f");
		public bool IsSystem
		{
			get { return this.Item.IsSystem; }
			set
			{
				this.Item.IsSystem = value;
				this.OnPropertyChanged();
				this.OnPropertyChanged(nameof(IsRegular));
			}
		}

		public bool IsRegular => !this.IsSystem;

		public LoginViewItem(Login login) : base(login)
		{
		}
	}

	public sealed class LoginsManager : Manager<Login, LoginViewItem>
	{
		public ILogger Logger { get; }
		public LoginAdapter Adapter { get; }

		public LoginsManager(ILogger logger, LoginAdapter adapter, Sorter<LoginViewItem> sorter, Searcher<LoginViewItem> searcher, FilterOption<LoginViewItem>[] filterOptions = null)
			: base(adapter, sorter, searcher, filterOptions)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Logger = logger;
			this.Adapter = adapter;
		}

		public override ValidationResult[] ValidateProperties(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var s = Stopwatch.StartNew();

			var results = Validator.GetViolated(new[]
			{
				Validator.ValidateNotNull(viewItem.Name, @"Name cannot be null"),
				Validator.ValidateNotEmpty(viewItem.Name, @"Name cannot be empty"),
				Validator.ValidateMaxLength(viewItem.Name, 8, @"Name cannot be more then 8"),

				Validator.ValidateNotNull(viewItem.Password, @"Password cannot be null"),
				Validator.ValidateNotEmpty(viewItem.Password, @"Password cannot be empty"),
				Validator.ValidateMinLength(viewItem.Password, 8, @"Password is too short. Must be at least 8 symbols"),
				Validator.ValidateMaxLength(viewItem.Password, 20, @"Password is too long. Must be less then or equal to 20")
			});
			this.Logger.Info($@"{nameof(ValidateProperties)}:{s.ElapsedMilliseconds}ms");

			return results;
		}

		public override async Task<PermissionResult> CanAddAsync(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var s = Stopwatch.StartNew();
			try
			{
				if (await this.Adapter.IsReservedAsync(viewItem.Name))
				{
					return PermissionResult.Deny(@"This name is reserved");
				}
				return PermissionResult.Allow;
			}
			finally
			{
				this.Logger.Info($@"{nameof(CanAddAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}

		public override Task<PermissionResult> CanUpdateAsync(LoginViewItem viewItem)
		{
			var s = Stopwatch.StartNew();
			try
			{
				return Task.FromResult(PermissionResult.Allow);
			}
			finally
			{
				this.Logger.Info($@"{nameof(CanUpdateAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}

		public override Task<PermissionResult> CanDeleteAsync(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var s = Stopwatch.StartNew();
			try
			{
				if (viewItem.Item.CreatedAt.Date == DateTime.Today)
				{
					return Task.FromResult(PermissionResult.Confirm(@"Cannot delete today logins"));
				}
				return Task.FromResult(PermissionResult.Allow);
			}
			finally
			{
				this.Logger.Info($@"{nameof(CanDeleteAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}

		public Task<PermissionResult> CanPromoteAsync(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var s = Stopwatch.StartNew();
			try
			{
				return Task.FromResult(PermissionResult.Allow);
				if (viewItem.IsSystem)
				{
					return Task.FromResult(PermissionResult.Deny(@"The user is already System user"));
				}
				return Task.FromResult(PermissionResult.Confirm(@"Are you sure to promoto user create today ?"));
			}
			finally
			{
				this.Logger.Info($@"{nameof(CanPromoteAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}
	}

	public sealed class LoginAdapter : IModifiableAdapter<Login>
	{
		private readonly ILogger _logger;
		private readonly List<Login> _logins = new List<Login>();

		public LoginAdapter(ILogger logger)
		{
			_logger = logger;
			_logins.Add(new Login(1, @"Petar", @"123", DateTime.Today.AddDays(-7), true));
			_logins.Add(new Login(1, @"Denis", @"123", DateTime.Today.AddDays(-7), true));
			_logins.Add(new Login(1, @"Teodor", @"123", DateTime.Today.AddDays(-7), true));
		}

		public Task<List<Login>> GetAllAsync()
		{
			return Task.FromResult(new List<Login>(_logins));
		}

		public Task InsertAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var s = Stopwatch.StartNew();
			try
			{
				_logins.Add(item);
				return Task.FromResult(true);
			}
			finally
			{
				_logger.Info($@"{nameof(InsertAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}

		public Task UpdateAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var s = Stopwatch.StartNew();
			try
			{
				return Task.FromResult(true);
			}
			finally
			{
				_logger.Info($@"{nameof(UpdateAsync)}:{s.ElapsedMilliseconds}ms");
			}
		}

		public Task DeleteAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			_logins.Remove(item);

			return Task.FromResult(true);
		}

		public Task<bool> IsReservedAsync(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			var isReserved = name.Trim().Equals(@"admin", StringComparison.OrdinalIgnoreCase);
			return Task.FromResult(isReserved);
		}
	}

	public sealed class LoginsViewModel : ViewObject
	{
		public ILogger Logger { get; }
		public LoginsManager Manager { get; }
		public ObservableCollection<LoginViewItem> Logins { get; } = new ObservableCollection<LoginViewItem>();
		public SortOption<LoginViewItem>[] SortOptions => this.Manager.Sorter.Options;
		public SearchOption<LoginViewItem>[] SearchOptions => this.Manager.Searcher.Options;

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetField(ref _textSearch, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySearch();
			}
		}

		private SearchOption<LoginViewItem> _searchOption;
		public SearchOption<LoginViewItem> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Logger.Info($@"Apply filter options:{string.Join(@",", selectedFilterOptions.Select(f => @"'" + f.DisplayName + @"'"))}");
				//this.Manager.Logger.Info($@"Searching for text:'{this.TextSearch}', option: '{this.SearchOption?.DisplayName}'");
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySearch();
			}
		}

		private SortOption<LoginViewItem> _sortOption;
		public SortOption<LoginViewItem> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				// TODO : Logger
				// TODO : Statistics to track functionality usage
				//this.Logger.Info($@"Apply filter options:{string.Join(@",", selectedFilterOptions.Select(f => @"'" + f.DisplayName + @"'"))}");
				//this.Manager.Logger.Info($@"Searching for text:'{this.TextSearch}', option: '{this.SearchOption?.DisplayName}'");
				//this.Stats[ExcludeSuppressed]++;
				this.ApplySort();
			}
		}

		private bool _isBusy;
		public bool IsBusy
		{
			get { return _isBusy; }
			private set { this.SetField(ref _isBusy, value); }
		}

		public string ErrorMessage { get; private set; } = string.Empty;

		public FeatureManager FeatureManager { get; }

		public LoginsViewModel(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
			this.FeatureManager = new FeatureManager();
			this.Manager = new LoginsManager(logger, new LoginAdapter(logger), new Sorter<LoginViewItem>(new[]
			{
				new SortOption<LoginViewItem>(@"By Name", (x,y)=> string.Compare(x.Item.Name, y.Item.Name, StringComparison.Ordinal)),
				new SortOption<LoginViewItem>(@"By Date", (x, y) =>
				{
					var cmp = x.Item.CreatedAt.CompareTo(y.Item.CreatedAt);
					if (cmp == 0)
					{
						cmp = string.Compare(x.Item.Name, y.Item.Name, StringComparison.Ordinal);
					}
					return cmp;
				})
			}), new Searcher<LoginViewItem>((v, s) => v.Item.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0));

			this.Manager.OperationStart += (sender, args) =>
			{
				this.IsBusy = true;
			};
			this.Manager.OperationEnd += (sender, args) =>
			{
				this.IsBusy = false;
				this.FeatureManager.Add(this.Logger.Context, args.Feature);
			};
			this.Manager.OperationError += (sender, args) => { this.ErrorMessage = args.Exception.Message; };

			this.Manager.ItemInserted += ManagerOnItemInserted;
			this.Manager.ItemUpdated += ManagerOnItemUpdated;
			this.Manager.ItemDeleted += ManagerOnItemDeleted;
		}

		private void ManagerOnItemInserted(object sender, ObjectEventArgs<LoginViewItem> args)
		{
			this.Manager.Insert(this.Logins, args.Item, this.TextSearch, this.SearchOption);
		}

		private void ManagerOnItemUpdated(object sender, ObjectEventArgs<LoginViewItem> args)
		{
			this.Manager.Update(this.Logins, args.Item, this.TextSearch, this.SearchOption);
		}

		private void ManagerOnItemDeleted(object sender, ObjectEventArgs<LoginViewItem> args)
		{
			this.Manager.Delete(this.Logins, args.Item);
		}

		public async Task LoadDataAsync()
		{
			this.Logins.Clear();
			var logins = (await this.Manager.Adapter.GetAllAsync()).Select(v => new LoginViewItem(v)).ToList();
			this.Manager.LoadData(logins);
			foreach (var login in logins)
			{
				this.Logins.Add(login);
			}
		}

		private void ApplySearch()
		{
			var viewItems = this.Manager.Search(this.TextSearch, this.SearchOption);

			this.Logins.Clear();
			foreach (var viewItem in viewItems)
			{
				this.Logins.Add(viewItem);
			}
		}

		private void ApplySort()
		{
			var index = 0;
			foreach (var viewItem in this.Manager.Sort(this.Logins, this.SortOption))
			{
				this.Logins[index++] = viewItem;
			}
		}

		public async Task AddAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			await this.Manager.AddAsync(viewItem, dialog, new Feature(nameof(AddAsync)));
		}

		public async Task DeleteAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			await this.Manager.DeleteAsync(viewItem, dialog, new Feature(nameof(DeleteAsync)));
		}

		public async Task PromoteUserAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var feature = new Feature(nameof(PromoteUserAsync));
			this.Manager.NotifyStart(feature);

			dialog.AcceptAction = dialog.CancelAction = dialog.DeclineAction = () =>
			{
				this.Manager.NotifyEnd(Feature.None);
			};

			var result = await this.Manager.CanPromoteAsync(viewItem);
			switch (result.Status)
			{
				case PermissionStatus.Allow:
					await PromoteValidatedAsync(viewItem, dialog, feature);
					break;
				case PermissionStatus.Confirm:
					dialog.AcceptAction = async () =>
					{
						try
						{
							await this.PromoteValidatedAsync(viewItem, dialog, feature);
						}
						catch (Exception ex)
						{
							try
							{
								this.Manager.NotifyError(feature, ex);
							}
							finally
							{
								this.Manager.NotifyEnd(feature);
							}
						}
					};
					await dialog.ConfirmAsync(result.Message, feature);
					break;
				case PermissionStatus.Deny:
					await dialog.DisplayAsync(result.Message, feature);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private async Task PromoteValidatedAsync(LoginViewItem viewItem, ModalDialog dialog, Feature feature)
		{
			try
			{
				viewItem.IsSystem = true;

				await this.Manager.UpdateAsync(viewItem, dialog, feature);

				throw new Exception(@"PPetrov");
			}
			catch (Exception ex)
			{
				this.Logger.Error(ex.ToString());
				try
				{

				}
				finally
				{
					
				}
			}
		}
	}
}