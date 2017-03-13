using System;

namespace Cchbc.iFSA.ReplicationModule.Objects
{
	public sealed class ReplicationConfig
	{
		public string Host { get; }
		public int Port { get; }

		public ReplicationConfig(string host, int port)
		{
			if (host == null) throw new ArgumentNullException(nameof(host));

			this.Host = host;
			this.Port = port;
		}
	}
}