using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Dynamic;
//using Microsoft.CSharp;

namespace Simple
{
    public class UniqueKeyGenerator<TKey>
		where TKey : struct
	{
		private IList<TKey> keys = null;
        private ReadOnlyCollection<TKey> keysAsReadOnly = null;
        private List<TKey> freeKeys = null;
		private TKey maxKey = Operator<TKey>.Zero; // Conversion.TryChangeType<TKey>(0);
		private TKey minKey = Operator<TKey>.One;
		private object lockObject = new object();
		  
        public UniqueKeyGenerator()
            : this(Operator<TKey>.One)
        {
        }

		public UniqueKeyGenerator(TKey minKey)
			: this(new List<TKey>(), minKey, false)
		{
		}

		public UniqueKeyGenerator(IList<TKey> keys, TKey minKey, bool reuseKeys)
        {
			this.keys = keys; //(keys != null) ? new List<TKey>(keys) : new List<TKey>();
			this.minKey = minKey;
            this.keysAsReadOnly = new ReadOnlyCollection<TKey>(this.keys);
            this.ReuseKeys = reuseKeys;
            this.CalculateMaxKey();
            this.CreateFreeKeys();
        }

		//public bool GenerateNewKeysLessThanMaxKeyValue { get; set; }
		public bool ReuseKeys { get; set; }

		public IList<TKey> Keys
        {
            get { return this.keysAsReadOnly; }
        }

		public TKey MaxKey
		{
			get { return this.maxKey; }
		}

        public TKey CreateKey()
        {
			TKey key;

            lock (lockObject)
            {
                if (this.ReuseKeys && this.freeKeys.Count > 0)
                {
                    key = this.freeKeys.Min();
                    
                    this.freeKeys.Remove(key);

					if (Operator<TKey>.GreaterThan(key, this.maxKey))
						this.maxKey = key;
                }
                else
                {
					//key = Operator<TKey>.Add(this.maxKey, Operator<TKey>.One);

					//if (Operator<TKey>.GreaterThan(key, this.maxKey))
					//	this.maxKey = key;

					key = Operator<TKey>.Add(this.maxKey, Operator<TKey>.One);
					this.maxKey = key;
				}

				this.keys.Add(key);
            }

            return key;
        }

		public void AddKey(TKey key)
		{
			//if (!this.keys.Contains(key))
			//{
				this.keys.Add(key);

				if (Operator<TKey>.GreaterThan(key, this.maxKey))
					this.maxKey = key;

				if (this.freeKeys.Contains(key))
					this.freeKeys.Remove(key);
			//}
		}

        public void RemoveKey(TKey key)
        {
			int index = this.IndexOf(key);

			if (index >= 0)
				this.RemoveAt(index);
		}

		public void RemoveAt(int index)
		{
			lock (lockObject)
			{
				TKey key = this.keys.ElementAt(index);
				
				this.keys.RemoveAt(index);

				if (this.maxKey.Equals(key))
				{
					this.CalculateMaxKey();
				}
				else
				{
					bool isInserted = false;

					for (int i = 0; i < this.freeKeys.Count; i++)
					{
						if (Operator<TKey>.LessThan(key, this.freeKeys[i]))
						{
							this.freeKeys.Insert(i, key);
							isInserted = true;

							break;
						}
					}

					if (!isInserted)
						this.freeKeys.Add(key);
				}
			}
		}

		public void Clear()
        {
            lock (lockObject)
            {
                this.freeKeys.Clear();
                this.keys.Clear();
                this.maxKey = Operator<TKey>.Zero;
            }
        }

        public bool ContainsKey(TKey key) => this.keys.Contains(key);

		public int IndexOf(TKey key) => this.keys.IndexOf(key);

        private void CreateFreeKeys()
        {
            this.freeKeys = new List<TKey>();

			for (TKey key = this.minKey; Operator<TKey>.LessThan(key, this.maxKey); key = Operator<TKey>.Add(key, Operator<TKey>.One))
				if (!this.ContainsKey(key))
					this.freeKeys.Add(key);

			//if (this.keys.Count > 0)
			//{
			//TKey i = this.minKey;
			//this.keys.Sort();

			//foreach (TKey key in this.keys)
			//{
			//	for (TKey j = i; Operator<TKey>.LessThan(j, key); Operator<TKey>.Add(j, Operator<TKey>.One))
			//		this.freeKeys.Add(j);

			//	i = Operator<TKey>.Add(key, Operator<TKey>.One);
			//}
			//}
		}

		private void CalculateMaxKey()
        {
			this.maxKey = (this.keys.Count > 0) ? this.keys.Max() : Operator<TKey>.Subtract(this.minKey, Operator<TKey>.One);
        }
    }
}
