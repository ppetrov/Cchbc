using System;
using System.Collections.Generic;
using Atos.iFSA.Objects;
using iFSA.AgendaModule.Objects;

namespace Atos.iFSA.AgendaModule
{
	public sealed class AgendaScreenParam
	{
		public User User { get; }
		public DateTime DateTime { get; }
		public List<AgendaOutlet> Outlets { get; }

		public AgendaScreenParam(User user, DateTime dateTime, List<AgendaOutlet> outlets = null)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.DateTime = dateTime;
			this.Outlets = outlets;
		}
	}
}