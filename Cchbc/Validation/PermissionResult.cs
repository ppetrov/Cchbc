using System;

namespace Cchbc.Validation
{
	public sealed class PermissionResult
	{
		public static readonly PermissionResult Allow = new PermissionResult(PermissionType.Allow, string.Empty);

		public PermissionType Type { get; }
		public string LocalizationKeyName { get; }

		public PermissionResult(PermissionType type, string localizationKeyName)
		{
			this.Type = type;
			this.LocalizationKeyName = localizationKeyName;
		}

		public static PermissionResult Deny(string localizationKeyName)
		{
			if (localizationKeyName == null) throw new ArgumentNullException(nameof(localizationKeyName));

			return new PermissionResult(PermissionType.Deny, localizationKeyName);
		}

		public static PermissionResult Confirm(string localizationKeyName)
		{
			if (localizationKeyName == null) throw new ArgumentNullException(nameof(localizationKeyName));

			return new PermissionResult(PermissionType.Confirm, localizationKeyName);
		}
	}
}