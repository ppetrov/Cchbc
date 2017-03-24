using System;

namespace Cchbc.Validation
{
	public sealed class ValidationResult
	{
		public static readonly ValidationResult Success = new ValidationResult(string.Empty);

		public string LocalizationKeyName { get; }

		public ValidationResult(string localizationKeyName)
		{
			if (localizationKeyName == null) throw new ArgumentNullException(nameof(localizationKeyName));

			this.LocalizationKeyName = localizationKeyName;
		}
	}
}