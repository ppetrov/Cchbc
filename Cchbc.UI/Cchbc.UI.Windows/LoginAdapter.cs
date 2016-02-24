using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.UI
{
	public sealed class LoginAdapter : IModifiableAdapter<Login>
	{
		public QueryHelper QueryHelper { get; }

		public LoginAdapter(QueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task InsertAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			//throw new NotImplementedException();
			return Task.FromResult(true);
		}

		public Task UpdateAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}

		public Task DeleteAsync(Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}

		public List<Login> GetAll()
		{
			return new List<Login>();
		}
	}
}