using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public abstract class BinaryEndiannessWriter
	{
		public abstract ISequenceWriter Provider { get; }

		/// <summary>
		/// Writes an unsigned byte to the current stream and advances the stream position by one byte.
		/// </summary>
		/// <param name="value">The unsigned byte to write.</param>
		public abstract void WriteByte(byte value);

		///// <summary>
		///// Writes a signed byte to the current stream and advances the stream position by one byte.
		///// </summary>
		///// <param name="value">The signed byte to write.</param>
		//public abstract void WriteSByte(sbyte value);

		/// <summary>
		/// Writes a byte array to the underlying stream.
		/// </summary>
		/// <param name="buffer">A byte array containing the data to write.</param>
		public void WriteByteArray(byte[] buffer) => this.WriteByteArray(buffer, 0, buffer.Length);

		/// <summary>
		/// Writes a region of a byte array direcly to the current stream.
		/// </summary>
		/// <param name="buffer">A byte array containing the data to write.</param>
		/// <param name="offset">The starting point in buffer at which to begin writing.</param>
		/// <param name="count">The number of bytes to write.</param>

		public abstract void WriteByteArray(byte[] buffer, int offset, int count);
		//{
		//	int stop = offset + count;

		//	for (int i = offset; i < stop; i++)
		//		this.WriteByte(buffer[i]);
		//}

		/// <summary>
		/// Writes a region of a byte array direcly to the current stream.
		/// </summary>
		/// <param name="buffer">A byte array containing the data to write.</param>
		/// <param name="offset">The starting point in buffer at which to begin writing.</param>
		/// <param name="count">The number of bytes to write.</param>
		//public abstract void WriteFromBuffer(byte[] buffer, int offset, int count);

		public virtual void WriteSpan(ReadOnlySpan<byte> span, int count) => this.Provider.WriteSpan(span, count);

		public virtual void WriteSpanSequence(ReadOnlySpanSegment<byte> first) => this.Provider.WriteSpanSequence(first);
		public virtual void WriteSpanSequence(SpanSegment<byte> first) => this.Provider.WriteSpanSequence(first);


		/// <summary>
		///  Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte signed integer to write.</param>
		public abstract void WriteInt16(short value);

		/// <summary>
		/// Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte signed integer to write.</param>
		public abstract void WriteInt32(int value);

		/// <summary>
		/// Writes a eight-byte signed integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public abstract void WriteInt64(long value);

		/// <summary>
		/// Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public abstract void WriteUInt16(ushort value);

		/// <summary>
		/// Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public abstract void WriteUInt32(uint value);

		/// <summary>
		/// Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public abstract void WriteUInt64(ulong value);

		/// <summary>
		/// Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public abstract void WriteSingle(float value);

		/// <summary>
		/// Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public abstract void WriteDouble(double value);

		/// <summary>
		/// Stores the specified <see cref="Decimal"/> value as bytes to the current stream>.
		/// Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="buffer">The byte array to store the value in.</param>
		public virtual void WriteDecimal(decimal value)
		{
			int[] parts = Decimal.GetBits(value);
			
			for (int i = 0; i < 4; i++)
			{
				int part = parts[i];
				
				this.WriteByte((byte)part);
				this.WriteByte((byte)(part >> 8));
				this.WriteByte((byte)(part >> 16));
				this.WriteByte((byte)(part >> 24));
			}
		}

		///// <summary>
		///// Writes a Unicode character to the current stream and advances the current position of the stream in accordance with the Encoding used and the specific characters being written to the stream.
		///// </summary>
		///// <param name="ch">The non-surrogate, Unicode character to write.</param>
		//public abstract void WriteChar(char ch);

		///// <summary>
		///// Writes a length-prefixed string to this stream in the current encoding of the System.IO.BinaryWriter, and advances the current position of the stream 
		///// in accordance with the encoding used and the specific characters being written to the stream.
		///// </summary>
		///// <param name="value">The value to write.</param>
		//public abstract void WriteString(string value);

		/// <summary>
		/// Writes a 16-bit integer in a compressed format.
		/// </summary>
		/// <param name="value">The 16-bit short integer to be written.</param>
		public void Write7BitEncodedInt16(short value) => this.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Writes a 16-bit unsigned short integer in a compressed format.
		/// </summary>
		/// <param name="value">The 16-bit unsugned short integer to be written.</param>
		public void Write7BitEncodedUInt16(ushort value) => this.Write7BitEncodedUInt64(unchecked(value));

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="value">The 32-bit integer to be written.</param>
		public void Write7BitEncodedInt32(int value) => this.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Writes a 32-bit unsigned integer in a compressed format.
		/// </summary>
		/// <param name="value">The 32-bit unsugned integer to be written.</param>
		public void Write7BitEncodedUInt32(uint value) => this.Write7BitEncodedUInt64(unchecked(value));

		/// <summary>
		/// Writes a 64-bit long integer in a compressed format.
		/// </summary>
		/// <param name="value">The 64-bit long integer to be written.</param>
		public void Write7BitEncodedInt64(long value) => this.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Writes a 64-bit unsigned long integer in a compressed format.
		/// </summary>
		/// <param name="value">The 64-bit unsugned long integer to be written.</param>
		public abstract void Write7BitEncodedUInt64(ulong value);

		/// <summary>
		/// Writes a 64-bit unsigned integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The value to be writen</param>
		public static void Write7BitEncodedUInt64(ISequenceWriter writer, ulong value)
		{
			if (BitConverter.IsLittleEndian)
			{
				BinaryLittleEndianWriter.WriteLittleEndian7BitEncodedUInt64(writer, value);
			}
			else
			{
				BinaryBigEndianWriter.WriteBigEndian7BitEncodedUInt64(writer, value);
			}
		}

		/// <summary>
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant
		/// information first, with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// Source https://github.com/antmicro/Packet.Net/blob/master/PacketDotNet/MiscUtil/IO/EndianBinaryWriter.cs
		/// </summary>
		/// <param name="value">The 7-bit encoded integer to write to the stream</param>
		//public void Write7BitEncodedInt(int value)
		//{
		//	CheckDisposed();
		//	if (value < 0)
		//	{
		//		throw new ArgumentOutOfRangeException("value", "Value must be greater than or equal to 0.");
		//	}
		//	int index = 0;
		//	while (value >= 128)
		//	{
		//		buffer[index++] = (byte)((value & 0x7f) | 0x80);
		//		value = value >> 7;
		//		index++;
		//	}
		//	buffer[index++] = (byte)value;
		//	stream.Write(buffer, 0, index);
		//}

		public static BinaryEndiannessWriter Create(ISequenceWriter writer)
		{
			if (BitConverter.IsLittleEndian)
				return new BinaryLittleEndianWriter(writer);
			else
				return new BinaryBigEndianWriter(writer);
		}
	}
}
