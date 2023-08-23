using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Modeling
{
	public abstract class PropertyModelBase
	{
		private PropertyModelCollection propertyModelCollection = null;

		public PropertyModelBase()
		{
			this.propertyModelCollection = new PropertyModelCollection(instanceModelHolder: this, owner: this);
		}

		//public ObjectPropertyModelBase(Comparison<TPropertyModel> sortingComparison)
		//{
		//	this.propertyModelCollection = new PropertyModelCollection<TPropertyModel>(instanceModelHolder: this, owner: this);
		//}

		public PropertyModel this[int propertyIndex]
		{
			get { return this.propertyModelCollection[propertyIndex]; }
		}

		public PropertyModel this[string propertyName]
		{
			get { return this.propertyModelCollection[propertyName]; }
		}

		public PropertyModelCollection GetCollection()
		{
			return this.propertyModelCollection;
		}
	}
}
