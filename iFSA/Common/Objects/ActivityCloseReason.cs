using System;

namespace iFSA.Common.Objects
{
	public sealed class ActivityCloseReason
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityCloseReason(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}