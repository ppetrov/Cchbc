using Cchbc.Objects;

namespace Cchbc.Data
{
	public interface IModifiableAdapter<T> where T : IDbObject
	{
		void Insert(T item);
		void Update(T item);
		void Delete(T item);
	}
}