using System;
using Cchbc.Features.ExceptionsModule.Rows;

namespace Cchbc.Features.ExceptionsModule.Objects
{
	public sealed class FeatureException
	{
		public ExceptionRow Row { get; }
		public string Name { get; }

		public FeatureException(ExceptionRow row)
		{
			if (row == null) throw new ArgumentNullException(nameof(row));

			this.Row = row;
			this.Name = row.Name;
		}
	}
}