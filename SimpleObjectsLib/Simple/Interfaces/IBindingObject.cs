using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public interface IBindingObject : IPropertyValue
    {
        //event BeforeChangePropertyValueEasyObjectRequesterEventHandler BeforePropertyValueChange;
        //event ChangePropertyValueEasyObjectRequesterEventHandler PropertyValueChange;

        object? this[int propertyIndex] { get; set; }

        //object GetPropertyValue(int propertyIndex);
        //T GetPropertyValue<T>(int propertyIndex);
        //object GetOldPropertyValue(int propertyIndex);
        //T GetOldPropertyValue<T>(int propertyIndex);
        void SetPropertyValue(int propertyIndex, object? value, object? requester);
		int[] GetChangedPropertyIndexes();

        //IModelBase GetModel();
        //object GetObject();
        string? GetImageName();

        //IDictionary<int, object> GetPropertyValueByIndexDictionary();
        //IDictionary<string, object> GetPropertyValueByNameDictionary();
        //IDictionary<int, object> GetOldPropertyValueByIndexDictionary();
        //IDictionary<string, object> GetOldPropertyValueByNameDictionary();
        //IDictionary<int, object> GetChangedPropertyValueByIndexDictionary();
        //IDictionary<string, object> GetChangedPropertyValueByNameDictionary();
        //IDictionary<int, object> GetChangedOldPropertyValueByIndexDictionary();
        //IDictionary<string, object> GetChangedOldPropertyValueByNameDictionary();

        void AcceptChanges(object requester);
        void RejectChanges(object requester);

        bool IsReadOnly { get; }
    }
}
