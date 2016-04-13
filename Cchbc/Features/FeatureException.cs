using System;

namespace Cchbc.Features
{
	public sealed class FeatureException : DbEntry
	{
		public Exception Exception { get; }

		public FeatureException(string context, string name, Exception exception) :
			base(context, name)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));

			this.Exception = exception;
		}
	}
}