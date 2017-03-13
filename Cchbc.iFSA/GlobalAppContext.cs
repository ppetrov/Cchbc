namespace Cchbc.iFSA
{
	public static class GlobalAppContext
	{
		public static Agenda Agenda { get; set; }
		public static AppContext AppContext { get; set; }
		public static IAppNavigator AppNavigator { get; set; }
	}
}