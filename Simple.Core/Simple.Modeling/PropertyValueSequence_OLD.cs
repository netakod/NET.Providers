//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Simple.Modeling
//{
//	public class PropertyValueSequence
//	{
//		public PropertyValueSequence()
//			: this(new IPropertyModel[0], new object[0])
//		{
//		}

//		public PropertyValueSequence(IPropertyModel[] propertyModels, object[] propertyValues)
//			: this(new PropertySequence(propertyModels), propertyValues)
//		{
//		}

//		public PropertyValueSequence(IPropertySequence modelSequence, object[] propertyValues)
//		{
//			this.PropertySequence = modelSequence;
//			this.PropertyValues = propertyValues;
//		}

//		public IPropertySequence PropertySequence { get; private set; }
//		public object[] PropertyValues { get; private set; }

//		public IPropertyModel[] PropertyModels
//		{
//			get { return this.PropertySequence.PropertyModels; }
//		}

//		public int[] PropertyIndexes
//		{
//			get { return this.PropertySequence.PropertyIndexes; }
//		}

//		public int[] PropertyTypeIds
//		{
//			get { return this.PropertySequence.PropertyTypeIds; }
//		}

//		public int Length
//		{
//			get { return this.PropertySequence.Length; }
//		}

//		//public PropertyValueSequence Reverse()
//		//{
//		//	IPropertyModel[] reversedPropertyModels = new IPropertyModel[this.Length];
//		//	object[] reversedPropertyValues = new object[this.Length];

//		//	for (int i = 0; i < this.Length; i++)
//		//	{
//		//		reversedPropertyModels[i] = this.PropertyModels[this.Length - i - 1];
//		//		reversedPropertyValues[i] = this.PropertyValues[this.Length - i - 1];
//		//	}

//		//	return new PropertyValueSequence(new PropertySequence(reversedPropertyModels), reversedPropertyValues);
//		//}
//	}
//}
