using System;
using Atos.Client;

namespace Atos.iFSA.PlanningTool
{
	public sealed class OutletPlanIndicatorViewModel : ViewModel
	{
		public OutletViewModel OutletViewModel { get; }
		public PlanIndicatorViewModel PlanIndicatorViewModel { get; }

		private string _activity = string.Empty;
		public string Activity
		{
			get { return _activity; }
			set { this.SetProperty(ref _activity, value); }
		}

		private int _quantity;
		public int Quantity
		{
			get { return _quantity; }
			set { this.SetProperty(ref _quantity, value); }
		}

		public OutletPlanIndicatorViewModel(OutletViewModel outletViewModel, PlanIndicatorViewModel planIndicatorViewModel)
		{
			if (outletViewModel == null) throw new ArgumentNullException(nameof(outletViewModel));
			if (planIndicatorViewModel == null) throw new ArgumentNullException(nameof(planIndicatorViewModel));

			this.OutletViewModel = outletViewModel;
			this.PlanIndicatorViewModel = planIndicatorViewModel;
		}
	}
}