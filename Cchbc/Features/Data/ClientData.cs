using System;

namespace Cchbc.Features.Data
{
	public sealed class ClientData
	{
		public readonly FeatureContextRow[] Contexts;
		public readonly FeatureExceptionRow[] Exceptions;
		public readonly FeatureRow[] Features;
		public readonly FeatureEntryRow[] FeatureEntries;
		public readonly FeatureExceptionEntryRow[] ExceptionEntries;

		public ClientData(FeatureContextRow[] contexts, FeatureExceptionRow[] exceptions, FeatureRow[] features, FeatureEntryRow[] featureEntries, FeatureExceptionEntryRow[] exceptionEntries)
		{
			if (contexts == null) throw new ArgumentNullException(nameof(contexts));
			if (exceptions == null) throw new ArgumentNullException(nameof(exceptions));
			if (features == null) throw new ArgumentNullException(nameof(features));
			if (featureEntries == null) throw new ArgumentNullException(nameof(featureEntries));
			if (exceptionEntries == null) throw new ArgumentNullException(nameof(exceptionEntries));

			this.Contexts = contexts;
			this.Exceptions = exceptions;
			this.Features = features;
			this.FeatureEntries = featureEntries;
			this.ExceptionEntries = exceptionEntries;
		}
	}
}