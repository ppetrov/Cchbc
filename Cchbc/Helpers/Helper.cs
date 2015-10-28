using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Objects;

namespace Cchbc.Helpers
{
	public abstract class Helper<T> : IHelper<T> where T : IDbObject
	{
		public Dictionary<long, T> Items { get; } = new Dictionary<long, T>();

		public Task LoadAsync(IReadOnlyAdapter<T> adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			return adapter.FillAsync(this.Items);
		}
	}
}