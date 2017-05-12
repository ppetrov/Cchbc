using System;

namespace Cchbc.Features.Data
{
	public sealed class ClientData
	{
		public readonly FeatureContextRow[] Contexts;
		public readonly FeatureRow[] Features;
		public readonly FeatureEntryRow[] FeatureEntries;
		public readonly FeatureExceptionEntryRow[] ExceptionEntries;
		public readonly FeatureExceptionRow[] Exceptions;

		public ClientData(FeatureContextRow[] contexts, FeatureRow[] features, FeatureEntryRow[] featureEntries, FeatureExceptionEntryRow[] exceptionEntries, FeatureExceptionRow[] exceptions)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (features == null) throw new ArgumentNullException(nameof(features));
			if (featureEntries == null) throw new ArgumentNullException(nameof(featureEntries));
			if (exceptionEntries == null) throw new ArgumentNullException(nameof(exceptionEntries));
			if (exceptions == null) throw new ArgumentNullException(nameof(exceptions));

			this.Contexts = contexts;
			this.Features = features;
			this.FeatureEntries = featureEntries;
			this.ExceptionEntries = exceptionEntries;
			this.Exceptions = exceptions;
		}
	}
}