using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.ArticlesModule
{
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

		public Task PopulateAsync(Dictionary<long, Article> items)
		{
			_logger.Info(@"Getting articles from db...");

			var s = Stopwatch.StartNew();

			items.Add(1, new Article(1, @"Coca Cola 2.0L PET", Brands[1], Flavors[1]));
			items.Add(2, new Article(2, @"Fanta 2.0L PET", Brands[2], Flavors[2]));
			items.Add(3, new Article(3, @"Sprite 2.0L PET", Brands[3], Flavor.Empty));

			_logger.Info($@"{items.Count} articles retrieved from db in {s.ElapsedMilliseconds} ms");

			return Task.FromResult(true);
		}
	}
}