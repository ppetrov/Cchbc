using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc;
using iFSA.AgendaModule.Data;
using iFSA.Common.Objects;
using iFSA.LoginModule.Objects;
using iFSA.ReplicationModule.Objects;

namespace iFSA.LoginModule.Data
{
	public sealed class LoginScreenData
	{
		public Func<UserSettings> GetUserSettings { get; }
		public Action<UserSettings> SaveUserSettings { get; }
		public Func<IEnumerable<Country>> GetCountries { get; }
		public Func<MainContext, List<User>> GetUsers { get; }
		public Func<AppSystem, Country, Task<ReplicationConfig>> GetReplicationConfig { get; }

		public LoginScreenData(Func<UserSettings> getUserSettings, Action<UserSettings> saveUserSettings, Func<IEnumerable<Country>> getCountries, Func<MainContext, List<User>> getUsers, Func<AppSystem, Country, Task<ReplicationConfig>> getReplicationConfig)
		{
			if (getUserSettings == null) throw new ArgumentNullException(nameof(getUserSettings));
			if (saveUserSettings == null) throw new ArgumentNullException(nameof(saveUserSettings));
			if (getCountries == null) throw new ArgumentNullException(nameof(getCountries));
			if (getUsers == null) throw new ArgumentNullException(nameof(getUsers));
			if (getReplicationConfig == null) throw new ArgumentNullException(nameof(getReplicationConfig));

			this.GetUserSettings = getUserSettings;
			this.SaveUserSettings = saveUserSettings;
			this.GetCountries = getCountries;
			this.GetUsers = getUsers;
			this.GetReplicationConfig = getReplicationConfig;
		}
	}
}