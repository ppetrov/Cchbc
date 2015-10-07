using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public interface IModifiableAdapter<T> where T : IDbObject
	{
		Task<bool> InsertAsync(T item);
		Task<bool> UpdateAsync(T item);
		Task<bool> DeleteAsync(T item);
	}
}