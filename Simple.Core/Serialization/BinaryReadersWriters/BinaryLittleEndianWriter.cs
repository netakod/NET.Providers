using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public class BinaryLittleEndianWriter : BinaryEndiannessWriter
	{
		private ISequenceWriter writer = null;

		public BinaryLittleEndianWriter(ISequenceWriter writer) => this.writer = writer;

		public override ISequenceWriter Provider => this.writer;

		public override void WriteByte(byte value) => this.writer.WriteByte(value);

		public override void WriteByteArray(byte[] buffer, int offset, int count) => this.writer.WriteByteArray(buffer, offset, count);

		public override void WriteInt16(short value)
		{
			this.WriteByte((byte)value);
			this.WriteByte((byte)(value >> 8));
		}

		public override void WriteInt32(int value)
		{
			this.WriteByte((byte)value);
			this.WriteByte((byte)(value >> 8));
			this.WriteByte((byte)(value >> 16));
			this.WriteByte((byte)(value >> 24));
		}

		public override void WriteInt64(long value)
		{
			this.WriteByte((byte)value);
			this.WriteByte((byte)(value >> 8));
			this.WriteByte((byte)(value >> 16));
			this.WriteByte((byte)(value >> 24));
			this.WriteByte((byte)(value >> 32));
			this.WriteByte((byte)(value >> 40));
			this.WriteByte((byte)(value >> 48));
			this.WriteByte((byte)(value >> 56));
		}

		public override void WriteUInt16(ushort value)
		{
			this.WriteByte((byte)value);
			this.WriteByte((byte)(value >> 8));
		}

		public override void WriteUInt32(uint value)
		{
			this.WriteByte((byte)value);
			this.WriteByte((byte)(value >> 8));
			this.WriteByte((byte)(value >> 16));
			this.WriteByte((byte)(value >> 24));
		}

		public override void WriteUInt64(ulong value)
		{
			this.WriteByte((byte)value);
			this.WriteByte((byte)(value >> 8));
			this.WriteByte((byte)(value >> 16));
			this.WriteByte((byte)(value >> 24));
			this.WriteByte((byte)(value >> 32));
			this.WriteByte((byte)(value >> 40));
			this.WriteByte((byte)(value >> 48));
			this.WriteByte((byte)(value >> 56));
		}

		public unsafe override void WriteSingle(float value)
		{
			UInt32 raw = *(UInt32*)&value;

			this.WriteByte((byte)raw);
			this.WriteByte((byte)(raw >> 8));
			this.WriteByte((byte)(raw >> 16));
			this.WriteByte((byte)(raw >> 24));
		}

		public override unsafe void WriteDouble(double value)
		{
			UInt64 raw = *(UInt64*)&value;

			this.WriteByte((byte)raw);
			this.WriteByte((byte)(raw >> 8));
			this.WriteByte((byte)(raw >> 16));
			this.WriteByte((byte)(raw >> 24));
			this.WriteByte((byte)(raw >> 32));
			this.WriteByte((byte)(raw >> 40));
			this.WriteByte((byte)(raw >> 48));
			this.WriteByte((byte)(raw >> 56));
		}

		public override void Write7BitEncodedUInt64(ulong value) => WriteLittleEndian7BitEncodedUInt64(this.writer, value);

		public static void WriteLittleEndian7BitEncodedUInt64(ISequenceWriter writer, ulong value) // From .NET 6
		{
			//ulong uValue = (ulong)value;

			// Write out an int 7 bits at a time. The high bit of the byte,
			// when on, tells reader to continue reading more bytes.
			//
			// Using the constants 0x7F and ~0x7F below offers smaller
			// codegen than using the constant 0x80.

			while (value > 0x7Fu)
			{
				writer.WriteByte((byte)((uint)value | ~0x7Fu));
				value >>= 7;
			}

			writer.WriteByte((byte)value);
		}


		private static void WriteLittleEndian7BitEncodedUInt64Old(ISequenceWriter writer, ulong value)
		{
			while (value >= 0x80)
			{
				writer.WriteByte((byte)(value | 0x80));
				value >>= 7;
			}

			writer.WriteByte((byte)value);
		}
	}
}
