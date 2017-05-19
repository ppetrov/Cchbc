using System;

namespace iFSA.Common.Objects
{
	public sealed class ActivityType
	{
		public long Id { get; }
		public string Name { get; }
		public string SapCode { get; }

		public ActivityType(long id, string name, string sapCode)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (sapCode == null) throw new ArgumentNullException(nameof(sapCode));

			this.Id = id;
			this.Name = name;
			this.SapCode = sapCode;
		}
	}
}