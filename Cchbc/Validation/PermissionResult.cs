using System;
using System.Threading.Tasks;

namespace Cchbc.Validation
{
	public sealed class PermissionResult
	{
		public static readonly Task<PermissionResult> Allow = Task.FromResult(new PermissionResult(PermissionType.Allow, string.Empty));

		public PermissionType Type { get; private set; }
		public string Message { get; private set; }

		public PermissionResult(PermissionType type, string message)
		{
			this.Type = type;
			this.Message = message;
		}

		public static PermissionResult Deny(string message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			return new PermissionResult(PermissionType.Deny, message);
		}

		public static PermissionResult Confirm(string message)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));

			return new PermissionResult(PermissionType.Confirm, message);
		}
	}
}