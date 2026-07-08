using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Simple.Serialization
{
	/// <summary>
	/// The BitStream class that can Read or Write bit segments from/into underlying Stream
	/// </summary>
	public class BitStream
	{
		private Stream baseStream = null;
		private long baseStreamPosition = 0;
		private uint bitBuffer = 0;
		private int position = 0;
		private bool requireUpdate = false;

		private static uint[] NumOfBitsMask = new uint[9];
		//private static byte[] InverseNumOfBitsMask = new byte[9];

		static BitStream()
		{
			NumOfBitsMask[0] = 0x00; // == 00000000
			NumOfBitsMask[1] = 0x01; // == 00000001
			NumOfBitsMask[2] = 0x03; // == 00000011
			NumOfBitsMask[3] = 0x07; // == 00000111
			NumOfBitsMask[4] = 0x0F; // == 00001111
			NumOfBitsMask[5] = 0x1F; // == 00011111
			NumOfBitsMask[6] = 0x3F; // == 00111111
			NumOfBitsMask[7] = 0x7F; // == 01111111
			NumOfBitsMask[8] = 0xFF; // == 11111111

			//InverseNumOfBitsMask[0] = 0xFF; // == 11111111
			//InverseNumOfBitsMask[1] = 0x7F; // == 01111111
			//InverseNumOfBitsMask[2] = 0x3F; // == 00111111
			//InverseNumOfBitsMask[3] = 0x1F; // == 00011111
			//InverseNumOfBitsMask[4] = 0x0F; // == 00001111
			//InverseNumOfBitsMask[5] = 0x07; // == 00000111
			//InverseNumOfBitsMask[6] = 0x03; // == 00000011
			//InverseNumOfBitsMask[7] = 0x01; // == 00000001
			//InverseNumOfBitsMask[8] = 0x00; // == 00000000

			//NumOfBitsMask[0] = 0xFF; // == 11111111
			//NumOfBitsMask[1] = 0x7F; // == 11111110
			//NumOfBitsMask[2] = 0x3F; // == 11111100
			//NumOfBitsMask[3] = 0x1F; // == 11111000
			//NumOfBitsMask[4] = 0x0F; // == 11110000
			//NumOfBitsMask[5] = 0x07; // == 11100000
			//NumOfBitsMask[6] = 0x03; // == 11000000
			//NumOfBitsMask[7] = 0x01; // == 10000000
			//NumOfBitsMask[8] = 0x00; // == 00000000

			//InverseNumOfBitsMask[0] = 0x00; // == 00000000
			//InverseNumOfBitsMask[1] = 0x80; // == 10000000
			//InverseNumOfBitsMask[2] = 0xC0; // == 11000000
			//InverseNumOfBitsMask[3] = 0x1F; // == 11100000
			//InverseNumOfBitsMask[4] = 0xE0; // == 11110000
			//InverseNumOfBitsMask[5] = 0xF8; // == 11111000
			//InverseNumOfBitsMask[6] = 0xFC; // == 11111100
			//InverseNumOfBitsMask[7] = 0xFE; // == 11111110
			//InverseNumOfBitsMask[8] = 0xFF; // == 11111111
		}

		public BitStream(Stream baseStream)
		{
			this.baseStream = baseStream;
		}

		/// <summary>
		/// Gets the underlying stream.
		/// </summary>
		public Stream BaseStream
		{
			get { return this.baseStream; }
		}

		///// <summary>
		///// Reads one bit from bit stream as a Boolean.
		///// </summary>
		///// <returns>The Boolean value true if readen bit is 1; otherwise false</returns>
		//public bool ReadBolean()
		//{
		//	return this.Read(1) == 1;
		//}

		/// <summary>
		/// Reads data bits from the bit stream.
		/// Number of bits is specified by count property.
		/// </summary>
		/// <param name="count">Number of bits to be read. Available value range is: 1-8.</param>
		/// <returns>The bit sequence stored in byte.</returns>
		public byte Read(int count) // Packing order of bit segments: | 5 | 4  | 3|2| 1 |  (The 1 is first written bit segment, 2 is second and so on.
		{
			uint result;

			if (this.position == 0)
			{
				this.bitBuffer = unchecked((uint)this.baseStream.ReadByte()); // bitBuffer remains the same, only position is shifted
				result = this.bitBuffer & NumOfBitsMask[count];

				this.position = (count < 8) ? count : 0;
			}
			else if (this.position + count > 8) // not enough bits in buffer -> read new byte from stream
			{
				this.bitBuffer |= unchecked((uint)this.baseStream.ReadByte()) << 8; // read new byte from stream and place them to be higher 2nd byte
				result = (this.bitBuffer >> this.position) & NumOfBitsMask[count];

				this.bitBuffer >>= 8;
				this.position += count - 8;
			}
			else
			{
				result = (this.bitBuffer >> this.position) & NumOfBitsMask[count];
				this.position += count;
			}

			return unchecked((byte)result);
		}

		//public void WriteBoolean(bool value)
		//{
		//	this.Write(value ? (byte)1 : (byte)0, 1);
		//}

		/// <summary>
		/// Writes data bits to the bit stream.
		/// Number of bits writen from data is specified by count property.
		/// The data should NOT excceed the max value possible by number of bit specified by count.
		/// </summary>
		/// <param name="data">The bits data to be writen.</param>
		/// <param name="count">Number of bits from data to be written. Available value range is: 1-8.</param>
		public void Write(byte data, int count) // Packing order of bit segments: | 5 | 4  | 3|2| 1 |  (The 1 is first written bit segment, 2 is second and so on.
		{
			if (this.position == 0)
			{
				this.bitBuffer = unchecked(data);
				this.position = count;
				this.baseStreamPosition = this.baseStream.Position;
				this.baseStream.WriteByte(data);

				if (this.position == 8)
					this.position = 0;
			}
			else
			{
				this.bitBuffer |= (unchecked((uint)data) << this.position);
				this.position += count;

				if (this.position == 8)
				{
					this.UpdateInternal();
					this.position = 0;
				}
				else if (this.position > 8) // byte overflow, update bit buffer where it is placed inside base stream
				{
					// Update bitBuffer data at the position where it is first written in a BaseStream 
					this.baseStream.Position = this.baseStreamPosition;
					this.baseStream.WriteByte(unchecked((byte)this.bitBuffer));
					this.baseStream.Position = this.baseStream.Length;

					this.bitBuffer >>= 8;

					// Insert new bitBuffer data into the BaseStream
					this.baseStreamPosition = this.baseStream.Length;
					this.baseStream.WriteByte(unchecked((byte)this.bitBuffer));

					this.position -= 8;
					this.requireUpdate = false;
				}
				else
				{
					this.requireUpdate = true;
					this.UpdateInternal();
				}
			}
		}

		/// <summary>
		/// Enforce any buffered data to be written to the underlying base stream.
		/// </summary>
		public void Update()
		{
			if (this.requireUpdate)
				this.UpdateInternal();
		}

		/// <summary>
		/// Enforce any buffered data to be written to the underlying base stream wihout checking if this.requireFlush is true.
		/// </summary>
		private void UpdateInternal()
		{
			long currentStreamPosition = this.baseStream.Position;

			this.baseStream.Position = this.baseStreamPosition;
			this.baseStream.WriteByte((byte)this.bitBuffer);
			this.baseStream.Position = currentStreamPosition;

			this.requireUpdate = false;
		}

		/// <summary>
		/// Resets the internal buffer and bit position.
		/// </summary>
		public void Reset()
		{
			this.bitBuffer = 0;
			this.position = 0;
		}
	}

	class BitStreamTest
	{
		static void BitBaseStreamReaderWriterTest(string[] args)
		{
			byte[] result;

			//FileStream fs = new FileStream();
			//fs.set
			MemoryStream stream = new MemoryStream(5);
			BitStream bitReaderWriter = new BitStream(stream);
			List<byte> readData = new List<byte>();
			List<byte> readDataFromBitStream = new List<byte>();
			//byte flag = 20;
			//int position;

			stream.WriteByte(0);
			//position = (int)stream.Length;
			//stream.WriteByte(flag);
			stream.WriteByte(1);
			stream.WriteByte(2);

			bitReaderWriter.Write(1, 1);

			stream.WriteByte(3);
			stream.WriteByte(4);
			stream.WriteByte(5);

			bitReaderWriter.Write(3, 2);

			stream.WriteByte(6);

			bitReaderWriter.Write(2, 4);

			stream.WriteByte(7);
			stream.WriteByte(8);

			bitReaderWriter.Write(5, 3);

			stream.WriteByte(9);
			stream.WriteByte(10);

			bitReaderWriter.Write(3, 3);

			stream.WriteByte(11);

			bitReaderWriter.Update();

			result = stream.ToArray();

			bitReaderWriter.Reset();
			stream.Position = 0;

			readData.Add((byte)stream.ReadByte()); // 0
			readData.Add((byte)stream.ReadByte()); // 1
			readData.Add((byte)stream.ReadByte()); // 2

			readDataFromBitStream.Add(bitReaderWriter.Read(1)); // 1

			readData.Add((byte)stream.ReadByte()); // 3
			readData.Add((byte)stream.ReadByte()); // 4
			readData.Add((byte)stream.ReadByte()); // 5

			readDataFromBitStream.Add(bitReaderWriter.Read(2)); // 3

			readData.Add((byte)stream.ReadByte()); // 6

			readDataFromBitStream.Add(bitReaderWriter.Read(4)); // 2

			readData.Add((byte)stream.ReadByte()); // 7
			readData.Add((byte)stream.ReadByte()); // 8

			readDataFromBitStream.Add(bitReaderWriter.Read(3)); // 5

			readData.Add((byte)stream.ReadByte()); // 9
			readData.Add((byte)stream.ReadByte()); // 10

			readDataFromBitStream.Add(bitReaderWriter.Read(3)); // 3

			readData.Add((byte)stream.ReadByte()); // 11

			//flag = 40;

			//stream.Position = position;

			//stream.Write(new byte[] { flag }, 0, 1);
			//stream.Position = stream.Length;

			//stream.WriteByte(6);

			return;
		}
	}
}
