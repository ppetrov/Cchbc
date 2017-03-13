using System;
using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.iFSA.LoginModule.Objects;
using Cchbc.iFSA.Objects;
using Cchbc.Localization;

namespace Cchbc.iFSA.LoginModule.Data
{
	public static class LoginModuleDataProvider
	{
		public static IEnumerable<Country> GetCountryCodes(LocalizationManager localizationManager)
		{
			if (localizationManager == null) throw new ArgumentNullException(nameof(localizationManager));

			// TODO : Customize captions
			var countries = new[]
			{
				new Country(@"Bulgaria", @"BG"),
				new Country(@"Hungary", @"HU"),
				new Country(@"Poland", @"PL"),
			};
			Array.Sort(countries, (x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));

			return countries;
		}

		public static List<User> GetUsers(IDbContext dbContext)
		{
			// TODO : Query the database
			return null;
		}
	}
}