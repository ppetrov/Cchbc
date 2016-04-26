using System;

namespace Cchbc.Features.Admin.FeatureUserModule.Objects
{
	public sealed class UserFeatureCount
	{
		public string User { get; }
		public int Value { get; }

		public UserFeatureCount(string user, int value)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.User = user;
			this.Value = value;
		}
	}
}