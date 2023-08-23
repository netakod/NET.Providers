using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
//using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Simple
{
    public static class ReflectionHelper
    {
		public static readonly string StrInstance = "Instance";

		//protected T CreateObject<T>(Type type)
		//{
		//	return (T)Microsoft.Extensions.DependencyInjection.ActivatorUtilities.CreateFactory(type, new Type[0]).Invoke(null, null);
		//}

		public static object? GetPropertyObject(PropertyInfo propertyInfo, object objectInstance)
        {
            object propertyObject;

            try
            {
                propertyObject = propertyInfo.GetValue(objectInstance, null);
            }
            catch
            {
                try
                {
                    propertyObject = propertyInfo.GetValue(null, null);
                }
                catch
                {
                    return null;
                }
            }

            return propertyObject;
        }

        public static object GetFieldObject(FieldInfo fieldInfo, object objectInstance)
        {
            object fieldObject;

            try
            {
                fieldObject = fieldInfo.GetValue(objectInstance);
            }
            catch
            {
                try
                {
                    fieldObject = fieldInfo.GetValue(null);
                }
                catch
                {
                    return null;
                }
            }

            return fieldObject;
        }

        public static Dictionary<string, TFieldType> GetFieldsByName<TObjectType, TFieldType>()
        {
            object? objectInstance = Activator.CreateInstance<TObjectType>();
            Dictionary<string, TFieldType> value = GetFieldsByName<TFieldType>(objectInstance);
            objectInstance = null;

            return value;
        }

		public static Dictionary<string, TFieldType> GetFieldsByName<TFieldType>(object objectInstance)
		{
			Dictionary<string, TFieldType> result = new Dictionary<string, TFieldType>();
			Dictionary<string, FieldInfo> fieldInfosByName = GetFieldInfosByName(objectInstance);

			foreach (var item in fieldInfosByName)
			{
				FieldInfo fieldInfo = item.Value;
				object fieldValue = ReflectionHelper.GetFieldObject(fieldInfo, objectInstance);

				if (fieldValue != null && fieldValue is TFieldType)
					result.Add(fieldInfo.Name, (TFieldType)fieldValue);
			}

			return result;
		}


		public static Dictionary<string, FieldInfo> GetFieldInfosByName(object objectInstance)
		{
			Dictionary<string, FieldInfo> result = new Dictionary<string, FieldInfo>();
			List<Dictionary<string, FieldInfo>> fieldsByInheritedObjects = new List<Dictionary<string, FieldInfo>>();

			if (objectInstance != null)
			{
				Type currentType = objectInstance.GetType();

				while (!currentType.Equals(typeof(object)))
				{
					Dictionary<string, FieldInfo> values = new Dictionary<string, FieldInfo>();
					FieldInfo[] fieldInfos = currentType.GetFields();

					foreach (FieldInfo fieldInfo in fieldInfos)
						values.Add(fieldInfo.Name, fieldInfo);

					if (values.Count > 0)
						fieldsByInheritedObjects.Add(values);

					currentType = currentType.BaseType;
				}

				fieldsByInheritedObjects.Reverse();

				foreach (Dictionary<string, FieldInfo> fields in fieldsByInheritedObjects)
					foreach (var fieldItem in fields)
						if (!result.ContainsKey(fieldItem.Key))
							result.Add(fieldItem.Key, fieldItem.Value);
			}

			return result;
		}

		public static List<FieldInfo> GetFieldInfos(object objectInstance, bool orderFromBaseType = true)
		{
			List<FieldInfo> result = new List<FieldInfo>();
			List<List<FieldInfo>> fieldsByInheritedObjects = new List<List<FieldInfo>>();

			if (objectInstance != null)
			{
				Type currentType = objectInstance.GetType();

				while (!currentType.Equals(typeof(object)))
				{
					List<FieldInfo> values = new List<FieldInfo>();
					FieldInfo[] fieldInfos = currentType.GetFields();

					foreach (FieldInfo fieldInfo in fieldInfos)
						values.Add(fieldInfo);

					if (values.Count > 0)
						fieldsByInheritedObjects.Add(values);

					currentType = currentType.BaseType;
				}

				if (orderFromBaseType)
					fieldsByInheritedObjects.Reverse();

				foreach (List<FieldInfo> fields in fieldsByInheritedObjects)
					result.AddRange(fields);
			}

			return result;
		}

		//public static IDictionary<string, TFieldType> GetFieldsByName<TFieldType>(object objectInstance)
		//      {
		//	Dictionary<string, TFieldType> result = new Dictionary<string, TFieldType>();
		//	IEnumerable<TFieldType> fields = GetFields<TFieldType>(objectInstance);

		//	foreach (TFieldType field in fields)
		//		result.Add(field.)


		//	if (objectInstance != null)
		//          {
		//              Type currentType = objectInstance.GetType();

		//              //FieldInfo[] fieldInfos = currentType.GetFields();

		//              //foreach (FieldInfo fieldInfo in fieldInfos)
		//              //{
		//              //    object fieldValue = ReflectionHelper.GetFieldObjectByReflection(fieldInfo, objectInstance);

		//              //    if (fieldValue != null && fieldValue is TFieldType && !values.ContainsKey(fieldInfo.Name))
		//              //        values.Add(fieldInfo.Name, (TFieldType)fieldValue);
		//              //}

		//              //// Collect Inherited static fields
		//              ////List<FieldInfo> InheritedStaticFieldInfoList = new List<FieldInfo>();

		//              //currentType = currentType.BaseType;

		//              while (!currentType.Equals(typeof(object)))
		//              {
		//			FieldInfo[] fieldInfos = currentType.GetFields();
		//			FieldInfo[] fieldInfos2 = (currentType == objectInstance.GetType()) ? currentType.GetFields() : currentType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		//			FieldInfo[] fieldInfos3 = currentType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		//			if (fieldInfos.Length != fieldInfos2.Length || fieldInfos.Length != fieldInfos3.Length)
		//				fieldInfos2 = fieldInfos;

		//			foreach (FieldInfo fieldInfo in fieldInfos)
		//                  {
		//                      object fieldValue = ReflectionHelper.GetFieldObjectByReflection(fieldInfo, objectInstance);

		//                      if (!result.ContainsKey(fieldInfo.Name) && fieldValue != null && fieldValue is TFieldType)
		//                          result.Add(fieldInfo.Name, (TFieldType)fieldValue);
		//                  }

		//                  currentType = currentType.BaseType;
		//              }
		//          }

		//	return result;
		//      }


		public static bool IsNullable(Type type)
		{
			if (type.IsGenericType)
			{
				return type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			else
			{
				return Nullable.GetUnderlyingType(type) != null;
			}
		}

		static bool IsNullable<T>(T obj)
		{
			if (obj == null) 
				return true; // obvious
			
			Type type = typeof(T);
			
			if (!type.IsValueType) 
				return true; // ref-type
			
			if (Nullable.GetUnderlyingType(type) != null) 
				return true; // Nullable<T>
			
			return false; // value-type
		}

		public static SimpleTypeInfo GetSimpleTypeInfo<T>()
        {
            return GetSimpleTypeInfo(typeof(T));
        }
        
        public static SimpleTypeInfo GetSimpleTypeInfo(Type type)
        {
            return new SimpleTypeInfo(type);
        }

        public static string GetTypeName(Type type)
        {
            string name;

			if (type == null)
			{
				name = "null";
			}
			else if (type == typeof(object))
			{
				name = "object";
			}
			else if (type == typeof(void))
			{
				name = "void";
			}
			else if (type == typeof(Char))
			{
				name = "char";
			}
			else if (type == typeof(Char[]))
			{
				name = "char[]";
			}
			else if (type == typeof(Char?))
			{
				name = "char?";
			}
			else if (type == typeof(Char?[]))
			{
				name = "char?[]";
			}
			else if (type == typeof(String))
			{
				name = "string";
			}
			else if (type == typeof(String[]))
			{
				name = "string[]";
			}
			else if (type == typeof(Boolean))
			{
				name = "bool";
			}
			else if (type == typeof(SByte))
			{
				name = "sbyte";
			}
			else if (type == typeof(Int16))
			{
				name = "short";
			}
			else if (type == typeof(Int32))
			{
				name = "int";
			}
			else if (type == typeof(Int64))
			{
				name = "long";
			}
			else if (type == typeof(Boolean[]))
			{
				name = "bool[]";
			}
			else if (type == typeof(SByte[]))
			{
				name = "sbyte[]";
			}
			else if (type == typeof(Int16[]))
			{
				name = "short[]";
			}
			else if (type == typeof(Int32[]))
			{
				name = "int[]";
			}
			else if (type == typeof(Int64[]))
			{
				name = "long[]";
			}
			else if (type == typeof(Boolean?))
			{
				name = "bool?";
			}
			else if (type == typeof(SByte?))
			{
				name = "sbyte?";
			}
			else if (type == typeof(Int16?))
			{
				name = "short?";
			}
			else if (type == typeof(Int32?))
			{
				name = "int?";
			}
			else if (type == typeof(Int64?))
			{
				name = "long?";
			}
			else if (type == typeof(Boolean?[]))
			{
				name = "bool?[]";
			}
			else if (type == typeof(SByte?[]))
			{
				name = "sbyte?[]";
			}
			else if (type == typeof(Int16?[]))
			{
				name = "short?[]";
			}
			else if (type == typeof(Int32?[]))
			{
				name = "int?[]";
			}
			else if (type == typeof(Int64?[]))
			{
				name = "long?[]";
			}
			else if (type == typeof(Byte))
			{
				name = "byte";
			}
			else if (type == typeof(UInt16))
			{
				name = "ushort";
			}
			else if (type == typeof(UInt32))
			{
				name = "uint";
			}
			else if (type == typeof(UInt64))
			{
				name = "ulong";
			}
			else if (type == typeof(Byte[]))
			{
				name = "byte[]";
			}
			else if (type == typeof(UInt16[]))
			{
				name = "ushort[]";
			}
			else if (type == typeof(UInt32[]))
			{
				name = "uint[]";
			}
			else if (type == typeof(UInt64[]))
			{
				name = "ulong[]";
			}
			else if (type == typeof(Byte?))
			{
				name = "byte?";
			}
			else if (type == typeof(UInt16?))
			{
				name = "ushort?";
			}
			else if (type == typeof(UInt32?))
			{
				name = "uint?";
			}
			else if (type == typeof(UInt64?))
			{
				name = "ulong?";
			}
			else if (type == typeof(Byte?[]))
			{
				name = "byte?[]";
			}
			else if (type == typeof(UInt16?[]))
			{
				name = "ushort?[]";
			}
			else if (type == typeof(UInt32?[]))
			{
				name = "uint?[]";
			}
			else if (type == typeof(UInt64?[]))
			{
				name = "ulong?[]";
			}
			else if (type == typeof(Half))
			{
				name = "Half";
			}
			else if (type == typeof(Single))
			{
				name = "float";
			}
			else if (type == typeof(Double))
			{
				name = "double";
			}
			else if (type == typeof(Decimal))
			{
				name = "decimal";
			}
			else if (type == typeof(Half[]))
			{
				name = "Half[]";
			}
			else if (type == typeof(Single[]))
			{
				name = "float[]";
			}
			else if (type == typeof(Double[]))
			{
				name = "double[]";
			}
			else if (type == typeof(Decimal[]))
			{
				name = "decimal[]";
			}
			else if (type == typeof(Half?))
			{
				name = "Half?";
			}
			else if (type == typeof(Single?))
			{
				name = "float?";
			}
			else if (type == typeof(Double?))
			{
				name = "double?";
			}
			else if (type == typeof(Decimal?))
			{
				name = "decimal?";
			}
			else if (type == typeof(Half?[]))
			{
				name = "Half?[]";
			}
			else if (type == typeof(Single?[]))
			{
				name = "float?[]";
			}
			else if (type == typeof(Double?[]))
			{
				name = "double?[]";
			}
			else if (type == typeof(Decimal?[]))
			{
				name = "decimal?[]";
			}
			else if (type == typeof(DateTime?))
			{
				name = "DateTime?";
			}
			else if (type == typeof(TimeSpan?))
			{
				name = "TimeSpan?";
			}
			else if (type == typeof(DateTime?[]))
			{
				name = "DateTime?[]";
			}
			else if (type == typeof(TimeSpan?[]))
			{
				name = "TimeSpan?[]";
			}
			else if (type == typeof(BitVector32[]))
			{
				name = "BitVector32[]";
			}
			else if (type == typeof(BitVector32?[]))
			{
				name = "BitVector32?[]";
			}
			else if (type == typeof(Guid?))
			{
				name = "Guid?";
			}
			else if (type == typeof(Guid?[]))
			{
				name = "Guid?[]";
			}
			else if (type.IsGenericType)
			{
				if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					name = GetTypeName(type.GenericTypeArguments[0]) + "?";
				}
				else
				{
					name = type.Name.Split('`')[0];
					name += "<";

					Type[] typeGenericArguments = type.GetGenericArguments();

					for (int i = 0; i < typeGenericArguments.Length; i++)
					{
						if (i > 0)
							name += ", ";

						name += GetTypeName(typeGenericArguments[i]);
					}

					name += ">";
				}
			}
			else
			{
				name = type.Name;
			}

			return name;
		}

        public static string GetAssemblyName(Type type)
        {
            string[] assemblyFullNameList = type.Assembly.FullName.Split(',');
            return assemblyFullNameList[0];
        }

        public static object? GetDefaultValue(Type declaredType)
        {
            object? result;

            if (declaredType.IsValueType)
            {
                result = Activator.CreateInstance(declaredType);
            }
            else if (declaredType == typeof(string))
            {
				result = String.Empty; // default(string);
            }
            else
            {
                result = null;
            }

            return result;
        }

		public static object? GetDefaultValue<T>() where T : notnull
		{
			return ReflectionHelper.GetDefaultValue(typeof(T));
			//return default(T);
		}

		public static void SetPropertiesToDefault<T>(T obj)
		{
			Type objectType = typeof(T);

			System.Reflection.PropertyInfo [] props = objectType.GetProperties();

			foreach (System.Reflection.PropertyInfo property in props)
			{
				if (property.CanWrite)
				{
					string propertyName = property.Name;
					Type propertyType = property.PropertyType;
					object? value = GetDefaultValue(propertyType);
					
					property.SetValue(obj, value, null);
				}
			}
		}

		public static IEnumerable<Type> SelectAssemblySubTypesOf(Type baseType)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			return SelectAssemblySubTypesOf(assemblies, baseType);
		}

		//public static IEnumerable<Type> SelectSubclassesOf(Predicate<Type> criteria)
		//{
		//	Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

		//	return SelectSubclassesOf(assemblies, criteria);
		//}

		public static IEnumerable<Type> SelectAssemblySubTypesOf<T>(IEnumerable<Assembly> assemblies)
		{
			return SelectAssemblySubTypesOf(assemblies, typeof(T));
		}

		public static IEnumerable<Type> SelectAssemblySubTypesOf(IEnumerable<Assembly> assemblies, Type baseType)
		{
			//IEnumerable<Type> collection = SelectAssemblyTypes(assemblies, type => type.IsSubclassOf(baseType) && baseType.IsAssignableFrom(type) && !type.IsInterface && !type.IsGenericType && !type.IsAbstract);
			IEnumerable<Type> collection = SelectAssemblyTypes(assemblies, type => type == baseType || baseType.IsSubclassOf(type) && !type.IsInterface && !type.IsGenericType);
			
			return SortTypesByTopInheritance(collection);
		}

		public static IEnumerable<Type> SelectAssemblySubTypesOf(IEnumerable<Type> assemblyTypes, Type baseType)
		{
			return from type in assemblyTypes
				   where type.IsSubclassOf(baseType) && baseType.IsAssignableFrom(type) && !type.IsInterface && !type.IsGenericType && !type.IsAbstract
				   select type;
		}

		public static IEnumerable<Type> SelectInheritedAssemblyTypes<T>()
		{
			return SelectInheritedAssemblyTypes(typeof(T));
		}

		public static IEnumerable<Type> SelectInheritedAssemblyTypes(Type baseType)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			return SelectInheritedAssemblyTypes(assemblies, baseType);
		}

		//public static IEnumerable<Type> SelectInheritedTypesInAssemblies(Predicate<Type> criteria)
		//{
		//	Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

		//	return SelectInheritedTypes(assemblies, criteria);
		//}

		public static IEnumerable<Type> SelectInheritedAssemblyTypes<T>(IEnumerable<Assembly> assemblies)
		{
			return SelectInheritedAssemblyTypes(assemblies, typeof(T));
		}

		public static IEnumerable<Type> SelectInheritedAssemblyTypes(IEnumerable<Assembly> assemblies, Type baseType)
		{
			IEnumerable<Type> assemblyTypes = SelectAssemblyTypes(assemblies);

			return SelectInheritedAssemblyTypes(assemblyTypes, baseType);
		}

		public static IEnumerable<Type> SelectInheritedAssemblyTypes<T>(IEnumerable<Type> assemblyTypes)
		{
			return SelectInheritedAssemblyTypes(assemblyTypes, typeof(T));
		}

		public static IEnumerable<Type> SelectInheritedAssemblyTypes(IEnumerable<Type> assemblyTypes, Type baseType)
		{
			IEnumerable<Type> collection = SelectAssemblyTypes(assemblyTypes, type => baseType.IsAssignableFrom(type) && !type.IsInterface && !type.IsGenericType && !type.IsAbstract);

			return SortTypesByTopInheritance(collection);
		}

		//public static IEnumerable<Type> SelectInheritedTypes(IEnumerable<Type> assemblyTypes, Predicate<Type> criteria)
		//{
		//	return from type in assemblyTypes
		//		   where criteria(type)
		//		   select type;
		//}

		public static IEnumerable<Type> SelectAssemblyTypes(Predicate<Type> criteria)
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			return SelectAssemblyTypes(assemblies, criteria);
		}

		public static IEnumerable<Type> SelectAssemblyTypes(IEnumerable<Assembly> assemblies, Predicate<Type> criteria)
		{
			IEnumerable<Type> assemblyTypes = SelectAssemblyTypes(assemblies);

			return SelectAssemblyTypes(assemblyTypes, criteria);
		}

		public static IEnumerable<Type> SelectAssemblyTypes(IEnumerable<Assembly> assemblies)
		{
			//// Some computer Net.Framework assemblies has an problem when calling assembly.GetTypes() that result in app crash.
			//// To avoid app chrash, we need an alternative method to get all ISimpleObjectModel object models class types.
			////
			//var objectModels = from assembly in AppDomain.CurrentDomain.GetAssemblies()
			//				   from type in assembly.GetTypes()
			//				   where type != null && match(type) //typeof(ISimpleObjectModel).IsAssignableFrom(type) && !type.IsInterface && !type.IsGenericType && !type.IsAbstract
			//				   select type;

			//// The problem is here....
			//foreach (Type type in objectModels)
			//	objectModels.Add(type);

			//List<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();

			HashSet<Type> result = new HashSet<Type>();

			foreach (Assembly assembly in assemblies)
			{
				Type[]? types = null;

				try
				{
					types = assembly.GetTypes();
				}
				catch
				{
					continue;
				}

				foreach (Type type in types)
					if (type != null && !result.Contains(type))
						result.Add(type);
			}

			return result;
		}

		//public static int IsSubclassComparer(Type a, Type b)
		//{
		//	if (a == null && b == null)
		//		return 0;
		//	else if (a == null)
		//		return -1;
		//	else if (b == null)
		//		return 1;
		//	else if (a == b)
		//		return 0;
		//	else
		//		return a.IsSubclassOf(b) ? 1 : 0;
		//}

		public static IEnumerable<Type> SelectAssemblyTypes(IEnumerable<Type> assemblyTypes, Predicate<Type> criteria)
		{
			return from type in assemblyTypes
				   where criteria(type)
				   select type;
		}

		public static Type FindTopInheritedTypeInAssembly<T>()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			return FindTopInheritedTypeInAssembly(assemblies, typeof(T));
		}

		public static Type FindTopInheritedTypeInAssembly<T>(IEnumerable<Assembly> assemblies)
		{
			return FindTopInheritedTypeInAssembly(assemblies, typeof(T));
		}

		public static Type FindTopInheritedTypeInAssembly(IEnumerable<Assembly> assemblies, Type type)
		{
			IEnumerable<Type> inheritedTypes = SelectInheritedAssemblyTypes(assemblies, type);
			Type resultType = FindTopInheritedClass(type, inheritedTypes);

			return resultType;
		}

		public static Type FindTopInheritedClass(Type baseType, IEnumerable<Type> collection)
		{
			Type result = baseType;

			foreach (Type item in collection)
				if (item.IsSubclassOf(result))
					result = item;

			return result;
		}

		public static IEnumerable<Type> SortTypesByTopInheritance(IEnumerable<Type> collection)
		{
			List<Type> unsortedResult = collection.ToList();
			List<Type> sortedResult = new List<Type>(unsortedResult.Count);

			while (unsortedResult.Count > 0)
			{
				Type topClassType = FindTopInheritedClass(unsortedResult[0], unsortedResult);

				sortedResult.Add(topClassType);
				unsortedResult.Remove(topClassType);
			}

			sortedResult.Reverse(); // Base class at the begining, top class at the end

			return sortedResult;
		}

		public static IEnumerable<Type> GetInheritedClasses(Type myType)
		{
			//if you want the abstract classes drop the type.IsAbstract but it is probably to instance so its a good idea to keep it.
			return Assembly.GetAssembly(myType).GetTypes().Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(myType));
		}

		public static IEnumerable<Assembly> GetInheritedAssemblies(Type myType)
		{
			return from type in GetInheritedClasses(myType)
				   select type.Assembly;
		}
	}

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
