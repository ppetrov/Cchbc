using System;

namespace Cchbc.Features.Db.Objects
{
	public sealed class DbExceptionRow
	{
		public readonly string Message;
		public readonly string StackTrace;
		public readonly DateTime CreatedAt;
		public readonly long FeatureId;

		public DbExceptionRow(string message, string stackTrace, DateTime createdAt, long featureId)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (stackTrace == null) throw new ArgumentNullException(nameof(stackTrace));

			this.Message = message;
			this.StackTrace = stackTrace;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}