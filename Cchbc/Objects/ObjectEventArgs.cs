using System;

namespace Cchbc.Objects
{
	public sealed class ObjectEventArgs<T> : EventArgs 
	{
		public T Item { get; }

		public ObjectEventArgs(T item)
		{
			this.Item = item;
		}
	}
}