using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.App.ArticlesModule.Data
{
	public sealed class QuantityAdapter
	{
		private ReadDataQueryHelper QueryHelper { get; }

		public QuantityAdapter(ReadDataQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task<Dictionary<long, long>> GetByDateAsync(DateTime date)
		{
			return Task.FromResult(new Dictionary<long, long>());
		}
	}
}