using System;
using System.Collections.Generic;

namespace iFSA.Common.Objects
{
	public sealed class ActivityType
	{
		public long Id { get; }
		public string Name { get; }
		public List<ActivityCloseReason> CloseReasons { get; } = new List<ActivityCloseReason>();
		public List<ActivityCancelReason> CancelReasons { get; } = new List<ActivityCancelReason>();

		public ActivityType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}