using Atos.Client.PlanningTool.Models;

namespace Atos.Client.PlanningTool.ViewModels
{
	public sealed class PlanScenarioViewModel : ViewModel<PlanScenario>
	{
		public PlanType Type => this.Model.PlanType;
		public string Description => this.Model.Description;

		public PlanScenarioViewModel(PlanScenario model) : base(model)
		{
		}
	}
}