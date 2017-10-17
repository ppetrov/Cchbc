using System;
using System.Collections.Generic;

namespace Atos.iFSA.Objects
{
	public sealed class Outlet
	{
		public long Id { get; }
		public string Name { get; }
		public List<OutletAddress> Addresses { get; } = new List<OutletAddress>();

		public Outlet(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}