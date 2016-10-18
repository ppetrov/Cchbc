using System.IO;
using Windows.Storage;

namespace Cchbc.AppBuilder.UI
{
	public sealed class Context
	{
		public static Core Core { get; } = new Core(null, new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"features.sqlite")), new ModalDialog());
	}
}