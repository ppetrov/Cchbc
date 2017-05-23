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

		public Activity Save(Activity activity)
		{
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			// TODO : How to get the Status ???
			//var activityStatus = new ActivityStatus(0, string.Empty);
			//var activity = new Activity(0, outlet, activityType, activityStatus, DateTime.Today, DateTime.Today, string.Empty);

			throw new NotImplementedException();
		}
	}
}