//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Simple.Collections
//{
//	/// <summary>
//	/// Represents a read-only collection of elements that can be accessed by index.
//	/// </summary>
//	/// <typeparam name="T">The type of elements in the read-only list. This type parameter is covariant.
//	/// That is, you can use either the type you specified or any type that is more derived.
//	/// For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
//	public interface IReadOnlyList<out T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
//	{
//		System.Collections.Generic.IReadOnlyList
		
//		/// <summary>
//		/// Gets the element at the specified index in the read-only list.
//		/// </summary>
//		/// <param name="index">The zero-based index of the element to get.</param>
//		/// <returns>The element at the specified index in the read-only list.</returns>
//		T this[int index] { get; }
//	}
//}
