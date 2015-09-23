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
	public sealed class ArticlesViewModel
	{
		private ReadOnlyManager<ArticleViewItem> Manager { get; }

		public ObservableCollection<ArticleViewItem> Articles { get; } = new ObservableCollection<ArticleViewItem>();

		public Searcher<ArticleViewItem> Searcher { get; } = new Searcher<ArticleViewItem>(new[]
		{
			new SearcherOption<ArticleViewItem>(@"All", v => true),
			new SearcherOption<ArticleViewItem>(@"Coca Cola", v => v.Brand[0] == 'C'),
			new SearcherOption<ArticleViewItem>(@"Fanta", v => v.Brand[0] == 'F'),
			new SearcherOption<ArticleViewItem>(@"Sprite", v => v.Brand[0] == 'S'),
		}, (item, search) => item.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

		public Sorter<ArticleViewItem> Sorter { get; } = new Sorter<ArticleViewItem>(new[]
		{
			new SortOption<ArticleViewItem>(@"Name", (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal)),
			new SortOption<ArticleViewItem>(@"Brand", (x, y) => string.Compare(x.Brand, y.Brand, StringComparison.Ordinal)),
			new SortOption<ArticleViewItem>(@"Flavor", (x, y) => string.Compare(x.Flavor, y.Flavor, StringComparison.Ordinal)),
		});

		public ArticlesViewModel(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Manager = new ArticleReadOnlyManager(logger);
		}

		public void Load()
		{
			this.Manager.Load();

			Load(this.Manager.ViewItems);
		}

		public void PerformSearch(string search)
		{
			if (search == null) throw new ArgumentNullException(nameof(search));

			this.PerformSearch(this.Searcher.CurrentOption, search);
		}

		public void PerformSearch(SearcherOption<ArticleViewItem> searcherOption)
		{
			if (searcherOption == null) throw new ArgumentNullException(nameof(searcherOption));

			this.PerformSearch(searcherOption, this.Searcher.Search);
		}

		private void PerformSearch(SearcherOption<ArticleViewItem> searcherOption, string search)
		{
			Load(this.Searcher.FindAll(this.Manager.ViewItems, search, searcherOption));
		}

		private void Load(IEnumerable<ArticleViewItem> viewItems)
		{
			this.Articles.Clear();

			foreach (var viewItem in viewItems)
			{
				this.Articles.Add(viewItem);
			}
		}
	}

	public abstract class ReadOnlyManager<T> where T : ViewObject
	{
		protected ILogger Logger { get; }

		public T[] ViewItems { get; set; }
		public Sorter<T> Sorter { get; set; }

		protected ReadOnlyManager(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
		}

		public void Load()
		{
			// Sort data
			this.ViewItems = this.GetData();
			this.Sorter?.Sort(this.ViewItems, this.Sorter.CurrentOption);
		}

		public abstract T[] GetData();
	}

	public class ArticleReadOnlyManager : ReadOnlyManager<ArticleViewItem>
	{
		public ArticleReadOnlyManager(ILogger logger)
			: base(logger)
		{
		}

		public override ArticleViewItem[] GetData()
		{
			var s = Stopwatch.StartNew();

			this.Logger.Info(@"Loading ArticleManager...");

			this.Logger.Info(@"Load BrandHelper");
			var brandHelper = new BrandHelper();
			brandHelper.Load(new BrandAdapter(this.Logger));

			this.Logger.Info(@"Load FlavorHelper");
			var flavorHelper = new FlavorHelper();
			flavorHelper.Load(new FlavorAdapter(this.Logger));

			this.Logger.Info(@"Load ArticleHelper");
			var articleHelper = new ArticleHelper();
			articleHelper.Load(new ArticleAdapter(this.Logger, brandHelper.Items, flavorHelper.Items));
			this.Logger.Info($@"ArticleManager loaded in {s.ElapsedMilliseconds} ms");

			return articleHelper.Items.Values.Select(v => new ArticleViewItem(v)).ToArray();
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