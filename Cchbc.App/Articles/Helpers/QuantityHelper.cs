using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.App.Articles.Data;

namespace Cchbc.App.Articles.Helpers
{
	public sealed class QuantityHelper
	{
		public Task<Dictionary<long, long>> GetByDateAsync(QuantityAdapter adapter, DateTime date)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			return adapter.GetByDateAsync(date.Date);
		}
	}
}