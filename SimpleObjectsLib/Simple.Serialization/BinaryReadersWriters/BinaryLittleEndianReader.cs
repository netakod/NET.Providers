using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
	public class BinaryLittleEndianReader : BinaryEndiannessReader
	{
		ISequenceReader reader = null;

		public BinaryLittleEndianReader(ISequenceReader reader) => this.reader = reader;

        public override ISequenceReader Provider => this.reader;

        public override byte ReadByte() => this.reader.ReadByte();
		public override byte[] ReadByteArray(int count, out int offset) => this.reader.ReadByteArray(count, out offset);
		//public override byte[] ReadBytes(int count) => this.reader.ReadBytes(count);
		//public override int ReadToBuffer(byte[] buffer, int offset, int count) => this.reader.ReadToBuffer(buffer, offset, count);


		public override short ReadInt16() => (Int16)(this.ReadByte() | this.ReadByte() << 8);

		public override int ReadInt32() => this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24;

		public override long ReadInt64() =>       this.ReadByte()		| (long)this.ReadByte() << 8  | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 24 |
											(long)this.ReadByte() << 32 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 56;
        public override ushort ReadUInt16() => (UInt16)(this.ReadByte() | this.ReadByte() << 8);

		public override uint ReadUInt32() => (UInt32)(this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24);

		public override ulong ReadUInt64() =>        this.ReadByte()       | (ulong)this.ReadByte() << 8  | (ulong)this.ReadByte() << 16 | (ulong)this.ReadByte() << 24 |
                                              (ulong)this.ReadByte() << 32 | (ulong)this.ReadByte() << 40 | (ulong)this.ReadByte() << 48 | (ulong)this.ReadByte() << 56;

		public override unsafe float ReadSingle()
		{
            Int32 raw = this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24;
            
            return *(Single*)&raw;
        }

		public override unsafe double ReadDouble()
		{
            Int64 raw =       this.ReadByte()       | (long)this.ReadByte() << 8  | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 24 | 
                        (long)this.ReadByte() << 32 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 56;            
            
            return *(Double*)&raw;
        }

        public override ulong Read7BitEncodedUInt64() => ReadLittleEndian7BitEncodedUInt64(this.reader);

        public override void ResetPosition() => this.reader.ResetPosition();

		// TODO: Check if this should be as method under
		public static ulong ReadLittleEndian7BitEncodedUInt64(ISequenceReader reader)
		{
            ulong result = 0;
            var bitShift = 0;

            while (true)
            {
                ulong nextByte = reader.ReadByte();

                result |= (nextByte & 0x7f) << bitShift;
                bitShift += 7;

                if ((nextByte & 0x80) == 0)
                    return result;
            }
        }

        /// <summary>
        /// Reads a 7-bit encoded integer from the stream. This is stored with the least significant
        /// information first, with 7 bits of information per byte of value, and the top
        /// bit as a continuation flag. This method is not affected by the endianness
        /// of the bit converter. https://github.com/antmicro/Packet.Net/blob/master/PacketDotNet/MiscUtil/IO/EndianBinaryReader.cs
        /// </summary>
        /// <returns>The 7-bit encoded integer read from the stream.</returns>
        private static int ReadLittleEndian7BitEncodedInt(ISequenceReader reader)
        {
            //CheckDisposed();
            int ret = 0;

            for (int shift = 0; shift < 35; shift += 7)
            {
                int b = reader.ReadByte();
                //if (b == -1)
                //{
                //    throw new EndOfStreamException();
                //}
                ret = ret | ((b & 0x7f) << shift);

                if ((b & 0x80) == 0)
                    return ret;
            }

            // Still haven't seen a byte with the high bit unset? Dodgy data.
            throw new System.IO.IOException("Invalid 7-bit encoded integer in stream.");
        }

        private static int Read7BitEncodedIntNET6(ISequenceReader reader) // From .NET 6
        {
            // Unlike writing, we can't delegate to the 64-bit read on
            // 64-bit platforms. The reason for this is that we want to
            // stop consuming bytes if we encounter an integer overflow.

            uint result = 0;
            byte byteReadJustNow;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 5 bytes,
            // or the fifth byte is about to cause integer overflow.
            // This means that we can read the first 4 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 4;

            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
            {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = reader.ReadByte();
                result |= (byteReadJustNow & 0x7Fu) << shift;

                if (byteReadJustNow <= 0x7Fu)
                    return (int)result; // early exit
            }

            // Read the 5th byte. Since we already read 28 bits,
            // the value of this byte must fit within 4 bits (32 - 28),
            // and it must not have the high bit set.

            byteReadJustNow = reader.ReadByte();

            if (byteReadJustNow > 0b_1111u)
                throw new Exception("Bad 7-bit Int");

            result |= (uint)byteReadJustNow << (MaxBytesWithoutOverflow * 7);

            return (int)result;
        }

        private static long Read7BitEncodedInt64NET6(ISequenceReader reader) // From .NET 6
        {
            ulong result = 0;
            byte byteReadJustNow;

            // Read the integer 7 bits at a time. The high bit
            // of the byte when on means to continue reading more bytes.
            //
            // There are two failure cases: we've read more than 10 bytes,
            // or the tenth byte is about to cause integer overflow.
            // This means that we can read the first 9 bytes without
            // worrying about integer overflow.

            const int MaxBytesWithoutOverflow = 9;

            for (int shift = 0; shift < MaxBytesWithoutOverflow * 7; shift += 7)
            {
                // ReadByte handles end of stream cases for us.
                byteReadJustNow = reader.ReadByte();
                result |= (byteReadJustNow & 0x7Ful) << shift;

                if (byteReadJustNow <= 0x7Fu)
                    return (long)result; // early exit
            }

            // Read the 10th byte. Since we already read 63 bits,
            // the value of this byte must fit within 1 bit (64 - 63),
            // and it must not have the high bit set.

            byteReadJustNow = reader.ReadByte();

            if (byteReadJustNow > 0b_1u)
                throw new Exception("Bad 7-bit Int");

            result |= (ulong)byteReadJustNow << (MaxBytesWithoutOverflow * 7);

            return (long)result;
        }
    }
}
