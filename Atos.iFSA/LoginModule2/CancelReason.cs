using System;

namespace Atos.iFSA.LoginModule2
{
	public sealed class CancelReason
	{
		public long Id { get; }
		public string Name { get; }

		public CancelReason(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}