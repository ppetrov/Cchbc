using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using Cchbc.Data;
using Cchbc.Helpers;
using Cchbc.Objects;

namespace Cchbc
{
	public sealed class ArticlesContext
	{
		private readonly ArticleManager _manager = new ArticleManager();

		public void Load()
		{
			_manager.Load();
		}
	}

	public sealed class LogLevel
	{
		public static readonly LogLevel Trace = new LogLevel(0, @"Trace");
		public static readonly LogLevel Info = new LogLevel(1, @"Info");
		public static readonly LogLevel Warn = new LogLevel(2, @"Warn");
		public static readonly LogLevel Error = new LogLevel(3, @"Error");

		public int Id { get; private set; }
		public string Name { get; private set; }

		public LogLevel(int id, string name)
		{
			if (id < 0) throw new ArgumentNullException("name");
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class ArticleManager
	{
		public ObservableCollection<ArticleViewItem> Articles { get; private set; }

		private readonly StringBuilder _log = new StringBuilder();

		public ArticleManager()
		{
			this.Articles = new ObservableCollection<ArticleViewItem>();
		}

		// TODO : !!! Filter && Sort
		// TODO : !!! Log

		public void Load()
		{
			_log.AppendLine(@"Loading articles manager...");

			var s = Stopwatch.StartNew();

			var brandHelper = new BrandHelper();
			brandHelper.Load(new BrandAdapter());

			var flavorHelper = new FlavorHelper();
			flavorHelper.Load(new FlavorAdapter());

			var articleHelper = new ArticleHelper();
			articleHelper.Load(new ArticleAdapter(brandHelper.Items, flavorHelper.Items));

			this.Articles.Clear();
			foreach (var article in GetSortedArticles(articleHelper))
			{
				this.Articles.Add(new ArticleViewItem(article));
			}

			s.Stop();

			_log.AppendLine(string.Format(@"ArticleManager loaded in {0} ms", s.ElapsedMilliseconds));
		}

		private IEnumerable<Article> GetSortedArticles(ArticleHelper articleHelper)
		{
			var articles = new Article[articleHelper.Items.Count];

			_log.AppendLine(@"Sorting articles...");

			var w = Stopwatch.StartNew();
			articleHelper.Items.Values.CopyTo(articles, 0);
			Array.Sort(articles, (x, y) =>
								 {
									 var cmp = string.Compare(x.Name, y.Name, StringComparison.Ordinal);

									 if (cmp == 0)
									 {
										 cmp = x.Id.CompareTo(y.Id);
									 }

									 return cmp;
								 });
			w.Stop();
			_log.AppendLine(string.Format(@"{0} articles sorted in {1} ms", articles.Length, w.ElapsedMilliseconds));

			Debug.WriteLine(_log);

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
			if (article == null) throw new ArgumentNullException("article");

			this.Name = article.Name;
			this.Brand = article.Brand.Name;
			this.Flavor = article.Flavor.Name;
		}
	}

	public sealed class Article : IReadOnlyObject
	{
		public long Id { get; private set; }
		public string Name { get; private set; }
		public Brand Brand { get; private set; }
		public Flavor Flavor { get; private set; }

		public Article(long id, string name, Brand brand, Flavor flavor)
		{
			if (name == null) throw new ArgumentNullException("name");
			if (brand == null) throw new ArgumentNullException("brand");
			if (flavor == null) throw new ArgumentNullException("flavor");

			this.Id = id;
			this.Name = name;
			this.Brand = brand;
			this.Flavor = flavor;
		}
	}

	public sealed class Brand : IReadOnlyObject
	{
		public static readonly Brand Empty = new Brand(-1, string.Empty);

		public long Id { get; private set; }
		public string Name { get; private set; }

		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Flavor : IReadOnlyObject
	{
		public static readonly Flavor Empty = new Flavor(-1, string.Empty);

		public long Id { get; private set; }
		public string Name { get; private set; }

		public Flavor(long id, string name)
		{
			if (name == null) throw new ArgumentNullException("name");

			this.Id = id;
			this.Name = name;
		}
	}


	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		private readonly StringBuilder _log = new StringBuilder();

		public void Fill(Dictionary<long, Brand> items)
		{
			_log.AppendLine(@"Getting brands from db...");

			var s = Stopwatch.StartNew();

			// TODO : !!!


			s.Stop();

			_log.AppendLine(string.Format(@"{0} brands retrieved from db in {1} ms", items.Count, s.ElapsedMilliseconds));

			Debug.WriteLine(_log);
		}
	}

	public sealed class BrandHelper : Helper<Brand>
	{

	}

	public sealed class FlavorAdapter : IReadOnlyAdapter<Flavor>
	{
		private readonly StringBuilder _log = new StringBuilder();

		public void Fill(Dictionary<long, Flavor> items)
		{
			_log.AppendLine(@"Getting flavors from db...");

			var s = Stopwatch.StartNew();

			// TODO : !!!


			s.Stop();

			_log.AppendLine(string.Format(@"{0} flavors retrieved from db in {1} ms", items.Count, s.ElapsedMilliseconds));

			Debug.WriteLine(_log);
		}
	}

	public sealed class FlavorHelper : Helper<Flavor>
	{

	}

	public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
	{
		private readonly StringBuilder _log = new StringBuilder();

		public Dictionary<long, Brand> Brands { get; private set; }
		public Dictionary<long, Flavor> Flavors { get; private set; }

		public ArticleAdapter(Dictionary<long, Brand> brands, Dictionary<long, Flavor> flavors)
		{
			if (brands == null) throw new ArgumentNullException("brands");
			if (flavors == null) throw new ArgumentNullException("flavors");

			this.Brands = brands;
			this.Flavors = flavors;
		}

		public void Fill(Dictionary<long, Article> items)
		{
			_log.AppendLine(@"Getting articles from db...");

			var s = Stopwatch.StartNew();

			// TODO : !!!

			s.Stop();

			_log.AppendLine(string.Format(@"{0} articles retrieved from db in {1} ms", items.Count, s.ElapsedMilliseconds));

			Debug.WriteLine(_log);
		}
	}

	public sealed class ArticleHelper : Helper<Article>
	{

	}
}