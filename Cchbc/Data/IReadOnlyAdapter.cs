using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public interface IReadOnlyAdapter<T> where T : IReadOnlyObject
	{
		void Fill(Dictionary<long, T> items);
	}
}