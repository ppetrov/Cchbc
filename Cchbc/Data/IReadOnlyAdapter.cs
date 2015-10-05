using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public interface IReadOnlyAdapter<T> where T : IDbObject
	{
		Task PopulateAsync(Dictionary<long, T> items);
	}
}