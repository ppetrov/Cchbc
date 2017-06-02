using System;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule
{
	public sealed class AgendaScreenParam
	{
		public User User { get; }
		public DateTime DateTime { get; }

		public AgendaScreenParam(User user, DateTime dateTime)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.DateTime = dateTime;
		}
	}
}