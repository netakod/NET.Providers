using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public class PropertyValues 
	{
		private Dictionary<int, object> valuesByIndex = new Dictionary<int, object>();
		private ReadOnlyDictionary<int, object>? readOnlyValues = null;

		public object GetValue(int index)
		{
			return this.GetValue<object>(index);
		}

		public T GetValue<T>(int index)
		{
			object result;

			if (!this.valuesByIndex.TryGetValue(index, out result))
				result = default(T);

			return (T)result;
		}

		public void SetValue<T>(int index, T value)
		{
			if (value.IsDefault<T>())
				this.valuesByIndex.Remove(index);
			else
				this.valuesByIndex[index] = value;
		}

		public virtual void LoadValues(Dictionary<int, object> valuesByIndex)
		{
			this.valuesByIndex = valuesByIndex;
			this.readOnlyValues = null;
		}

		public virtual void Clear()
		{
			this.valuesByIndex.Clear();
			this.readOnlyValues = null;
		}

		public ReadOnlyDictionary<int, object> GetValueDictionary()
		{
			if (this.readOnlyValues == null)
				this.readOnlyValues = new ReadOnlyDictionary<int, object>(this.valuesByIndex);

			return this.readOnlyValues;
		}

		//object IPropertyValue.GetPropertyValue(int propertyIndex)
		//{
		//	return this.GetValue(propertyIndex);
		//}

		//void IPropertyValue.SetPropertyValue(int propertyIndex, object value)
		//{
		//	this.SetValue(propertyIndex, value);
		//}
	}
}
