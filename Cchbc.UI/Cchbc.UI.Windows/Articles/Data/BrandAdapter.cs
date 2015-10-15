using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.ArticlesModule
{
	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		private readonly ILogger _logger;

		public BrandAdapter(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			_logger = logger;
		}

		public Task FillAsync(Dictionary<long, Brand> items)
		{
			var s = Stopwatch.StartNew();
			_logger.Info(@"Getting brands from db...");

			items.Add(1, new Brand(1, @"Coca Cola"));
			items.Add(2, new Brand(2, @"Fanta"));
			items.Add(3, new Brand(3, @"Sprite"));

			//using (var mre = new ManualResetEventSlim(false))
			//{
			//	mre.Wait(TimeSpan.FromSeconds(3));
			//}

			_logger.Info($@"{items.Count} brands retrieved from db in {s.ElapsedMilliseconds} ms");

			return Task.FromResult(true);
		}
	}
}