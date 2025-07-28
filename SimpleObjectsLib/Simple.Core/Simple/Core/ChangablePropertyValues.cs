using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public class ChangablePropertyValues : PropertyValues
	{
		private Dictionary<int, object> oldValuesByIndex = new Dictionary<int, object>();
		private readonly object lockObject = new object();

		public int ChangeCount
		{
			get { return this.oldValuesByIndex.Count; }
		}

		public object GetOldValue(int index)
		{
			return this.GetOldValue<object>(index);
		}

		public T GetOldValue<T>(int index)
		{
			object result;

			if (!this.oldValuesByIndex.TryGetValue(index, out result))
				result = default(T);

			return (T)result;
		}

		//public override void SetValue(int index, object value)
		//{
		//	this.SetValue<object>(index, value);
		//}

		public new void SetValue<T>(int index, T value)
		{
			T lastValue = this.GetValue<T>(index);
			T oldValue;
			bool isChanged = false;

			lock (this.lockObject)
			{
				if (Comparison.IsEqual(value, lastValue))
					return;

				if (this.oldValuesByIndex.ContainsKey(index))
				{
					oldValue = (T)this.oldValuesByIndex[index];
					isChanged = !Comparison.IsEqual<T>(value, oldValue);

					if (!isChanged)
						this.oldValuesByIndex.Remove(index);
				}
				else
				{
					this.oldValuesByIndex.Add(index, lastValue);
					isChanged = true;
				}

				base.SetValue<T>(index, value);
			}
		}

		public bool IsChanged(int index)
		{
			return this.oldValuesByIndex.ContainsKey(index);
		}

		public void CommitChanges()
		{
			lock (this.lockObject)
			{
				this.oldValuesByIndex.Clear();
			}
		}

		public Dictionary<int, object> GetChangedValueDictionary()
		{
			Dictionary<int, object> result = new Dictionary<int, object>();

			lock (this.lockObject)
			{
				foreach (var item in this.oldValuesByIndex)
					result.Add(item.Key, this.GetValue(item.Key));

				return result;
			}
		}

		public override void LoadValues(Dictionary<int, object> valuesByIndex)
		{
			lock (this.lockObject)
			{
				base.LoadValues(valuesByIndex);
				this.oldValuesByIndex.Clear();
			}
		}

		public override void Clear()
		{
			lock (this.lockObject)
			{
				base.Clear();
				this.oldValuesByIndex.Clear();
			}
		}
	}
}
