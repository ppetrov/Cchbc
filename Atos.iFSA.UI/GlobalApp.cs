using System;
using Atos.Client;
using Atos.Client.Data;
using Atos.Client.Features;
using Atos.Client.Localization;
using Atos.Client.Logs;

namespace Atos.iFSA.UI
{
	public static class GlobalApp
	{
		public static MainContext Context { get; } = new MainContext();

		public static void Initialize()
		{
			Func<IDbContext> contextCreator = () => new DebugDbContext();

			Context.RegisterService<ILogger>(new Logger());
			Context.RegisterService<ILocalizationManager>(new LocalizationManager());			
			Context.RegisterServiceCreator(contextCreator);
			Context.RegisterService<IFeatureManager>(new FeatureManager(contextCreator));
		}

		public static void Load()
		{
			var lines = new string[0];
			Context.GetService<LocalizationManager>().Load(lines);
		}
	}
}