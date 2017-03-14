using System;

namespace iFSA.Common.Objects
{
	public sealed class ActivityCancelReason
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityCancelReason(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}