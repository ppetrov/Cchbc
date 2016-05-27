using System;

namespace Cchbc.Features.Data
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
}