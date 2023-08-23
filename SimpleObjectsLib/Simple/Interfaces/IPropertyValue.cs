using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple
{
    public interface IPropertyValue
    {
        object? GetPropertyValue(int propertyIndex);
        object? GetPropertyValue(string propertyName);
		void SetPropertyValue(int propertyIndex, object? value);
    }
}
