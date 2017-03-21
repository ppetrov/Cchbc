using System.Diagnostics;
using System.IO;
using Windows.Storage;

namespace Cchbc.Features.DashboardUI
{
	public sealed class AppContextObsolete
	{
		public static MainContext MainContext { get; } = new MainContext((msg, level) =>
		{
			Debug.WriteLine(msg, level.ToString());
		}, new TransactionContextCreator(Path.Combine(ApplicationData.Current.LocalFolder.Path, @"features.sqlite")).Create, null, null, null);
	}
}