using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Helpers;
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
			}
		}

		public LoginViewItem(Login login) : base(login)
		{
		}
	}

	public sealed class LoginsManager : Manager<Login, LoginViewItem>
	{
		public LoginAdapter Adapter { get; }

		public LoginsManager(LoginAdapter adapter, Sorter<LoginViewItem> sorter, Searcher<LoginViewItem> searcher, FilterOption<LoginViewItem>[] filterOptions = null) : base(adapter, sorter, searcher, filterOptions)
		{
			this.Adapter = adapter;
		}

		public override ValidationResult[] ValidateProperties(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			return Validator.GetViolated(new[]
			{
				Validator.ValidateNotNull(viewItem.Name, @"Name cannot be null"),
				Validator.ValidateNotEmpty(viewItem.Name, @"Name cannot be empty"),
				Validator.ValidateMaxLength(viewItem.Name, 8, @"Name cannot be more then 8"),

				Validator.ValidateNotNull(viewItem.Password, @"Password cannot be null"),
				Validator.ValidateNotEmpty(viewItem.Password, @"Password cannot be empty"),
				Validator.ValidateMinLength(viewItem.Password, 8, @"Password is too short. Must be at least 8 symbols"),
				Validator.ValidateMaxLength(viewItem.Password, 20, @"Password is too long. Must be less then or equal to 20")
			});
		}

		public override async Task<PermissionResult> CanAddAsync(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var name = viewItem.Name;

			foreach (var v in this.ViewItems)
			{
				if (v.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
				{
					return PermissionResult.Deny(@"Login with the same name already exists");
				}
			}

			if (await this.Adapter.IsReservedAsync(viewItem.Name))
			{
				return PermissionResult.Deny(@"This name is reserved");
			}

			return PermissionResult.Allow;
		}

		public override Task<PermissionResult> CanUpdateAsync(LoginViewItem viewItem)
		{
			return Task.FromResult(PermissionResult.Allow);
		}

		public override Task<PermissionResult> CanDeleteAsync(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			if (viewItem.Item.CreatedAt.Date == DateTime.Today)
			{
				return Task.FromResult(PermissionResult.Deny(@"Cannot delete today logins"));
			}

			return Task.FromResult(PermissionResult.Allow);
		}

		public Task<PermissionResult> CanPromoteAsync(LoginViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			if (viewItem.IsSystem)
			{
				return Task.FromResult(PermissionResult.Deny(@"The user is already System user"));
			}

			return Task.FromResult(PermissionResult.Confirm(@"Are you sure to promoto user create today ?"));
		}

		//public ILogger Logger { get; }
		//public IProgress<string> OperationProgress { get; }
		//public CommentAdapter Adapter { get; }

		//public CommentsManager(ILogger logger, IProgress<string> operationProgress, CommentAdapter adapter,
		//	Sorter<CommentViewItem> sorter,
		//	Searcher<CommentViewItem> searcher,
		//	FilterOption<CommentViewItem>[] filterOptions = null) : base(adapter, sorter, searcher, filterOptions)
		//{
		//	this.Logger = logger;
		//	this.OperationProgress = operationProgress;
		//	this.Adapter = adapter;
		//}

		//public override ValidationResult[] ValidateProperties(CommentViewItem viewItem)
		//{
		//	if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

		//	var s = Stopwatch.StartNew();

		//	// All validation logic 
		//	this.OperationProgress.Report(@"Validating fields");
		//	var validationResults = Validator.GetViolated(new[]
		//	{
		//		Validator.ValidateNotNull(viewItem.Contents, @"Contents cannot be null"),
		//	});
		//	this.Logger.Info($@"Validating fields took {s.ElapsedMilliseconds} ms");

		//	return validationResults;
		//}

		//public override Task<PermissionResult> CanAddAsync(CommentViewItem viewItem)
		//{
		//	if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

		//	// All validation logic
		//	var s = Stopwatch.StartNew();

		//	try
		//	{
		//		this.OperationProgress.Report(@"Checking for items on the same date");
		//		if (this.ViewItems.Any(v => v.CreatedAt.Date == viewItem.CreatedAt.Date))
		//		{
		//			return Task.FromResult(PermissionResult.Deny(@"Already have a comment for this date"));
		//		}
		//		this.OperationProgress.Report(@"Checking items limits");
		//		if (this.ViewItems.Count > 10)
		//		{
		//			return Task.FromResult(PermissionResult.Confirm(@"Too many comments already. Are you sure you want one more?"));
		//		}
		//		return Task.FromResult(PermissionResult.Allow);
		//	}
		//	finally
		//	{
		//		this.Logger.Info($@"CanAdd took { s.ElapsedMilliseconds} ms");
		//	}
		//}

		//public override Task<PermissionResult> CanUpdateAsync(CommentViewItem viewItem)
		//{
		//	if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

		//	return Task.FromResult(PermissionResult.Allow);
		//}

		//public override Task<PermissionResult> CanDeleteAsync(CommentViewItem viewItem)
		//{
		//	if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

		//	// All validation logic
		//	var s = Stopwatch.StartNew();

		//	try
		//	{
		//		this.OperationProgress.Report(@"Delete unreplicated ???");
		//		if (viewItem.Type == null)
		//		{
		//			return Task.FromResult(PermissionResult.Deny(@"Cannot delete comment of this type"));
		//		}
		//		return Task.FromResult(PermissionResult.Allow);
		//	}
		//	finally
		//	{
		//		this.Logger.Info($@"CanAdd took { s.ElapsedMilliseconds} ms");
		//	}
		//}

		//public PermissionResult CanMark(CommentViewItem viewItem)
		//{
		//	return PermissionResult.Allow;
		//}

	}

	public sealed class LoginAdapter : IModifiableAdapter<Login>
	{
		private readonly List<Login> _logins = new List<Login>();

		public LoginAdapter()
		{
			_logins.Add(new Login(1, @"Petar", @"123", DateTime.Today.AddDays(-7), true));
		}

		public Task<List<Login>> GetAllAsync()
		{
			return Task.FromResult(new List<Login>(_logins));
		}

		public Task<bool> InsertAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			_logins.Add(item);
			return Task.FromResult(true);
		}

		public Task<bool> UpdateAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			// Do nothing

			return Task.FromResult(true);
		}

		public Task<bool> DeleteAsync(Login item)
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

		public LoginsViewModel(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
			this.Manager = new LoginsManager(new LoginAdapter(), new Sorter<LoginViewItem>(new[]
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

			this.Manager.ItemInserted += ManagerOnItemInserted;
			this.Manager.ItemUpdated += ManagerOnItemUpdated;
			this.Manager.ItemDeleted += ManagerOnItemDeleted;
		}

		private void ManagerOnItemUpdated(object sender, ObjectEventArgs<LoginViewItem> args)
		{
			// TODO : !!! Re-apply current filter
			//throw new NotImplementedException();
		}

		private void ManagerOnItemDeleted(object sender, ObjectEventArgs<LoginViewItem> args)
		{
			this.Logins.Remove(args.Item);
		}

		private void ManagerOnItemInserted(object sender, ObjectEventArgs<LoginViewItem> args)
		{
			this.Logins.Add(args.Item);
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

		public async Task AddAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			try
			{
				await this.Manager.AddAsync(viewItem, dialog);
			}
			catch (Exception ex)
			{
				this.Logger.Error(ex.ToString());
			}
		}

		public async Task DeleteAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			try
			{
				await this.Manager.DeleteAsync(viewItem, dialog);
			}
			catch (Exception ex)
			{
				this.Logger.Error(ex.ToString());
			}
		}

		public async Task PromoteAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			try
			{
				var result = await this.Manager.CanPromoteAsync(viewItem);
				switch (result.Status)
				{
					case PermissionStatus.Allow:
						await PromoteValidatedAsync(viewItem, dialog);
						break;
					case PermissionStatus.Confirm:
						dialog.AcceptAction = async () => await this.PromoteValidatedAsync(viewItem, dialog);
						await dialog.ConfirmAsync(result.Message);
						break;
					case PermissionStatus.Deny:
						await dialog.DisplayAsync(result.Message);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (Exception ex)
			{
				this.Logger.Error(ex.ToString());
			}
		}

		private async Task PromoteValidatedAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			viewItem.IsSystem = true;

			await this.Manager.UpdateAsync(viewItem, dialog);
		}
	}


	//public sealed class CommentsViewModel : ViewObject
	//{
	//	private ILogger Logger { get; }
	//	private LoginsManager Manager { get; }

	//	public ObservableCollection<CommentViewItem> Comments { get; } = new ObservableCollection<CommentViewItem>();
	//	public SortOption<CommentViewItem>[] SortOptions => this.Manager.Sorter.Options;
	//	public SearchOption<CommentViewItem>[] SearchOptions => this.Manager.Searcher.Options;

	//	private string _operationProgress = string.Empty;

	//	private string _textSearch = string.Empty;
	//	public string TextSearch
	//	{
	//		get { return _textSearch; }
	//		set
	//		{
	//			this.SetField(ref _textSearch, value);
	//			// TODO : Logger
	//			// TODO : Statistics to track functionality usage
	//			//this.Stats[ExcludeSuppressed]++;
	//			this.ApplySearch();
	//		}
	//	}

	//	private SearchOption<CommentViewItem> _searchOption;
	//	public SearchOption<CommentViewItem> SearchOption
	//	{
	//		get { return _searchOption; }
	//		set
	//		{
	//			this.SetField(ref _searchOption, value);
	//			// TODO : Logger
	//			// TODO : Statistics to track functionality usage
	//			//this.Logger.Info($@"Apply filter options:{string.Join(@",", selectedFilterOptions.Select(f => @"'" + f.DisplayName + @"'"))}");
	//			//this.Manager.Logger.Info($@"Searching for text:'{this.TextSearch}', option: '{this.SearchOption?.DisplayName}'");
	//			//this.Stats[ExcludeSuppressed]++;
	//			this.ApplySearch();
	//		}
	//	}

	//	private SortOption<CommentViewItem> _sortOption;
	//	public SortOption<CommentViewItem> SortOption
	//	{
	//		get { return _sortOption; }
	//		set
	//		{
	//			this.SetField(ref _sortOption, value);
	//			// TODO : Logger
	//			// TODO : Statistics to track functionality usage
	//			//this.Logger.Info($@"Apply filter options:{string.Join(@",", selectedFilterOptions.Select(f => @"'" + f.DisplayName + @"'"))}");
	//			//this.Manager.Logger.Info($@"Searching for text:'{this.TextSearch}', option: '{this.SearchOption?.DisplayName}'");
	//			//this.Stats[ExcludeSuppressed]++;
	//			this.ApplySort();
	//		}
	//	}

	//	public CommentsViewModel(ILogger logger)
	//	{
	//		if (logger == null) throw new ArgumentNullException(nameof(logger));

	//		this.Logger = logger;
	//		this.Manager = new LoginsManager(logger,
	//			new Progress<string>(this.DisplayOperationProgress),
	//			new CommentAdapter(),
	//			new Sorter<CommentViewItem>(new[]
	//			{
	//				new SortOption<CommentViewItem>(@"Type", (x, y) => string.Compare(x.Type, y.Type, StringComparison.Ordinal)),
	//				new SortOption<CommentViewItem>(@"CreateAt", (x, y) => x.CreatedAt.CompareTo(y.CreatedAt)),
	//			}), new Searcher<CommentViewItem>(new[]
	//			{
	//				new SearchOption<CommentViewItem>(@"All", v => true, true),
	//				//new SearchOption<CommentViewModel>(@"Coca Cola", v => v.Brand[0] == 'C'),
	//				//new SearchOption<CommentViewModel>(@"Fanta", v => v.Brand[0] == 'F'),
	//				//new SearchOption<CommentViewModel>(@"Sprite", v => v.Brand[0] == 'S'),
	//			}));

	//		//(item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0), new[]
	//		//	{
	//		//		new FilterOption<CommentViewModel>(@"Exclude suppressed", v => v.Name.IndexOf('S') >= 0),
	//		//		new FilterOption<CommentViewModel>(@"Exclude not in territory", v => v.Name.IndexOf('F') >= 0),
	//		//	}

	//		this.Manager.ItemInserted += this.ManagerOnItemInserted;
	//		this.Manager.ItemDeleted += this.ManagerOnItemDeleted;
	//	}

	//	private void ManagerOnItemInserted(object sender, ObjectEventArgs<CommentViewItem> e)
	//	{
	//		var viewItem = e.Item;
	//		var search = this.Manager.Search(this.TextSearch, this.SearchOption, new List<CommentViewItem>(1) { viewItem });
	//		if (search.Any())
	//		{
	//			// TODO : Find the right place
	//			var index = this.Comments.Count;
	//			this.Comments.Insert(index, viewItem);
	//		}
	//	}

	//	private void ManagerOnItemDeleted(object sender, ObjectEventArgs<CommentViewItem> args)
	//	{
	//		this.Comments.Remove(args.Item);

	//		// TODO : !!
	//		this.Manager.OperationProgress.Report(string.Empty);
	//	}

	//	public string OperationProgress
	//	{
	//		get { return _operationProgress; }
	//		set { this.SetField(ref _operationProgress, value); }
	//	}

	//	private void DisplayOperationProgress(string message)
	//	{
	//		this.OperationProgress = message;
	//	}

	//	public async Task LoadDataAsync()
	//	{
	//		var s = Stopwatch.StartNew();

	//		// TODO : Log operation usage
	//		this.Manager.OperationProgress.Report(@"Loading");

	//		var helper = new CommentTypeHelper();
	//		await helper.LoadAsync(new CommentTypeAdapter());

	//		var comments = await this.Manager.Adapter.GetByDateAsync(DateTime.Today, helper.Items);

	//		this.Manager.LoadData(comments.Select(v => new CommentViewItem(v)));

	//		this.ApplySearch();
	//		this.Logger.Info($@"LoadData took {s.ElapsedMilliseconds} ms");

	//		this.Manager.OperationProgress.Report(string.Empty);
	//	}

	//	public void ExcludeSuppressed()
	//	{
	//		//var s = Stopwatch.StartNew();
	//		//this.Logger.Info(@"Loading articles...");

	//		// TODO : !!! Log operation & time
	//		this.Manager.FilterOptions[0].Flip();
	//		this.ApplySearch();

	//		// TODO : Logger
	//		// TODO : Statistics to track functionality usage
	//		//this.Stats[ExcludeSuppressed]++;
	//	}

	//	public void ExcludeNotInTerritory()
	//	{
	//		// TODO : !!! Log operation & time
	//		this.Manager.FilterOptions[1].Flip();
	//		this.ApplySearch();
	//	}

	//	private void ApplySearch()
	//	{
	//		//var viewItems = this.Manager.Search(this.TextSearch, this.SearchOption);

	//		//this.Articles.Clear();
	//		//foreach (var viewItem in viewItems)
	//		//{
	//		//	this.Articles.Add(viewItem);
	//		//}
	//	}

	//	private void ApplySort()
	//	{
	//		var index = 0;
	//		//foreach (var viewItem in this.Manager.Sort(this.Articles, this.SortOption))
	//		//{
	//		//	this.Articles[index++] = viewItem;
	//		//}
	//	}

	//	public async Task AddAsync(ModalDialog dialog)
	//	{
	//		if (dialog == null) throw new ArgumentNullException(nameof(dialog));

	//		// TODO : Log usage !!!
	//		try
	//		{
	//			await this.Manager.AddAsync(new CommentViewItem(new Comment()), dialog);
	//		}
	//		catch (Exception ex)
	//		{
	//			this.Logger.Error(ex.ToString());
	//		}
	//	}

	//	public async Task DeleteAsync(ModalDialog dialog)
	//	{
	//		if (dialog == null) throw new ArgumentNullException(nameof(dialog));

	//		// TODO : Log usage !!!
	//		try
	//		{
	//			await this.Manager.DeleteAsync(new CommentViewItem(new Comment()), dialog);
	//		}
	//		catch (Exception ex)
	//		{
	//			this.Logger.Error(ex.ToString());
	//		}
	//	}

	//	public async Task MarkAsync(ModalDialog dialog)
	//	{
	//		if (dialog == null) throw new ArgumentNullException(nameof(dialog));

	//		// TODO : Log usage !!!
	//		try
	//		{
	//			var viewItem = new CommentViewItem(new Comment());
	//			var result = this.Manager.CanMark(viewItem);
	//			switch (result.Status)
	//			{
	//				case PermissionStatus.Allow:
	//					await MarkValidatedAsync(viewItem, dialog);
	//					break;
	//				case PermissionStatus.Confirm:
	//					dialog.AcceptAction = async () => this.MarkValidatedAsync(viewItem, dialog);
	//					await dialog.ShowAsync(result.Message);
	//					break;
	//				case PermissionStatus.Deny:
	//					await dialog.ShowAsync(result.Message);
	//					break;
	//				default:
	//					throw new ArgumentOutOfRangeException();
	//			}
	//		}
	//		catch (Exception ex)
	//		{
	//			this.Logger.Error(ex.ToString());
	//		}
	//	}

	//	private async Task MarkValidatedAsync(CommentViewItem viewItem, ModalDialog dialog)
	//	{
	//		viewItem.IsMarked = true;

	//		await this.Manager.UpdateAsync(viewItem, dialog);
	//	}
	//}
}