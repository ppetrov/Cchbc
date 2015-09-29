using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.ArticlesModule
{
	public sealed class FlavorAdapter : IReadOnlyAdapter<Flavor>
	{
		private readonly ILogger _logger;

		public FlavorAdapter(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			_logger = logger;
		}

		public Task PopulateAsync(Dictionary<long, Flavor> items)
		{
			_logger.Info(@"Getting flavors from db...");

			var s = Stopwatch.StartNew();

			items.Add(1, new Flavor(1, @"Coke"));
			items.Add(2, new Flavor(2, @"Orange"));

			_logger.Info($@"{items.Count} flavors retrieved from db in {s.ElapsedMilliseconds} ms");

			return Task.FromResult(true);
		}
	}
}