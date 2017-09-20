using System;
using System.Collections.Generic;

namespace Atos.iFSA.Objects
{
	public sealed class AgendaOutlet
	{
		public Outlet Outlet { get; }
		public List<Activity> Activities { get; }

		public AgendaOutlet(Outlet outlet, List<Activity> activities)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));
			if (activities == null) throw new ArgumentNullException(nameof(activities));

			this.Outlet = outlet;
			this.Activities = activities;
		}
	}
}