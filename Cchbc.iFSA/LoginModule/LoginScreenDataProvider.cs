using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc;
using iFSA.Common.Objects;
using iFSA.LoginModule.Objects;
using iFSA.ReplicationModule.Objects;

namespace iFSA.LoginModule
{
	public sealed class LoginScreenDataProvider
	{
		public IUserSettingsProvider UserSettingsProvider { get; }
		public Func<MainContext, List<User>> UsersProvider { get; }
		public Func<MainContext, User, DateTime, List<Visit>> VisitsProvider { get; }
		public Func<AppSystem, Country, Task<ReplicationConfig>> ReplicationConfigProvider { get; }
		public Func<IEnumerable<Country>> CountriesProvider { get; }

		public LoginScreenDataProvider(IUserSettingsProvider userSettingsProvider, Func<MainContext, List<User>> usersProvider, Func<MainContext, User, DateTime, List<Visit>> visitsProvider, Func<AppSystem, Country, Task<ReplicationConfig>> replicationConfigProvider, Func<IEnumerable<Country>> countriesProvider)
		{
			this.UserSettingsProvider = userSettingsProvider;
			this.UsersProvider = usersProvider;
			this.VisitsProvider = visitsProvider;
			this.ReplicationConfigProvider = replicationConfigProvider;
			this.CountriesProvider = countriesProvider;
		}
	}
}