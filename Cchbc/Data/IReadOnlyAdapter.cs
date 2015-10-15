using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public interface IReadOnlyAdapter<T> where T : IDbObject
	{
		Task FillAsync(Dictionary<long, T> items);
	}
}