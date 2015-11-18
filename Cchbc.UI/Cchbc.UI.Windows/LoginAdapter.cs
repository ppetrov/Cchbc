using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.UI
{
public sealed class LoginAdapter : IModifiableAdapter<Login>
{
	public Task InsertAsync(Login item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		throw new NotImplementedException();
	}

	public Task UpdateAsync(Login item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		throw new NotImplementedException();
	}

	public Task DeleteAsync(Login item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		throw new NotImplementedException();
	}

	public Task<List<Login>> GetAllAsync()
	{
		throw new NotImplementedException();
	}
}
}