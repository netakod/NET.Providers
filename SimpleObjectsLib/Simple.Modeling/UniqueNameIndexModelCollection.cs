using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;
using Simple.Collections;

namespace Simple.Modeling
{
	public class UniqueNameIndexModelCollection<TModel> : IUniqueNameIndexModelCollection<TModel>
		where TModel : ModelElement, IModelElement
	{
		//private object instanceModelHolder;
		private TModel[] modelsByIndexArray;
		protected SimpleDictionary<string, TModel> modelsByName;
		//private List<T> innerList;

		public UniqueNameIndexModelCollection(Type modelHolderClassType)
			: this(Activator.CreateInstance(modelHolderClassType))
		{
		}

		public UniqueNameIndexModelCollection(object instanceModelHolder)
			: this(instanceModelHolder, ReflectionHelper.GetFieldsByName<TModel>(instanceModelHolder))
		{
		}

		public UniqueNameIndexModelCollection(object owner, IDictionary<string, TModel> modelsByName)
		{
			List<int> indexes = new List<int>();
			List<string> names = new List<string>();
			this.modelsByName = new SimpleDictionary<string,TModel>(modelsByName);
			string ownerName = (owner != null) ? owner.GetType().Name : "null";

			//this.innerList = new List<T>(modelByName.Values);

			// First pass
			int indexer = 0;

			foreach (KeyValuePair<string, TModel> keyValuePair in this.modelsByName)
				if (keyValuePair.Value.Index >= 0)
					indexes.Add(keyValuePair.Value.Index);

			// Set indexes for those with empty (-1) default values
			foreach (KeyValuePair<string, TModel> keyValuePair in this.modelsByName)
			{
				if (!indexes.Contains(keyValuePair.Value.Index))
				{
					while (indexes.Contains(indexer))
						indexer++;

					keyValuePair.Value.Index = indexer;
					indexes.Add(indexer);
				}
			}

			indexes.Clear();

			foreach (KeyValuePair<string, TModel> item in this.modelsByName)
			{
				string fieldName = item.Key;
				TModel propertyModel = item.Value;

				if (String.IsNullOrEmpty(propertyModel.Name))
					propertyModel.Name = fieldName;

				if (String.IsNullOrEmpty(propertyModel.Caption))
					propertyModel.Caption = propertyModel.Name;

				//ModelHelper.SetModelNameAndCaptionIfIsNullOrDefault(propertyModel, fieldName);

				//if (model.Index == default(int))
				//	model.Index = indexer++;

				if (indexes.Contains(propertyModel.Index))
				{
					throw new ArgumentOutOfRangeException(String.Format("Duplicate Index number: Owner={0}, Property Name={1}, Index={2}", ownerName, propertyModel.Name, propertyModel.Index));
				}
				else
				{
					indexes.Add(propertyModel.Index);
				}

				if (names.Contains(propertyModel.Name))
				{
					throw new ArgumentOutOfRangeException(String.Format("Duplicate Name property: Owner={0}, Property Name={0}, Index={1}", ownerName, propertyModel.Name, propertyModel.Index));
				}
				else
				{
					names.Add(propertyModel.Name);
				}
			}

            if (indexes.Count > 0)
            {
                this.modelsByIndexArray = new TModel[indexes.Max() + 1];

                foreach (KeyValuePair<string, TModel> item in this.modelsByName)
                    this.modelsByIndexArray[item.Value.Index] = item.Value;
            }
		}

		public TModel this[int propertyIndex]
		{
			get { return this.modelsByIndexArray[propertyIndex]; }
		}

		public TModel this[string propertyName]
		{
			get 
			{ 
				TModel value;
				if (!this.modelsByName.TryGetValue(propertyName, out value))
					value = null;

				return value;
			}
		}

		///// <summary>
		///// Gets the number of elements contained in the collection.
		///// </summary>
		///// <value></value>
		///// <returns>The number of elements contained in the collection.</returns>
		//public int Count
		//{
		//	get { return this.innerList.Count; }
		//}

		//public System.Collections.ObjectModel.ReadOnlyCollection<PropertyModel> Collection
		//{
		//	get { return this.innerList.AsReadOnly(); }
		//}

		//public PropertyModel GetPropertyModel(int propertyIndex)
		//{
		//	if (propertyIndex >= 0 && propertyIndex < this.modelsByIndexArray.Count())
		//	{
		//		return this.modelsByIndexArray[propertyIndex];
		//	}
		//	else
		//	{
		//		return null;
		//	}
		//}

		//public PropertyModel GetPropertyModel(string propertyName)
		//{
		//	PropertyModel result = null;
		//	this.modelsByName.TryGetValue(propertyName, out result);

		//	return result;
		//}

		public int Count
		{
			get { return this.modelsByName.Values.Count; }
		}

		public int GetMaxIndex()
		{
			return (this.modelsByName.Values.Count > 0) ? this.modelsByName.Values.Max(model => model.Index) : -1;
		}

		TModel IUniqueNameIndexModelCollection<TModel>.this[int index]
		{
			get { return this[index]; }
		}

		TModel IUniqueNameIndexModelCollection<TModel>.this[string name]
		{
			get { return this[name]; }
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.</returns>
		IEnumerator<TModel> IEnumerable<TModel>.GetEnumerator()
		{
			return this.modelsByName.Values.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.modelsByName.Values.GetEnumerator();
		}
	}

	public interface IUniqueNameIndexModelCollection<TModel> : IEnumerable<TModel> 
        where TModel : IModelElement
	{
		TModel this[int index] { get; }
		TModel this[string name] { get; }
		int Count { get; }
		//T GetModel(int index);
		//T GetModel(string name);
	}
}
