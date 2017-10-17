using System;
using Atos.Client;

namespace ConsoleClient.NewDesign
{
	public sealed class AgendaTabViewModel : ViewModel
	{
		private AgendaTab AgendaTab { get; }

		public string Name => this.AgendaTab.Name;
		public AgendaTabCategory Category => this.AgendaTab.Category;

		public AgendaTabViewModel(AgendaTab agendaTab)
		{
			if (agendaTab == null) throw new ArgumentNullException(nameof(agendaTab));

			this.AgendaTab = agendaTab;
		}
	}
}