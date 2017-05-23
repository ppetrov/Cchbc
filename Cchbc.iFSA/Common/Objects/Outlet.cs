using System;
using System.Collections.Generic;

namespace iFSA.Common.Objects
{
	public sealed class Outlet
	{
		public long Id { get; }
		public string Name { get; }
		public List<OutletAddress> Addresses { get; } = new List<OutletAddress>();
		public bool IsSuppressed { get; }

		public Outlet(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
			this.IsSuppressed = false;
		}
	}
}