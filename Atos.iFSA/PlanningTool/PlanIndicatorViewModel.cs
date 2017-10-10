using System;

namespace Atos.iFSA.PlanningTool
{
	public sealed class PlanIndicatorViewModel
	{
		public PlanIndicator PlanIndicator { get; }

		public string Name { get; }

		public PlanIndicatorViewModel(PlanIndicator planIndicator)
		{
			if (planIndicator == null) throw new ArgumentNullException(nameof(planIndicator));

			this.PlanIndicator = planIndicator;
			this.Name = planIndicator.Name;
		}
	}
}