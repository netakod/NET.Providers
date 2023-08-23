//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Dynamic;
//using System.Diagnostics.CodeAnalysis;
//#nullable enable

//namespace Simple
//{
//	public static class Math2
//	{
//		public static object? Max([AllowNull] object obj1, [AllowNull] object obj2)
//		{

//			if (obj2 == null)
//			{
//				return obj1;
//			}
//			else if (obj1 == null)
//			{
//				return obj2;
//			}
			
//			//if (obj1.GetType() != obj2.GetType())
//			//	return null;

//			if (obj1.GetType() == typeof(long))
//			{
//				return System.Math.Max((long)obj1, (long)obj2);
//			}
//			else if (obj1.GetType() == typeof(int))
//			{
//				return System.Math.Max((int)obj1, (int)obj2);
//			}
//			else if (obj1.GetType() == typeof(byte))
//			{
//				return System.Math.Max((byte)obj1, (byte)obj2);
//			}
//			else if (obj1.GetType() == typeof(short))
//			{
//				return System.Math.Max((short)obj1, (short)obj2);
//			}
//			else if (obj1.GetType() == typeof(decimal))
//			{
//				return System.Math.Max((decimal)obj1, (decimal)obj2);
//			}
//			else if (obj1.GetType() == typeof(double))
//			{
//				return System.Math.Max((double)obj1, (double)obj2);
//			}
//			else if (obj1.GetType() == typeof(float))
//			{
//				return System.Math.Max((float)obj1, (float)obj2);
//			}
//			else if (obj1.GetType() == typeof(sbyte))
//			{
//				return System.Math.Max((sbyte)obj1, (sbyte)obj2);
//			}
//			else if (obj1.GetType() == typeof(ushort))
//			{
//				return System.Math.Max((ushort)obj1, (ushort)obj2);
//			}
//			else if (obj1.GetType() == typeof(uint))
//			{
//				return System.Math.Max((uint)obj1, (uint)obj2);
//			}
//			else if (obj1.GetType() == typeof(ulong))
//			{
//				return System.Math.Max((ulong)obj1, (ulong)obj2);
//			}
//			else
//			{
//				throw new ArgumentOutOfRangeException("SystemSimpleObjectCollection Max function comparison object must be one of the following: byte, short, ushort, int, uint, long or ulong.");
//			}
//		}

//		public static T? Max<T>([AllowNull] T obj1, [AllowNull] T obj2)
//		{
//			var result = (T?)Max((object?)obj1, (object?)(obj2));

//			return result;
//		}

//		public static object? Sum([AllowNull] object obj1, [AllowNull] object obj2)
//		{
//			if (obj2 == null)
//			{
//				return obj1;
//			}
//			else if (obj1 == null)
//			{
//				return obj2;
//			}
			
//			if (obj1.GetType() == typeof(long))
//			{
//				return (long)obj1 + (long)obj2;
//			}
//			else if (obj1.GetType() == typeof(int))
//			{
//				return (int)obj1 + (int)obj2;
//			}
//			if (obj1.GetType() == typeof(byte))
//			{
//				return (byte)obj1 + (byte)obj2;
//			}
//			else if (obj1.GetType() == typeof(short))
//			{
//				return (short)obj1 + (short)obj2;
//			}
//			else if (obj1.GetType() == typeof(decimal))
//			{
//				return (decimal)obj1 + (decimal)obj2;
//			}
//			else if (obj1.GetType() == typeof(double))
//			{
//				return (double)obj1 + (double)obj2;
//			}
//			else if (obj1.GetType() == typeof(float))
//			{
//				return (float)obj1 + (float)obj2;
//			}
//			else if (obj1.GetType() == typeof(sbyte))
//			{
//				return (sbyte)obj1 + (sbyte)obj2;
//			}
//			else if (obj1.GetType() == typeof(ushort))
//			{
//				return (ushort)obj1 + (ushort)obj2;
//			}
//			else if (obj1.GetType() == typeof(uint))
//			{
//				return (uint)obj1 + (uint)obj2;
//			}
//			else if (obj1.GetType() == typeof(ulong))
//			{
//				return (ulong)obj1 + (ulong)obj2;
//			}
//			else
//			{
//				throw new ArgumentOutOfRangeException("SystemSimpleObjectCollection Sum function comparison object must be one of the following: byte, short, ushort, int, uint, long or ulong.");
//			}
//		}

