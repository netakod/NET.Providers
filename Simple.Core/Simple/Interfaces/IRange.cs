using System;

namespace Simple
{
	public interface IRangeFunc<R>
	{
		R Run<T>(IRange<T> range) where T : struct, IComparable<T>;
	}

	public interface IRange<T>
		where T : struct, IComparable<T>
	{
		string ToId(T value);
		int Size { get; }
		byte[] GetBytes(T value);
		T FromBytes(byte[] array);
		T MinValue { get; }
		T MaxValue{ get; }
	}
}
