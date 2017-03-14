using System;

namespace iFSA.Common.Objects
{
	public sealed class OutletAddress
	{
		public long Outlet { get; }
		public long Id { get; }
		public string Street { get; }
		public int Number { get; }
		public string City { get; }

		public OutletAddress(long outlet, long id, string street, int number, string city)
		{
			if (street == null) throw new ArgumentNullException(nameof(street));
			if (city == null) throw new ArgumentNullException(nameof(city));

			this.Outlet = outlet;
			this.Id = id;
			this.Street = street;
			this.Number = number;
			this.City = city;
		}
	}
}