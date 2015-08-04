using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using Cchbc.Data;
using Cchbc.Helpers;
using Cchbc.Objects;

namespace Cchbc
{
	public sealed class ArticlesContext
	{
		private ArticleManager _manager;

		public void Load(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			var brandHelper = new BrandHelper(new BrandAdapter(logger));
			var flavorHelper = new FlavorHelper(new FlavorAdapter(logger));

			_manager = new ArticleManager(logger);
			_manager.Load(brandHelper, flavorHelper, new ArticleHelper(new ArticleAdapter(logger, brandHelper.Items, flavorHelper.Items)));
		}
	}

	public sealed class ArticleManager
	{
		private readonly ILogger _logger;
		public ObservableCollection<ArticleViewItem> Articles { get; }

		// TODO : !!! User settings
		// TODO : !!! Filter && Sorter

		public ArticleManager(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this._logger = logger;
			this.Articles = new ObservableCollection<ArticleViewItem>();
		}

		public void Load(IHelper<Brand> brandHelper, IHelper<Flavor> flavorHelper, IHelper<Article> articleHelper)
		{
			if (articleHelper == null) throw new ArgumentNullException(nameof(articleHelper));
			if (brandHelper == null) throw new ArgumentNullException(nameof(brandHelper));
			if (flavorHelper == null) throw new ArgumentNullException(nameof(flavorHelper));

			var s = Stopwatch.StartNew();

			_logger.Info(@"Loading ArticleManager");

			_logger.Info(@"Load BrandHelper");
			brandHelper.Load();

			_logger.Info(@"Load FlavorHelper");
			flavorHelper.Load();

			_logger.Info(@"Load ArticleHelper");
			articleHelper.Load();

			this.Articles.Clear();
			foreach (var article in GetSorted(articleHelper.Items))
			{
				this.Articles.Add(new ArticleViewItem(article));
			}

			_logger.Info($@"ArticleManager loaded in {s.ElapsedMilliseconds} ms");
		}

		private Article[] GetSorted(Dictionary<long, Article> items)
		{
			_logger.Info(@"Sorting articles...");

			var s = Stopwatch.StartNew();
			var articles = new Article[items.Count];
			items.Values.CopyTo(articles, 0);
			Array.Sort(articles, (x, y) =>
								 {
									 var cmp = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);

									 if (cmp == 0)
									 {
										 cmp = x.Id.CompareTo(y.Id);
									 }

									 return cmp;
								 });
			_logger.Info($@"{articles.Length} articles sorted in {s.ElapsedMilliseconds} ms");

			return articles;
		}
	}

	public sealed class ArticleViewItem : ViewObject
	{
		public string Name { get; private set; }
		public string Brand { get; private set; }
		public string Flavor { get; private set; }

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

			// TODO : !!!
			// TODO : !!!
			using (var mre = new ManualResetEvent(false))
			{
				mre.WaitOne(235);
			}

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

			// TODO : !!!
			using (var mre = new ManualResetEvent(false))
			{
				mre.WaitOne(127);
			}

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

			// TODO : !!!
			using (var mre = new ManualResetEvent(false))
			{
				mre.WaitOne(574);
			}

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