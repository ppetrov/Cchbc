using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cchbc.Data
{
	public interface IReadOnlyAdapter<T>
	{
		Task FillAsync(Dictionary<long, T> items, Func<T, long> selector);
	}
}