using System;

namespace ConsoleClient.Exceptions
{
	public sealed class FeatureExceptionEntryRow
	{
		public long Id { get; }
		public long FeatureId { get; }
		public long ExceptionId { get; }
		public long UserId { get; }
		public long VersionId { get; }
		public DateTime CreatedAt { get; }

		public FeatureExceptionEntryRow(long id, long featureId, long exceptionId, long userId, long versionId, DateTime createdAt)
		{
			this.Id = id;
			this.FeatureId = featureId;
			this.ExceptionId = exceptionId;
			this.UserId = userId;
			this.VersionId = versionId;
			this.CreatedAt = createdAt;
		}
	}

	public sealed class FeatureExceptionEntry
	{
		public long Id { get; }
		public FeatureRow Feature { get; }
		public FeatureExceptionRow Exception { get; }
		public FeatureUserRow User { get; }
		public FeatureVersionRow Version { get; }
		public DateTime CreatedAt { get; }

		public string Message { get; }

		public FeatureExceptionEntry(long id, FeatureRow feature, FeatureExceptionRow exception, FeatureUserRow user, FeatureVersionRow version, DateTime createdAt)
		{
			if (exception == null) throw new ArgumentNullException(nameof(exception));
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Id = id;
			this.Feature = feature;
			this.Exception = exception;
			this.User = user;
			this.Version = version;
			this.CreatedAt = createdAt;
			this.Message = exception.Name;
		}
	}
}