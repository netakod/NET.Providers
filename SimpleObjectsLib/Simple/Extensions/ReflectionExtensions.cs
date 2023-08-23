using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Simple
{
	public static class ReflectionExtensions
	{
		public static object? GetDefaultValue(this Type type)
		{
			return ReflectionHelper.GetDefaultValue(type);
		}

        /// <summary>
        /// Creates an instance of the specified type using that type's default constructor.
        /// </summary>
        /// <typeparam name="T">The type to create.</typeparam>
        /// <param name="type">Type of the instance to create.</param>
        /// <returns>A reference to the newly created object.</returns>
        internal static T? CreateInstance<T>(this Type type) where T : class
        {
            if (type == null)
                return null;

            return Activator.CreateInstance(type) as T;
        }

		/// <summary>
		/// Creates a delegate of the specified type to represent the specified static method.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodInfo"/> describing the static or instance method the delegate is to represent. 
		/// Only static methods are supported in the .NET Framework version 1.0 and 1.1.</param>
		/// <param name="type">The <see cref="System.Type"/> of delegate to create.</param>
		/// <returns>A delegate of the specified type to represent the specified static method.</returns>
		/// <exception cref="ArgumentNullException">type is null.-or- method is null.</exception>
		/// <exception cref="ArgumentException">type does not inherit System.MulticastDelegate.-or-type is not a RuntimeType.
		/// See Runtime Types in Reflection. -or- method is not a static method, and the.NET Framework version is 1.0 or 1.1. 
		/// -or-method cannot be bound.-or-method is not a <see cref="RuntimeMethodInfo"/>. See Runtime Types in Reflection.</exception>
		/// <exception cref="MissingMethodException">The Invoke method of type is not found.</exception>
		/// <exception cref="MethodAccessException">The caller does not have the permissions necessary to access method.</exception>
		public static Delegate CreateDelegate(this MethodInfo methodInfo, Type type)
		{
			return Delegate.CreateDelegate(type, methodInfo);
		}

		/// <summary>
		/// When overridden in a derived class, indicates whether one or more instance of attributeType is applied to this member. 
		/// Member's inheritance chain to find the attributes is included.
		/// </summary>
		/// <param name="memberInfo">The <see cref="MemberInfo"/> instance.</param>
		/// <param name="attributeType">The Type object to which the custom attributes are applied.</param>
		/// <returns> true if one or more instance of attributeType is applied to this member; otherwise false.</returns>
		public static bool IsDefined(this MemberInfo memberInfo, Type attributeType)
		{
			return memberInfo.IsDefined(attributeType, inherit: true);
		}

		
		public static string? GetCultureName(this AssemblyName assemblyName)
		{
			return assemblyName.CultureInfo?.Name;
		}

		/// <summary>
		/// Perform a deep Copy of the object.
		/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
		/// Provides a method for performing a deep copy of an object.
		/// Binary Serialization is used to perform the copy.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(this T source)
		{
			throw new NotImplementedException("This method using BinaryFormatter serialization that is obsolete. " +
											  "Due to security vulnerabilities in BinaryFormatter, the following APIs are marked as obsolete, starting in .NET 5");
			
			//if (!typeof(T).IsSerializable)
			//	throw new ArgumentException("The type must be serializable.", "source");

			//// Don't serialize a null object, simply return the default for that object
			//if (Object.ReferenceEquals(source, null))
			//	return default(T);

			//IFormatter formatter = new BinaryFormatter();
			//Stream stream = new MemoryStream();

			//using (stream)
			//{
			//	formatter.Serialize(stream, source);
			//	stream.Seek(0, SeekOrigin.Begin);

			//	return (T)formatter.Deserialize(stream);
			//}
		}

		public static string GetName(this Type type)
		{
			return ReflectionHelper.GetTypeName(type);
		}
	}
}
