using System;

namespace ConsoleClient.NewDesign
{
	public sealed class AgendaTab
	{
		public string Name { get; }
		public AgendaTabCategory Category { get; }

		public AgendaTab(string name, AgendaTabCategory category)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			this.Name = name;
			this.Category = category;
		}
	}
}