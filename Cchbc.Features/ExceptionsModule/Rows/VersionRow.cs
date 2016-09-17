using System;

namespace Cchbc.Features.ExceptionsModule.Rows
{
	public sealed class VersionRow
	{
		public long Id { get; }
		public string Name { get; }

		public VersionRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}