using System;

namespace Cchbc.Validation
{
	public sealed class ValidationResult
	{
		public static readonly ValidationResult Success = new ValidationResult(string.Empty);

		public string ErrorMessage { get; private set; }

		public ValidationResult(string errorMessage)
		{
			if (errorMessage == null) throw new ArgumentNullException(nameof(errorMessage));

			this.ErrorMessage = errorMessage;
		}
	}
}