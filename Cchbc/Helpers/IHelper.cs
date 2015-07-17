using System.Collections.Generic;
using Cchbc.Data;
using Cchbc.Objects;

namespace Cchbc.Helpers
{
	public interface IHelper<T> where T : IReadOnlyObject
	{
		Dictionary<long, T> Items { get; }

		void Load(IReadOnlyAdapter<T> adapter);
	}
}