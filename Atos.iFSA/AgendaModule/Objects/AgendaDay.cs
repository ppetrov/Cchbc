using System;
using System.Collections.Generic;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AgendaModule.Objects
{
	public sealed class AgendaDay
	{
		public User User { get; }
		public DateTime Date { get; }
		public List<AgendaOutlet> Outlets { get; }

		public AgendaDay(User user, DateTime date, List<AgendaOutlet> outlets)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));

			this.User = user;
			this.Date = date;
			this.Outlets = outlets;
		}
	}

	public sealed class AgendaDayStatus
	{
		public DateTime Date { get; }
		public string Name { get; }

		public AgendaDayStatus(DateTime date, string name)
		{
			this.Date = date;
			this.Name = name;
		}
	}
}