using System;
using iFSA.LoginModule.Objects;

namespace iFSA.LoginModule.ViewModels
{
	public sealed class CountryViewModel
	{
		public Country Country { get; }
		public string Name => this.Country.Name;
		public string Code => this.Country.Code;

		public CountryViewModel(Country country)
		{
			if (country == null) throw new ArgumentNullException(nameof(country));

			this.Country = country;
		}
	}
}