using System;

namespace Atos.Features.ExceptionsModule.Rows
{
	public sealed class UserRow
	{
		public long Id { get; }
		public string Name { get; }

		public UserRow(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}
}