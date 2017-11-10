using System;
using System.Collections.Generic;
using Atos.Client;
using Atos.iFSA.Objects;

namespace ConsoleClient.NewDesign
{
	public sealed class AgendaInfoPageViewModel : PageViewModel
	{
		public AgendaInfoPageViewModel(MainContext mainContext) : base(mainContext)
		{
		}
	}



	public sealed class OutletImage
	{

	}

	public sealed class OutletImageViewModel : ViewModel
	{
		public OutletImage OutletImage { get; }

		public OutletImageViewModel(OutletImage outletImage)
		{
			if (outletImage == null) throw new ArgumentNullException(nameof(outletImage));

			this.OutletImage = outletImage;
		}
	}


	public sealed class PerformanceIndicator
	{
		public Outlet Outlet { get; }

		public string Name { get; }
		public int Current { get; }
		public int Total { get; }
		public bool IsPercent { get; }
	}

	public sealed class PerformanceIndicatorViewModel : ViewModel
	{
		public PerformanceIndicator PerformanceIndicator { get; }

		public string Name { get; }
		public string Value { get; }
		public int Percent { get; }

		public PerformanceIndicatorViewModel(PerformanceIndicator performanceIndicator)
		{
			if (performanceIndicator == null) throw new ArgumentNullException(nameof(performanceIndicator));

			this.PerformanceIndicator = performanceIndicator;
		}
	}

	public enum AgendaActivityTabType
	{
		Today,
		LongTerm,
		ViewAll
	}

	public sealed class AgendaActivityTab
	{
		public string Name { get; }
		public AgendaActivityTabType Type { get; }
	}

	public sealed class AgendaActivityTabViewModel : ViewModel
	{
		public string Name { get; }
		public AgendaActivityTabType Type { get; }
	}








	public sealed class AgendaOutletViewModel : ViewModel
	{
		public Outlet Outlet { get; }

		public string Name => this.Outlet.Name;

		public AgendaOutletViewModel(Outlet outlet)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));
			Outlet = outlet;
		}
	}






	public sealed class AgendaPromoViewModel : ViewModel
	{

	}

	public sealed class AgendaNotesViewModel : ViewModel
	{

	}

	public sealed class AgendaDocsViewModel : ViewModel
	{

	}


}