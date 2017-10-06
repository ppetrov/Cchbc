using System;
using System.Collections.Generic;
using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AgendaModule
{
	public interface IAgendaDataProvider
	{
		List<AgendaOutlet> GetAgendaOutlets(DataQueryContext context, User user, DateTime date);
	}
}