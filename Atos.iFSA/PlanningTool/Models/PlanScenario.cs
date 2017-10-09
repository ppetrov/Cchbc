using System;

namespace Atos.iFSA.PlanningTool.Models
{
	public sealed class PlanScenario
	{
		public PlanType PlanType { get; }
		public string Description { get; }

		public PlanScenario(PlanType planType, string description)
		{
			if (description == null) throw new ArgumentNullException(nameof(description));

			this.PlanType = planType;
			this.Description = description;
		}
	}
}