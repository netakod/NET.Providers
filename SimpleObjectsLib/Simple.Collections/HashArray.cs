using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    public class HashArray<T>
    {
		private const int DefaultCapacity = 4;
		private T[] array = null;

		public HashArray()
			: this(DefaultCapacity)
		{
		}

		public HashArray(int capacity)
		{
			this.array = new T[capacity];
		}

        public T this[int index]
        {
            get { return this.array[index]; }
			set { this.SetValue(index, value); }
        }

        public IEnumerable<T> Collection
        {
            get { return this.array; }
        }

		public int MaxIndex
		{
			get { return this.array.Length; }
		}

		//public bool ContainsIndex(int index)
  //      {
		//	return this.GetValue(index) != null;
  //      }

        public bool ContainsValue(T value)
        {
            lock (this.array)
            {
                return this.array.Contains(value);
            }
        }

		public T GetValue(int index)
		{
			lock (this.array)
			{
				if (index >= array.Length)
					return default(T);

				return this.array[index];
			}
		}

		public T TryGetValue(int index)
		{
			if (this.TryGetValue(index, out T result))
				return result;

			return default;
		}

        public bool TryGetValue(int index, out T value)
        {
			lock (this.array)
			{
				if (index >= array.Length)
				{
					value = default(T);

					return false;
				}

				value = this.array[index];

				return true;
			}
		}

		public void SetValue(int index, T value)
		{
			lock (this.array)
			{
				if (index >= this.array.Length)
					Array.Resize<T>(ref this.array, Math.Max(index + 1, this.array.Length * 2)); // Double the size

				this.array[index] = value;

				//// If index is last element and value is default(T) -> remove last null elements
				//if ((value == null && default(T) == null || value.Equals(default(T))) && index == this.array.Length - 1)
				//	while (this.array[this.array.Length - 1].Equals(default(T)) && this.array.Length > 0)
				//		Array.Resize<T>(ref this.array, this.array.Length - 1);
			}
		}

		public void Clear()
        {
            lock (this.array)
            {
				int length = this.array.Length;
				
				this.array = new T[length];
            }
        }
    }
}
