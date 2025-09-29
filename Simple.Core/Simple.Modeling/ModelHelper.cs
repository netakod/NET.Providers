using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace Simple.Modeling
{
    public static class ModelHelper
    {
        public static ModelCollection<TFieldType> CreateModelCollectionByReflection<TObjectType, TFieldType>(object owner)
            where TFieldType : ModelElement
        {
            object objectInstance = Activator.CreateInstance<TObjectType>();
            return CreateModelCollectionByReflection<TFieldType>(objectInstance, owner);
        }

		public static ModelCollection<TFieldType> CreateModelCollectionByReflection<TFieldType>(object objectInstance, object owner)
            where TFieldType : ModelElement
        {
            ModelCollection<TFieldType> modelCollection = new ModelCollection<TFieldType>();
            //SetOwnerToModel(modelCollection, owner);
            IDictionary<string, TFieldType> fieldsByName = ReflectionHelper.GetFieldsByName<TFieldType>(objectInstance);

            foreach (KeyValuePair<string, TFieldType> keyValuePair in fieldsByName)
            {
                string fieldName = keyValuePair.Key;
                TFieldType model = keyValuePair.Value;
                
                modelCollection.Add(model);

				SetModelNameAndCaptionIfNullOrDefault(model, fieldName);
                model.Owner = owner;
                //SetOwnerToModel(model, owner);
            }

            return modelCollection;
        }

		public static ModelDictionary<TKey, TFieldType> CreateModelDictionaryByReflection<TObjectType, TKey, TFieldType>(Func<TFieldType, TKey> getKey, object owner)
            where TFieldType : ModelElement
        {
            object objectInstance = Activator.CreateInstance<TObjectType>();
            return CreateModelDictionaryByReflection<TKey, TFieldType>(objectInstance, getKey, owner);
        }

		public static ModelDictionary<TKey, TFieldType> CreateModelDictionaryByReflection<TKey, TFieldType>(object objectInstance, Func<TFieldType, TKey> getKey, object owner)
            where TFieldType : ModelElement
        {
            ModelDictionary<TKey, TFieldType> modelDictionary = new ModelDictionary<TKey, TFieldType>();
            //SetOwnerToModel(modelDictionary, owner);
            IDictionary<string, TFieldType> fieldsByName = ReflectionHelper.GetFieldsByName<TFieldType>(objectInstance);
			
			foreach (KeyValuePair<string, TFieldType> keyValuePair in fieldsByName)
            {
                string fieldName = keyValuePair.Key;
                TFieldType model = keyValuePair.Value;
				
				SetModelNameAndCaptionIfNullOrDefault(model, fieldName);
                model.Owner = owner;
                //SetOwnerToModel(model, owner);
                
				TKey key = getKey(model);
                
                modelDictionary.Add(key, model);
            }

            return modelDictionary;
        }

        public static PropertyModelCollection<TPropertyModel> CreatePropertyModelCollectionByReflection<TPropertyModel>(object objectInstance, object owner)
            where TPropertyModel : PropertyModel
        {
            return new PropertyModelCollection<TPropertyModel>(objectInstance, owner);
        }

        public static void SetModelIndexesNamesAndCaptions<TFieldType>(object objectModelFieldHolderInstance) where TFieldType : ModelElement
		{
			SetModelIndexesNamesAndCaptions<TFieldType>(objectModelFieldHolderInstance, enforceUniqueIndexAndName: true);
		}

		public static void SetModelIndexesNamesAndCaptions<TFieldType>(object objectModelFieldHolderInstance, bool enforceUniqueIndexAndName) where TFieldType : ModelElement
		{
			SetModelIndexesNamesAndCaptions<TFieldType>(objectModelFieldHolderInstance, enforceUniqueIndexAndName, null);
		}

		public static void SetModelIndexesNamesAndCaptions<TFieldType>(object objectModelFieldHolderInstance, bool enforceUniqueIndexAndName, Func<TFieldType> getKey) 
            where TFieldType : ModelElement
		{
			IDictionary<string, TFieldType> fieldsByName = ReflectionHelper.GetFieldsByName<TFieldType>(objectModelFieldHolderInstance);
			List<int> indexes = new List<int>();
			List<string> names = new List<string>();
			int indexer = default(int);

			foreach (KeyValuePair<string, TFieldType> keyValuePair in fieldsByName)
			{
				string fieldName = keyValuePair.Key;
				TFieldType model = keyValuePair.Value;
				ModelHelper.SetModelNameAndCaptionIfNullOrDefault(model, fieldName);
				
				if (model.Index == default(int))
					model.Index = indexer++;

				if (enforceUniqueIndexAndName)
				{
					if (indexes.Contains(model.Index))
					{
						throw new ArgumentOutOfRangeException(String.Format("Duplicate Index number: Owner={0}, Property Name={0}, Index={1}", objectModelFieldHolderInstance.GetType().Name, model.Name, model.Index));
					}
					else
					{
						indexes.Add(model.Index);
					}
				
					if (names.Contains(model.Name))
					{
						throw new ArgumentOutOfRangeException(String.Format("Duplicate Name property: Owner={0}, Property Name={0}, Index={1}", objectModelFieldHolderInstance.GetType().Name, model.Name, model.Index));
					}
					else
					{
						names.Add(model.Name);
					}
				}
			}
		}
		
		public static void SetModelNameAndCaptionIfNullOrDefault(ModelElement model, string fieldName)
        {
			if (String.IsNullOrEmpty(model.Name))
                model.Name = fieldName;

			if (String.IsNullOrEmpty(model.ImageName))
				model.ImageName = fieldName;

			if (String.IsNullOrEmpty(model.Caption))
                model.Caption = model.Name.InsertSpaceOnUpperChange();
        }


		//     private static void SetOwnerToModel(Model model, object owner)
		//     {
		//model.Owner = owner;
		//     }
	}
}
