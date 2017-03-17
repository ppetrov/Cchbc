using System;
using iFSA.AgendaModule.Objects;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule
{
	public sealed class AgendaScreenParam
	{
		public Agenda Agenda { get; }
		public User User { get; }
		public DateTime Date { get; }

		public AgendaScreenParam(Agenda agenda)
		{
			if (agenda == null) throw new ArgumentNullException(nameof(agenda));

			this.Agenda = agenda;
		}

		public AgendaScreenParam(User user, DateTime date)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.Date = date;
		}
	}
}