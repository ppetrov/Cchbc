using System;
using System.Collections.Generic;

namespace Cchbc.Features.Replication
{
	public sealed class ServerData
	{
		public Dictionary<string, long> Versions { get; }
		public Dictionary<string, long> Users { get; }
		public Dictionary<string, long> Contexts { get; }
		public Dictionary<string, long> Steps { get; }
		public Dictionary<string, long> Exceptions { get; }
		public Dictionary<long, Dictionary<string, int>> FeaturesByContext { get; }

		public ServerData(Dictionary<string, long> versions, Dictionary<string, long> users, Dictionary<string, long> contexts, Dictionary<string, long> steps, Dictionary<string, long> exceptions, Dictionary<long, Dictionary<string, int>> featuresByContext)
		{
			if (versions == null) throw new ArgumentNullException(nameof(versions));
			if (users == null) throw new ArgumentNullException(nameof(users));
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (steps == null) throw new ArgumentNullException(nameof(steps));
			if (exceptions == null) throw new ArgumentNullException(nameof(exceptions));
			if (featuresByContext == null) throw new ArgumentNullException(nameof(featuresByContext));

			this.Versions = versions;
			this.Users = users;
			this.Contexts = contexts;
			this.Steps = steps;
			this.Exceptions = exceptions;
			this.FeaturesByContext = featuresByContext;
		}
	}
}