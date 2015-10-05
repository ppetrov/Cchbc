using System;

namespace Cchbc.Objects
{
	public class ViewItem<T> : ViewObject where T : IDbObject
	{
		public T Item { get; }

		public ViewItem(T item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			this.Item = item;
		}
	}
}