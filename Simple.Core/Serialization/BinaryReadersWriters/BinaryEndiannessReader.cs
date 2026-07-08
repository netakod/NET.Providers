using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public abstract class BinaryEndiannessReader
	{
		public abstract ISequenceReader Provider { get; }
		
		//public abstract long Position { get; }

		//public abstract object GetCurrentPositionToken();

		public bool CanRead => this.Provider.CanRead;

		public abstract void ResetPosition();
		//public abstract long Length { get; }


		/// <summary>
		/// Reads the next byte from the current stream and advances the current position of the stream by one byte.
		/// </summary>
		/// <returns>The next byte read from the current stream.</returns>
		public abstract byte ReadByte();

		///// <summary>
		///// Reads a signed byte from this stream and advances the current position of the stream by one byte.
		///// </summary>
		///// <returns>A signed byte read from the current sequence.</returns>
		//public abstract sbyte ReadSByte();

		/// <summary>
		/// Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.
		/// </summary>
		/// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
		/// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
		public abstract byte[] ReadByteArray(int count, out int offset);


		public byte[] ReadByteArray(int count)
		{
			int offset;
			byte[] result = this.ReadByteArray(count, out offset);

			if (offset == 0)
				return result;
			else
			{
				// offset exists -> need to create new buffer with no offset
				byte[] newBuffer = new byte[result.Length - offset];

				Array.Copy(result, offset, newBuffer, 0, newBuffer.Length);

				return newBuffer;
			}
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
		/// <param name="count">The maximum number of bytes to be read from the current stream.</param>
		/// <returns></returns>
		public int ReadToBuffer(byte[] buffer, int offset, int count)
		{
			int position = offset;
			int bytesRead = 0;


			//for (bytesRead = 0; bytesRead < count; bytesRead++)
			//{
			//	if (reader.CanRead)
			//		buffer[position++] = reader.ReadByte();
			//	else
			//		return bytesRead; // bytes read;
			//}

			//return count;

			while (this.CanRead && bytesRead < count)
			{
				buffer[position++] = this.ReadByte();
				bytesRead++;
			}

			return bytesRead;
		}

		
		/// <summary>
		/// Reads the Span
		/// </summary>
		/// <param name="length"></param>
		/// <returns></returns>
		public ReadOnlySpan<byte> ReadSpan(int length) => this.Provider.ReadSpan(length);

		/// <summary>
		/// Reads the specified number of bytes directly from the stream, starting from a specified point in the byte value.
		/// </summary>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="offset">The starting point in the buffer at which to begin writing into the buffer.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>The number of bytes read into buffer. This might be less than the number of bytes 
		/// requested if that many bytes are not available, or it might be zero if the end of the stream is reached.
		/// </returns>
		//public abstract int ReadToBuffer(byte[] buffer, int offset, int count);

		/// <summary>
		/// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
		/// </summary>
		/// <returns>A 2-byte signed integer read from the current stream.</returns>
		public abstract short ReadInt16();

		/// <summary>
		/// Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.
		/// </summary>
		/// <returns>A 4-byte signed integer read from the current stream.</returns>
		public abstract int ReadInt32();

		/// <summary>
		/// eads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte signed integer read from the current stream.</returns>
		public abstract long ReadInt64();

		/// <summary>
		/// Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.
		/// </summary>
		/// <returns>A 2-byte unsigned integer read from this stream.</returns>
		public abstract ushort ReadUInt16();

		/// <summary>
		/// Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.
		/// </summary>
		/// <returns>A 4-byte unsigned integer read from this stream.</returns>
		public abstract uint ReadUInt32();

		/// <summary>
		/// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte unsigned integer read from this stream.</returns>
		public abstract ulong ReadUInt64();

		/// <summary>
		/// Reads an 4-byte floating point value from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 4-byte floating point value read from the current stream.</returns>
		public abstract float ReadSingle();

		/// <summary>
		/// Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte floating point value read from the current stream.</returns>
		public abstract double ReadDouble();

		/// <summary>
		/// Returns an <see cref="Decimal"/> instance converted from the bytes from the current stream>.
		/// Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
		/// </summary>
		/// <param name="buffer">The byte array storing the raw data.</param>
		/// <returns>The converted value.</returns>
		public virtual decimal ReadDecimal()
		{
			int[] parts = new int[4];
			
			for (int i = 0; i < 4; i++)
				parts[i] = this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24;
			
			return new Decimal(parts);
		}

		///// <summary>
		///// Reads the next character from the current stream and advances the current position of the stream in accordance with the Encoding used and the specific character being read from the stream.
		///// </summary>
		///// <returns>A character read from the current stream.</returns>
		//public abstract char ReadChar();

		/// <summary>
		/// Returns a Int16 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int16 value.</returns>
		public short Read7BitEncodedInt16() => unchecked((short)this.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns a UInt16 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A UInt16 value.</returns>
		public ushort Read7BitEncodedUInt16() => (ushort)Read7BitEncodedUInt64();

		/// <summary>
		/// Returns a Int32 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int32 value.</returns>
		public int Read7BitEncodedInt32() => unchecked((int)Read7BitEncodedUInt64());
		
		/// <summary>
		/// Returns a UInt32 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A UInt32 value.</returns>
		public uint Read7BitEncodedUInt32() => (uint)Read7BitEncodedUInt64();

		/// <summary>
		/// Returns a Int64 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int64 value.</returns>
		public long Read7BitEncodedInt64() => unchecked((long)Read7BitEncodedUInt64());
		
		/// <summary>
		/// Reads in a 64-bit unigned long in compressed format.
		/// </summary>
		/// <returns> A 64-bit unsigned long in compressed format.</returns>
		public abstract ulong Read7BitEncodedUInt64();

		/// <summary>
		/// Returns a Int16 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int16 value.</returns>
		public static short Read7BitEncodedInt16(ISequenceReader reader) => unchecked((short)Read7BitEncodedUInt64(reader));

		/// <summary>
		/// Returns a UInt32 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A UInt32 value.</returns>
		public static ushort Read7BitEncodedUInt16(ISequenceReader reader) => (ushort)Read7BitEncodedUInt64(reader);

		/// <summary>
		/// Returns a Int32 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int32 value.</returns>
		public static int Read7BitEncodedInt32(ISequenceReader reader) => unchecked((int)Read7BitEncodedUInt64(reader));
		
		/// <summary>
		/// Returns a UInt32 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A UInt32 value.</returns>
		public static uint Read7BitEncodedUInt32(ISequenceReader reader) => (uint)Read7BitEncodedUInt64(reader);

		/// <summary>
		/// Returns a Int64 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int64 value.</returns>
		public static long Read7BitEncodedInt64(ISequenceReader reader) => unchecked((long)Read7BitEncodedUInt64(reader));

		/// <summary>
		/// Returns a UInt64 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A UInt64 value.</returns>
		public static ulong Read7BitEncodedUInt64(ISequenceReader reader)
		{
			if (BitConverter.IsLittleEndian)
				return BinaryLittleEndianReader.ReadLittleEndian7BitEncodedUInt64(reader);
			else
				return BinaryBigEndianReader.ReadBigEndian7BitEncodedUInt64(reader);
		}

		public static BinaryEndiannessReader Create(ISequenceReader reader)
		{
			if (BitConverter.IsLittleEndian)
				return new BinaryLittleEndianReader(reader);
			else
				return new BinaryBigEndianReader(reader);
		}

	}
}
