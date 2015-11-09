using System;
using System.Threading.Tasks;

namespace Cchbc.Validation
{
	public sealed class PermissionResult
	{
		public static readonly Task<PermissionResult> Allow = Task.FromResult(new PermissionResult(PermissionStatus.Allow, string.Empty));

		public PermissionStatus Status { get; private set; }
		public string Message { get; private set; }

		public PermissionResult(PermissionStatus status, string message)
		{
			this.Status = status;
			this.Message = message;
		}

		public static PermissionResult Deny(string message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			return new PermissionResult(PermissionStatus.Deny, message);
		}

		public static PermissionResult Confirm(string message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			return new PermissionResult(PermissionStatus.Confirm, message);
		}
	}
}