using System;
using Atos.Features.ExceptionsModule.Rows;

namespace Atos.Features.ExceptionsModule.Objects
{
	public sealed class FeatureVersion
	{
		public VersionRow Row { get; }
		public string Name { get; }

		public FeatureVersion(VersionRow row)
		{
			if (row == null) throw new ArgumentNullException(nameof(row));

			this.Row = row;
			this.Name = row.Name;
		}
	}
}