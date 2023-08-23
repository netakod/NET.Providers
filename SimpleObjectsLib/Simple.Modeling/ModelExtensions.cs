using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Modeling
{
	public static class ModelExtension
	{

		public static int[] ToIndexSequence(this IModelElement[] propertyModelSequence)
		{
			int[] values = new int[propertyModelSequence.Length];

			for (int i = 0; i < values.Length; i++)
				values[i] = propertyModelSequence[i].Index;

			return values;
		}

		public static string[] ToNameSequence(this IModelElement[] propertyModelSequence)
		{
			string[] values = new string[propertyModelSequence.Length];

			for (int i = 0; i < values.Length; i++)
				values[i] = propertyModelSequence[i].Name;

			return values;
		}

		public static int[] ToTypeIdSequence(this IPropertyModel[] propertyModelSequence)
		{
			int[] values = new int[propertyModelSequence.Length];

			for (int i = 0; i < propertyModelSequence.Length; i++)
				values[i] = propertyModelSequence[i].PropertyTypeId;

			return values;
		}

		public static IPropertyModel[] ToModelSequence(this int[] propertyIndexSequence, IPropertyModelCollection<IPropertyModel> propertyModelCollection)
		{
			IPropertyModel[] values = new IPropertyModel[propertyIndexSequence.Length];

			for (int i = 0; i < values.Length; i++)
				values[i] = propertyModelCollection[propertyIndexSequence[i]];

			return values;
		}
	}
}
