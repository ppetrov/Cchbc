using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Atos.Client;
using Atos.Client.Data;
using iFSA.Common.Objects;
using iFSA.LoginModule.Objects;
using iFSA.ReplicationModule.Objects;

namespace iFSA.LoginModule.Data
{
	public class LoginScreenDataProvider
	{
		public IEnumerable<Country> GetCountries()
		{
			var countries = new[]
			{
				new Country(@"Bulgaria", @"BG"),
				new Country(@"Hungary", @"HU"),
				new Country(@"Poland", @"PL"),
			};
			Array.Sort(countries, (x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));

			return countries;
		}

		public IEnumerable<User> GetUsers(FeatureContext featureContext)
		{
			if (featureContext == null) throw new ArgumentNullException(nameof(featureContext));

			return featureContext.DbContext.Execute(new Query<User>(@"", r => null));
		}

		public UserSettings GetUserSettings()
		{
			throw new NotImplementedException();
		}

		public void SaveUserSettings(UserSettings userSettings)
		{
			throw new NotImplementedException();
		}

		public Task<ReplicationConfig> GetReplicationConfig(AppSystem system, Country country)
		{
			throw new NotImplementedException();
		}
	}
}