using System;

namespace iFSA.Common.Objects
{
	public sealed class Activity
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
		public ActivityType Type { get; set; }
		public ActivityStatus Status { get; set; }
		public DateTime FromDate { get; set; }
		public DateTime ToDate { get; set; }
		public string Details { get; set; }
		public ActivityCloseReason CloseReason { get; set; }
		public ActivityCancelReason CancelReason { get; set; }

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