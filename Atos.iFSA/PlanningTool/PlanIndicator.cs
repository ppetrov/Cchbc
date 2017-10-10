using System;

namespace Atos.iFSA.PlanningTool
{
	public sealed class PlanIndicator
	{
		public long Id { get; }
		public string Name { get; }
		public PlanIndicatorType Type { get; }

		public PlanIndicator(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}