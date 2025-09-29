using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Simple;
using Simple.Collections;

namespace Simple.Modeling
{
	public class PropertyModelCollection : PropertyModelCollection<PropertyModel>, IPropertyModelCollection<PropertyModel>, IEnumerable<PropertyModel>
	{
		public PropertyModelCollection(Type modelHolderClassType, object owner)
			: base(modelHolderClassType, owner)
		{
		}

		public PropertyModelCollection(object instanceModelHolder, object owner)
			: base(instanceModelHolder, owner) // (s1, s2) => s1.Index.CompareTo(s2.Index), owner)
		{
		}

		//public PropertyModelCollection(object instanceModelHolder, Comparison<TPropertyModel> sortingComparison, object owner)
		//	: this(ReflectionHelper.GetFieldsByName<TPropertyModel>(instanceModelHolder), sortingComparison, owner)
		//{
		//}

		public PropertyModelCollection(IDictionary<string, PropertyModel> propertyModelsByName, object owner) //, Comparison<TPropertyModel> sortingComparison, object owner)
		: base(propertyModelsByName, owner)
		{
		}
	}

	public class PropertyModelCollection<TPropertyModel> : IPropertyModelCollection<TPropertyModel>, IEnumerable<TPropertyModel> //, IPropertySequence
		where TPropertyModel : class, IPropertyModel
	{
		private TPropertyModel[] propertyModelsByIndexArray;
		//private PropertySequence propertySequence = null;
		private SimpleDictionary<string, TPropertyModel> propertyModelsByPropertyName;
		private SimpleDictionary<string, TPropertyModel> propertyModelsByDatastoreFieldName;


		public PropertyModelCollection(Type modelHolderClassType, object owner)
			: this(Activator.CreateInstance(modelHolderClassType)!, owner)
		{
		}

		public PropertyModelCollection(object instanceModelHolder, object owner)
			: this(ReflectionHelper.GetFieldsByName<TPropertyModel>(instanceModelHolder), owner) // (s1, s2) => s1.Index.CompareTo(s2.Index), owner)
		{
		}

		//public PropertyModelCollection(object instanceModelHolder, Comparison<TPropertyModel> sortingComparison, object owner)
		//	: this(ReflectionHelper.GetFieldsByName<TPropertyModel>(instanceModelHolder), sortingComparison, owner)
		//{
		//}

		public PropertyModelCollection(IDictionary<string, TPropertyModel> propertyModelsByName, object owner) //, Comparison<TPropertyModel> sortingComparison, object owner)
		{
			SimpleList<TPropertyModel> propertyModels = new SimpleList<TPropertyModel>(propertyModelsByName.Count);
			int maxIndex = (propertyModelsByName.Count > 0) ? propertyModelsByName.Values.Max(item => item.PropertyIndex) : 0;

			// First pass
			if (typeof(TPropertyModel) == typeof(PropertyModel) || typeof(TPropertyModel).IsSubclassOf(typeof(PropertyModel)))
			{
				HashSet<int> indexes = new HashSet<int>();
				HashSet<string> names = new HashSet<string>();
				string ownerName = (owner != null) ? owner.GetType().Name : "null";

				foreach (KeyValuePair<string, TPropertyModel> keyValuePair in propertyModelsByName)
				{
					if (keyValuePair.Value.PropertyIndex >= 0)
					{
						indexes.Add(keyValuePair.Value.PropertyIndex);
						propertyModels.Add(keyValuePair.Value);
					}
				}

				// Set indexes for those with empty (-1) default values with reversing order - no need for reverse
				int indexer = maxIndex;

				foreach (KeyValuePair<string, TPropertyModel> keyValuePair in propertyModelsByName) //.Reverse())
				{
					if (!indexes.Contains(keyValuePair.Value.PropertyIndex))
					{
						//while (indexes.Contains(indexer))
						//	indexer++;

						if (keyValuePair.Value is PropertyModel propertyModel)
							propertyModel.PropertyIndex = ++indexer;

						indexes.Add(indexer);
						propertyModels.Add(keyValuePair.Value);
					}
				}

				propertyModels.Sort((x, y) => x.PropertyIndex.CompareTo(y.PropertyIndex)); // Sort by index;
				maxIndex = (indexes.Count > 0) ? indexes.Max() : 0;
				indexes.Clear();

				foreach (KeyValuePair<string, TPropertyModel> item in propertyModelsByName)
				{
					string fieldName = item.Key;
					TPropertyModel model = item.Value;

					if (model is PropertyModel propertyModel)
					{
						propertyModel.Owner = owner;

						if (String.IsNullOrEmpty(propertyModel.PropertyName))
							propertyModel.PropertyName = fieldName;

						if (String.IsNullOrEmpty(propertyModel.Caption) && propertyModel.PropertyName != null)
						{
							propertyModel.Caption = propertyModel.PropertyName.InsertSpaceOnUpperChange();
							propertyModel.Caption2 = propertyModel.PropertyName;
						}
					}

					//ModelHelper.SetModelNameAndCaptionIfIsNullOrDefault(propertyModel, fieldName);

					//if (model.Index == default(int))
					//	model.Index = indexer++;

					if (indexes.Contains(model.PropertyIndex))
						throw new ArgumentOutOfRangeException(String.Format("Duplicate Index number: Owner={0}, Property Name={1}, Index={2}", ownerName, model.PropertyName, model.PropertyIndex));
					else
						indexes.Add(model.PropertyIndex);

					if (names.Contains(model.PropertyName))
						throw new ArgumentOutOfRangeException(String.Format("Duplicate Name property: Owner={0}, Property Name={0}, Index={1}", ownerName, model.PropertyName, model.PropertyIndex));
					else
						names.Add(model.PropertyName);

					//this.propertyModelsByIndexArray[item.Value.Index] = item.Value;
				}

				//this.propertyModelsByName = new SimpleDictionary<string, TPropertyModel>(propertyModelsByName.Count);

				//for (int i = 0; i < this.propertyModelsByIndexArray.Length; i++)
				//{
				//	TPropertyModel propertyModel = this.propertyModelsByIndexArray[i];

				//	if (propertyModel != null)
				//		this.propertyModelsByName.Add(propertyModel.Name, propertyModel);
				//}
			}
			//else
			//{
			//	this.propertyModelsByName = new SimpleDictionary<string, TPropertyModel>(propertyModelsByName);
			//	this.propertyModelsByIndexArray = new TPropertyModel[maxIndex + 1];

			//	for (int i = 0; i < this.propertyModelsByName.Count; i++)
			//	{
			//		TPropertyModel propertyModel = this.propertyModelsByName.ElementAt(i).Value;
			//		this.propertyModelsByIndexArray[propertyModel.Index] = propertyModel;
			//	}
			//}

			//foreach (var item in propertyModelsByName)
			//	propertyModels.Add(item.Value);

			//propertyModels.Sort(sortingComparison);

			this.propertyModelsByIndexArray = new TPropertyModel[maxIndex + 1];
			this.propertyModelsByPropertyName = new SimpleDictionary<string, TPropertyModel>(propertyModels.Count);
			this.propertyModelsByDatastoreFieldName = new SimpleDictionary<string, TPropertyModel>(propertyModels.Count);

			foreach(TPropertyModel propertyModel in propertyModels)
			{
				this.propertyModelsByIndexArray[propertyModel.PropertyIndex] = propertyModel;
				this.propertyModelsByPropertyName.Add(propertyModel.PropertyName, propertyModel);

				if (propertyModel.IsStorable)
					this.propertyModelsByDatastoreFieldName.Add(propertyModel.DatastoreFieldName, propertyModel);
			}

			//this.propertySequence = new PropertySequence(propertyModels.AsCustom<IPropertyModel>().ToArray());
		}

		internal PropertyModelCollection(TPropertyModel[] propertyModelsByIndexArray, IDictionary<string, TPropertyModel> propertyModelsByName, IDictionary<string, TPropertyModel> propertyModelsByDatastoreFieldName)
		{
			this.propertyModelsByIndexArray = propertyModelsByIndexArray;
			this.propertyModelsByPropertyName = new SimpleDictionary<string, TPropertyModel>(propertyModelsByName);
			this.propertyModelsByDatastoreFieldName = new SimpleDictionary<string, TPropertyModel>(propertyModelsByDatastoreFieldName);
			//this.propertySequence = new PropertySequence(this.propertyModelsByName.AsCustom<string, IPropertyModel>().Values.ToArray());
		}

		public TPropertyModel this[int propertyIndex]
		{
			get
			{
				//if (propertyIndex >= 0 && propertyIndex < this.propertyModelsByIndexArray.Count())
				//{
					return this.propertyModelsByIndexArray[propertyIndex];
				//}
				//else
				//{
				//	return null;
				//}
			}
		}

		public TPropertyModel? this[string propertyName]
		{
			get 
			{
				TPropertyModel value;

				if (this.propertyModelsByPropertyName.TryGetValue(propertyName, out value))
					return value;

				return default;
			}
		}

		///// <summary>
		///// Gets the number of elements contained in the collection.
		///// </summary>
		///// <value></value>
		///// <returns>The number of elements contained in the collection.</returns>
		public int Count
		{
			get { return this.propertyModelsByPropertyName.Values.Count; }
		}

		public int GetMaxIndex()
		{
			return (this.propertyModelsByPropertyName.Values.Count > 0) ? this.propertyModelsByPropertyName.Values.Max(model => model.PropertyIndex) : -1;
		}

		public TPropertyModel? GetPropertyModel(int propertyIndex)
		{
			if (propertyIndex >= 0 && propertyIndex < this.propertyModelsByIndexArray.Count())
				return this.propertyModelsByIndexArray[propertyIndex];
			else
				return default;
		}

		public TPropertyModel? GetPropertyModel(string propertyName) => this[propertyName];

		public TPropertyModel? GetPropertyModelByDatastoreFieldName(string datastoreFieldName)
		{
			TPropertyModel value;

			if (this.propertyModelsByDatastoreFieldName.TryGetValue(datastoreFieldName, out value))
				return value;

			return default;
		}


		//public CustomDictionary<string, T> ToDictionary<T>()
		//	where T : IPropertyModelBase
		//{
		//	return this.propertyModelsByName.AsCustom<T>();
		//}

		//public PropertyModelCollection<T> AsCustom<T>()
		//	where T : class, IPropertyModel
		//{
		//	T[] customPropertyModelsByIndexArray = new T[this.propertyModelsByIndexArray.Length];
		//	IDictionary<string, T> customPropertyModelsByName = this.propertyModelsByPropertyName.AsCustom<T>();

		//	for (int i = 0; i < customPropertyModelsByIndexArray.Length; i++)
		//	{
		//		object item = this.propertyModelsByIndexArray[i];
		//		customPropertyModelsByIndexArray[i] = (T)item;
		//	}

		//	return new PropertyModelCollection<T>(customPropertyModelsByIndexArray, customPropertyModelsByName);
		//}

		//public TPropertyModel[] ToArray()
		//{
		//	return this.to propertyModelsByIndexArray;
		//}

		//IPropertyModelBase IPropertyModelCollection<IPropertyModelBase>.this[int propertyIndex]
		//{
		//	get { return this[propertyIndex]; }
		//}


		//IPropertyModelBase IPropertyModelCollection<IPropertyModelBase>.this[string propertyName]
		//{
		//	get { return this[propertyName]; }
		//}


		//IPropertyModelBase IPropertyModelCollection<IPropertyModelBase>.GetPropertyModel(int propertyIndex)
		//{
		//	return this.GetPropertyModel(propertyIndex);
		//}

		//IPropertyModelBase IPropertyModelCollection<IPropertyModelBase>.GetPropertyModel(string propertyName)
		//{
		//	return this.GetPropertyModel(propertyName);
		//}

		//IEnumerator<IPropertyModelBase> IPropertyModelCollection<IPropertyModelBase>.GetEnumerator()
		//{
		//	return this.propertyModelsByName.Values.GetEnumerator();
		//}

		//int[] IServerPropertySequence.PropertyIndexes => this.propertySequence.PropertyIndexes;
		//int[] IServerPropertySequence.PropertyTypeIds => this.propertySequence.PropertyTypeIds;
		//int IServerPropertySequence.Length => this.propertySequence.Length;
		//IPropertyModel[] IPropertySequence.PropertyModels => this.propertySequence.PropertyModels;
	

		IEnumerator<TPropertyModel> IEnumerable<TPropertyModel>.GetEnumerator()
		{
			return this.propertyModelsByPropertyName.Values.GetEnumerator();
//			return (this as IPropertyModelCollection<IPropertyModelBase>).GetEnumerator();
		}

		///// <summary>
		///// Returns an enumerator that iterates through a collection.
		///// </summary>
		///// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.</returns>
		//IEnumerator<TPropertyModel> IEnumerable<TPropertyModel>.GetEnumerator()
		//{
		//	return this.propertyModelsByName.Values.GetEnumerator();
		//}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.propertyModelsByPropertyName.Values.GetEnumerator();
		}
	}

	public interface IPropertyModelCollection<TPropertyModel> : IEnumerable<TPropertyModel>
		where TPropertyModel : class, IPropertyModel
	{
		TPropertyModel? this[int propertyIndex] { get; }
		TPropertyModel? this[string propertyName] { get; }

		TPropertyModel? GetPropertyModel(int propertyIndex);
		TPropertyModel? GetPropertyModel(string propertyName);
	}
}
