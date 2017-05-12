using System;
using System.Collections.Generic;

namespace Cchbc.Features.Replication
{
	public sealed class ServerData
	{
		public Dictionary<string, long> Versions { get; }
		public Dictionary<string, long> Users { get; }
		public Dictionary<string, long> Contexts { get; }
		public Dictionary<string, long> Exceptions { get; }
		public Dictionary<long, Dictionary<string, long>> FeaturesByContext { get; }

		public ServerData(Dictionary<string, long> versions, Dictionary<string, long> users, Dictionary<string, long> contexts, Dictionary<string, long> exceptions, Dictionary<long, Dictionary<string, long>> featuresByContext)
		{
			if (versions == null) throw new ArgumentNullException(nameof(versions));
			if (users == null) throw new ArgumentNullException(nameof(users));
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (exceptions == null) throw new ArgumentNullException(nameof(exceptions));
			if (featuresByContext == null) throw new ArgumentNullException(nameof(featuresByContext));

			this.Versions = versions;
			this.Users = users;
			this.Contexts = contexts;
			this.Exceptions = exceptions;
			this.FeaturesByContext = featuresByContext;
		}
	}
}