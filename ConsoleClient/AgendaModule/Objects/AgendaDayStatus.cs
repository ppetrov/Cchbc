using System;

namespace Atos.iFSA.AgendaModule.Objects
{
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