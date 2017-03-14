using Cchbc;

namespace iFSA
{
	public static class GlobalAppContext
	{
		public static Agenda Agenda { get; set; }
		public static MainContext MainContext { get; set; }
		public static IAppNavigator AppNavigator { get; set; }
	}
}