using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atos.Client;
using Atos.Client.Logs;
using Atos.iFSA.LoginModule.Objects;
using iFSA;
using iFSA.Common.Objects;
using iFSA.ReplicationModule.Objects;

namespace Atos.iFSA.LoginModule.Data
{
	public sealed class LoginScreenDataProvider
	{
		public UserSettings GetUserSettings()
		{
			// TODO : !!!
			return null;
		}

		public void SaveUserSettings(UserSettings userSettings)
		{
			// TODO : !!!
		}

		public IEnumerable<User> GetUsers(FeatureContext featureContext)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));

			// TODO : !!! 
			featureContext.MainContext.Log(@"Load all users from database", LogLevel.Info);

			yield break;
		}
	}
}