using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;

namespace Simple.Serialization
{
	public class BinaryBigEndianReader : BinaryEndiannessReader
	{
		ISequenceReader reader;

		public BinaryBigEndianReader(ISequenceReader reader) => this.reader = reader;

		//public long Position => this.reader.Position;

		//public override object GetCurrentPositionToken() => this.reader.GetCurrentPositionToken();
		//public override long Length => this.reader.Length;
		public override ISequenceReader Provider => this.reader;

		public override byte ReadByte() => this.reader.ReadByte();

        public override byte[] ReadByteArray(int count, out int offset) => this.reader.ReadByteArray(count, out offset);
		//public override byte[] ReadBytes(int count) => this.reader.ReadBytes(count);
		//public override int ReadToBuffer(byte[] buffer, int offset, int count) => this.reader.ReadToBuffer(buffer, offset, count);

		public override short ReadInt16() =>  (Int16)(this.ReadByte() << 8 | this.ReadByte());
		public override int ReadInt32() => (Int32)(this.ReadByte() << 24 | this.ReadByte() << 16 | this.ReadByte() << 8 | this.ReadByte());
		public override long ReadInt64() => (Int64)((long)this.ReadByte() << 56 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 32 |
													(long)this.ReadByte() << 24 | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 8  | this.ReadByte());

		public override ushort ReadUInt16() => (UInt16) (this.ReadByte() << 8 | this.ReadByte());
		public override uint ReadUInt32() => (UInt32)(this.ReadByte() << 24 | this.ReadByte() << 16 | this.ReadByte() << 8 | this.ReadByte());

		public override ulong ReadUInt64() => (ulong)this.ReadByte() << 56 | (ulong)this.ReadByte() << 48 | (ulong)this.ReadByte() << 40 | (ulong)this.ReadByte() << 32 | 
											  (ulong)this.ReadByte() << 24 | (ulong)this.ReadByte() << 16 | (ulong)this.ReadByte() << 8  | this.ReadByte();

		[SecuritySafeCritical]
		public override unsafe float ReadSingle()
		{
			Int32 raw = this.ReadByte() << 24 | this.ReadByte() << 16 | this.ReadByte() << 8 | this.ReadByte();
		
			return *(Single*)&raw;
		}

		[SecuritySafeCritical]
		public override unsafe double ReadDouble()
		{
			Int64 raw = (long)this.ReadByte() << 56 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 32 | 
						(long)this.ReadByte() << 24 | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 8  | this.ReadByte();
			
			return *(Double*)&raw;
		}

		public override ulong Read7BitEncodedUInt64() => ReadBigEndian7BitEncodedUInt64(this.reader);


		public static ulong ReadBigEndian7BitEncodedUInt64(ISequenceReader reader)
		{
			ulong result = 0;
            ulong nextByte;
			var bitShift = 0;

            do
            {
                nextByte = reader.ReadByte();
                result |= (nextByte & 0x7f) << bitShift;
                bitShift += 7;
            }
            while ((nextByte & 0x80) != 0);

            return result;
		}

		public override void ResetPosition() => this.reader.ResetPosition();

		/// <summary>
		/// Reads a 7-bit encoded integer from the stream. This is stored with the most significant
		/// information first, with 7 bits of information per byte of value, and the top
		/// bit as a continuation flag. This method is not affected by the endianness
		/// of the bit converter. https://github.com/antmicro/Packet.Net/blob/master/PacketDotNet/MiscUtil/IO/EndianBinaryReader.cs
		/// </summary>
		/// <returns>The 7-bit encoded integer read from the stream.</returns>
		private static int ReadBigEndian7BitEncodedInt(ISequenceReader reader)
        {
            //CheckDisposed();
            int result = 0;

            //for (int i = 0; i < 5; i++)
			while (true)
            {
                int nextByte = reader.ReadByte();
                //if (nextByte == -1)
                //{
                //    throw new EndOfStreamException();
                //}
                result = (result << 7) | (nextByte & 0x7f);

                if ((nextByte & 0x80) == 0)
                    return result;
            }

            // Still haven't seen a byte with the high bit unset? Dodgy data.
            //throw new System.IO.IOException("Invalid 7-bit encoded integer in stream.");
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
                throw new Exception ("Bad 7-bit Int");

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
