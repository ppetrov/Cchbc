using System.Diagnostics;
using Atos.Client;

namespace Atos.iFSA.UI.LoginModule
{
	public static class GlobalApp
	{
		public static MainContext Context { get; } = new MainContext();

		//	(msg, l) =>
		//{
		//	Debug.WriteLine(l + @":" + msg);
		//}, () => new DebugDbContext(), new ModalDialog(), new DebugFeatureManager(), new DebugLocalizationManager()
	}
}