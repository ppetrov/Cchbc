using System.IO;
using Windows.Storage;

namespace Cchbc.AppBuilder.UI
{
	public sealed class MainContext
	{
		public static AppContext AppContext { get; } = new AppContext(null, new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"features.sqlite")).Create, new ModalDialog());
	}
}