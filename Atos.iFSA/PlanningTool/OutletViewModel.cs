using System;
using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.PlanningTool
{
	public sealed class OutletViewModel : ViewModel
	{
		public Outlet Outlet { get; }
		public bool HasVisitForToday { get; }
		public string Name => this.Outlet.Name;

		public OutletViewModel(Outlet outlet, bool hasVisitForToday)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			this.Outlet = outlet;
			this.HasVisitForToday = hasVisitForToday;
		}
	}
}