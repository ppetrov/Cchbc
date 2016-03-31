using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace Cchbc.Helpers
{
	public abstract class Helper<T> : IHelper<T>
	{
		public Dictionary<long, T> Items { get; } = new Dictionary<long, T>();

		public void Load(ITransactionContext context, IReadOnlyAdapter<T> adapter, Func<T, long> selector)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			this.Items.Clear();

            adapter.Fill(context, this.Items, selector);
		}
	}
}