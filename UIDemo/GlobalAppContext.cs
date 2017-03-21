using System.Diagnostics;
using Cchbc;
using Cchbc.Data;
using iFSA;

namespace UIDemo
{
	public static class GlobalAppContext
	{
		public static MainContext MainContext { get; set; }
		public static IAppNavigator AppNavigator { get; set; }

		static GlobalAppContext()
		{
			MainContext = new MainContext((message, logLevel) =>
			{
				Debug.WriteLine(logLevel.ToString() + ":" + message);
			},
				() => default(IDbContext), new ModalDialog(), new DebugFeatureManager(), new DebugLocalizationManager());

			AppNavigator = new AppNavigator(false);
		}
	}
}