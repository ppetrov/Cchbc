using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Helpers;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;
using Cchbc.Validation;

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