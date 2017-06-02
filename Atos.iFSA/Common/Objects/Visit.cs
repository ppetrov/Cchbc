using System;
using System.Collections.Generic;

namespace iFSA.Common.Objects
{
	public sealed class Visit
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
		public DateTime FromDate { get; }
		public DateTime ToDate { get; }
		public List<Activity> Activities { get; } = new List<Activity>();

		public Visit(long id, Outlet outlet, DateTime fromDate, DateTime toDate)
		{
			Id = id;
			Outlet = outlet;
			FromDate = fromDate;
			ToDate = toDate;
		}
	}
}