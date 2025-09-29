using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Simple;
using Simple.Collections;

namespace Simple.Modeling
{
	//public class ModelElement<TObject, TModel> : ModelElement, IModelElement
	//	where TModel : ModelElement<TObject, TModel>, new()
	//{
	//	private static TModel instance = null;
	//	private static object lockObjectInstance = new object();

	//	public ModelElement()
	//	{
	//		this.ObjectType = typeof(TObject);
	//		this.Name = this.ObjectType.Name;
	//		this.Caption = this.Name.InsertOnUpperChange(" ");
	//	}

	//	public static TModel Instance
	//	{
	//		get
	//		{
	//			lock (lockObjectInstance)
	//			{
	//				if (instance == null)
	//					instance = new TModel();
	//			}

	//			return instance;
	//		}
	//	}
	//}

	public class ModelElement<TKey> : ModelElement, IModelElement
	{
		public ModelElement(TKey key)
			: this(key, null)
		{
		}

		public ModelElement(TKey key, string caption)
			: this(key, caption, caption)
		{
		}

		public ModelElement(TKey key, string shortName, string caption)
			: this(key, shortName, shortName, caption, null)
		{
		}

		public ModelElement(TKey key, string shortName, string symbol, string caption)
			: this(key, shortName, symbol, caption, null)
		{
		}

		public ModelElement(TKey key, string shortName, string symbol, string caption, string description)
			: base(default(int), shortName, symbol, caption, description)
		{
			this.Key = key;

			if (typeof(TKey).IsEnum)
				this.Index = Conversion.TryChangeType<int>(key);
		}

		public TKey Key { get; set; }

		protected ModelDictionary<TKey, TFieldType> CreateModelDictionary<TFieldType>(object objectModelElementFieldHolderInstance)
			where TFieldType : ModelElement<TKey>
		{
			return base.CreateModelDictionary<TKey, TFieldType>(objectModelElementFieldHolderInstance, modelElement => modelElement.Key);
		}
	}

	public class ModelElement : IModelElement
	{
		private object owner = null;
		private Hashtable modelCollectionsByModelType = null;
		private Hashtable modelDictionariesByModelType = null;
		private Hashtable secondKeyModelDictionariesByOriginalDictionary = null;

		public ModelElement()
		{
		}

		public ModelElement(int index)
			: this(index, null)
		{
		}

		public ModelElement(int index, string caption)
			: this(index, caption, caption)
		{
		}

		public ModelElement(int index, string shortName, string caption)
			: this(index, shortName, shortName, caption, null)
		{
		}

		public ModelElement(int index, string shortName, string symbol, string caption)
			: this(index, shortName, symbol, caption, null)
		{
		}

		public ModelElement(int index, string shortName, string symbol, string caption, string description)
		{
			this.Index = index;
			this.ShortName = shortName;
			this.Symbol = symbol;
			this.Caption = caption;

			if (String.IsNullOrEmpty(caption) && !String.IsNullOrEmpty(shortName))
				this.Caption = shortName.InsertSpaceOnUpperChange();

			this.Caption2 = this.Caption;
			this.Description = description;
		}

