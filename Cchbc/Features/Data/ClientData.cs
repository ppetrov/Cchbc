using System;
using System.Collections.Generic;

namespace Cchbc.Features.Data
{
	public sealed class ClientData
	{
		public readonly List<DbFeatureContextRow> ContextRows;
		public readonly List<DbFeatureExceptionRow> ExceptionRows;
		public readonly List<DbFeatureRow> FeatureRows;
		public readonly List<DbFeatureEntryRow> FeatureEntryRows;
		public readonly List<DbFeatureExceptionEntryRow> ExceptionEntryRows;

		public ClientData(
			List<DbFeatureContextRow> contextRows, List<DbFeatureExceptionRow> exceptionRows,
			List<DbFeatureRow> featureRows, List<DbFeatureEntryRow> featureEntryRows,
			List<DbFeatureExceptionEntryRow> exceptionEntryRows)
		{
			if (contextRows == null) throw new ArgumentNullException(nameof(contextRows));
			if (featureRows == null) throw new ArgumentNullException(nameof(featureRows));
			if (featureEntryRows == null) throw new ArgumentNullException(nameof(featureEntryRows));
			if (exceptionRows == null) throw new ArgumentNullException(nameof(exceptionRows));
			if (exceptionEntryRows == null) throw new ArgumentNullException(nameof(exceptionEntryRows));

			this.ContextRows = contextRows;
			this.ExceptionRows = exceptionRows;
			this.FeatureRows = featureRows;
			this.FeatureEntryRows = featureEntryRows;
			this.ExceptionEntryRows = exceptionEntryRows;
		}
	}
}