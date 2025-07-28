using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Simple
{
	public class SimpleTypeInfo
	{
		public SimpleTypeInfo(Type type)
		{
			this.Type = type;
		}

		public Type Type { get; private set; }

		public string Name
		{
			get { return ReflectionHelper.GetTypeName(this.Type); }
		}

		public bool IsGeneric
		{
			get { return this.Type.IsGenericType; }
		}

		public bool IsNullable
		{
			get
			{
				bool result = true;
				GenericParameterAttributes sConstraints = this.Type.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask;

				if (GenericParameterAttributes.None != (sConstraints & GenericParameterAttributes.NotNullableValueTypeConstraint))
					result = false;

				return result;
			}
		}

		public bool IsVoid
		{
			get { return false; }
		}
	}
}
