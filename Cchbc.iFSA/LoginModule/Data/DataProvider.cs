using System;
using System.Collections.Generic;
using Cchbc.Data;
using iFSA.Common.Objects;
using iFSA.LoginModule.Objects;

namespace iFSA.LoginModule.Data
{
	public static class DataProvider
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

		public static List<User> GetUsers(IDbContext dbContext)
		{
			// TODO : Query the database
			return null;
		}
	}
}