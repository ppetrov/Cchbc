using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Helpers
{
	public abstract class Helper<T> : IHelper<T>
	{
		public Dictionary<long, T> Items { get; } = new Dictionary<long, T>();

		public void Load(IReadOnlyAdapter<T> adapter, Func<T, long> selector)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

            adapter.Fill(this.Items, selector);
		}
	}
}