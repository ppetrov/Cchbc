using System;

namespace Atos.Validation
{
	public static class Validator
	{
		public static PermissionResult ValidateNotNull(object value, string localizationKeyName)
		{
			if (localizationKeyName == null) throw new ArgumentNullException(nameof(localizationKeyName));

			return value == null ? PermissionResult.Deny(localizationKeyName) : PermissionResult.Allow;
		}

		public static PermissionResult ValidateNotEmpty(string value, string localizationKeyName)
		{
			return string.IsNullOrWhiteSpace(value) ? PermissionResult.Deny(localizationKeyName) : PermissionResult.Allow;
		}

		public static PermissionResult[] GetViolated(PermissionResult[] results)
		{
			if (results == null) throw new ArgumentNullException(nameof(results));
			if (results.Length == 0) throw new ArgumentOutOfRangeException(nameof(results));

			var totalViolations = 0;
			foreach (var result in results)
			{
				if (result != PermissionResult.Allow)
				{
					totalViolations++;
				}
			}
			if (totalViolations > 0)
			{
				var violations = new PermissionResult[totalViolations];

				var index = 0;
				foreach (var result in results)
				{
					if (result != PermissionResult.Allow)
					{
						violations[index++] = result;
					}
				}

				return violations;
			}
			return new PermissionResult[0];
		}

		public static PermissionResult ValidateMinLength(string value, int minLength, string localizationKeyName)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));

			if (value.Length < minLength)
			{
				return PermissionResult.Deny(localizationKeyName);
			}
			return PermissionResult.Allow;
		}

		public static PermissionResult ValidateMaxLength(string value, int maxLength, string localizationKeyName)
		{
			if (value.Length > maxLength)
			{
				return PermissionResult.Deny(localizationKeyName);
			}
			return PermissionResult.Allow;
		}

		public static PermissionResult ValidateLength(string value, int min, int max, string localizationKeyName)
		{
			var length = value.Length;
			if (min > length || length > max)
			{
				return PermissionResult.Deny(localizationKeyName + $@"({min}:{max})");
			}
			return PermissionResult.Allow;
		}
	}
}