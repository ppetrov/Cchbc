using System;
using Windows.Storage;
using Atos.Client.Settings;

namespace Atos.iFSA.UI
{
	public sealed class SettingsProvider : ISettingsProvider
	{
		public object GetValue(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			return ApplicationData.Current.LocalSettings.Values[name];
		}

		public void Save(string name, object value)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			ApplicationData.Current.LocalSettings.Values[name] = value;
		}
	}
}