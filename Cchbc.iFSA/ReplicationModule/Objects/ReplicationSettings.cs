using System;
using iFSA.LoginModule.Objects;

namespace iFSA.ReplicationModule.Objects
{
	public sealed class ReplicationSettings
	{
		public ReplicationConfig Config { get; }
		public Login Login { get; }

		public ReplicationSettings(ReplicationConfig config, Login login)
		{
			if (config == null) throw new ArgumentNullException(nameof(config));
			if (login == null) throw new ArgumentNullException(nameof(login));

			this.Config = config;
			this.Login = login;
		}
	}
}