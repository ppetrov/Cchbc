using System;
using Atos.Client;
using Atos.Client.Logs;
using Atos.Client.Settings;
using Atos.iFSA.Data;
using Atos.iFSA.Objects;

namespace Atos.iFSA.LoginModule.Data
{
	public sealed class LoginScreenDataProvider
	{
		public IUserSettingsProvider UserSettingsProvider { get; }

		public LoginScreenDataProvider(IUserSettingsProvider userSettingsProvider)
		{
			if (userSettingsProvider == null) throw new ArgumentNullException(nameof(userSettingsProvider));

			this.UserSettingsProvider = userSettingsProvider;
		}

		public UserSettings GetUserSettings()
		{
			return this.UserSettingsProvider.GetValue(nameof(UserSettings)) as UserSettings;
		}

		public void SaveUserSettings(UserSettings userSettings)
		{
			if (userSettings == null) throw new ArgumentNullException(nameof(userSettings));

			this.UserSettingsProvider.Save(nameof(UserSettings), userSettings);
		}

		public User[] GetUsers(FeatureContext featureContext)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));

			featureContext.MainContext.Log(nameof(GetUsers), LogLevel.Info);

			return UserDataProvider.GetUsers(featureContext);
		}

		public User GetUser(FeatureContext featureContext, string username, string password, User[] users)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));
			if (username == null) throw new ArgumentNullException(nameof(username));
			if (password == null) throw new ArgumentNullException(nameof(password));
			if (users == null) throw new ArgumentNullException(nameof(users));

			featureContext.MainContext.Log(nameof(GetUser), LogLevel.Info);

			foreach (var user in users)
			{
				if (user.Name.Equals(username, StringComparison.OrdinalIgnoreCase))
				{
					return user;
				}
			}
			return null;
		}
	}
}