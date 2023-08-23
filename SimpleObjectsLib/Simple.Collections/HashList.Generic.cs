using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Collections
{
    public class HashList<T>
    {
		private List<T> list = null;

		public HashList()
		{
			this.list = new List<T>();
		}

		public HashList(int capacity)
		{
			this.list = new List<T>(capacity);
		}

        public T this[int index]
        {
            get { return this.list[index]; }
			set { this.SetValue(index, value); }
        }

        public IEnumerable<T> Collection
        {
            get { return this.list; }
        }

        public bool ContainsIndex(int index)
        {
			//bool result = false;

            lock (this.list)
            {
				//for (int i = 0; i < this.list.Count; i++)
				//{
					//if (this.GetValue(index) != null)
					//{
					//	result = true;
						//break;
					//}
				//}

				return this.GetValue(index) != null;
            }
			//return result;
        }

        public bool ContainsValue(T value)
        {
            if (value == null)
                throw new ArgumentNullException("value cannot be null");

            lock (this.list)
            {
                return this.list.Contains(value);
            }
        }

        public T GetValue(int index)
        {
			T value = default(T);

			lock (this.list)
			{
				if (index >= 0 && index < this.list.Count)
					value = this.list[index];
			}

			return value;
		}

		public void SetValue(int index, T value)
		{
			lock (this.list)
			{
				if (index >= this.list.Count)
					this.ExpandList(index + 1);

				this.list[index] = value;

				// If index is last element and value is null -> remove last null elements
				if (value.Equals(default(T)) && index == this.list.Count - 1)
					while (this.list[this.list.Count - 1] == null && this.list.Count > 0)
						this.list.RemoveAt(this.list.Count - 1);
			}
		}

		public void Clear()
        {
            lock (this.list)
            {
                this.list.Clear();
            }
        }

        private void ExpandList(int size)
        {
            lock (this.list)
            {
                while (this.list.Count < size)
                    this.list.Add(default(T));
            }
        }
    }
}