//		//public static object Sum(object obj1, int value)
//		//{
//		//	if (obj1 == null)
//		//		return value;

//		//	if (obj1.GetType() == typeof(long))
//		//	{
//		//		return (long)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(int))
//		//	{
//		//		return (int)obj1 + value;
//		//	}
//		//	if (obj1.GetType() == typeof(byte))
//		//	{
//		//		return (byte)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(short))
//		//	{
//		//		return (short)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(decimal))
//		//	{
//		//		return (decimal)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(double))
//		//	{
//		//		return (double)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(float))
//		//	{
//		//		return (float)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(sbyte))
//		//	{
//		//		return (sbyte)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(ushort))
//		//	{
//		//		return (ushort)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(uint))
//		//	{
//		//		return (uint)obj1 + value;
//		//	}
//		//	else if (obj1.GetType() == typeof(ulong))
//		//	{
//		//		return (ulong)obj1 + (ulong)value;
//		//	}
//		//	else
//		//	{
//		//		throw new ArgumentOutOfRangeException("SystemSimpleObjectCollection Sum function first argument (obj1) must be one of the following: byte, short, ushort, int, uint, long or ulong.");
//		//	}
//		//}

//		//public T Sum<T>(T obj1, T obj2)
//		//{
//		//	NumberContainer<T> numContainer = new NumberContainer<T>();
//		//	numContainer.ValueA = obj1;
//		//	numContainer.ValueB = obj2;

//		//	return numContainer.Total;
//		//}

//		//public static T Sum<T>(T obj1, T obj2) 
//		//{
//		//	dynamic dobj1 = obj1, dobj2 = obj2;
//		//	return dobj1 + dobj2;
//		//}

//		//public class NumberContainer<T> where T : struct
//		//{
//		//	public T ValueA { get; private set; }
//		//	public T ValueB { get; private set; }
//		//	public T Total { get { return ((dynamic)ValueA) + ((dynamic)ValueB); } }
//		//}

//		public static T? Sum<T>([AllowNull] T obj1, [AllowNull] T obj2)
//		{
//			return (T?)Sum((object?)obj1, (object?)(obj2));
//		}

//		public static T? Sum<T>([AllowNull] T obj1,	int value)
//		{
//			return (T?)Sum((object?)obj1, (object?)value);
//		}
//	}

//	public class NumberContainer<T>
//	{
//		[AllowNull]
//		public T? ValueA { get; set; }

//		[AllowNull]
//		public T? ValueB { get; set; }
		
//		private static readonly Func<T, T, T> adder;
//		static NumberContainer()
//		{
//			var p1 = Expression.Parameter(typeof(T));
//			var p2 = Expression.Parameter(typeof(T));
			
//			adder = (Func<T, T, T>)Expression
//				.Lambda(Expression.Add(p1, p2), p1, p2)
//				.Compile();
//		}

//		public T Total { get { return adder(ValueA, ValueB); } }
		
		
//		//private static readonly Func<T, T, T> adder;
		
//		//static NumberContainer() 
//		//{
//		//	var p1 = Expression.Parameter(typeof (T));
//		//	var p2 = Expression.Parameter(typeof (T));
//		//	adder = (Func<T, T, T>)Expression
//		//		.Lambda(Expression.Add(p1, p2), p1, p2)
//		//		.Compile();
//		//}

//		//public static T Sum<T>(T obj1, T obj2)
//		//{
//		//	dynamic dobj1 = obj1, dobj2 = obj2;
//		//	return dobj1 + dobj2;
//		//}
//	}
//}
