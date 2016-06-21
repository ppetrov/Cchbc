using System;

namespace Cchbc.Features.Data
{
	public sealed class DbFeatureExceptionRow
	{
		public readonly int Id;
		public readonly string Contents;

		public DbFeatureExceptionRow(int id, string contents)
		{
			if (contents == null) throw new ArgumentNullException(nameof(contents));

			this.Id = id;
			this.Contents = contents;
		}
	}
}