		public int Index { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string Caption { get; set; }
		public string Caption2 { get; set; }
		public string ShortName { get; set; }
		public string Symbol { get; set; }
		public string ImageName { get; set; }
		public bool BeginGroup { get; set; }

		public object Owner
		{
			get { return this.owner; }
			set
			{
				this.owner = value;
				this.OnSetOwner();
			}
		}

		//public IModel GetModel()
		//{ 
		//	return this.model;
		//}

		private Hashtable ModelCollectionsByModelType
		{
			get
			{
				if (this.modelCollectionsByModelType == null)
					this.modelCollectionsByModelType = new Hashtable();

				return this.modelCollectionsByModelType;
			}
		}

		private Hashtable ModelDictionariesByModelType
		{
			get
			{
				if (this.modelDictionariesByModelType == null)
					this.modelDictionariesByModelType = new Hashtable();

				return this.modelDictionariesByModelType;
			}
		}

		private Hashtable SecondKeyDictionariesByOriginalDictionary
		{
			get
			{
				if (this.secondKeyModelDictionariesByOriginalDictionary == null)
					this.secondKeyModelDictionariesByOriginalDictionary = new Hashtable();

				return this.secondKeyModelDictionariesByOriginalDictionary;
			}
		}

		//private Hashtable ModelInterfaceCollectionsByModelType
		//{
		//    get
		//    {
		//        if (this.modelInterfaceCollectionsByModelType == null)
		//        {
		//            this.modelInterfaceCollectionsByModelType = new Hashtable();
		//        }

		//        return this.modelInterfaceCollectionsByModelType;
		//    }
		//}

		//protected ModelCollection<T> GetModelCollection<T>() where T : Model
		//{
		//    return this.GetModelCollection<T, ModelCollection<T>>();
		//}


		//protected ModelCollection<T> GetModelCollection<T>(Func<ModelCollection<T>> creteModelCollection) where T : Model
		//{
		//    ModelCollection<T> value = this.ModelCollectionsByModelType[typeof(T)] as ModelCollection<T>;

		//    if (value == null)
		//    {
		//        value = creteModelCollection();
		//        (value as ISetModel).SetModel(this.GetModel()); //new ModelCollection<T>(this.model);
		//        this.ModelCollectionsByModelType.Add(typeof(T), value);
		//    }

		//    return value;
		//}

		protected ModelCollection<TModel> GetModelCollection<TModel>()
			where TModel : ModelElement
		{
			ModelCollection<TModel> modelCollection = this.ModelCollectionsByModelType[typeof(TModel)] as ModelCollection<TModel>;

			if (modelCollection == null)
			{
				modelCollection = new ModelCollection<TModel>();
				//this.SetOwnerToModel(modelCollection);
				this.ModelCollectionsByModelType.Add(typeof(TModel), modelCollection);
			}

			return modelCollection;
		}

		protected void SetModelCollection<TModel>(ModelCollection<TModel> value) where TModel : ModelElement
		{
			if (value != null)
			{
				this.ModelCollectionsByModelType[typeof(TModel)] = value;
				//this.SetOwnerToModel(value);
			}
			else
			{
				if (this.ModelCollectionsByModelType.ContainsKey(typeof(TModel)))
					this.ModelCollectionsByModelType.Remove(typeof(TModel));
			}
		}

		protected ModelDictionary<TKey, TValue> GetModelDictionary<TKey, TValue>()
			where TValue : ModelElement
		{
			ModelDictionary<TKey, TValue> modelDictionary = this.ModelDictionariesByModelType[typeof(TValue)] as ModelDictionary<TKey, TValue>;

			if (modelDictionary == null)
			{
				modelDictionary = new ModelDictionary<TKey, TValue>();
				//this.SetOwnerToModel(modelDictionary);
				this.ModelDictionariesByModelType.Add(typeof(TValue), modelDictionary);
			}

			return modelDictionary;
		}

		protected void SetModelDictionary<TKey, TValue>(ModelDictionary<TKey, TValue> value)
			where TValue : ModelElement
		{
			if (value != null)
			{
				ModelDictionary<TKey, TValue> oldValue = this.ModelDictionariesByModelType[typeof(TValue)] as ModelDictionary<TKey, TValue>;

				if (oldValue != null && value != oldValue && this.SecondKeyDictionariesByOriginalDictionary.ContainsKey(oldValue))
					this.SecondKeyDictionariesByOriginalDictionary.Remove(oldValue);

				this.ModelDictionariesByModelType[typeof(TValue)] = value;
				//this.SetOwnerToModel(value);
			}
			else if (this.ModelDictionariesByModelType.ContainsKey(typeof(TValue)))
			{
				this.ModelDictionariesByModelType.Remove(typeof(TValue));
			}
		}

		protected ModelDictionary<TSecondKey, TValue> GetSecondKeyModelDictionary<TKey, TSecondKey, TValue>(IDictionaryWithEvents<TKey, TValue> originalDictionary, Func<TKey, TValue, TSecondKey> getSecondKey)
			where TValue : ModelElement
		{
			ModelDictionary<TSecondKey, TValue> secondKeyModelDictionary = this.SecondKeyDictionariesByOriginalDictionary[originalDictionary] as ModelDictionary<TSecondKey, TValue>;

			if (secondKeyModelDictionary == null)
			{
				SecondKeyDictionary<TKey, TSecondKey, TValue> secondKeyDictionary = new SecondKeyDictionary<TKey, TSecondKey, TValue>(originalDictionary, getSecondKey);
				secondKeyModelDictionary = new ModelDictionary<TSecondKey, TValue>(secondKeyDictionary);
				this.SecondKeyDictionariesByOriginalDictionary.Add(originalDictionary, secondKeyModelDictionary);
			}

			return secondKeyModelDictionary;
		}

		protected ModelCollection<TModel> CreateModelCollection<TModel>(object objectModelFieldHolderInstance)
			where TModel : ModelElement
		{
			ModelCollection<TModel> modelCollection = new ModelCollection<TModel>();
			//this.SetOwnerToModel(modelCollection);
			IDictionary<string, TModel> fieldsByName = ReflectionHelper.GetFieldsByName<TModel>(objectModelFieldHolderInstance);

			foreach (KeyValuePair<string, TModel> keyValuePair in fieldsByName)
			{
				string fieldName = keyValuePair.Key;
				TModel model = keyValuePair.Value;

				//this.SetOwnerToModel(model);
				ModelHelper.SetModelNameAndCaptionIfNullOrDefault(model, fieldName);
				modelCollection.Add(keyValuePair.Value);
				this.OnCreteModelCollectionElement<TModel>(keyValuePair.Key, keyValuePair.Value, modelCollection);
			}

			return modelCollection;
		}

		protected virtual void OnCreteModelCollectionElement<TModel>(string fieldName, TModel model, ModelCollection<TModel> modelCollection)
			where TModel : ModelElement
		{
		}

		protected ModelDictionary<TKey, TFieldType> CreateModelDictionary<TKey, TFieldType>(object objectModelFieldHolderInstance)
			where TFieldType : ModelElement
		{
			return this.CreateModelDictionary<TKey, TFieldType>(objectModelFieldHolderInstance, null);
		}

		protected ModelDictionary<TKey, TFieldType> CreateModelDictionary<TKey, TFieldType>(object objectModelFieldHolderInstance, Func<TFieldType, TKey> getKey)
			where TFieldType : ModelElement
		{
			Dictionary<string, FieldInfo> fieldInfosByName = ReflectionHelper.GetFieldInfosByName(objectModelFieldHolderInstance);
			IDictionary<string, TFieldType> fieldsByName = ReflectionHelper.GetFieldsByName<TFieldType>(objectModelFieldHolderInstance);
			ModelDictionary<TKey, TFieldType> modelDictionary = new ModelDictionary<TKey, TFieldType>();
			//this.SetOwnerToModel(modelDictionary);
			int indexer = default(int);

			foreach (KeyValuePair<string, TFieldType> keyValuePair in fieldsByName)
			{
				string fieldName = keyValuePair.Key;
				TFieldType model = keyValuePair.Value;
				FieldInfo fieldInfo = fieldInfosByName[fieldName];
				model.Owner = this.Owner;
				ModelHelper.SetModelNameAndCaptionIfNullOrDefault(model, fieldName);
				TKey key = default(TKey);

				if (model.Index == default(int))
					model.Index = indexer++;

				if (getKey != null)
				{
					key = getKey(model);
				}
				else if (typeof(TKey).IsNumeric())
				{
					key = Conversion.TryChangeType<TKey>(model.Index);
				}
				else if (typeof(TKey) == typeof(string))
				{
					key = Conversion.TryChangeType<TKey>(model.Name);
				}
				else if (typeof(TKey).IsEnum)
				{
					key = Conversion.TryChangeType<TKey>(fieldName);
				}

				if (modelDictionary.ContainsKey(key))
					throw new Exception("Model Dictionary Key must be unique, ObjectType:" + objectModelFieldHolderInstance.GetType().ToString());

				modelDictionary.Add(key, model);

				this.OnCreateDictionaryElement<TKey, TFieldType>(fieldInfo, fieldName, key, model, modelDictionary);
			}

			return modelDictionary;
		}

		public NullableDictionary<TKey, TFieldType> CreateNullableDictionary<TKey, TFieldType>(object objectModelElementFieldHolderInstance)
			where TFieldType : ModelElement
		{
			return this.CreateDictionary<NullableDictionary<TKey, TFieldType>, TKey, TFieldType>(objectModelElementFieldHolderInstance);
		}

		public NullableDictionary<TKey, TFieldType> CreateNullableDictionary<TKey, TFieldType>(object objectModelElementFieldHolderInstance, Func<TFieldType, TKey> getKey)
			where TFieldType : ModelElement
		{
			return this.CreateDictionary<NullableDictionary<TKey, TFieldType>, TKey, TFieldType>(objectModelElementFieldHolderInstance, getKey);
		}

		public Dictionary<TKey, TFieldType> CreateDictionary<TKey, TFieldType>(object objectModelElementFieldHolderInstance, Func<TFieldType, TKey> getKey)
			where TFieldType : ModelElement
		{
			return this.CreateDictionary<Dictionary<TKey, TFieldType>, TKey, TFieldType>(objectModelElementFieldHolderInstance, getKey);
		}

		public Dictionary<TKey, TFieldType> CreateDictionary<TKey, TFieldType>(object objectModelElementFieldHolderInstance)
			where TFieldType : ModelElement
		{
			return this.CreateDictionary<Dictionary<TKey, TFieldType>, TKey, TFieldType>(objectModelElementFieldHolderInstance);
		}

		public T CreateDictionary<T, TKey, TFieldType>(object objectModelElementFieldHolderInstance)
			where TFieldType : ModelElement
			where T : IDictionary<TKey, TFieldType>, new()
		{
			Func<TFieldType, TKey> getKey = null;

			if (typeof(TFieldType) == typeof(ModelElement) || typeof(TFieldType).IsSubclassOf(typeof(ModelElement)))
				getKey = modelElement => (modelElement as ModelElement<TKey>).Key;

			return CreateDictionary<T, TKey, TFieldType>(objectModelElementFieldHolderInstance, getKey);
		}

		public T CreateDictionary<T, TKey, TFieldType>(object objectModelElementFieldHolderInstance, Func<TFieldType, TKey> getKey)
			where TFieldType : ModelElement 
			where T : IDictionary<TKey, TFieldType>, new()
		{
			Dictionary<string, FieldInfo> fieldInfosByName = ReflectionHelper.GetFieldInfosByName(objectModelElementFieldHolderInstance);
			IDictionary<string, TFieldType> fieldsByName = ReflectionHelper.GetFieldsByName<TFieldType>(objectModelElementFieldHolderInstance);
			T result = new T();
			int indexer = default(int);

			foreach (KeyValuePair<string, TFieldType> keyValuePair in fieldsByName)
			{
				string fieldName = keyValuePair.Key;
				TFieldType modelElement = keyValuePair.Value;
				FieldInfo fieldInfo = fieldInfosByName[fieldName];
				modelElement.Owner = this.Owner;
				ModelHelper.SetModelNameAndCaptionIfNullOrDefault(modelElement, fieldName);
				TKey key = default(TKey);

				if (modelElement.Index == default(int))
					modelElement.Index = indexer++;

				if (getKey != null)
				{
					key = getKey(modelElement);
				}
				else if (typeof(TKey).IsNumeric())
				{
					key = Conversion.TryChangeType<TKey>(modelElement.Index);
				}
				else if (typeof(TKey) == typeof(string))
				{
					key = Conversion.TryChangeType<TKey>(modelElement.Name);
				}
				else
				{
					throw new Exception("Model Dictionary Key cannot be identified. ObjectType:" + objectModelElementFieldHolderInstance.GetType().ToString());
				}

				if (key == null)
					key = default(TKey);

				if (result.ContainsKey(key))
					throw new Exception("Model Dictionary Key must be unique, ObjectType:" + objectModelElementFieldHolderInstance.GetType().ToString());

				result.Add(key, modelElement);

				this.OnCreateDictionaryElement<TKey, TFieldType>(fieldInfo, fieldName, key, modelElement, result);
			}

			return result;
		}

		protected virtual void OnCreateDictionaryElement<TKey, TFieldType>(FieldInfo fieldInfo, string fieldName, TKey key, TFieldType modelElement, IDictionary<TKey, TFieldType> modelDictionary)
			where TFieldType : ModelElement
		{
		}

		//protected void SetOwnerToModel(Model model)
		//{
		//    model.Owner = this.Owner;
		//    //model.SetModel(this.GetModel());
		//}

		protected virtual void OnSetOwner()
		{
		}

		public override string ToString()
		{
			return this.Name;
		}

		//void ISetModel.SetModel(IModel model)
		//{
		//	this.model = model;
		//	this.OnSetModel(model);
		//}
	}

	//public interface ISimpleModel : IModel
	//{
	//    Type ObjectType { get; }
	//    string Caption { get; }
	//    string Name
	//    { get; set; }
	//}

	public interface IModelElement
	{
		int Index { get; }
		string Name { get; }
		string ShortName { get; }
		string Description { get; }
		string Caption { get; }
		string Caption2 { get; }
		string Symbol { get; }
		string ImageName { get; }
		bool BeginGroup { get; set; }
		object Owner { get; }
	}
}