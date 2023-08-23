//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Simple;
//using Simple.Collections;
//using Simple.Modeling;

//namespace Simple.Modeling
//{
//	public class PropertySequence : IPropertySequence, IServerPropertySequence
//	{
//		private int[] propertyIndexes = null;
//		private int[] propertyTypeIds = null;

//		public PropertySequence(IPropertyModel[] modelSequence) 
//		{
//			this.PropertyModels = modelSequence;
//		}

//		public PropertySequence(IPropertyModel[] modelSequence, int[] propertyTypeIds)
//			: this(modelSequence)
//		{
//			this.propertyTypeIds = propertyTypeIds;
//		}

//		public IPropertyModel[] PropertyModels { get; private set; }

//		public int[] PropertyIndexes
//		{
//			get
//			{
//				if (this.propertyIndexes == null)
//					this.propertyIndexes = this.PropertyModels.ToIndexSequence();

//				return this.propertyIndexes;
//			}
//		}

//		public int[] PropertyTypeIds
//		{
//			get
//			{
//				if (this.propertyTypeIds == null)
//					this.propertyTypeIds = this.PropertyModels.ToTypeIdSequence();

//				return this.propertyTypeIds;
//			}
//		}

//		public int Length
//		{
//			get { return this.PropertyModels.Length; }
//		}
//	}

//	public class SystemPropertySequenceHolder : IServerPropertySequence
//	{
//		public SystemPropertySequenceHolder(int[] propertyIndexes, int[] propertyTypeIds)
//		{
//			this.PropertyIndexes = propertyIndexes;
//			this.PropertyTypeIds = propertyTypeIds;
//		}

//		public int[] PropertyIndexes { get; private set; }
//		public int[] PropertyTypeIds { get; private set; }

//		public int Length
//		{
//			get { return this.PropertyIndexes.Length; }
//		}
//	}



//	public interface IPropertySequence : IServerPropertySequence
//	{
//		IPropertyModel[] PropertyModels { get; }
//		//IPropertyModel this[int index] { get; }
//	}

//	public interface IServerPropertySequence
//	{
//		//int PropertySequenceId { get; }
//		//IPropertyModel[] ModelSequence { get; }
//		int[] PropertyIndexes { get; }
//		int[] PropertyTypeIds { get; }
//		int Length { get; }
//		//IPropertyModel GetPropertyModel(int propertyIndex);
//	}


//	//public struct PropertyIndexTypePair
//	//{
//	//	public PropertyIndexTypePair(int propertyIndex, int propertyTypeId)
//	//	{
//	//		this.PropertyIndex = propertyIndex;
//	//		this.PropertyTypeId = propertyTypeId;
//	//	}

//	//	public int PropertyIndex { get; private set; }
//	//	public int PropertyTypeId { get; private set; }
//	//}
//}
