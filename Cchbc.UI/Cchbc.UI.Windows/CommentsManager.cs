using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

namespace Cchbc.UI.Comments
{
	public sealed class Comment : IDbObject
	{
		public long Id { get; set; }
		public string Type { get; set; }
		public string Contents { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public sealed class CommentViewItem : ViewItem<Comment>
	{
		public long Id { get; set; }
		public string Type { get; set; } = string.Empty;
		public string Contents { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.Now;
		public int Count { get; set; } = -1;

		public CommentViewItem(Comment item) : base(item)
		{
		}
	}

	public sealed class CommentsManager : Manager<Comment, CommentViewItem>
	{
		public ILogger Logger { get; }
		public IProgress<string> OperationProgress { get; }
		public CommentAdapter Adapter { get; }

		public CommentsManager(ILogger logger, IProgress<string> operationProgress, CommentAdapter adapter,
			Sorter<CommentViewItem> sorter,
			Searcher<CommentViewItem> searcher,
			FilterOption<CommentViewItem>[] filterOptions = null) : base(adapter, sorter, searcher, filterOptions)
		{
			this.Logger = logger;
			this.OperationProgress = operationProgress;
			this.Adapter = adapter;
		}

		public override ValidationResult[] ValidateProperties(CommentViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var s = Stopwatch.StartNew();

			// TODO : Customize !!!
			this.OperationProgress.Report(@"Validating fields");
			var validationResults = Validator.GetResults(new[]
			{
				// TODO : !!! Customize !!!
				Validator.ValidateNotNull(viewItem.Contents, @"Contents cannot be null"),
			});
			this.Logger.Info($@"Validating fields took {s.ElapsedMilliseconds} ms");

			return validationResults;
		}

		public override async Task<PermissionResult> CanAddAsync(CommentViewItem viewItem)
		{
			if (viewItem == null) throw new ArgumentNullException(nameof(viewItem));

			var s = Stopwatch.StartNew();

			try
			{
				// TODO : !!! Customize !!!
				this.OperationProgress.Report(@"Checking for unreplicated data");
				var hasUnreplicated = await this.Adapter.HasUnreplicatedAsync();
				if (hasUnreplicated)
				{
					// TODO : !!! Customize !!!
					return PermissionResult.Deny(@"Has unreplicated records. Please sync first");
				}
				// TODO : !!! Customize !!!
				this.OperationProgress.Report(@"Checking for items on the same date");
				if (this.ViewItems.Any(v => v.CreatedAt.Date == viewItem.CreatedAt.Date))
				{
					// TODO : !!! Customize !!!
					return PermissionResult.Deny(@"Already have a comment for this date");
				}
				// TODO : !!! Customize !!!
				this.OperationProgress.Report(@"Checking items limits");
				if (this.ViewItems.Count > 10)
				{
					// TODO : !!! Customize !!!
					return PermissionResult.Confirm(@"Too many comments already. Are you sure you want one more?");
				}
				return PermissionResult.Allow;
			}
			finally
			{
				this.Logger.Info($@"CanAdd took { s.ElapsedMilliseconds} ms");
			}
		}
	}










	public sealed class CommentsViewModel : ViewObject
	{
		private ILogger Logger { get; }
		private CommentsManager Manager { get; }

		public ObservableCollection<CommentViewItem> Articles { get; } = new ObservableCollection<CommentViewItem>();
		public ObservableCollection<string> Operations { get; } = new ObservableCollection<string>();
		//public SortOption<CommentViewItem>[] SortOptions => this.Module.Sorter.Options;
		//public SearchOption<CommentViewItem>[] SearchOptions => this.Module.Searcher.Options;

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

		private SearchOption<CommentViewItem> _searchOption;
		public SearchOption<CommentViewItem> SearchOption
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

		private SortOption<CommentViewItem> _sortOption;
		private string _operationProgress;

		public SortOption<CommentViewItem> SortOption
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

		public CommentsViewModel(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
			this.Manager = new CommentsManager(logger,
				new Progress<string>(DisplayOperationProgress),
				new CommentAdapter(),
				new Sorter<CommentViewItem>(new[]
				{
					new SortOption<CommentViewItem>(@"Type", (x, y) => string.Compare(x.Type, y.Type, StringComparison.Ordinal)),
					new SortOption<CommentViewItem>(@"CreateAt", (x, y) => x.CreatedAt.CompareTo(y.CreatedAt)),
				}), new Searcher<CommentViewItem>(new[]
				{
					new SearchOption<CommentViewItem>(@"All", v => true, true),
					//new SearchOption<CommentViewModel>(@"Coca Cola", v => v.Brand[0] == 'C'),
					//new SearchOption<CommentViewModel>(@"Fanta", v => v.Brand[0] == 'F'),
					//new SearchOption<CommentViewModel>(@"Sprite", v => v.Brand[0] == 'S'),
				}));

			//(item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0), new[]
			//	{
			//		new FilterOption<CommentViewModel>(@"Exclude suppressed", v => v.Name.IndexOf('S') >= 0),
			//		new FilterOption<CommentViewModel>(@"Exclude not in territory", v => v.Name.IndexOf('F') >= 0),
			//	}

			this.Manager.ItemInserted += (sender, args) =>
			{
				args.Item.Count++;
			};
		}

		public string OperationProgress
		{
			get { return _operationProgress; }
			set { this.SetField(ref _operationProgress, value); }
		}

		private void DisplayOperationProgress(string message)
		{
			if (message == string.Empty)
			{
				this.Operations.Clear();
			}
			else
			{
				this.OperationProgress = message;
				this.Operations.Add(message);
			}
		}

		public async Task LoadDataAsync()
		{
			// TODO : Log operation & time
			var s = Stopwatch.StartNew();
			this.Logger.Info(@"Loading comments...");

			//var brandHelper = new BrandHelper();
			//await brandHelper.LoadAsync(new BrandAdapter(this.Logger));

			//var flavorHelper = new FlavorHelper();
			//await flavorHelper.LoadAsync(new FlavorAdapter(this.Logger));

			//var articleHelper = new ArticleHelper();
			//await articleHelper.LoadAsync(new ArticleAdapter(this.Logger, brandHelper.Items, flavorHelper.Items));

			var index = 0;
			var items = new List<CommentViewItem>();
			//foreach (var item in articleHelper.Items.Values)
			//{
			//	items[index++] = new CommentViewModel(item);
			//}
			this.Manager.LoadData(items);

			this.ApplySearch();
			this.Logger.Info($@"Comments loaded in {s.ElapsedMilliseconds} ms");
		}

		public void ExcludeSuppressed()
		{
			//var s = Stopwatch.StartNew();
			//this.Logger.Info(@"Loading articles...");

			// TODO : !!! Log operation & time
			this.Manager.FilterOptions[0].Flip();
			this.ApplySearch();

			// TODO : Logger
			// TODO : Statistics to track functionality usage
			//this.Stats[ExcludeSuppressed]++;
		}

		public void ExcludeNotInTerritory()
		{
			// TODO : !!! Log operation & time
			this.Manager.FilterOptions[1].Flip();
			this.ApplySearch();
		}

		private void ApplySearch()
		{
			//var viewItems = this.Manager.Search(this.TextSearch, this.SearchOption);

			//this.Articles.Clear();
			//foreach (var viewItem in viewItems)
			//{
			//	this.Articles.Add(viewItem);
			//}
		}

		private void ApplySort()
		{
			var index = 0;
			//foreach (var viewItem in this.Manager.Sort(this.Articles, this.SortOption))
			//{
			//	this.Articles[index++] = viewItem;
			//}
		}

		public async Task AddAsync()
		{
			// TODO : Time operation ?!??? ==> No because we have many chunks
			// 1. Check if we can addd - fast, medium or slow -> need to time = DONE
			// 2. add(insert into db) - 100% need to time -> need to time = DONE
			// 3. fire events & update collections - fast be can be something stupid to cause medium slow -> need to time DONE if need it
			// TODO : Log usage
			try
			{
				this.Manager.OperationProgress.Report(string.Empty);
				await this.Manager.AddAsync(new CommentViewItem(new Comment()), new WinRtModalDialog());
			}
			catch (Exception ex)
			{
				this.Logger.Error(ex.ToString());
			}
		}

		public async Task DeleteAsync()
		{
			await this.Manager.DeleteAsync(new CommentViewItem(new Comment()), new WinRtModalDialog());
		}
	}

	public sealed class UsageManager
	{
		// TODO : !!! Context To be string ????
		// TODO : !!! Operation to be string ???
		public void Add(string context, string operation)
		{
			// Context:Agenda
			// Operation:Add
			// Count:23
		}
	}


	public sealed class CommentAdapter : IModifiableAdapter<Comment>
	{
		public Task<bool> InsertAsync(Comment item)
		{
			try
			{
				Debug.WriteLine(@"Inserting comment ...");
				return Task.FromResult(true);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(@"Unable to insert comment" + ex);
			}
			return Task.FromResult(false);
		}

		public Task<bool> UpdateAsync(Comment item)
		{
			return Task.FromResult(true);
		}

		public Task<bool> DeleteAsync(Comment item)
		{
			return Task.FromResult(true);
		}

		public Task<bool> HasUnreplicatedAsync()
		{
			return Task.Run(() =>
			{
				using (var mre = new ManualResetEventSlim(false))
				{
					mre.Wait(TimeSpan.FromSeconds(3));
				}
				return true;
			});
			//await Task.Delay(5000);
			//return true;
		}
	}

	public sealed class WinRtModalDialog : ModalDialog
	{
		private MessageDialog _dialog;

		public override async Task ShowAsync(string message, DialogType? type = null)
		{
			_dialog = new MessageDialog(message);
			_dialog.Commands.Add(new UICommand("Yes", _ => { this.AcceptAction(); }));
			_dialog.Commands.Add(new UICommand("No", _ => { this.DeclineAction(); }));
			await _dialog.ShowAsync();
		}
	}
}