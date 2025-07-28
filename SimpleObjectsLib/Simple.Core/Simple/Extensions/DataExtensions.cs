using System;
using System.Collections.Generic;
#if !SILVERLIGHT
using System.Diagnostics;
#endif
using System.Globalization;
using System.Net;

namespace Simple
{
    /// <summary>
    /// Collection of different extension method for data manipulation
    /// </summary>
    public static class DataExtensions
    {
        public static BigInteger ToBigInteger(this byte[] data)
        {
            var reversed = new byte[data.Length];
            Buffer.BlockCopy(data, 0, reversed, 0, data.Length);
            
            return new BigInteger(reversed.Reverse());
        }

        /// <summary>
        /// Reverses the sequence of the elements in the entire one-dimensional <see cref="Array"/>.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> to reverse.</param>
        /// <returns>
        /// The <see cref="Array"/> with its elements reversed.
        /// </returns>
        public static T[] Reverse<T>(this T[] array)
        {
            Array.Reverse(array);
            
            return array;
        }

#if SILVERLIGHT
#else
        /// <summary>
        /// Prints out 
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public static void DebugPrint(this IEnumerable<byte> bytes)
        {
            foreach (var b in bytes)
                Debug.Write(string.Format(CultureInfo.CurrentCulture, "0x{0:x2}, ", b));

            Debug.WriteLine(string.Empty);
        }
#endif

        /// <summary>
        /// Trims the leading zero from bytes array.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Data without leading zeros.</returns>
        public static IEnumerable<byte> TrimLeadingZero(this IEnumerable<byte> data)
        {
            var leadingZero = true;
            foreach (var item in data)
            {
                if (item == 0 & leadingZero)
                    continue;

                leadingZero = false;
                yield return item;
            }
        }

        /// <summary>
        /// Returns the specified 16-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 2.</returns>
        public static byte[] GetBytes(this Int16 value)
        {
            return new[] { (byte)(value >> 8), (byte)(value & 0xFF) };
        }


		/// <summary>
		/// Returns the specified 16-bit unsigned integer value as an array of bytes.
		/// </summary>
		/// <param name="value">The number to convert.</param>
		/// <returns>An array of bytes with length 2.</returns>
		//[CLSCompliant(false)]
		public static byte[] GetBytes(this UInt16 value)
        {
            return new[] { (byte)(value >> 8), (byte)(value & 0xFF) };
        }

        /// <summary>
        /// Returns the specified 32-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 4.</returns>
        public static byte[] GetBytes(this Int32 value)
        {
#if TUNING
            var buffer = new byte[4];
            value.Write(buffer, 0);
            return buffer;
#else
            return new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF) };
#endif
        }

		/// <summary>
		/// Returns the specified 32-bit unsigned integer value as an array of bytes.
		/// </summary>
		/// <param name="value">The number to convert.</param>
		/// <returns>An array of bytes with length 4.</returns>
		//[CLSCompliant(false)]
		public static byte[] GetBytes(this UInt32 value)
        {
#if TUNING
            var buffer = new byte[4];
            value.Write(buffer, 0);
            return buffer;
#else
            return new[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF) };
#endif
        }

        /// <summary>
        /// Returns the specified 32-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <param name="buffer">The array of bytes to write <paramref name="value"/> to.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin writing.</param>
        public static void Write(this int value, byte[] buffer, int offset)
        {
            buffer[offset++] = (byte)(value >> 24);
            buffer[offset++] = (byte)(value >> 16);
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset] = (byte)(value & 0xFF);
        }

		/// <summary>
		/// Returns the specified 32-bit unsigned integer value as an array of bytes.
		/// </summary>
		/// <param name="value">The number to convert.</param>
		/// <param name="buffer">The array of bytes to write <paramref name="value"/> to.</param>
		/// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin writing.</param>
		//[CLSCompliant(false)]
		public static void Write(this uint value, byte[] buffer, int offset)
        {
            buffer[offset++] = (byte)(value >> 24);
            buffer[offset++] = (byte)(value >> 16);
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset] = (byte)(value & 0xFF);
        }

        /// <summary>
        /// Returns the specified 64-bit signed integer value as an array of bytes.
        /// </summary>
        /// <param name="value">The number to convert.</param>
        /// <returns>An array of bytes with length 8.</returns>
        public static byte[] GetBytes(this Int64 value)
        {
            return new[]
                {
                    (byte) (value >> 56), (byte) (value >> 48), (byte) (value >> 40), (byte) (value >> 32),
                    (byte) (value >> 24), (byte) (value >> 16), (byte) (value >> 8), (byte) (value & 0xFF)
                };
        }

		/// <summary>
		/// Returns the specified 64-bit unsigned integer value as an array of bytes.
		/// </summary>
		/// <param name="value">The number to convert.</param>
		/// <returns>An array of bytes with length 8.</returns>
		//[CLSCompliant(false)]
		public static byte[] GetBytes(this UInt64 value)
        {
            return new[]
                {
                    (byte) (value >> 56), (byte) (value >> 48), (byte) (value >> 40), (byte) (value >> 32),
                    (byte) (value >> 24), (byte) (value >> 16), (byte) (value >> 8), (byte) (value & 0xFF)
                };
        }

        /// <summary>
        /// Returns a specified number of contiguous bytes from a given offset.
        /// </summary>
        /// <param name="data">The array to return a number of bytes from.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin taking bytes.</param>
        /// <param name="length">The number of bytes to take from <paramref name="data"/>.</param>
        /// <returns>
        /// A <see cref="byte"/> array that contains the specified number of bytes at the specified offset
        /// of the input array.
        /// </returns>
        public static byte[] Take(this byte[] data, int offset, int length)
        {
            var taken = new byte[length];
            Buffer.BlockCopy(data, offset, taken, 0, length);
            
            return taken;
        }
    }
}
