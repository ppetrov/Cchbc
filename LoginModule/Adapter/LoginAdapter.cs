using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using LoginModule.Objects;

namespace LoginModule.Adapter
{
	public sealed class LoginAdapter : IModifiableAdapter<Login>
	{
		public Task InsertAsync(ITransactionContext context, Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}

		public Task UpdateAsync(ITransactionContext context, Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}

		public Task DeleteAsync(ITransactionContext context, Login item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			return Task.FromResult(true);
		}
		
		public Task<List<Login>> GetAllAsync(ITransactionContext context)
		{
			return Task.FromResult(new List<Login>());
		}
	}
}