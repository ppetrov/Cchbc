using System;
using System.Collections.Generic;
using Cchbc.Helpers;

namespace Cchbc
{
	public sealed class DataCache
	{
		private readonly Dictionary<string, object> _helpers = new Dictionary<string, object>();

		public void AddHelper<T>(Helper<T> helper)
		{
			if (helper == null) throw new ArgumentNullException(nameof(helper));

			_helpers.Add(typeof(T).Name, helper);
		}

		public Helper<T> GetHelper<T>()
		{
			object helper;
			if (_helpers.TryGetValue(typeof(T).Name, out helper))
			{
				return (Helper<T>)helper;
			}
			return null;
		}

		public void Clear()
		{
			_helpers.Clear();
		}
	}
}