using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.UI.Comments
{
	public sealed class CommentsModule : Manager<CommentViewModel>
	{
		public CommentsModule(IModifiableAdapter<CommentViewModel> adapter,
			Sorter<CommentViewModel> sorter,
			Searcher<CommentViewModel> searcher,
			FilterOption<CommentViewModel>[] filterOptions = null)
			: base(adapter, sorter, searcher, filterOptions)
		{
		}
	}

	//public class CommentsModifiableAdapter<CommentViewModel>

	public sealed class CommentsViewModel : ViewObject
	{
		private ILogger Logger { get; }
		private Manager<CommentViewModel> Module { get; }

		public ObservableCollection<CommentViewModel> Articles { get; } = new ObservableCollection<CommentViewModel>();
		public SortOption<CommentViewModel>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<CommentViewModel>[] SearchOptions => this.Module.Searcher.Options;

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

		private SearchOption<CommentViewModel> _searchOption;
		public SearchOption<CommentViewModel> SearchOption
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

		private SortOption<CommentViewModel> _sortOption;
		public SortOption<CommentViewModel> SortOption
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
			this.Module = new CommentsModule(null,
				new Sorter<CommentViewModel>(new[]
				{
					new SortOption<CommentViewModel>(@"Type", (x, y) => string.Compare(x.Type, y.Type, StringComparison.Ordinal)),
					new SortOption<CommentViewModel>(@"CreateAt", (x, y) => x.CreatedAt.CompareTo(y.CreatedAt)),
				}), new Searcher<CommentViewModel>(new[]
				{
					new SearchOption<CommentViewModel>(@"All", v => true, true),
					//new SearchOption<CommentViewModel>(@"Coca Cola", v => v.Brand[0] == 'C'),
					//new SearchOption<CommentViewModel>(@"Fanta", v => v.Brand[0] == 'F'),
					//new SearchOption<CommentViewModel>(@"Sprite", v => v.Brand[0] == 'S'),
				}));

			//(item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0), new[]
			//	{
			//		new FilterOption<CommentViewModel>(@"Exclude suppressed", v => v.Name.IndexOf('S') >= 0),
			//		new FilterOption<CommentViewModel>(@"Exclude not in territory", v => v.Name.IndexOf('F') >= 0),
			//	}
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
			var items = new List<CommentViewModel>();
			//foreach (var item in articleHelper.Items.Values)
			//{
			//	items[index++] = new CommentViewModel(item);
			//}
			this.Module.LoadData(items);

			this.ApplySearch();
			this.Logger.Info($@"Articles loaded in {s.ElapsedMilliseconds} ms");
		}

		public void ExcludeSuppressed()
		{
			//var s = Stopwatch.StartNew();
			//this.Logger.Info(@"Loading articles...");

			// TODO : !!! Log operation & time
			this.Module.FilterOptions[0].Flip();
			this.ApplySearch();

			// TODO : Logger
			// TODO : Statistics to track functionality usage
			//this.Stats[ExcludeSuppressed]++;
		}

		public void ExcludeNotInTerritory()
		{
			// TODO : !!! Log operation & time
			this.Module.FilterOptions[1].Flip();
			this.ApplySearch();
		}

		private void ApplySearch()
		{
			var viewItems = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Articles.Clear();
			foreach (var viewItem in viewItems)
			{
				this.Articles.Add(viewItem);
			}
		}

		private void ApplySort()
		{
			var index = 0;
			foreach (var viewItem in this.Module.Sort(this.Articles, this.SortOption))
			{
				this.Articles[index++] = viewItem;
			}
		}
	}

	public sealed class CommentViewModel : IModifiableObject
	{
		public long Id { get; set; }
		private Comment Comment { get; }
		public string Type { get; set; } = string.Empty;
		public string Contents { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public CommentViewModel(Comment comment)
		{
			if (comment == null) throw new ArgumentNullException(nameof(comment));

			this.Id = comment.Id;
			this.Comment = comment;
		}
	}

	public sealed class Comment : IModifiableObject
	{
		public long Id { get; set; }
		public string Type { get; set; }
		public string Contents { get; set; }
		public DateTime CreatedAt { get; set; }

	}
}