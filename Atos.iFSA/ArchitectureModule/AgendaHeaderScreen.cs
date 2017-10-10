using Atos.Client;

namespace Atos.iFSA.ArchitectureModule
{
	public static class AgendaHeaderScreen
	{
		public static void Load()
		{
			var context = default(MainContext);
			var viewModel = new AgendaHeaderScreenViewModel(context, default(IAgendaHeaderScreenDataProvider),
				AgendaHeaderValidator.CanAddHeader,
				AgendaHeaderValidator.CanUpdateHeaderDate,
				AgendaHeaderValidator.CanUpdateHeaderAddress
			);
		}
	}
}