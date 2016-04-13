using System;
using System.Collections.Generic;
using System.Linq;
using Cchbc.App.OrderModule;
using Cchbc.Data;

namespace Cchbc.ConsoleClient
{
	public sealed class DbReplication
	{
		public void Replicate(ITransactionContext serverContext, ITransactionContext clientContext)
		{
			if (serverContext == null) throw new ArgumentNullException(nameof(serverContext));
			if (clientContext == null) throw new ArgumentNullException(nameof(clientContext));

			var contextMap = ContextReplicator.ReplicateContexts(serverContext, clientContext);
			var stepMap = StepReplicator.ReplicateSteps(serverContext, clientContext);
			var featuresMap = FeatureReplicator.ReplicateFeatures(serverContext, clientContext, contextMap);

			// TODO : Feature entry & Feature entry steps
		}
	}

	public sealed class Context
	{
		public long Id;
		public string Name;

		public Context(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Step
	{
		public long Id;
		public string Name;

		public Step(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class FeatureRow
	{
		public long Id;
		public string Name;
		public long ContextId;

		public FeatureRow(long id, string name, long contextId)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
			this.ContextId = contextId;
		}
	}
}