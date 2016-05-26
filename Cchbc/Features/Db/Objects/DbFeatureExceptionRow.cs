using System;

namespace Cchbc.Features.Db.Objects
{
	public sealed class DbFeatureExceptionRow
	{
		public readonly long Id;
		public readonly string Message;
		public readonly string StackTrace;

		public DbFeatureExceptionRow(long id, string message, string stackTrace)
		{
			if (message == null) throw new ArgumentNullException(nameof(message));
			if (stackTrace == null) throw new ArgumentNullException(nameof(stackTrace));

			this.Id = id;
			this.Message = message;
			this.StackTrace = stackTrace;
		}
	}

	public sealed class DbFeatureExceptionEntryRow
	{
		public readonly long ExceptionRowId;
		public readonly DateTime CreatedAt;
		public readonly long FeatureId;

		public DbFeatureExceptionEntryRow(long exceptionRowId, DateTime createdAt, long featureId)
		{
			this.ExceptionRowId = exceptionRowId;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}