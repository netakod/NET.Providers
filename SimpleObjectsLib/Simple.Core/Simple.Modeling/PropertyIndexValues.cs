using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Modeling
{
	public struct PropertyIndexValues //: IEnumerable<KeyValuePair<int, object>>, IEnumerable
	{
		//private Func<int, IPropertyModel> getPropertyModel = null;
		//private IEnumerable<IPropertyModel> propertyModels = null;
		//private KeyValuePair<int, object>[] keyValuePairs = null;

		//public PropertyIndexValues(IEnumerable<IPropertyModel> propertyModels, object[] propertyValues)
		//{
		//	if (propertyModels.Count() != propertyValues.Length)
		//		throw new ArgumentOutOfRangeException("Input indexes and values arrays must be the same size");

		//	this.propertyModels = propertyModels;
		//	this.PropertyIndexes = new int[propertyModels.Count()];

		//	for (int i = 0; i < propertyModels.Count(); i++)
		//		this.PropertyIndexes[i] = propertyModels.ElementAt(i).Index;

		//	this.PropertyValues = propertyValues;
		//	this.getPropertyModel = (propertyIndex) => this.propertyModels.FirstOrDefault(propertyModel => propertyModel.Index == propertyIndex);
		//	this.Length = propertyValues.Length;
		//}

		//public PropertyIndexValues(int[] propertyIndexes, object[] propertyValues)
		//	: this(propertyIndexes, propertyValues, getPropertyModel: null)
		//{
		//}

		public PropertyIndexValues(IEnumerable<int> propertyIndexes, IEnumerable<object?> propertyValues) //, Func<int, IPropertyModel> getPropertyModel = null)
		{
			if (propertyIndexes.Count() != propertyValues.Count())
				throw new ArgumentOutOfRangeException("Input indexes and values arrays must be the same size");

			this.PropertyIndexes = propertyIndexes;
			this.PropertyValues = propertyValues;
			//this.getPropertyModel = getPropertyModel;
			this.Count = propertyIndexes.Count();
		}

		public IEnumerable<int> PropertyIndexes { get; private set; }
		public IEnumerable<object?> PropertyValues { get; private set; }
		public int Count { get; private set; }

		public object? GetValue(int propertyIndex) => this.PropertyValues.ElementAt(propertyIndex);

		//public IPropertyModel GetPropertyModel(int porpertyIndex)
		//{
		//	if (this.getPropertyModel != null)
		//		return this.getPropertyModel(porpertyIndex);

		//	return null;
		//}

		///// <summary>
		///// Returns an enumerator that iterates through the dictionary.
		///// </summary>
		///// <returns>
		///// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the dictionary.
		///// </returns>
		//public IEnumerator<KeyValuePair<int, object>> GetEnumerator()
		//{
		//	foreach (var item in this.KeyValuePairs)
		//		yield return item;

		//	//for (int i = 0; i < this.Length; i++)
		//	//	yield return this.keyValuePairs[i];
		//}

		//private KeyValuePair<int, object>[] KeyValuePairs
		//{
		//	get
		//	{
		//		if (this.keyValuePairs != null)
		//		{
		//			this.keyValuePairs = new KeyValuePair<int, object>[this.Length];

		//			for (int i = 0; i < this.Length; i++)
		//				this.keyValuePairs[i] = new KeyValuePair<int, object>(this.PropertyIndexes[i], this.PropertyValues[i]);
		//		}

		//		return this.keyValuePairs;
		//	}
		//}

		///// <summary>
		///// Provides an IEnumerator that can be used to iterate all the members of the
		///// collection. This implementation uses the IEnumerator&lt;T&gt; that was overridden
		///// by the derived classes to enumerate the members of the collection.
		///// </summary>
		///// <returns>An IEnumerator that can be used to iterate the collection.</returns>
		//IEnumerator IEnumerable.GetEnumerator()
		//{
		//	foreach (KeyValuePair<int, object> item in this)
		//		yield return item;
		//}
	}
}
