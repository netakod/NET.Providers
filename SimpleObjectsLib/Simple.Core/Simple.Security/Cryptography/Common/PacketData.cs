using System;
using System.Collections.Generic;
#if !TUNING
using System.Linq;
#endif
using System.Text;
using System.Globalization;
using Simple;

namespace Simple.Security
{
	/// <summary>
	/// Base packet data serialization type
	/// </summary>
	public abstract class PacketData
	{
		public const int DefaultCapacity = 64;
		public static readonly Encoding Ascii = new ASCIIEncoding();

#if SILVERLIGHT
        public static readonly Encoding Utf8 = Encoding.UTF8;
#else
		public static readonly Encoding Utf8 = Encoding.Default;
#endif

#if TUNING
		private DataStream dataStream;

		protected DataStream DataStream
		{
			get { return dataStream; }
		}
#else
        /// <summary>
        /// Data byte array that hold message unencrypted data
        /// </summary>
        private List<byte> data;
        private int readerIndex;
#endif

		private byte[] loadedData;
#if TUNING
		private int offset;
#endif

		/// <summary>
		/// Gets a value indicating whether all data from the buffer has been read.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is end of data; otherwise, <c>false</c>.
		/// </value>
		protected bool IsEndOfData
		{
			get
			{
#if TUNING
				return this.dataStream.Position >= this.dataStream.Length;
#else
                return this.readerIndex >= data.Count();
#endif
			}
		}

		/// <summary>
		/// Gets the index that represents zero in current data type.
		/// </summary>
		/// <value>
		/// The index of the zero reader.
		/// </value>
		protected virtual int ZeroReaderIndex
		{
			get { return 0; }
		}

#if TUNING
		/// <summary>
		/// Gets the size of the message in bytes.
		/// </summary>
		/// <value>
		/// The size of the messages in bytes.
		/// </value>
		protected virtual int BufferCapacity
		{
			get { return 0; }
		}
#endif

		/// <summary>
		/// Gets data bytes array
		/// </summary>
		/// <returns>Byte array representation of data structure.</returns>
		public
#if !TUNING
        virtual
#endif
		byte[] GetBytes()
		{
#if TUNING
			var messageLength = this.BufferCapacity;
			var capacity = messageLength != -1 ? messageLength : PacketData.DefaultCapacity;
			var dataStream = new DataStream(capacity);

			this.WriteBytes(this.dataStream);

			return this.dataStream.ToArray();
#else
            this.data = new List<byte>();
            this.SaveData();

            return this.data.ToArray();
#endif
		}

#if TUNING
		/// <summary>
		/// Writes the current message to the specified <see cref="SshDataStream"/>.
		/// </summary>
		/// <param name="stream">The <see cref="SshDataStream"/> to write the message to.</param>
		protected virtual void WriteBytes(DataStream stream)
		{
			this.dataStream = stream;
			SaveData();
		}
#endif

		internal T OfType<T>() where T : PacketData, new()
		{
			var result = new T();
#if TUNING
			result.LoadBytes(this.loadedData, this.offset);
#else
            result.LoadBytes(this.loadedData);
#endif
			result.LoadData();
			return result;
		}

		/// <summary>
		/// Loads data from specified bytes.
		/// </summary>
		/// <param name="value">Bytes array.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
		public void Load(byte[] value)
		{
#if TUNING
			this.Load(value, 0);
#else
            if (value == null)
                throw new ArgumentNullException("value");

            LoadBytes(value);
            LoadData();
#endif
		}

#if TUNING
		/// <summary>
		/// Loads data from the specified buffer.
		/// </summary>
		/// <param name="value">Bytes array.</param>
		/// <param name="offset">The zero-based offset in <paramref name="value"/> at which to begin reading SSH data.</param>
		/// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
		public void Load(byte[] value, int offset)
		{
			this.LoadBytes(value, offset);
			this.LoadData();
		}
#endif

		/// <summary>
		/// Called when type specific data need to be loaded.
		/// </summary>
		protected abstract void LoadData();

		/// <summary>
		/// Called when type specific data need to be saved.
		/// </summary>
		protected abstract void SaveData();

