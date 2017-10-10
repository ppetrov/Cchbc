using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Atos.Client;

namespace Atos.iFSA.PlanningTool
{
	public sealed class PlanByOutletViewModel : ViewModel
	{
		public OutletViewModel OutletViewModel { get; }
		public ObservableCollection<OutletPlanIndicatorViewModel> Indicators { get; } = new ObservableCollection<OutletPlanIndicatorViewModel>();

		public PlanByOutletViewModel(OutletViewModel outletViewModel)
		{
			if (outletViewModel == null) throw new ArgumentNullException(nameof(outletViewModel));

			this.OutletViewModel = outletViewModel;
		}

		public void Load(IEnumerable<PlanIndicatorViewModel> viewModels)
		{
			if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));

			var activities = default(Dictionary<long, string>);
			var quantities = default(Dictionary<long, int>);

			this.Indicators.Clear();
			foreach (var viewModel in viewModels)
			{
				var indicator = viewModel.PlanIndicator;
				var indicatorId = indicator.Id;

				var indicatorViewModel = new OutletPlanIndicatorViewModel(this.OutletViewModel, viewModel);

				switch (indicator.Type)
				{
					case PlanIndicatorType.Activity:
						activities = activities ?? GetActivities(this.OutletViewModel);
						string activity;
						if (activities.TryGetValue(indicatorId, out activity))
						{
							indicatorViewModel.Activity = activity;
						}
						break;
					case PlanIndicatorType.Volume:
						quantities = quantities ?? GetQuantities(this.OutletViewModel);
						int quantity;
						if (quantities.TryGetValue(indicatorId, out quantity))
						{
							indicatorViewModel.Quantity = quantity;
						}
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				this.Indicators.Add(indicatorViewModel);
			}
		}

		private Dictionary<long, string> GetActivities(OutletViewModel outlet)
		{
			throw new NotImplementedException();
		}

		private Dictionary<long, int> GetQuantities(OutletViewModel outlet)
		{
			throw new NotImplementedException();
		}
	}
}