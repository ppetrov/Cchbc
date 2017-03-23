using System;

namespace iFSA.Common.Objects
{
	public sealed class Activity
	{
		public long Id { get; set; }
		public ActivityType Type { get; }
		public ActivityStatus Status { get; }
		public DateTime Date { get; }
		public string Details { get; }
		public Outlet Outlet { get; }
	}
}