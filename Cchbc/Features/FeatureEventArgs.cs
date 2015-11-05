using System;

namespace Cchbc.Features
{
	public sealed class FeatureEventArgs : EventArgs
	{
		public Feature Feature { get; }
		public Exception Exception { get; }

		public FeatureEventArgs(Feature feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Feature = feature;
			this.Exception = null;
		}

		private FeatureEventArgs(Feature feature, Exception exception)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Feature = feature;
			this.Exception = exception;
		}

		public FeatureEventArgs WithException(Exception exception)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			return new FeatureEventArgs(this.Feature, exception);
		}
	}
}