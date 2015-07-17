using System;
using System.Collections.Generic;
using Cchbc.Objects;

namespace Cchbc
{
	public interface IManager<T> where T : IModifiableObject
	{
		List<T> Items { get; }

		event EventHandler<ObjectEventArgs<T>> ItemInserted;
		event EventHandler<ObjectEventArgs<T>> ItemUpdated;
		event EventHandler<ObjectEventArgs<T>> ItemDeleted;

		//void InsertAsync(T item, IDialog dialog);
		//void UpdateAsync(T item, IDialog dialog);
		//void DeleteAsync(T item, IDialog dialog);
	}
}