using System;
using System.Collections.Generic;
using System.Linq;
 
namespace Simple.Collections
{
	public class LazyList<T> : IList<T>, IDisposable
	{
		readonly List<T> cached;
		readonly IEnumerator<T> enumerator;
		bool isFinished;

		public LazyList(IEnumerable<T> list)
		{
			enumerator = list.GetEnumerator();
			isFinished = false;
			cached = new List<T>();
		}

		public T this[int index]
		{
			get
			{
				if (index < 0)
					throw new ArgumentOutOfRangeException("index");

				while (this.cached.Count <= index && !this.isFinished)
					this.GetNext();

				return this.cached[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public int Count
		{
			get
			{
				this.Finish();
				return this.cached.Count;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			int current = 0;

			while (current < this.cached.Count || !isFinished)
			{
				if (current == this.cached.Count)
					this.GetNext();

				if (current != this.cached.Count)
					yield return this.cached[current];

				current++;
			}
		}

		public void Dispose()
		{
			this.enumerator.Dispose();
			this.isFinished = true;
		}

		public int IndexOf(T item)
		{
			int result = this.cached.IndexOf(item);

			while (result == -1 && !this.isFinished)
			{
				this.GetNext();

				if (this.cached.Last().Equals(item))
					result = this.cached.Count - 1;
			}

			return result;
		}

		public void Insert(int index, T item)
		{
			throw new NotSupportedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		public void Add(T item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			throw new NotSupportedException();
		}

		public bool Contains(T item)
		{
			return IndexOf(item) != -1;
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			foreach (var item in this)
				array[arrayIndex++] = item;
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		public bool Remove(T item)
		{
			throw new NotSupportedException();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private void GetNext()
		{
			if (!this.isFinished)
			{
				if (enumerator.MoveNext())
				{
					this.cached.Add(this.enumerator.Current);
				}
				else
				{
					this.isFinished = true;
					this.enumerator.Dispose();
				}
			}
		}

		private void Finish()
		{
			while (!this.isFinished)
				this.GetNext();
		}
	}
}
