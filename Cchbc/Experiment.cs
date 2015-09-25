using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Cchbc.Data;
using Cchbc.Helpers;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc
{
	public sealed class ArticlesViewModel : ViewObject
	{
		private ReadOnlyManager<ArticleViewItem> Manager { get; }

		public ObservableCollection<ArticleViewItem> Articles { get; } = new ObservableCollection<ArticleViewItem>();
		public SortOption<ArticleViewItem>[] SortOptions => this.Manager.Sorter.Options;
		public SearchOption<ArticleViewItem>[] SearchOptions => this.Manager.Searcher.Options;

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetField(ref _textSearch, value);
				this.Display(this.Manager.PerformSearch(this.SearchOption, value));
			}
		}

		private SearchOption<ArticleViewItem> _searchOption;
		public SearchOption<ArticleViewItem> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				this.Display(this.Manager.PerformSearch(value, this.TextSearch));
			}
		}

		public ArticlesViewModel(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Manager = new ArticleReadOnlyManager(logger, DataLoader, CreateSorter(), CreateSearcher());
		}

		private static ArticleViewItem[] DataLoader(ILogger logger)
		{
			var s = Stopwatch.StartNew();

			logger.Info(@"Loading articles...");

			var brandHelper = new BrandHelper();
			brandHelper.Load(new BrandAdapter(logger));

			var flavorHelper = new FlavorHelper();
			flavorHelper.Load(new FlavorAdapter(logger));

			var articleHelper = new ArticleHelper();
			articleHelper.Load(new ArticleAdapter(logger, brandHelper.Items, flavorHelper.Items));
			logger.Info($@"Articles loaded in {s.ElapsedMilliseconds} ms");

			return articleHelper.Items.Values.Select(v => new ArticleViewItem(v)).ToArray();
		}

		private static Searcher<ArticleViewItem> CreateSearcher()
		{
			return new Searcher<ArticleViewItem>(new[]
			{
				new SearchOption<ArticleViewItem>(@"All", v => true, true),
				new SearchOption<ArticleViewItem>(@"Coca Cola", v => v.Brand[0] == 'C'),
				new SearchOption<ArticleViewItem>(@"Fanta", v => v.Brand[0] == 'F'),
				new SearchOption<ArticleViewItem>(@"Sprite", v => v.Brand[0] == 'S'),
			}, (item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
		}

		private static Sorter<ArticleViewItem> CreateSorter()
		{
			return new Sorter<ArticleViewItem>(new[]
			{
				new SortOption<ArticleViewItem>(@"Name", (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal)),
				new SortOption<ArticleViewItem>(@"Brand", (x, y) => string.Compare(x.Brand, y.Brand, StringComparison.Ordinal)),
				new SortOption<ArticleViewItem>(@"Flavor", (x, y) => string.Compare(x.Flavor, y.Flavor, StringComparison.Ordinal)),
			});
		}

		public void Load()
		{
			this.Display(this.Manager.Load());
		}

		private void Display(IEnumerable<ArticleViewItem> viewItems)
		{
			this.Articles.Clear();

			foreach (var viewItem in viewItems)
			{
				this.Articles.Add(viewItem);
			}
		}
	}

	public class ReadOnlyManager<T> where T : ViewObject
	{
		public ILogger Logger { get; }
		public Func<ILogger, T[]> DataLoader { get; }
		public Sorter<T> Sorter { get; }
		public Searcher<T> Searcher { get; }

		public T[] ViewItems { get; private set; }

		public ReadOnlyManager(ILogger logger, Func<ILogger, T[]> dataLoader, Sorter<T> sorter, Searcher<T> searcher)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (dataLoader == null) throw new ArgumentNullException(nameof(dataLoader));
			if (sorter == null) throw new ArgumentNullException(nameof(sorter));
			if (searcher == null) throw new ArgumentNullException(nameof(searcher));

			this.Logger = logger;
			this.DataLoader = dataLoader;
			this.Sorter = sorter;
			this.Searcher = searcher;
		}

		public T[] Load()
		{
			this.ViewItems = this.DataLoader(this.Logger);
			this.Logger.Info(@"Sort items");
			this.Sorter.Sort(this.ViewItems, this.Sorter.CurrentOption);
			this.Logger.Info(@"Setup counts for options");
			this.Searcher.SetupCounts(this.ViewItems);

			return this.ViewItems;
		}

		public IEnumerable<T> PerformSearch(SearchOption<T> searchOption, string textSearch)
		{
			this.Logger.Info(@"Searching for items...");
			return this.Searcher.FindAll(this.ViewItems, textSearch, searchOption);
		}
	}

	public sealed class ArticleReadOnlyManager : ReadOnlyManager<ArticleViewItem>
	{
		public ArticleReadOnlyManager(ILogger logger, Func<ILogger, ArticleViewItem[]> dataLoader,
			Sorter<ArticleViewItem> sorter, Searcher<ArticleViewItem> searcher) : base(logger, dataLoader, sorter, searcher)
		{
		}
	}

	public sealed class ArticleViewItem : ViewObject
	{
		public string Name { get; }
		public string Brand { get; }
		public string Flavor { get; }

		public ArticleViewItem(Article article)
		{
			if (article == null) throw new ArgumentNullException(nameof(article));

			this.Name = article.Name;
			this.Brand = article.Brand.Name;
			this.Flavor = article.Flavor.Name;
		}
	}

	public sealed class Article : IReadOnlyObject
	{
		public long Id { get; }
		public string Name { get; }
		public Brand Brand { get; }
		public Flavor Flavor { get; }

		public Article(long id, string name, Brand brand, Flavor flavor)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (brand == null) throw new ArgumentNullException(nameof(brand));
			if (flavor == null) throw new ArgumentNullException(nameof(flavor));

			this.Id = id;
			this.Name = name;
			this.Brand = brand;
			this.Flavor = flavor;
		}
	}

	public sealed class Brand : IReadOnlyObject
	{
		public static readonly Brand Empty = new Brand(-1, string.Empty);

		public long Id { get; }
		public string Name { get; }

		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Flavor : IReadOnlyObject
	{
		public static readonly Flavor Empty = new Flavor(-1, string.Empty);

		public long Id { get; }
		public string Name { get; }

		public Flavor(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public interface ILogger
	{
		bool IsDebugEnabled { get; }
		bool IsInfoEnabled { get; }
		bool IsWarnEnabled { get; }
		bool IsErrorEnabled { get; }

		void Debug(string message);
		void Info(string message);
		void Warn(string message);
		void Error(string message);
	}

	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		private readonly ILogger _logger;

		public BrandAdapter(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			_logger = logger;
		}

		public void Fill(Dictionary<long, Brand> items)
		{
			var s = Stopwatch.StartNew();
			_logger.Info(@"Getting brands from db...");

			items.Add(1, new Brand(1, @"Coca Cola"));
			items.Add(2, new Brand(2, @"Fanta"));
			items.Add(3, new Brand(3, @"Sprite"));

			_logger.Info($@"{items.Count} brands retrieved from db in {s.ElapsedMilliseconds} ms");
		}
	}

	public sealed class BrandHelper : Helper<Brand>
	{

	}

	public sealed class FlavorAdapter : IReadOnlyAdapter<Flavor>
	{
		private readonly ILogger _logger;

		public FlavorAdapter(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			_logger = logger;
		}

		public void Fill(Dictionary<long, Flavor> items)
		{
			_logger.Info(@"Getting flavors from db...");

			var s = Stopwatch.StartNew();

			items.Add(1, new Flavor(1, @"Coke"));
			items.Add(2, new Flavor(2, @"Orange"));

			_logger.Info($@"{items.Count} flavors retrieved from db in {s.ElapsedMilliseconds} ms");
		}
	}

	public sealed class FlavorHelper : Helper<Flavor>
	{
	}

	public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
	{
		private readonly ILogger _logger;
		public Dictionary<long, Brand> Brands { get; }
		public Dictionary<long, Flavor> Flavors { get; }

		public ArticleAdapter(ILogger logger, Dictionary<long, Brand> brands, Dictionary<long, Flavor> flavors)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (brands == null) throw new ArgumentNullException(nameof(brands));
			if (flavors == null) throw new ArgumentNullException(nameof(flavors));

			_logger = logger;
			this.Brands = brands;
			this.Flavors = flavors;
		}

		public void Fill(Dictionary<long, Article> items)
		{
			_logger.Info(@"Getting articles from db...");

			var s = Stopwatch.StartNew();

			items.Add(1, new Article(1, @"Coca Cola 2.0L PET", Brands[1], Flavors[1]));
			items.Add(2, new Article(2, @"Fanta 2.0L PET", Brands[2], Flavors[2]));
			items.Add(3, new Article(3, @"Sprite 2.0L PET", Brands[3], Flavor.Empty));

			_logger.Info($@"{items.Count} articles retrieved from db in {s.ElapsedMilliseconds} ms");
		}
	}

	public sealed class ArticleHelper : Helper<Article>
	{
	}
}