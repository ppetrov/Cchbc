using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI
{
	public sealed class LoginViewModel : ViewObject
	{
		private Core Core { get; }
		private LoginManager Manager { get; }
		private string Context { get; } = nameof(LoginViewModel);

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
				this.ApplySort();
			}
		}

		private bool _isBusy;
		public bool IsBusy
		{
			get { return _isBusy; }
			private set { this.SetField(ref _isBusy, value); }
		}

		public LoginViewModel(Core core)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));

			this.Core = core;
			this.Manager = new LoginManager(core.Logger, new LoginAdapter(core.Logger), new Sorter<LoginViewItem>(new[]
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
				Debug.WriteLine(@"START!");
			};
			this.Manager.OperationEnd += (sender, args) =>
			{
				this.IsBusy = false;
				this.Core.FeatureManager.Stop(args.Feature);
				Debug.WriteLine(@"END!");
			};
			this.Manager.OperationError += (sender, args) =>
			{
				this.Core.Logger.Error(args.Exception.ToString());
				Debug.WriteLine(@"ERROR!" + args.Exception.ToString());
			};

			this.Manager.ItemInserted += ManagerOnItemInserted;
			this.Manager.ItemUpdated += ManagerOnItemUpdated;
			this.Manager.ItemDeleted += ManagerOnItemDeleted;
		}

		public async Task LoadDataAsync()
		{
			this.Logins.Clear();
			this.Manager.SetupData((await this.Manager.Adapter.GetAllAsync()).Select(v => new LoginViewItem(v)).ToList());
			foreach (var login in this.Manager.ViewItems)
			{
				this.Logins.Add(login);
			}
		}

		public Task AddAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			return this.Manager.InsertAsync(viewItem, dialog, new Feature(this.Context, nameof(AddAsync), string.Empty));
		}

		public Task DeleteAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			return this.Manager.DeleteAsync(viewItem, dialog, new Feature(this.Context, nameof(DeleteAsync), string.Empty));
		}

		public Task PromoteUserAsync(LoginViewItem viewItem, ModalDialog dialog)
		{
			if (dialog == null) throw new ArgumentNullException(nameof(dialog));
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			return this.Manager.ExecuteAsync(viewItem, dialog, new Feature(this.Context, nameof(PromoteUserAsync), string.Empty), this.Manager.CanPromoteAsync, this.PromoteValidatedAsync);
		}

		private async Task PromoteValidatedAsync(LoginViewItem viewItem, FeatureEventArgs args)
		{
			viewItem.IsSystem = true;

			await this.Manager.UpdateValidatedAsync(viewItem, args);
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
	}
}