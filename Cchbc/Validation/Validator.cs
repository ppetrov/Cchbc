using System;
using System.Text;

namespace Cchbc.Validation
{
	public static class Validator
	{
		public static ValidationResult ValidateNotNull(object value, string message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			return value == null ? new ValidationResult(message) : ValidationResult.Success;
		}

		public static ValidationResult ValidateNotEmpty(string value, string message)
		{
			return string.IsNullOrWhiteSpace(value) ? new ValidationResult(message) : ValidationResult.Success;
		}

		public static ValidationResult[] GetResults(ValidationResult[] results)
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

		public static string CombineResults(ValidationResult[] results)
		{
			if (results == null) throw new ArgumentNullException(nameof(results));

			if (results.Length == 0)
			{
				return string.Empty;
			}

			var buffer = new StringBuilder();

			foreach (var result in results)
			{
				if (result != ValidationResult.Success)
				{
					if (buffer.Length > 0)
					{
						buffer.Append(@", ");
					}
					buffer.Append(result.ErrorMessage);
				}
			}

			return buffer.ToString();
		}

		public static ValidationResult ValidateMinLength(string value, int minLength, string message)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			if (value.Length < minLength)
			{
				return new ValidationResult(message);
			}
			return ValidationResult.Success;
		}

		public static ValidationResult ValidateMaxLength(string value, int maxLength, string message)
		{
			if (value.Length > maxLength)
			{
				return new ValidationResult(message);
			}
			return ValidationResult.Success;
		}

		public static ValidationResult ValidateLength(string value, int min, int max, string message)
		{
			var length = value.Length;
			if (min <= length && length <= max)
			{
				return ValidationResult.Success;
			}
			return new ValidationResult(message + string.Format(@"({0}:{1})", min, max));
		}
	}
}