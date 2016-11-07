using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using LoginModule.Objects;

namespace LoginModule.Adapter
{
	public sealed class LoginAdapter
	{
		public Task InsertAsync(IDbContext context, Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}

		public Task UpdateAsync(IDbContext context, Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}

		public Task DeleteAsync(IDbContext context, Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}

		public Task<List<Login>> GetAllAsync(IDbContext context)
		{
			return Task.FromResult(new List<Login>());
		}
	}
}