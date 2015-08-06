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
        private ArticleManager Manager { get; }

        public ILogger Logger { get; }
        public ObservableCollection<ArticleViewItem> Articles { get; } = new ObservableCollection<ArticleViewItem>();
        public ObservableCollection<SearcherOption<ArticleViewItem>> FilterOptions { get; } = new ObservableCollection<SearcherOption<ArticleViewItem>>();

        public ArticlesViewModel(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this.Logger = logger;
            this.Manager = new ArticleManager(logger);
        }

        public void Load()
        {
            var brandHelper = new BrandHelper(new BrandAdapter(this.Logger));
            var flavorHelper = new FlavorHelper(new FlavorAdapter(this.Logger));

            // Load articles
            this.Manager.Load(brandHelper, flavorHelper, new ArticleHelper(new ArticleAdapter(this.Logger, brandHelper.Items, flavorHelper.Items)));

            this.FilterOptions.Add(new SearcherOption<ArticleViewItem>(@"All", v => true));
            this.FilterOptions.Add(new SearcherOption<ArticleViewItem>(@"Coca Cola", v => v.Brand[0] == 'C'));
            this.FilterOptions.Add(new SearcherOption<ArticleViewItem>(@"Fanta", v => v.Brand[0] == 'F'));
            this.FilterOptions.Add(new SearcherOption<ArticleViewItem>(@"Sprite", v => v.Brand[0] == 'S'));

            // Create the filter from this.Manager.Article(All articles)
            this.Manager.Searcher = new Searcher<ArticleViewItem>(this.Manager.Articles, this.FilterOptions, (vi, s) => vi.Name.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
            var viewItems = this.Manager.Articles;

            // Apply any initial filter
            if (this.Manager.Searcher != null)
            {
                var filter = this.Manager.Searcher;
                if (filter.CurrentOption != null || filter.Search != string.Empty)
                {
                    viewItems = new ObservableCollection<ArticleViewItem>(filter.FindAll(filter.CurrentOption, filter.Search, this.Manager.Articles));
                }
            }

            // Add articles to collection
            this.Articles.Clear();
            foreach (var viewItem in viewItems)
            {
                this.Articles.Add(viewItem);
            }
        }

        public void ApplyFilter(SearcherOption<ArticleViewItem> searcherOption)
        {
            if (searcherOption == null) throw new ArgumentNullException(nameof(searcherOption));

            this.ApplyFilter(searcherOption, this.Manager.Searcher.Search);
        }

        public void ApplyFilter(string search)
        {
            this.ApplyFilter(this.Manager.Searcher.CurrentOption, this.Manager.Searcher.Search);
        }

        private void ApplyFilter(SearcherOption<ArticleViewItem> searcherOption, string search)
        {
            if (this.Manager.Searcher.IsChanged(searcherOption, search))
            {
                var result = this.Manager.Searcher.FindAll(searcherOption, this.Manager.Searcher.Search);

                this.Articles.Clear();
                foreach (var viewItem in result)
                {
                    this.Articles.Add(viewItem);
                }
            }
        }
    }

    public sealed class ArticleManager
    {
        public ILogger Logger { get; }
        public ObservableCollection<ArticleViewItem> Articles { get; }

        public Sorter<ArticleViewItem> Sorter { get; set; }
        public Searcher<ArticleViewItem> Searcher { get; set; }

        public ArticleManager(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this.Logger = logger;
            this.Articles = new ObservableCollection<ArticleViewItem>();
        }

        public void Load(IHelper<Brand> brandHelper, IHelper<Flavor> flavorHelper, IHelper<Article> articleHelper)
        {
            if (articleHelper == null) throw new ArgumentNullException(nameof(articleHelper));
            if (brandHelper == null) throw new ArgumentNullException(nameof(brandHelper));
            if (flavorHelper == null) throw new ArgumentNullException(nameof(flavorHelper));

            var s = Stopwatch.StartNew();

            this.Logger.Info(@"Loading ArticleManager");

            this.Logger.Info(@"Load BrandHelper");
            brandHelper.Load();

            this.Logger.Info(@"Load FlavorHelper");
            flavorHelper.Load();

            this.Logger.Info(@"Load ArticleHelper");
            articleHelper.Load();

            // Sort articles
            var viewItems = new ObservableCollection<ArticleViewItem>(articleHelper.Items.Values.Select(v => new ArticleViewItem(v)).ToList());
            this.Sorter?.Sort(viewItems, this.Sorter.DefaultOption);

            // Add articles to the collection
            this.Articles.Clear();
            foreach (var viewItem in viewItems)
            {
                this.Articles.Add(viewItem);
            }

            this.Logger.Info($@"ArticleManager loaded in {s.ElapsedMilliseconds} ms");
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
        public BrandHelper(IReadOnlyAdapter<Brand> adapter) : base(adapter)
        {
        }
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
        public FlavorHelper(IReadOnlyAdapter<Flavor> adapter) : base(adapter)
        {
        }
    }

    public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
    {
        private readonly ILogger _logger;
        public Dictionary<long, Brand> Brands { get; private set; }
        public Dictionary<long, Flavor> Flavors { get; private set; }

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
        public ArticleHelper(IReadOnlyAdapter<Article> adapter) : base(adapter)
        {
        }
    }
}