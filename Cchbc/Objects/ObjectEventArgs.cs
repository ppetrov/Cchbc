using System;

namespace Cchbc.Objects
{
	public sealed class ObjectEventArgs<T> : EventArgs where T : IModifiableObject
	{
		public T Item { get; }

		public ObjectEventArgs(T item)
		{
			this.Item = item;
		}
	}
}