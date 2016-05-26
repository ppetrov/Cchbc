using System;
using System.Collections.Generic;

namespace Cchbc.Features.Db.Objects
{
	public sealed class FeatureClientData
	{
		public readonly List<DbFeatureContextRow> ContextRows;
		public readonly List<DbFeatureStepRow> StepRows;
		public readonly List<DbFeatureExceptionRow> ExceptionRows;
		public readonly List<DbFeatureRow> FeatureRows;
		public readonly List<DbFeatureEntryRow> FeatureEntryRows;
		public readonly List<DbFeatureEntryStepRow> EntryStepRows;		
		public readonly List<DbFeatureExceptionEntryRow> ExceptionEntryRows;

		public FeatureClientData(List<DbFeatureContextRow> contextRows, List<DbFeatureStepRow> stepRows, List<DbFeatureExceptionRow> exceptionRows, List<DbFeatureRow> featureRows, List<DbFeatureEntryRow> featureEntryRows, List<DbFeatureEntryStepRow> entryStepRows, List<DbFeatureExceptionEntryRow> exceptionEntryRows)
		{
			if (contextRows == null) throw new ArgumentNullException(nameof(contextRows));
			if (stepRows == null) throw new ArgumentNullException(nameof(stepRows));
			if (featureRows == null) throw new ArgumentNullException(nameof(featureRows));
			if (featureEntryRows == null) throw new ArgumentNullException(nameof(featureEntryRows));
			if (entryStepRows == null) throw new ArgumentNullException(nameof(entryStepRows));
			if (exceptionRows == null) throw new ArgumentNullException(nameof(exceptionRows));
			if (exceptionEntryRows == null) throw new ArgumentNullException(nameof(exceptionEntryRows));

			this.ContextRows = contextRows;
			this.StepRows = stepRows;
			this.FeatureRows = featureRows;
			this.FeatureEntryRows = featureEntryRows;
			this.EntryStepRows = entryStepRows;
			this.ExceptionRows = exceptionRows;
			this.ExceptionEntryRows = exceptionEntryRows;
		}
	}
}