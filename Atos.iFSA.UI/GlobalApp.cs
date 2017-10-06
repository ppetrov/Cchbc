using Atos.Client;
using Atos.Client.Data;
using Atos.Client.Localization;
using Atos.Client.Logs;

namespace Atos.iFSA.UI
{
	public static class GlobalApp
	{
		public static MainContext Context { get; } = new MainContext();

		public static void Initialize()
		{
			Context.RegisterService<ILogger>(new Logger());
			Context.RegisterService(new LocalizationManager());
			Context.RegisterServiceCreator<IDbContext>(() => new DebugDbContext());
		}

		public static void Load()
		{
			var lines = new string[0];
			Context.GetService<LocalizationManager>().Load(lines);
		}
	}
}