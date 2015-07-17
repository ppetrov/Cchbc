using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Objects;

namespace Cchbc.Helpers
{
	public abstract class Helper<T> : IHelper<T> where T : IReadOnlyObject
	{
		public Dictionary<long, T> Items { get; } = new Dictionary<long, T>();

		public IReadOnlyAdapter<T> Adapter { get; }

		protected Helper(IReadOnlyAdapter<T> adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			this.Adapter = adapter;
		}

		public void Load()
		{
			this.Adapter.Fill(this.Items);
		}
	}
}