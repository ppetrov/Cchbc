using System;
using Atos.Client.PlanningTool.Models;

namespace Atos.Client.PlanningTool.ViewModels
{
	public sealed class PlanScenarioViewModel : ViewModel
	{
		public PlanScenario PlanScenario { get; }
		public PlanType Type => this.PlanScenario.PlanType;
		public string Description => this.PlanScenario.Description;

		public PlanScenarioViewModel(PlanScenario planScenario)
		{
			if (planScenario == null) throw new ArgumentNullException(nameof(planScenario));

			this.PlanScenario = planScenario;
		}
	}
}