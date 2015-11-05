using System;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc
{
	public sealed class DataCache
	{
		public async Task LoadAsync(ILogger logger, ReadDataQueryHelper queryHelper)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			// TODO : !!!
		}
	}
}