		/// <summary>
		/// Loads data bytes into internal buffer.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
		protected void LoadBytes(byte[] bytes)
		{
#if TUNING
			this.LoadBytes(bytes, 0);
#else
            // Note about why I check for null here, and in Load(byte[]) in this class.
            // This method is called by several other classes, such as SshNet.Messages.Message, SshNet.Sftp.SftpMessage.
            if (bytes == null)
                throw new ArgumentNullException("bytes");

            this.ResetReader();
            this.loadedData = bytes;
            this.data = new List<byte>(bytes);
#endif
		}

#if TUNING
		/// <summary>
		/// Loads data bytes into internal buffer.
		/// </summary>
		/// <param name="bytes">The bytes.</param>
		/// <param name="offset">The zero-based offset in <paramref name="bytes"/> at which to begin reading SSH data.</param>
		/// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
		protected void LoadBytes(byte[] bytes, int offset)
		{
			if (bytes == null)
				throw new ArgumentNullException("bytes");

			this.loadedData = bytes;
			this.offset = offset;

			this.dataStream = new DataStream(bytes);
			this.ResetReader();
		}
#endif

		/// <summary>
		/// Resets internal data reader index.
		/// </summary>
		protected void ResetReader()
		{
#if TUNING
			this.dataStream.Position = ZeroReaderIndex + this.offset;
#else
            this.readerIndex = ZeroReaderIndex;  //  Set to 1 to skip first byte which specifies message type
#endif
		}

		/// <summary>
		/// Reads all data left in internal buffer at current position.
		/// </summary>
		/// <returns>An array of bytes containing the remaining data in the internal buffer.</returns>
		protected byte[] ReadBytes()
		{
#if TUNING
			var bytesLength = (int)(dataStream.Length - dataStream.Position);
			var data = new byte[bytesLength];
			this.dataStream.Read(data, 0, bytesLength);
			return data;
#else
            var data = new byte[this.data.Count - readerIndex];
            this.data.CopyTo(readerIndex, data, 0, data.Length);
            return data;
#endif
		}

		/// <summary>
		/// Reads next specified number of bytes data type from internal buffer.
		/// </summary>
		/// <param name="length">Number of bytes to read.</param>
		/// <returns>An array of bytes that was read from the internal buffer.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is greater than the internal buffer size.</exception>
		protected byte[] ReadBytes(int length)
		{
			// Note that this also prevents allocating non-relevant lengths, such as if length is greater than _data.Count but less than int.MaxValue.
			// For the nerds, the condition translates to: if (length > data.Count && length < int.MaxValue)
			// Which probably would cause all sorts of exception, most notably OutOfMemoryException.

#if TUNING
			var data = new byte[length];
			var bytesRead = this.dataStream.Read(data, 0, length);

			if (bytesRead < length)
				throw new ArgumentOutOfRangeException("length");

			return data;
#else
            if (length > this.data.Count)
                throw new ArgumentOutOfRangeException("length");

            var result = new byte[length];
            this.data.CopyTo(readerIndex, result, 0, length);
            this.readerIndex += length;
            return result;
#endif
		}

		/// <summary>
		/// Reads next byte data type from internal buffer.
		/// </summary>
		/// <returns>Byte read.</returns>
		protected byte ReadByte()
		{
#if TUNING
			var byteRead = this.dataStream.ReadByte();

			if (byteRead == -1)
				throw new InvalidOperationException("Attempt to read past the end of the packet data stream.");

			return (byte)byteRead;
#else
            return this.ReadBytes(1).FirstOrDefault();
#endif
		}

		/// <summary>
		/// Reads next boolean data type from internal buffer.
		/// </summary>
		/// <returns>Boolean read.</returns>
		protected bool ReadBoolean()
		{
			return this.ReadByte() != 0;
		}

		/// <summary>
		/// Reads next uint16 data type from internal buffer.
		/// </summary>
		/// <returns>uint16 read</returns>
		protected ushort ReadUInt16()
		{
			var data = this.ReadBytes(2);
			return (ushort)(data[0] << 8 | data[1]);
		}

