using System;

namespace Cchbc.Validation
{
	public static class Validator
	{
		public static ValidationResult ValidateNotNull(object value, string localizationKeyName)
		{
			if (localizationKeyName == null) throw new ArgumentNullException(nameof(localizationKeyName));

			return value == null ? new ValidationResult(localizationKeyName) : ValidationResult.Success;
		}

		public static ValidationResult ValidateNotEmpty(string value, string localizationKeyName)
		{
			return string.IsNullOrWhiteSpace(value) ? new ValidationResult(localizationKeyName) : ValidationResult.Success;
		}

		public static ValidationResult[] GetViolated(ValidationResult[] results)
		{
			if (results == null) throw new ArgumentNullException(nameof(results));
			if (results.Length == 0) throw new ArgumentOutOfRangeException(nameof(results));

			var totalViolations = 0;
			foreach (var result in results)
			{
				if (result != ValidationResult.Success)
				{
					totalViolations++;
				}
			}
			if (totalViolations > 0)
			{
				var violations = new ValidationResult[totalViolations];

				var index = 0;
				foreach (var result in results)
				{
					if (result != ValidationResult.Success)
					{
						violations[index++] = result;
					}
				}

				return violations;
			}
			return new ValidationResult[0];
		}

		public static ValidationResult ValidateMinLength(string value, int minLength, string localizationKeyName)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			if (value.Length < minLength)
			{
				return new ValidationResult(localizationKeyName);
			}
			return ValidationResult.Success;
		}

		public static ValidationResult ValidateMaxLength(string value, int maxLength, string localizationKeyName)
		{
			if (value.Length > maxLength)
			{
				return new ValidationResult(localizationKeyName);
			}
			return ValidationResult.Success;
		}

		public static ValidationResult ValidateLength(string value, int min, int max, string localizationKeyName)
		{
			var length = value.Length;
			if (min <= length && length <= max)
			{
				return ValidationResult.Success;
			}
			return new ValidationResult(localizationKeyName + $@"({min}:{max})");
		}
	}
}