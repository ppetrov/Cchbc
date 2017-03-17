using System;
using System.Collections.Generic;
using Cchbc;
using iFSA.Common.Objects;

namespace iFSA.AgendaModule.Objects
{
	public sealed class AgendaData
	{
		public Func<MainContext, User, DateTime, List<AgendaOutlet>> GetAgendaOutlets { get; }
		public Func<MainContext, Outlet, OutletImage> GetDefaultOutletImage { get; }

		public AgendaData(Func<MainContext, User, DateTime, List<AgendaOutlet>> getAgendaOutlets, Func<MainContext, Outlet, OutletImage> getDefaultOutletImage)
		{
			if (getAgendaOutlets == null) throw new ArgumentNullException(nameof(getAgendaOutlets));
			if (getDefaultOutletImage == null) throw new ArgumentNullException(nameof(getDefaultOutletImage));

			this.GetAgendaOutlets = getAgendaOutlets;
			this.GetDefaultOutletImage = getDefaultOutletImage;
		}
	}
}