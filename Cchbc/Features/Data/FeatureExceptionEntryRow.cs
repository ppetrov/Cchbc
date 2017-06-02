using System;

namespace Atos.Features.Data
{
	public sealed class FeatureExceptionEntryRow
	{
		public readonly long ExceptionId;
		public readonly DateTime CreatedAt;
		public readonly long FeatureId;

		public FeatureExceptionEntryRow(long exceptionId, DateTime createdAt, long featureId)
		{
			this.ExceptionId = exceptionId;
			this.CreatedAt = createdAt;
			this.FeatureId = featureId;
		}
	}
}