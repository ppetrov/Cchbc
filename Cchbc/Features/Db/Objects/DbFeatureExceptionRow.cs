using System;

namespace Cchbc.Features.Db.Objects
{
	public sealed class DbFeatureExceptionRow
	{
		public readonly long Id;
		public readonly string Contents;

		public DbFeatureExceptionRow(long id, string contents)
		{
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			this.Id = id;
			this.Contents = contents;
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