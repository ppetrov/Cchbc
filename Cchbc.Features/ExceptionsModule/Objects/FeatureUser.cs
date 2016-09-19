using System;
using Cchbc.Features.ExceptionsModule.Rows;

namespace Cchbc.Features.ExceptionsModule.Objects
{
	public sealed class FeatureUser
	{
		public UserRow Row { get; }
		public string Name { get; }

		public FeatureUser(UserRow row)
		{
			if (row == null) throw new ArgumentNullException(nameof(row));

			this.Row = row;
			this.Name = row.Name;
		}
	}
}