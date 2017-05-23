using System;

namespace iFSA.Common.Objects
{
	public sealed class Activity
	{
		public long Id { get; }
		public Outlet Outlet { get; }
		public ActivityType Type { get; }
		public ActivityStatus Status { get; }
		public DateTime FromDate { get; }
		public DateTime ToDate { get; }
		public string Details { get; }

		public Activity(long id, Outlet outlet, ActivityType type, ActivityStatus status, DateTime fromDate, DateTime toDate, string details)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (status == null) throw new ArgumentNullException(nameof(status));

			this.Id = id;
			this.Type = type;
			this.Status = status;
			this.FromDate = fromDate;
			this.ToDate = toDate;
			this.Details = details;
			this.Outlet = outlet;
		}
	}
}