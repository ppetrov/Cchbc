using System;

namespace Cchbc.Data
{
	public sealed class ReadDataAdapter
	{
		public ReadDataQueryHelper ReadDataQueryHelper { get; }

		public ReadDataAdapter(ReadDataQueryHelper readDataQueryHelper)
		{
			if (readDataQueryHelper == null) throw new ArgumentNullException(nameof(readDataQueryHelper));

			this.ReadDataQueryHelper = readDataQueryHelper;
		}
	}
}