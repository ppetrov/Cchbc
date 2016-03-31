using System.Threading.Tasks;
using Cchbc.Objects;

namespace Cchbc.Data
{
	public interface IModifiableAdapter<T> where T : IDbObject
	{
		Task InsertAsync(ITransactionContext context, T item);
		Task UpdateAsync(ITransactionContext context, T item);
		Task DeleteAsync(ITransactionContext context, T item);
	}
}