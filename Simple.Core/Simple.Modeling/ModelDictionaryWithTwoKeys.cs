using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.Collections;

namespace Simple.Modeling
{
    public class ModelDictionaryWithTwoKeys<TKey1, TKey2, TValue> : SimpleTwoKeyDictionaryBase<TKey1, TKey2, TValue>, IModelElement 
        where TValue : ModelElement
    {
		//private IModel model = null;
		private object owner = null;

        public ModelDictionaryWithTwoKeys()
            : this(null)
        {
        }

        public ModelDictionaryWithTwoKeys(object owner)
        {
            this.owner = owner;
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
				foreach (KeyValuePair<TKey1, TValue> model in this)
					model.Value.Owner = this.Owner;
			}
		}

		//public TValue GetValueByKey1(TKey1 key1)
		//{
		//	return this.DictionaryGetByKey1(key1);
		//}

        //public TValue GetValueByKey2(TKey2 key2)
        //{
        //    return this.DictionaryGetByKey2(key2);
        //}

        //public void SetValueByKey1(TKey1 key1, TValue value)
        //{
        //    this.DictionarySetByKey1(key1, value);
        //}

        //public void SetValueByKey2(TKey2 key2, TValue value)
        //{
        //    this.DictionarySetByKey2(key2, value);
        //}

        //public void Add(TKey1 key1, TKey2 key2, TValue value)
        //{
        //    this.DictionaryAdd(key1, key2, value);
        //}

        //public bool RemoveByKey1(TKey1 key1)
        //{
        //    return this.DictionaryRemoveByKey1(key1);
        //}

        //public bool RemoveByKey2(TKey2 key2)
        //{
        //    return this.DictionaryRemoveByKey2(key2);
        //}

        //public void Clear()
        //{
        //    this.DictionaryClear();
        //}

		//public IModel GetModel()
		//{
		//	return this.model;
		//}

        protected override void OnSet(TKey1 key1, TKey2 key2, TValue oldValue, TValue newValue)
        {
            base.OnSet(key1, key2, oldValue, newValue);
			newValue.Owner = this.Owner;
			//this.SetModel(newValue);
        }

        protected override void OnAdd(TKey1 key1, TKey2 tkey2, TValue value)
        {
            base.OnAdd(key1, tkey2, value);
			value.Owner = this.Owner;
			//this.SetModel(value);
        }
        
		//private void SetModel(ISetModel model)
		//{
		//	model.SetModel(this.model);
		//}

		//void ISetModel.SetModel(IModel model)
		//{
		//	this.model = model;
            
		//	foreach (KeyValuePair<TKey1, TValue> model in this)
		//		(model.Value as ISetModel).SetModel(this.GetModel());
		//}
    }
}
