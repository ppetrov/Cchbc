using System;

namespace Cchbc.Features
{
	public sealed class FeatureEventArgs : EventArgs
	{
		public Feature Feature { get; }
		public Exception Exception { get; private set; }

		public FeatureEventArgs(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Feature = feature;
			this.Exception = null;
		}

		public FeatureEventArgs WithException(Exception exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Exception = exception;

			return this;
		}
	}
}