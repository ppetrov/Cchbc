using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public interface IModifiableAdapter<T> where T : IDbObject
	{
		Task InsertAsync(T item);
		Task UpdateAsync(T item);
		Task DeleteAsync(T item);
	}
}