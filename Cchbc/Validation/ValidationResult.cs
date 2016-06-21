using System;

namespace Cchbc.Validation
{
	public sealed class ValidationResult
	{
		public static readonly ValidationResult Success = new ValidationResult(string.Empty);

		public string ErrorMessage { get; }
		public string Property { get; }

		public ValidationResult(string errorMessage, string property = null)
		{
			if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

			this.ErrorMessage = errorMessage;
			this.Property = property ?? string.Empty;
		}
	}
}