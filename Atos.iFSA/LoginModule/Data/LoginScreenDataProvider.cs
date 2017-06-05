using System;
using Atos.Client;
using Atos.Client.Logs;
using Atos.Client.Settings;
using Atos.iFSA.Common.Objects;

namespace Atos.iFSA.LoginModule.Data
{
	public sealed class LoginScreenDataProvider
	{
		public ISettingsProvider SettingsProvider { get; }

		public LoginScreenDataProvider(ISettingsProvider settingsProvider)
		{
			if (settingsProvider == null) throw new ArgumentNullException(nameof(settingsProvider));

			this.SettingsProvider = settingsProvider;
		}

		public UserSettings GetUserSettings()
		{
			return this.SettingsProvider.GetValue(nameof(UserSettings)) as UserSettings;
		}

		public void SaveUserSettings(UserSettings userSettings)
		{
			if (userSettings == null) throw new ArgumentNullException(nameof(userSettings));

			this.SettingsProvider.Save(nameof(UserSettings), userSettings);
		}

		public User GetUser(FeatureContext featureContext, string username, string password)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));
			if (username == null) throw new ArgumentNullException(nameof(username));
			if (password == null) throw new ArgumentNullException(nameof(password));

			// TODO : !!! 
			// TODO : !!! 
			// TODO : !!! 
			featureContext.MainContext.Log(@"Check the username & password", LogLevel.Info);

			return null;
		}
	}
}