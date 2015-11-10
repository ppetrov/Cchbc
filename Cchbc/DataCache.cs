using System;
using System.Collections.Generic;
using Cchbc.Helpers;
using Cchbc.Objects;

namespace Cchbc
{
	public sealed class DataCache
	{
		private readonly Dictionary<string, object> _helpers = new Dictionary<string, object>();

		public void Add<T>(Helper<T> helper) where T : IDbObject
		{
			if (helper == null) throw new ArgumentNullException(nameof(helper));

			_helpers.Add(typeof(T).Name, helper);
		}

		public void Remove<T>() where T : IDbObject
		{
			_helpers.Remove(typeof(T).Name);
		}

		public Helper<T> Get<T>() where T : IDbObject
		{
			object helper;
			if (_helpers.TryGetValue(typeof(T).Name, out helper))
			{
				return (Helper<T>)helper;
			}
			return null;
		}
	}
}