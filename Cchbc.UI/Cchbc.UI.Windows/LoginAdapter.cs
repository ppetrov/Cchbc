using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.UI
{
public sealed class LoginAdapter : IModifiableAdapter<Login>
{
	public void Insert(Login item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		throw new NotImplementedException();
	}

	public void Update(Login item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));

		throw new NotImplementedException();
	}

	public void Delete(Login item)
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