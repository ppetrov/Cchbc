using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Objects;

namespace Cchbc.Helpers
{
	public abstract class Helper<T> : IHelper<T> where T : IReadOnlyObject
	{
		public Dictionary<long, T> Items { get; } = new Dictionary<long, T>();

		public void Load(IReadOnlyAdapter<T> adapter)
		{
			if (adapter == null) throw new ArgumentNullException(nameof(adapter));

			adapter.Fill(this.Items);
		}
	}
}