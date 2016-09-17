using System;

namespace Cchbc.Features.ExceptionsModule.Rows
{
	public sealed class ExceptionEntryRow
	{
		public long Id { get; }
		public long FeatureId { get; }
		public long ExceptionId { get; }
		public long UserId { get; }
		public long VersionId { get; }
		public DateTime CreatedAt { get; }

		public ExceptionEntryRow(long id, long featureId, long exceptionId, long userId, long versionId, DateTime createdAt)
		{
			this.Id = id;
			this.FeatureId = featureId;
			this.ExceptionId = exceptionId;
			this.UserId = userId;
			this.VersionId = versionId;
			this.CreatedAt = createdAt;
		}
	}
}