		/// <summary>
		/// Reads next uint32 data type from internal buffer.
		/// </summary>
		/// <returns>uint32 read</returns>
		protected uint ReadUInt32()
		{
			var data = this.ReadBytes(4);
			return (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
		}

		/// <summary>
		/// Reads next uint64 data type from internal buffer.
		/// </summary>
		/// <returns>uint64 read</returns>
		protected ulong ReadUInt64()
		{
			var data = this.ReadBytes(8);
			return ((ulong)data[0] << 56 | (ulong)data[1] << 48 | (ulong)data[2] << 40 | (ulong)data[3] << 32 | (ulong)data[4] << 24 | (ulong)data[5] << 16 | (ulong)data[6] << 8 | data[7]);
		}

#if !TUNING
        /// <summary>
        /// Reads next string data type from internal buffer.
        /// </summary>
        /// <returns>string read</returns>
        protected string ReadAsciiString()
        {
            var length = this.ReadUInt32();

            if (length > int.MaxValue)
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));

            return Ascii.GetString(this.ReadBytes((int)length), 0, (int)length);
        }
#endif

		/// <summary>
		/// Reads next string data type from internal buffer.
		/// </summary>
		/// <returns>string read</returns>
		protected string ReadString()
		{
			return this.ReadString(Utf8);
		}

		/// <summary>
		/// Reads next string data type from internal buffer.
		/// </summary>
		/// <returns>string read</returns>
		protected string ReadString(Encoding encoding)
		{
#if TUNING
			return this.dataStream.ReadString(encoding);
#else
            var length = this.ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }
            return encoding.GetString(ReadBytes((int)length), 0, (int)length);
#endif
		}

#if TUNING
		/// <summary>
		/// Reads next data type as byte array from internal buffer.
		/// </summary>
		/// <returns>
		/// The bytes read.
		/// </returns>
		protected byte[] ReadBinary()
		{
			return this.dataStream.ReadBinary();
		}
#else
        /// <summary>
        /// Reads next string data type from internal buffer.
        /// </summary>
        /// <returns>string read</returns>
        protected byte[] ReadBinaryString()
        {
            var length = this.ReadUInt32();

            if (length > int.MaxValue)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Strings longer than {0} is not supported.", int.MaxValue));
            }

            return this.ReadBytes((int)length);
        }
#endif

		/// <summary>
		/// Reads next name-list data type from internal buffer.
		/// </summary>
		/// <returns>String array or read data..</returns>
		protected string[] ReadNamesList()
		{
			var namesList = this.ReadString();
			return namesList.Split(',');
		}

		/// <summary>
		/// Reads next extension-pair data type from internal buffer.
		/// </summary>
		/// <returns>Extensions pair dictionary.</returns>
		protected IDictionary<string, string> ReadExtensionPair()
		{
			var result = new Dictionary<string, string>();

			while (!this.IsEndOfData)
			{
				var extensionName = this.ReadString();
				var extensionData = this.ReadString();

				result.Add(extensionName, extensionData);
			}

			return result;
		}

#if !TUNING

        protected BigInteger ReadBigInt()
        {
            int count = (int)this.ReadUInt32();
            byte[] bytes = this.ReadBytes(count);

            return new BigInteger(bytes);
        }

        protected void WriteAscii(string s)
        {
            this.WriteBinaryString(Ascii.GetBytes(s));
        }
#endif


#if TUNING
		/// <summary>
		/// Writes bytes array data into internal buffer.
		/// </summary>
		/// <param name="data">Byte array data to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
		protected void Write(byte[] data)
		{
			this.dataStream.Write(data);
		}
#else
        /// <summary>
        /// Writes bytes array data into internal buffer.
        /// </summary>
        /// <param name="data">Byte array data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        protected void Write(IEnumerable<byte> data)
        {
            this.data.AddRange(data);
        }
#endif

#if TUNING
		/// <summary>
		/// Writes a sequence of bytes to the current SSH data stream and advances the current position
		/// within this stream by the number of bytes written.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method write <paramref name="count"/> bytes from buffer to the current SSH data stream.</param>
		/// <param name="offset">The zero-based offset in <paramref name="buffer"/> at which to begin writing bytes to the SSH data stream.</param>
		/// <param name="count">The number of bytes to be written to the current SSH data stream.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		protected void Write(byte[] buffer, int offset, int count)
		{
			this.dataStream.Write(buffer, offset, count);
		}
