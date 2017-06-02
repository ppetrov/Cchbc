using System;
using System.Collections.Generic;
using Atos;
using Atos.Client;
using iFSA.Common.Objects;
using iFSA.LoginModule.Objects;

namespace iFSA.LoginModule.Data
{
	public static class LoginScreenDataProvider
	{
		public static IEnumerable<Country> GetCountries()
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

		public static List<User> GetUsers(MainContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// TODO : Query the database
			return null;
		}
	}
}