using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureExceptionEntryRow
	{
		public readonly int ExceptionId;
		public readonly DateTime CreatedAt;
		public readonly int FeatureId;

		public DbFeatureExceptionEntryRow(int exceptionId, DateTime createdAt, int featureId)
		{
			this.ExceptionId = exceptionId;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}