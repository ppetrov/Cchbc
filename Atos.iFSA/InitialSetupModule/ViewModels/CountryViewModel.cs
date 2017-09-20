using System;
using Atos.iFSA.InitialSetupModule.Objects;

namespace Atos.iFSA.InitialSetupModule.ViewModels
{
	public sealed class CountryViewModel
	{
		private Country Country { get; }
		public string Name => this.Country.Name;
		public string Code => this.Country.Code;

		public CountryViewModel(Country country)
		{
			if (country == null) throw new ArgumentNullException(nameof(country));

			this.Country = country;
		}
	}
}