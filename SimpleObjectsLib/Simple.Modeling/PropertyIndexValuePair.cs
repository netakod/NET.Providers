using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Modeling
{
	public struct PropertyIndexValuePair
	{
		public PropertyIndexValuePair(int propertyIndex, object? propertyValue)
		{
			this.PropertyIndex = propertyIndex;
			this.PropertyValue = propertyValue;
		}

		public int PropertyIndex { get; private set; }
		public object? PropertyValue { get; set; }
	}
}
