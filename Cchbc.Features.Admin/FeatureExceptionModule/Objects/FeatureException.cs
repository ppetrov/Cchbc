using System;

namespace Cchbc.Features.Admin.FeatureExceptionModule.Objects
{
	public sealed class FeatureException
	{
		public string Context { get; }
		public string Feature { get; }
		public string User { get; }
		public string Message { get; }
		public string StackTrace { get; }
		public DateTime CreatedAt { get; }

		public FeatureException(string context, string feature, string user, string message, string stackTrace, DateTime createdAt)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (stackTrace == null) throw new ArgumentNullException(nameof(stackTrace));

			this.Context = context;
			this.Feature = feature;
			this.User = user;
			this.Message = message;
			this.StackTrace = stackTrace;
			this.CreatedAt = createdAt;
		}
	}
}