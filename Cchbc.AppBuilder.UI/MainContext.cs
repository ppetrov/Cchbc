using System.IO;
using Windows.Storage;

namespace Atos.AppBuilder.UI
{
	public sealed class GlobalMainContext
	{
		public static MainContext MainContext { get; } = new MainContext(null, new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"features.sqlite")).Create, new ModalDialog(), null, null);
	}
}