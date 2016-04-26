#if SQLITE
using System.Collections;
using System.Collections.Generic;

namespace System.SQLite
{
	public class SQLiteDataParameterCollection : IDataParameterCollection
	{
		private readonly List<IDbDataParameter> _parameters = new List<IDbDataParameter>();

		public IEnumerator<IDbDataParameter> GetEnumerator()
		{
			return _parameters.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public int Count { get { return _parameters.Count; } }
		public bool IsReadOnly { get { return false; } }

		public IDbDataParameter this[int index]
		{
			get { return _parameters[index]; }
			set { _parameters[index] = value; }
		}

		public void Add(IDbDataParameter item)
		{
			if (item == null) throw new ArgumentNullException("item");

			_parameters.Add(item);
		}

		public void Insert(int index, IDbDataParameter item)
		{
			if (item == null) throw new ArgumentNullException("item");

			_parameters.Insert(index, item);
		}

		public bool Contains(IDbDataParameter item)
		{
			if (item == null) throw new ArgumentNullException("item");

			return _parameters.Contains(item);
		}

		public int IndexOf(IDbDataParameter item)
		{
			if (item == null) throw new ArgumentNullException("item");

			return _parameters.IndexOf(item);
		}

		public void CopyTo(IDbDataParameter[] array, int arrayIndex)
		{
			_parameters.CopyTo(array, arrayIndex);
		}

		public bool Remove(IDbDataParameter item)
		{
			if (item == null) throw new ArgumentNullException("item");

			return _parameters.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_parameters.RemoveAt(index);
		}

		public void Clear()
		{
			_parameters.Clear();
		}
	}
}
#endif