#endif

		/// <summary>
		/// Writes byte data into internal buffer.
		/// </summary>
		/// <param name="data">Byte data to write.</param>
		protected void Write(byte data)
		{
#if TUNING
			this.dataStream.WriteByte(data);
#else
            this.data.Add(data);
#endif
		}

		/// <summary>
		/// Writes boolean data into internal buffer.
		/// </summary>
		/// <param name="data">Boolean data to write.</param>
		protected void Write(bool data)
		{
			this.Write(data ? (byte)1 : (byte)0);
		}

		/// <summary>
		/// Writes uint32 data into internal buffer.
		/// </summary>
		/// <param name="data">uint32 data to write.</param>
		protected void Write(uint data)
		{
#if TUNING
			this.dataStream.Write(data);
#else
            this.Write(data.GetBytes());
#endif
		}

		/// <summary>
		/// Writes uint64 data into internal buffer.
		/// </summary>
		/// <param name="data">uint64 data to write.</param>
		protected void Write(ulong data)
		{
#if TUNING
			this.dataStream.Write(data);
#else
            this.Write(data.GetBytes());
#endif
		}

		/// <summary>
		/// Writes string data into internal buffer using default encoding.
		/// </summary>
		/// <param name="data">string data to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
		protected void Write(string data)
		{
			this.Write(data, Utf8);
		}

		/// <summary>
		/// Writes string data into internal buffer using the specified encoding.
		/// </summary>
		/// <param name="data">string data to write.</param>
		/// <param name="encoding">The character encoding to use.</param>
		/// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="encoding"/> is null.</exception>
		protected void Write(string data, Encoding encoding)
		{
#if TUNING
			this.dataStream.Write(data, encoding);
#else
            if (data == null)
                throw new ArgumentNullException("data");
            if (encoding == null)
                throw new ArgumentNullException("encoding");

            var bytes = encoding.GetBytes(data);

            this.Write((uint)bytes.Length);
            this.Write(bytes);
#endif
		}

#if TUNING
		/// <summary>
		/// Writes data into internal buffer.
		/// </summary>
		/// <param name="buffer">The data to write.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
		protected void WriteBinaryString(byte[] buffer)
		{
			this.dataStream.WriteBinary(buffer);
		}

		/// <summary>
		/// Writes data into internal buffer.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method write <paramref name="count"/> bytes from buffer to the current SSH data stream.</param>
		/// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which to begin writing bytes to the SSH data stream.</param>
		/// <param name="count">The number of bytes to be written to the current SSH data stream.</param>
		/// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
		/// <exception cref="ArgumentException">The sum of <paramref name="offset"/> and <paramref name="count"/> is greater than the buffer length.</exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> or <paramref name="count"/> is negative.</exception>
		protected void WriteBinary(byte[] buffer, int offset, int count)
		{
			this.dataStream.WriteBinary(buffer, offset, count);
		}
#else
        /// <summary>
        /// Writes string data into internal buffer.
        /// </summary>
        /// <param name="data">string data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="data"/> is null.</exception>
        protected void WriteBinaryString(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            this.Write((uint)data.Length);
            this.data.AddRange(data);
        }
#endif

		/// <summary>
		/// Writes mpint data into internal buffer.
		/// </summary>
		/// <param name="data">mpint data to write.</param>
		protected void Write(BigInteger data)
		{
#if TUNING
			this.dataStream.Write(data);
#else
            var bytes = data.ToByteArray().Reverse().ToList();
            this.Write((uint)bytes.Count);
            this.Write(bytes);
#endif
		}

		/// <summary>
		/// Writes name-list data into internal buffer.
		/// </summary>
		/// <param name="data">name-list data to write.</param>
		protected void Write(string[] data)
		{
			this.Write(string.Join(",", data), Ascii);
		}

		/// <summary>
		/// Writes extension-pair data into internal buffer.
		/// </summary>
		/// <param name="data">extension-pair data to write.</param>
		protected void Write(IDictionary<string, string> data)
		{
			foreach (var item in data)
			{
				this.Write(item.Key, Ascii);
				this.Write(item.Value, Ascii);
			}
		}
	}
}
