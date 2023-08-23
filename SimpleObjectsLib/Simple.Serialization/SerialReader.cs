using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.Serialization;
using System.Buffers;
using System.Threading.Tasks;
using System.Security;
using System.Diagnostics;
using Simple;
using Simple.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
//using System.Runtime.InteropServices.ComTypes;
//using System.Runtime.CompilerServices;
//using System.Reflection.Metadata.Ecma335;

namespace Simple.Serialization
{
	/// <summary>
	///  Read serialized primitive data types as binary values in a specific encoding.
	/// A SerializationReader is used to read stored values and objects from byte array.
	/// Once an instance is created, use the various methods to read the required data.
	/// The data read MUST be exactly the same type and in the same order as it was written.
	/// </summary>
	public class SerialReader //: ISerialReader
	{
        #region |   Private Members   |

        //private byte[]? charBytes;
        private char[]? charBuffer = null;
		//private Decoder decoder;
		private bool twoBytesPerChar;

		private const int MaxCharBytesSize = 128;
		private readonly int maxCharsSize;  // From MaxCharBytesSize & Encoding

		//private ISequenceReader baseReader;
		private BinaryEndiannessReader reader;
		//private BinaryReader reader = null;
		private BitReader bitReader;
		private Encoding characterEncoding;
		private List<string>? stringTokenList = null;
		private bool useStringLookupOptimization = false;

		private static readonly BitArray FullyOptimizableTypedArray = new BitArray(0);
		private static HashArray<Func<SerialReader, object?>> ReadFuncsByTypeId		   = new HashArray<Func<SerialReader, object?>>(PropertyTypes.Count);
		private static HashArray<Func<SerialReader, object?>> ReadOptimizedFuncsByTypeId = new HashArray<Func<SerialReader, object?>>(PropertyTypes.Count);


		#endregion |   Private Members   |

		#region |   Public Static Members   |

		/// <summary>
		/// Gets the default character encoding.
		/// </summary>
		public static readonly Encoding DefaultEncoding = SerialWriter.DefaultEncoding; // Encoding.UTF8; //new UTF8Encoding(false);

		#endregion |   Public Static Members   |

		#region |   Constructors and Initialization   |

		///// <summary>
		///// Initializes a new instance of the <see cref="SimpleReader"/> class with an initial <see cref="MemoryStream"/> expandable capacity initialized to zero.
		///// </summary>
		//public SerializationReaderNew()
		//	: this(new StreamReader(new MemoryStream()))r
		//{
		//}

		///// <summary>
		///// Initializes a new instance of the <see cref="SimpleReader"/> class with an initial <see cref="MemoryStream"/> with initial capacity as specified.
		///// </summary>
		///// <param name="capacity">The initial size of the internal array in bytes.</param>
		//public SerializationReaderNew(int capacity)
		//	: this(new MemoryStream(capacity))
		//{
		//}

		///// <summary>
		///// Creates a SerializationReader using a byte[] previous created by SerializationWriter
		///// A MemoryStream is used to access the data without making a copy of it.
		///// </summary>
		///// <param name="data">The byte[] containining serialized data.</param>
		//public SerializationReader(byte[] data) : this(new MemoryStream(data)) { }

		//public SerializationReader(ReadOnlySequence<byte> sequence)
		//	: this(new MemoryStream(sequence.ToArray()))
		//{
		//}

		public SerialReader(byte[] data) 
			: this(data, DefaultEncoding) 
		{ 
		}

		public SerialReader(byte[] data, Encoding characterEncoding) 
			: this(new BufferSequenceReader(new BufferSegment<byte>(data)), characterEncoding) 
		{ 
		}

		public SerialReader(BufferSequenceReader bufferSequence) 
			: this(bufferSequence, DefaultEncoding) 
		{ 
		}

		public SerialReader(BufferSequenceReader bufferSequence, Encoding characterEncoding) 
			: this((ISequenceReader)bufferSequence, characterEncoding) 
		{ 
		}

		//public SerializationReader(ReadOnlySequence<byte> sequence) : this(PipeReader.Create(sequence).AsStream()) { }

		//public SerializationReader(ref SequenceReader<byte> sequence) : this(new SequenceReader(ref sequence)) { }

		//public SerializationReader(ReadOnlySequence<byte> sequence)
		//	: this(PipeReader.Create(sequence).AsStream(), encoding)
		//{
		//}
		public SerialReader(List<ArraySegment<byte>> arraySegmentList) 
			: this(arraySegmentList, DefaultEncoding) 
		{ 
		}


		public SerialReader(List<ArraySegment<byte>> arraySegmentList, Encoding characterEncoding) 
			: this((ISequenceReader)new ArraySegmentListReader(arraySegmentList), characterEncoding) 
		{ 
		}

		

		public SerialReader(ref ReadOnlySequence<byte> readOnlySequence) 
			: this(ref readOnlySequence, DefaultEncoding) 
		{ 
		}


		public SerialReader(ref ReadOnlySequence<byte> readOnlySequence, Encoding characterEncoding) 
			: this((ISequenceReader)new ReadOnlySequenceReader(ref readOnlySequence), characterEncoding) 
		{ 
		}
		//{
		//	//var sequence = new SequenceReader<byte>(buffer);

		//	this.Initialize(new SequenceReader(buffer));
		//}
		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleReader"/> class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> 
		/// and a specific character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="Simple.Serialization.BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as a underlying.
		/// </summary>
		/// <param name="stream">The stream from which to read data.</param>
		/// <param name="encoding">The character encoding.</param>
		public SerialReader(Stream stream) 
			: this(stream, DefaultEncoding) 
		{ 
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleReader"/> class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> 
		/// and a specific character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="Simple.Serialization.BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as a underlying.
		/// </summary>
		/// <param name="stream">The stream from which to read data.</param>
		/// <param name="encoding">The character encoding.</param>
		public SerialReader(Stream stream, Encoding characterEncoding) 
			: this((ISequenceReader)new StreamSequenceReader(stream), characterEncoding) 
		{ 
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleReader"/> class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> 
		/// and a specific character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="Simple.Serialization.BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as a underlying.
		/// </summary>
		/// <param name="sequenceReader"></param>
		public SerialReader(ISequenceReader sequenceReader) 
			: this(sequenceReader, DefaultEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialReader"/> that serialize class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> 
		/// and a specific character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as a underlying.
		/// </summary>
		/// <param name="sequenceReader">The sequence reader from which to read data.</param>
		/// <param name="characterEncoding">The character/string encoding.</param>
		public SerialReader(ISequenceReader sequenceReader, Encoding characterEncoding)
		{
			this.reader = BinaryEndiannessReader.Create(sequenceReader);
			this.bitReader = new BitReader(sequenceReader);

			this.characterEncoding = characterEncoding;
			//this.decoder = characterEncoding.GetDecoder();
			this.maxCharsSize = characterEncoding.GetMaxCharCount(MaxCharBytesSize);
			this.twoBytesPerChar = characterEncoding is UnicodeEncoding; // For Encodings that always use 2 bytes per char(or more), special case them here to make Read() & Peek() faster.

			var myFunc = this.ReadByte;
		}

		static SerialReader()
		{
			ReadFuncsByTypeId[(int)PropertyTypeId.String] = (reader) => reader.ReadString();
			ReadFuncsByTypeId[(int)PropertyTypeId.Boolean] = (reader) => reader.ReadBoolean();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableBoolean] = (reader) => reader.ReadNullableBoolean();
			ReadFuncsByTypeId[(int)PropertyTypeId.SByte] = (reader) => reader.ReadSByte();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableSByte] = (reader) => reader.ReadNullableSByte();
			ReadFuncsByTypeId[(int)PropertyTypeId.Int16] = (reader) => reader.ReadInt16();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableInt16] = (reader) => reader.ReadNullableInt16();
			ReadFuncsByTypeId[(int)PropertyTypeId.Int32] = (reader) => reader.ReadInt32();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableInt32] = (reader) => reader.ReadNullableInt32();
			ReadFuncsByTypeId[(int)PropertyTypeId.Int64] = (reader) => reader.ReadInt64();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableInt64] = (reader) => reader.ReadNullableInt64();
			ReadFuncsByTypeId[(int)PropertyTypeId.Byte] = (reader) => reader.ReadByte();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableByte] = (reader) => reader.ReadNullableByte();
			ReadFuncsByTypeId[(int)PropertyTypeId.UInt16] = (reader) => reader.ReadUInt16();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableUInt16] = (reader) => reader.ReadNullableUInt16();
			ReadFuncsByTypeId[(int)PropertyTypeId.UInt32] = (reader) => reader.ReadUInt32();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableUInt32] = (reader) => reader.ReadNullableUInt32();
			ReadFuncsByTypeId[(int)PropertyTypeId.UInt64] = (reader) => reader.ReadUInt64();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableUInt64] = (reader) => reader.ReadNullableUInt64();
			ReadFuncsByTypeId[(int)PropertyTypeId.Half] = (reader) => reader.ReadHalf();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableHalf] = (reader) => reader.ReadNullableHalf();
			ReadFuncsByTypeId[(int)PropertyTypeId.Single] = (reader) => reader.ReadSingle();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableSingle] = (reader) => reader.ReadNullableSingle();
			ReadFuncsByTypeId[(int)PropertyTypeId.Double] = (reader) => reader.ReadDouble();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableDouble] = (reader) => reader.ReadNullableDouble();
			ReadFuncsByTypeId[(int)PropertyTypeId.Decimal] = (reader) => reader.ReadDecimal();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableDecimal] = (reader) => reader.ReadNullableDecimal();
			ReadFuncsByTypeId[(int)PropertyTypeId.DateTime] = (reader) => reader.ReadDateTime();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableDateTime] = (reader) => reader.ReadNullableDateTime();
			ReadFuncsByTypeId[(int)PropertyTypeId.TimeSpan] = (reader) => reader.ReadTimeSpan();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableTimeSpan] = (reader) => reader.ReadNullableTimeSpan();
			ReadFuncsByTypeId[(int)PropertyTypeId.BitArray] = (reader) => reader.ReadBitArray();
			ReadFuncsByTypeId[(int)PropertyTypeId.BitVector32] = (reader) => reader.ReadBitVector32();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableBitVector32] = (reader) => reader.ReadNullableBitVector32();
			ReadFuncsByTypeId[(int)PropertyTypeId.Guid] = (reader) => reader.ReadGuid();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableGuid] = (reader) => reader.ReadNullableGuid();
			ReadFuncsByTypeId[(int)PropertyTypeId.Char] = (reader) => reader.ReadChar();
			ReadFuncsByTypeId[(int)PropertyTypeId.NullableChar] = (reader) => reader.ReadNullableChar();

			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.String] = (reader) => reader.ReadStringOptimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Boolean] = (reader) => reader.ReadBoolean();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableBoolean] = (reader) => reader.ReadNullableBoolean();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.SByte] = (reader) => reader.ReadSByte();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableSByte] = (reader) => reader.ReadNullableSByte();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Int16] = (reader) => reader.ReadInt16Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableInt16] = (reader) => reader.ReadNullableInt16Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Int32] = (reader) => reader.ReadInt32Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableInt32] = (reader) => reader.ReadNullableInt32Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Int64] = (reader) => reader.ReadInt64Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableInt64] = (reader) => reader.ReadNullableInt64Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Byte] = (reader) => reader.ReadByte();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableByte] = (reader) => reader.ReadNullableByte();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.UInt16] = (reader) => reader.ReadUInt16Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableUInt16] = (reader) => reader.ReadNullableUInt16Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.UInt32] = (reader) => reader.ReadUInt32Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableUInt32] = (reader) => reader.ReadNullableUInt32Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.UInt64] = (reader) => reader.ReadUInt64Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableUInt64] = (reader) => reader.ReadNullableUInt64Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Half] = (reader) => reader.ReadHalf();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableHalf] = (reader) => reader.ReadNullableHalf();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Single] = (reader) => reader.ReadSingle();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableSingle] = (reader) => reader.ReadNullableSingle();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Double] = (reader) => reader.ReadDouble();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableDouble] = (reader) => reader.ReadNullableDouble();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Decimal] = (reader) => reader.ReadDecimalOptimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableDecimal] = (reader) => reader.ReadNullableDecimalOptimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.DateTime] = (reader) => reader.ReadDateTimeOptimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableDateTime] = (reader) => reader.ReadNullableDateTimeOptimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.TimeSpan] = (reader) => reader.ReadTimeSpanOptimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableTimeSpan] = (reader) => reader.ReadNullableTimeSpanOptimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.BitArray] = (reader) => reader.ReadBitArray();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.BitVector32] = (reader) => reader.ReadBitVector32Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableBitVector32] = (reader) => reader.ReadNullableBitVector32Optimized();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Guid] = (reader) => reader.ReadGuid();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableGuid] = (reader) => reader.ReadNullableGuid();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.Char] = (reader) => reader.ReadChar();
			ReadOptimizedFuncsByTypeId[(int)PropertyTypeId.NullableChar] = (reader) => reader.ReadNullableChar();
		}

		#endregion |   Constructors and Initialization   |

		#region |   Private Properties   |


		///// <summary>
		///// Gets the bit stream from which to read bit data.
		///// </summary>
		//private BitStreamReader BitStream
		//{
		//	get 
		//	{
		//		if (this.bitStream == null)
		//			this.bitStream = new BitStreamReader(this.reader);

		//		return this.bitStream; 
		//	}
		//}

		//private List<string> StringTokenList
		//{
		//	get
		//	{
		//		if (this.stringTokenList == null)
		//			this.stringTokenList = new List<string>();

		//		return this.stringTokenList;
		//	}
		//}

		#endregion |   Private Properties   |

		#region |   Public Properties   |

		public long BytesConsumed => this.reader.Provider.BytesConsumed;
		///// <summary>
		///// Gets the underlying endianess data reader that read actual data.
		///// </summary>
		///// <returns>The <see cref="BinaryEndiannessReader"></see> that that holds the data.</returns>
		public BinaryEndiannessReader BaseReader => this.reader;

		/// <summary>
		/// Gets the specified character encoding.
		/// </summary>
		public Encoding CharacterEncoding => this.characterEncoding;


		public bool UserStringLookupOptimisation { get => this.useStringLookupOptimization; set => this.useStringLookupOptimization = value; }

		///// <summary>
		///// Gets the current byte reading position (number of bytes read).
		///// </summary>
		//public long Position => this.reader.Position;

		///// <summary>
		///// Gets the total length of reading buffer.
		///// </summary>
		//public long Length => this.reader.Length;

		#endregion |   Public Properties   |

		#region |   Public Methods  |

		///// <summary>
		///// Gets the underlying data buffer.
		///// </summary>
		///// <returns></returns>
		//public ReadOnlySequence<byte> GetBuffer() => this.reader.BaseSequenceReader.GetBuffer();

		public List<ArraySegment<byte>> ToArraySegmentList() => this.reader.Provider.ToArraySegmentList();

		public void WriteDataTo(IBufferWriter<byte> writer) => this.reader.Provider.WriteTo(writer);

		public void AdvancePosition(long count) => this.reader.Provider.Advance(count);

		public void ResetPosition()
		{
			this.reader.ResetPosition();
			this.bitReader.Reset();
		}

		//public ReadOnlySpanSegment<byte> AsSpanSequence() => this.reader.Provider.AsSpanSequence();

		///// <summary>
		///// Returns a byte[] containing all of the serialized data from a stream.
		///// Only call this method once after all of the data has been serialized.
		///// </summary>
		///// <returns>A byte[] containing all serialized data.</returns>
		//public byte[] ToArray() => this.reader.BaseReader.ToArray();

		#endregion |   Public Methods  |

		#region |   ISerializationReader Interface   |

		#region |   Primitive Types: bool, sbyte, short, int, long   |

		/// <summary>
		/// Reads a Boolean value from the current stream and advances the current position of the stream by one byte.
		/// </summary>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public Boolean ReadBoolean() => this.bitReader.ReadBit();

		/// <summary>
		/// Returns a Nullable Boolean from the stream.
		/// </summary>
		/// <returns>A Nullable Boolean.</returns>
		public Boolean? ReadNullableBoolean() => this.ReadNullable<Boolean>(this.ReadBoolean);

		/// <summary>
		/// Reads a signed byte from this stream and advances the current position of the stream by one byte.
		/// </summary>
		/// <returns>A signed byte read from the current stream.</returns>
		public SByte ReadSByte() => (SByte)this.reader.ReadByte();

		/// <summary>
		/// Returns a Nullable SByte from the stream.
		/// </summary>
		/// <returns>A Nullable SByte.</returns>
		public SByte? ReadNullableSByte() => this.ReadNullable<SByte>(this.ReadSByte);

		/// <summary>
		/// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
		/// </summary>
		/// <returns>A 2-byte signed integer read from the current stream.</returns>
		public Int16 ReadInt16() => (Int16)(this.ReadByte() | this.ReadByte() << 8);

		/// <summary>
		/// Returns a Nullable Int16 from the stream.
		/// </summary>
		/// <returns>A Nullable Int16.</returns>
		public Int16? ReadNullableInt16() => this.ReadNullable<Int16>(this.ReadInt16);

		/// <summary>
		/// Returns an Int16 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Int16 value.</returns>
		public Int16 ReadInt16Optimized() => unchecked((short)this.reader.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns an Nullable Int16 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable Int16 value.</returns>
		public Int16? ReadNullableInt16Optimized() => this.ReadNullable<Int16>(this.ReadInt16Optimized);

		/// <summary>
		/// Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.
		/// </summary>
		/// <returns>A 4-byte signed integer read from the current stream.</returns>
		public Int32 ReadInt32() => (Int32)(this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24);

		/// <summary>
		/// Returns a Nullable Int32 from the stream.
		/// </summary>
		/// <returns>A Nullable Int32.</returns>
		public Int32? ReadNullableInt32() => this.ReadNullable<Int32>(this.ReadInt32);

		/// <summary>
		/// Returns an Int32 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Int32 value.</returns>
		public Int32 ReadInt32Optimized() => unchecked((int)this.reader.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns an Nullable Int32 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable Int32 value.</returns>
		public Int32? ReadNullableInt32Optimized() => this.ReadNullable<Int32>(this.ReadInt32Optimized);

		/// <summary>
		/// Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte signed integer read from the current stream.</returns>
		public Int64 ReadInt64() => (Int64)(this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24 |
											this.ReadByte() << 32 | this.ReadByte() << 40 | this.ReadByte() << 48 | this.ReadByte() << 56);

		/// <summary>
		/// Returns a Nullable Int64 from the stream.
		/// </summary>
		/// <returns>A Nullable Int64.</returns>
		public Int64? ReadNullableInt64() => this.ReadNullable<Int64>(this.ReadInt64);

		/// <summary>
		/// Returns an Int64 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Int64 value.</returns>
		public Int64 ReadInt64Optimized() => unchecked((long)this.reader.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns an Nullable Int64 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable Int64 value.</returns>
		public Int64? ReadNullableInt64Optimized() => this.ReadNullable<Int64>(this.ReadInt64Optimized);


		#endregion |   Primitive Types: bool, abyte, short, int, long   |

		#region |   Unsigned Primitive Types: byte, ushort, uint, ulong   |

		/// <summary>
		/// Reads the next byte from the current stream and advances the current position of the stream by one byte.
		/// </summary>
		/// <returns>The next byte read from the current stream.</returns>
		public Byte ReadByte() => this.reader.ReadByte();

		/// <summary>
		/// Returns a Nullable Byte from the stream.
		/// </summary>
		/// <returns>A Nullable Byte.</returns>
		public Byte? ReadNullableByte() => this.ReadNullable<Byte>(this.ReadByte);

		/// <summary>
		/// Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.
		/// </summary>
		/// <returns>A 2-byte unsigned integer read from this stream.</returns>
		public UInt16 ReadUInt16() => (UInt16)(this.reader.ReadByte() | this.reader.ReadByte() << 8);

		/// <summary>
		/// Returns a Nullable UInt16 from the stream.
		/// </summary>
		/// <returns>A Nullable UInt16.</returns>
		public UInt16? ReadNullableUInt16() => this.ReadNullable<UInt16>(this.ReadUInt16);

		/// <summary>
		/// Returns an UInt16 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An UInt16 value.</returns>
		public UInt16 ReadUInt16Optimized() => (ushort)this.reader.Read7BitEncodedUInt64();

		/// <summary>
		/// Returns an Nullable UInt16 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable UInt16 value.</returns>
		public UInt16? ReadNullableUInt16Optimized() => this.ReadNullable<UInt16>(this.ReadUInt16Optimized);

		/// <summary>
		/// Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.
		/// </summary>
		/// <returns>A 4-byte unsigned integer read from this stream.</returns>
		public UInt32 ReadUInt32() => (UInt32)(this.reader.ReadByte() | this.reader.ReadByte() << 8 | this.reader.ReadByte() << 16 | this.reader.ReadByte() << 24);

		/// <summary>
		/// Returns a Nullable UInt32 from the stream.
		/// </summary>
		/// <returns>A Nullable UInt32.</returns>
		public UInt32? ReadNullableUInt32() => this.ReadNullable<UInt32>(this.ReadUInt32);
		
		/// <summary>
		/// Returns an UInt32 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An UInt32 value.</returns>
		public UInt32 ReadUInt32Optimized() => (uint)this.reader.Read7BitEncodedUInt64();

		/// <summary>
		/// Returns an Nullable UInt32 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable UInt32 value.</returns>
		public UInt32? ReadNullableUInt32Optimized() => this.ReadNullable<UInt32>(this.ReadUInt32Optimized);

		/// <summary>
		/// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte unsigned integer read from this stream.</returns>
		public UInt64 ReadUInt64() => (UInt64)(this.reader.ReadByte() | this.reader.ReadByte() << 8 | this.reader.ReadByte() << 16 | this.reader.ReadByte() << 24 |
											   this.reader.ReadByte() << 32 | this.reader.ReadByte() << 40 | this.reader.ReadByte() << 48 | this.reader.ReadByte() << 56);

		/// <summary>
		/// Returns a Nullable UInt64 from the stream.
		/// </summary>
		/// <returns>A Nullable UInt64.</returns>
		public UInt64? ReadNullableUInt64() => this.ReadNullable<UInt64>(this.ReadUInt64);

		/// <summary>
		/// Returns an UInt64 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An UInt64 value.</returns>
		public UInt64 ReadUInt64Optimized() => this.reader.Read7BitEncodedUInt64();

		/// <summary>
		/// Returns an Nullable UInt64 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable UInt64 value.</returns>
		public UInt64? ReadNullableUInt64Optimized() => this.ReadNullable<UInt64>(this.ReadUInt64Optimized);

		#endregion |   Unsigned Primitive Types: byte, ushort, uint, ulong   |

		#region |   Floating Point Types & Decimal Types: Half, float, double, decimal   |

		/// <summary>
		/// Reads a <see cref="Half" /> from the sequence (2 bytes).
		/// </summary>
		/// <returns>The Half value.</returns>
		/// </exception>
		public Half ReadHalf() => new Half(this.ReadUInt16());

		/// <summary>
		/// Returns a Nullable <see cref="Half" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable Single.</returns>
		public Half? ReadNullableHalf() => this.ReadNullable<Half>(this.ReadHalf);

		/// <summary>
		/// Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.
		/// </summary>
		/// <returns>A 4-byte floating point value read from the current stream.</returns>
		[SecuritySafeCritical]
		public unsafe Single ReadSingle()
		{
			Int32 raw = this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24;

			return *(Single*)&raw;
		}

		/// <summary>
		/// Returns a Nullable Single from the stream.
		/// </summary>
		/// <returns>A Nullable Single.</returns>
		public Single? ReadNullableSingle() => this.ReadNullable<Single>(this.ReadSingle);

		/// <summary>
		/// Reads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte floating point value read from the current stream.</returns>
		[SecuritySafeCritical]
		public unsafe Double ReadDouble()
		{
			Int64 raw = this.ReadByte() | (long)this.ReadByte() << 8 | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 24 | (long)this.ReadByte() << 32
										| (long)this.ReadByte() << 40 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 56;

			return *(Double*)&raw;
		}

		/// <summary>
		/// Returns a Nullable Double from the stream.
		/// </summary>
		/// <returns>A Nullable Double.</returns>
		public Double? ReadNullableDouble() => this.ReadNullable<Double>(this.ReadDouble);

		/// <summary>
		/// Reads a decimal value from the current stream and advances the current position of the stream by sixteen bytes.
		/// </summary>
		/// <returns>A decimal value read from the current stream.</returns>
		public Decimal ReadDecimal()
		{
			// Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
			int[] parts = new int[4];

			for (int i = 0; i < 4; i++)
				parts[i] = this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24;

			return new Decimal(parts);
		}

		/// <summary>
		/// Returns a Nullable Decimal from the stream.
		/// </summary>
		/// <returns>A Nullable Decimal.</returns>
		public Decimal? ReadNullableDecimal() => this.ReadNullable<Decimal>(this.ReadDecimal);

		/// <summary>
		/// Returns a Decimal value from the stream that was stored optimized.
		/// </summary>
		/// <returns>A Decimal value.</returns>
		public Decimal ReadDecimalOptimized()
		{
			var flags = this.ReadByte();
			var lo = 0;
			var mid = 0;
			var hi = 0;
			byte scale = 0;

			if ((flags & 0x02) != 0)
				scale = this.ReadByte();

			if ((flags & 4) == 0)
				lo = (flags & 32) != 0 ? this.ReadInt32Optimized() : this.ReadInt32();

			if ((flags & 8) == 0)
				mid = (flags & 64) != 0 ? this.ReadInt32Optimized() : this.ReadInt32();

			if ((flags & 16) == 0)
				hi = (flags & 128) != 0 ? this.ReadInt32Optimized() : this.ReadInt32();

			return new decimal(lo, mid, hi, (flags & 0x01) != 0, scale);
		}

		/// <summary>
		/// Returns a Nullable Decimal from the stream that was stored optimized.
		/// </summary>
		/// <returns>A Nullable Decimal.</returns>
		public Decimal? ReadNullableDecimalOptimized() => this.ReadNullable<Decimal>(this.ReadDecimalOptimized);

		#endregion |   Floating Point Types & Decimal Types: float, double, decimal   |

		#region |   Date & Time: DateTime, TimeSpan   |

		/// <summary>
		/// Returns a DateTime value from the stream.
		/// </summary>
		/// <returns>A DateTime value.</returns>
		public DateTime ReadDateTime() => DateTime.FromBinary(this.ReadInt64());

		/// <summary>
		/// Returns a Nullable DateTime from the stream.
		/// </summary>
		/// <returns>A Nullable DateTime.</returns>
		public DateTime? ReadNullableDateTime() => this.ReadNullable<DateTime>(this.ReadDateTime);
		
		/// <summary>
		/// Returns a DateTime value from the stream that was stored optimized.
		/// </summary>
		/// <returns>A DateTime value.</returns>
		public DateTime ReadDateTimeOptimized()
		{
			// Read date information from first three bytes
			var dateMask = new BitVector32(this.ReadByte() | (this.ReadByte() << 8) | (this.ReadByte() << 16));
			var result = new DateTime(dateMask[SerialWriter.DateYearMask],
									  dateMask[SerialWriter.DateMonthMask],
									  dateMask[SerialWriter.DateDayMask]);

			if (dateMask[SerialWriter.DateHasTimeOrKindMask] == 1)
			{
				var initialByte = ReadByte();
				var dateTimeKind = (DateTimeKind)(initialByte & 0x03);

				// Remove the IsNegative and HasDays flags which are never true for a DateTime
				initialByte &= 0xfc;

				if (dateTimeKind != DateTimeKind.Unspecified)
					result = DateTime.SpecifyKind(result, dateTimeKind);

				if (initialByte == 0)
				{
					this.ReadByte(); // No need to call decodeTimeSpan if there is no time information
				}
				else
				{
					result = result.Add(this.DecodeTimeSpan(this.reader.Provider, initialByte));
				}
			}

			return result;
		}

		/// <summary>
		/// Returns a Nullable DateTime from the stream that was stored optimized.
		/// </summary>
		/// <returns>A Nullable DateTime.</returns>
		public DateTime? ReadNullableDateTimeOptimized() => this.ReadNullable<DateTime>(this.ReadDateTimeOptimized);

		/// <summary>
		/// Returns a TimeSpan value from the stream.
		/// </summary>
		/// <returns>A TimeSpan value.</returns>
		public TimeSpan ReadTimeSpan() => new TimeSpan(this.ReadInt64());

		/// <summary>
		/// Returns a Nullable TimeSpan from the stream.
		/// </summary>
		/// <returns>A Nullable TimeSpan.</returns>
		public TimeSpan? ReadNullableTimeSpan() => this.ReadNullable<TimeSpan>(this.ReadTimeSpan);

		/// <summary>
		/// Returns a TimeSpan value from the stream that was stored optimized.
		/// </summary>
		/// <returns>A TimeSpan value.</returns>
		public TimeSpan ReadTimeSpanOptimized() => this.DecodeTimeSpan(this.reader.Provider, this.ReadByte());

		/// <summary>
		/// Returns a Nullable TimeSpan from the stream that was stored optimized.
		/// </summary>
		/// <returns>A Nullable TimeSpan.</returns>
		public TimeSpan? ReadNullableTimeSpanOptimized() => this.ReadNullable<TimeSpan>(this.ReadTimeSpanOptimized);

		#endregion |   Date & Time: DateTime, TimeSpan   |

		#region |   Specific Types: BitArray, BitVector32, Guid   |

		/// <summary>
		/// Returns a BitArray or null from the stream.
		/// </summary>
		/// <returns>A BitArray instance.</returns>
		public BitArray ReadBitArray()
		{
			//return this.ReadNullableClass(
			//	() =>
			//	{
			int length = this.ReadInt32Optimized();

			//return new BitArray(this.reader.ReadBytes(((length + 7) / 8))) { Length = length };
			var result = new BitArray(length);

			for (int i = 0; i < length; i++)
				result[i] = this.ReadBoolean();

			return result;
			//});
		}

		/// <summary>
		/// Returns a BitArray from the stream that was stored optimized.
		/// </summary>
		/// <returns>A BitArray instance.</returns>
		public BitArray ReadBitArrayOptimized() // Since Boolen require reads/writes on 1/8 of byte (1 bit) ReadBitArray could be more compressable than this
		{
			var length = this.ReadInt32Optimized();

			if (length == 0)
				return FullyOptimizableTypedArray;

			return new BitArray(this.ReadByteArray((length + 7) / 8)) { Length = length };
		}

		/// <summary>
		/// Returns a BitVector32 value from the stream.
		/// </summary>
		/// <returns>A BitVector32 value.</returns>
		public BitVector32 ReadBitVector32() => new BitVector32(this.ReadInt32());

		/// <summary>
		/// Returns a Nullable BitVector32 from the stream.
		/// </summary>
		/// <returns>A Nullable BitVector32.</returns>
		public BitVector32? ReadNullableBitVector32() => this.ReadNullable<BitVector32>(this.ReadBitVector32);
		
		/// <summary>
		/// Returns a BitVector32 value from the stream that was stored optimized.
		/// </summary>
		/// <returns>A BitVector32 value.</returns>
		public BitVector32 ReadBitVector32Optimized() => new BitVector32(this.ReadInt32Optimized());
		
		/// <summary>
		/// Returns a Nullable BitVector32 from the stream that was stored optimized.
		/// </summary>
		/// <returns>A Nullable BitVector32.</returns>
		public BitVector32? ReadNullableBitVector32Optimized() => this.ReadNullable<BitVector32>(this.ReadBitVector32Optimized);

		/// <summary>
		/// Returns a Guid value from the stream.
		/// </summary>
		/// <returns>A Guid value.</returns>
		public Guid ReadGuid() => new Guid(this.reader.ReadByteArray(16));

		/// <summary>
		/// Returns a Nullable Guid from the stream.
		/// </summary>
		/// <returns>A Nullable Guid.</returns>
		public Guid? ReadNullableGuid() => this.ReadNullable<Guid>(this.ReadGuid);

		#endregion |   Specific Types: BitArray, BitVector32, Guid   |

		#region |   Char & String   |

		/// <summary>
		/// Reads the next character from the current stream and advances the current position of the stream in accordance with the Encoding used and the specific character being read from the stream.
		/// </summary>
		/// <returns>A character read from the current stream.</returns>
		public Char ReadChar() // => this.CharacterEncoding.GetChars(this.ReadByteArray())[0]; 
		{
			int charsRead = 0;
			int numBytes;
			object readerPosition = this.BytesConsumed;

			//if (_stream.CanSeek)
			//posSav = _stream.Position;

			//this.charBytes ??= new byte[MaxCharBytesSize];
			byte[] charBytes = new byte[2];
			Span<char> singleChar = stackalloc char[1];

			while (charsRead == 0)
			{
				// We really want to know what the minimum number of bytes per char
				// is for our encoding.  Otherwise for UnicodeEncoding we'd have to
				// do ~1+log(n) reads to read n characters.
				// Assume 1 byte can be 1 char unless _2BytesPerChar is true.
				numBytes = this.twoBytesPerChar ? 2 : 1;

				int r = this.ReadByte();
				charBytes[0] = (byte)r;

				if (r == -1)
					numBytes = 0;

				if (numBytes == 2)
				{
					r = this.ReadByte();
					charBytes[1] = (byte)r;

					if (r == -1)
						numBytes = 1;
				}

				if (numBytes == 0)
					throw new Exception("End off file exception"); //return -1;

				Debug.Assert(numBytes == 1 || numBytes == 2, "BinaryReader::ReadOneChar assumes it's reading one or 2 bytes only.");

				//try
				//{
				charsRead = this.CharacterEncoding.GetDecoder().GetChars(new ReadOnlySpan<byte>(charBytes, 0, numBytes), singleChar, flush: false);
				//}
				//catch
				//{
				//	// Handle surrogate char
				//	if (_stream.CanSeek)
				//	{
				//		_stream.Seek(posSav - _stream.Position, SeekOrigin.Current);
				//	}
				//	// else - we can't do much here

				//	throw;
				//}

				Debug.Assert(charsRead < 2, "BinaryReader::ReadOneChar - assuming we only got 0 or 1 char, not 2!");
			}

			Debug.Assert(charsRead > 0);

			if (singleChar[0] == -1)
				throw new Exception("End off file exception");

			return singleChar[0];
		}

		/// <summary>
		/// Returns a Nullable Char from the stream.
		/// </summary>
		/// <returns>A Nullable Char.</returns>
		public Char? ReadNullableChar() => this.ReadNullable<Char>(this.ReadChar);

		public virtual string ReadString2()
		{
			if (this.ReadBoolean())
				return String.Empty;

			if (!reader.CanRead)
				throw new EndOfStreamException("ReadString: End of stream detected.");

			try
			{
				int charRead = 0;
				int strLength = this.ReadInt32Optimized(); //Read7BitEncodedInt();
				byte[] charBytes;

				//if (strLength < 0)
				//	throw new IOException(SR.Format(SR.IO_InvalidStringLen_Len, strLength));

				if (strLength == 0)
					return String.Empty;

				charBytes = new byte[MaxCharBytesSize];

				if (this.charBuffer is null || this.charBuffer.Length < strLength)
					this.charBuffer = new char[Math.Max(this.maxCharsSize, strLength)];

				StringBuilder? stringBuilder = null;

				do
				{
					int count = ((strLength - charRead) > MaxCharBytesSize) ? MaxCharBytesSize : (strLength - charRead);
					int readCount = this.reader.ReadToBuffer(charBytes, 0, count);

					if (readCount == 0)
						throw new Exception("Get end of file");

					int chars = this.CharacterEncoding.GetChars(charBytes, 0, readCount, this.charBuffer, 0);

					if (charRead == 0 && readCount == strLength)
						return new string(this.charBuffer, 0, chars);

					// Since we could be reading from an untrusted data source, limit the initial size of the
					// StringBuilder instance we're about to get or create. It'll expand automatically as needed.

					if (stringBuilder == null)
						stringBuilder = StringBuilderCache.Acquire(capacity: Math.Min(strLength, StringBuilderCache.MaxBuilderSize)); // Actual string length in chars may be smaller

					stringBuilder.Append(this.charBuffer, 0, chars);
					charRead += readCount;
				}
				while (charRead < strLength);

				return StringBuilderCache.GetStringAndRelease(stringBuilder);
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		///// <summary>
		///// Returns a string value from the sequence.
		///// </summary>
		///// <returns>A string value.</returns>
		//public String ReadString() //=> this.ReadNullable<string>(() => this.CharacterEncoding.GetString(this.reader.ReadBytes(this.ReadInt32Optimized())));

		/// <summary>
		/// Reads a string from the current sequence. The string is prefixed with the length, encoded as an integer seven bits at a time.
		/// </summary>
		/// <returns>The string being read.</returns>
		public virtual string ReadStringX()
		{
			int strLength;
            int numOfChars;
			byte[] charBytes;

            try
			{
				if (this.ReadBoolean())
					return String.Empty;

				if (!reader.CanRead)
					strLength = 0;
                
				// Length of the string in bytes, not chars
                strLength = this.ReadInt32Optimized(); //  BinaryEndiannessReader.Read7BitEncodedInt32(this.reader.BaseReader); // Read7BitEncodedInt();

				//if (stringLength < 0)
				//	return null;
				//	//throw new Exception("Invalid string length: " + stringLength);

				if (strLength == 0)
					return String.Empty;

				charBytes = new byte[strLength];
				this.reader.ReadToBuffer(charBytes, 0, strLength);

				//this.charBuffer ??= new char[this.maxCharsSize];
				if (this.charBuffer is null || this.charBuffer.Length < strLength)
					this.charBuffer = new char[Math.Max(this.maxCharsSize, strLength)];

				numOfChars = this.CharacterEncoding.GetChars(charBytes, 0, strLength, this.charBuffer, 0);

				return new string(this.charBuffer, 0, numOfChars);
			}
			catch (Exception ex)
			{
				return null;
			}
		}


		public unsafe string ReadString3()
		{
			//
			// TODO: implement WriteSpan() and GetSpan() complementary methods
			//
			// First write Span length, then Span array; when read first read Span length then ptr to array an create Span
			//
			
			if (this.ReadBoolean())
				return String.Empty;

			int length = this.ReadInt32Optimized();

			if (length == 0)
				return String.Empty;

			var span = this.reader.ReadSpan(length);

			return this.CharacterEncoding.GetString(span); //.Slice(length));
																   //return this.CharacterEncoding.GetString((byte*)Unsafe.AsPointer(ref readOnlySpan.AsSpan().GetPinnableReference()), readOnlySpan.Length);
		}



		private byte[]? charBytes = null;
		/// <summary>
		/// Reads a string from the current stream. The string is prefixed with the length, encoded as an integer seven bits at a time.
		/// </summary>
		/// <returns>The string being read.</returns>
		/// <exception cref="IOException"></exception>
		public string ReadString()
		{
			if (this.ReadBoolean())
				return String.Empty;

			int num = 0;
			int num2 = Read7BitEncodedInt();
			
			if (num2 < 0)
				throw new IOException("String deserialization error: IO.IO_InvalidStringLen, Len=" + num2);

			if (num2 == 0)
				return string.Empty;

			this.charBytes ??= new byte[128];
			this.charBuffer ??= new char[this.maxCharsSize];

			StringBuilder? stringBuilder = null;
			
			do
			{
				int count = ((num2 - num > 128) ? 128 : (num2 - num));
				int num3 = this.reader.ReadToBuffer(this.charBytes, 0, count);
				
				if (num3 == 0)
					throw new EndOfStreamException("String deserialization error: End of stream");

				int chars = this.CharacterEncoding.GetChars(this.charBytes, 0, num3, this.charBuffer, 0);
				
				if (num == 0 && num3 == num2)
					return new string(this.charBuffer, 0, chars);

				if (stringBuilder == null)
					stringBuilder = StringBuilderCache.Acquire(num2);

				stringBuilder.Append(this.charBuffer, 0, chars);
				num += num3;
			}
			while (num < num2);

			return StringBuilderCache.GetStringAndRelease(stringBuilder);
		}

		private int Read7BitEncodedInt()
		{
			int num = 0;
			int num2 = 0;
			byte b;
			
			do
			{
				if (num2 == 35)
					throw new FormatException("String deserialization error: Bad7BitInt32");

				b = ReadByte();
				num |= (b & 0x7F) << num2;
				num2 += 7;
			}
			while ((b & 0x80u) != 0);
			
			return num;
		}


		/// <summary>
		/// Returns a string value from the stream that was stored optimized.
		/// </summary>
		/// <returns>A string value.</returns>
		public string ReadStringOptimized() => this.useStringLookupOptimization ? this.ReadStringLookupOptimized() : this.ReadString();

		/// <summary>
		/// Returns a string value from the stream that was stored lookup optimized.
		/// </summary>
		/// <returns>A string value.</returns>
		public string ReadStringLookupOptimized()
		{
			// TODO: Only StringTokenList optimization
			byte code = this.ReadBits(3);

			switch (code)
			{
				case 0: return String.Empty;
				case 1: return "Y";
				case 2: return "N";
				case 3: return " ";
				case 4: return Char.ToString(this.ReadChar());
				
				case 5: string value = this.ReadString();
					
						this.StringTokenList.Add(value);

						return value;

				case 6: return this.StringTokenList[this.ReadInt32Optimized()];

				default: throw new InvalidOperationException("Unrecognized String Type Code");
			}
		}

		#endregion |   Char & String   |

		#region |   Bits, ByteArray   |

		/// <summary>
		/// Read single bits, up to 8 bits. Number of bits reaed is specified by count property (1-8).
		/// </summary>
		/// <param name="count">The number of bits to read, avilable reange is 1-8.</param>
		/// <returns>The number of bits read stored in byte.</returns>
		public byte ReadBits(int count) => this.bitReader.Read(count);

		/// <summary>
		/// Read the byte array for the specified bytes count of the underlying stream.
		/// </summary>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>Array of bytes</returns>
		public byte[] ReadByteArray(int count) => this.reader.ReadByteArray(count);

		/// <summary>
		/// Read the byte array for the specified bytes count of the underlying stream.
		/// </summary>
		/// <param name="count">The number of bytes to read.</param>
		/// <param name="offset">The offset position of the first byte.</param>
		/// <returns></returns>
		public byte[] ReadByteArray(int count, out int offset) => this.reader.ReadByteArray(count, out offset);
	
		#endregion |   Bits, ByteArray   |

		#region |   Span  |

		public ReadOnlySpan<byte> ReadSpan(int length) => this.reader.ReadSpan(length);

		#endregion |   Span  |

		#region |   Type & Object   |

		///// <summary>
		///// Returns a Type or null from the stream that was writen oprimized.
		///// Throws an exception if the Type cannot be found and throwOnError is true.
		///// </summary>
		///// <returns>A Type instance.</returns>
		//public Type ReadTypeOptimized() => this.ReadType(); // Currently there are no optimization on Type

		///// <summary>
		///// Returns a Type or null from the stream that was writen oprimized.
		///// Throws an exception if the Type cannot be found and throwOnError is true.
		///// </summary>
		///// <param name="throwOnError">If true throw an error if occur.</param>
		///// <returns>A Type instance.</returns>
		//public Type ReadTypeOptimized(bool throwOnError) => this.ReadType(throwOnError); // Currently there are no optimization on Type

		///// Returns a Type or null from the stream that was stored optimized.
		///// 
		///// Throws an exception if the Type cannot be found and throwOnError is true.
		///// </summary>
		///// <returns>A Type instance.</returns>
		//public Type ReadTypeOptimized() => this.ReadTypeOptimized(throwOnError: true);

		///// <summary>
		///// Returns a Type or null from the stream that was stored optimized.
		///// 
		///// Throws an exception if the Type cannot be found and throwOnError is true.
		///// </summary>
		///// <param name="throwOnError">If true throw an error if occur.</param>
		///// <returns>A Type instance.</returns>
		//public Type ReadTypeOptimized(bool throwOnError) => this.ReadNullable<Type>(() => Type.GetType(this.ReadStringOptimized(), throwOnError));

		///// <summary>
		///// Reads an object from the stream. Use this method only if cannot use any of other method available 
		///// or if you for any reason not able to manualy serialize it using serializating its properties.
		///// <param name="objectType">The known type of the object.</param>
		///// </summary>
		//public object ReadObject(Type objectType)
		//{
		//	return SimpleReader.ReadObject(this.GetBaseStream(), objectType);
		//}

		///// <summary>
		///// Reads an object from the stream. Use this method only if cannot use any of other method available 
		///// or if you for any reason not able to manualy serialize it using serializating its properties.
		///// </summary>
		//public object ReadObject()
		//{
		//	return this.ReadNullable<object>(() => SimpleReader.ReadObject(this.GetBaseStream(), this.ReadTypeOptimized(true)));
		//}

		///// <summary>
		///// Reads an object[] from the stream.
		///// </summary>
		//public object[] ReadObjectArray()
		//{
		//	return this.ReadArray<object>(this.ReadObject);
		//}

		#endregion |   Type & Object   |

		#endregion |   ISerializationReader Interface   |

		#region |   Helper Methods   |

		/// <summary>
		/// Reads a value from the current stream.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		public T? Read<T>() => (T?)this.Read(typeof(T));

		///// <summary>
		///// Reads a value from the current stream that MUST be the same as it is written. ObjectTypeId specify written object type and is defined in <see cref="Simple.ObjectTypes"/> class.
		///// </summary>
		///// <param name="objectTypeId">The object typeId that uniquely identify writen object type.</param>
		///// <returns>The object that is read from the stream.</returns>
		//public object Read(int objectTypeId)
		//{
		//	return this.Read(objectTypeId);
		//}

		/// <summary>
		/// Reads a value from the current stream that MUST be the same as it is written, using the fewest number of bytes possible.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		public T? ReadOptimized<T>() => (T?)this.ReadOptimized(typeof(T));

		///// <summary>
		///// Reads a value from the current stream that MUST be the same as it is written, using the fewest number of bytes possible. TypeId specify written object type and is defined in <see cref="Simple.ObjectTypes"/> class.
		///// </summary>
		///// <param name="typeId">The object typeId that uniquely identify writen object type.</param>
		///// <returns>The object that is read from the stream.</returns>
		//public object ReadOptimized(int objectTypeId)
		//{
		//	return this.ReadOptimized(typeId);
		//}

		///// <summary>
		///// Reads an object from the stream. Use this method only if cannot use any of other method available 
		///// or if you for any reason not able to manualy serialize it using serializating its properties.
		///// <typeparam name="T">The type of the value.</typeparam>
		///// <param name="objectType">The known type of the object.</param>
		///// </summary>
		//public T ReadObject<T>()
		//{
		//	return (T)this.ReadObject(typeof(T));
		//}

		/// <summary>
		/// Reads the object specified by objectType that MUST be the same as it is written.
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray and Type.
		/// </summary>
		/// <param name="objectType">The <see cref="Type"/> of the object.</param>
		/// <param name="value">The object value.</param>
		public object? Read(Type objectType)
		{
			int typeId = PropertyTypes.GetPropertyTypeId(objectType);

			if (typeId >= 0)
				return this.Read(typeId);
			else
				throw new NotSupportedException(String.Format("The object type '{0}' is not supperted for serialization.", objectType.ToString()));
		}

		/// <summary>
		/// Reads the object specified by typeId that MUST be the same type as it is written.
		/// ObjectTypeId is specified in <see cref="Simple.PropertyTypes"/> class.
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray and Type.
		/// </summary>
		/// <param name="objectTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <returns>The object that is read from the stream.</returns>
		public object? Read(int objectTypeId)
		{
			return ReadFuncsByTypeId[objectTypeId](this);
		}

		/// <summary>
		/// Reads the object specified by objectType that MUST be the same as it is written, using the fewest number of bytes possible.
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray and Type.
		/// </summary>
		/// <param name="objectType">The <see cref="Type"/> of the object.</param>
		/// <param name="value">The object value.</param>
		public object? ReadOptimized(Type objectType)
		{
			int typeId = PropertyTypes.GetPropertyTypeId(objectType);

			if (typeId >= 0)
				return this.ReadOptimized(typeId);
			else
				throw new NotSupportedException(String.Format("The object type '{0}' is not supperted for serialization.", objectType.ToString()));
		}

		/// <summary>
		/// Reads the object specified by typeId that MUST be the same type as it is written, using the fewest number of bytes possible.
		/// ObjectTypeId is specified in <see cref="Simple.PropertyTypes"/> class.
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray and Type.
		/// </summary>
		/// <param name="objectTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <returns>The object that is read from the stream.</returns>
		public object? ReadOptimized(int objectTypeId)
		{
			return ReadOptimizedFuncsByTypeId[objectTypeId](this);
		}

		/// <summary>
		/// Gets the read Func method that reads from given sream by specified object type.
		/// </summary>
		/// <param name="objectType">The type of the object.</param>
		/// <returns>The Func method for writing to stream.</returns>
		//[CLSCompliant(false)]
		public static Func<SerialReader, object?> GetReaderFunc(int objectTypeId)
		{
			return ReadFuncsByTypeId[objectTypeId];
		}

		/// <summary>
		/// Gets the optimized read Func method that reads optimized from given sream by specified object type.
		/// </summary>
		/// <param name="objectType">The object type identifier.</param>
		/// <returns>The Func method for optimized writing to stream.</returns>
		//[CLSCompliant(false)]
		public static Func<SerialReader, object?> GetReaderFuncOptimized(int objectTypeId)
		{
			return ReadOptimizedFuncsByTypeId[objectTypeId];
		}

		//public void ResetBitstreamPosition()
		//{
		//	if (this.Reader is SimpleReader)
		//		(this.Reader as SimpleReader).ResetBitstreamPosition();
		//}

		///// <summary>
		///// Sets the new read custom actions. The related ObjectTypeId is given as result value.
		///// </summary>
		///// <param name="readActionByTypeId">The serialization read action.</param>
		///// <param name="optimizedReadActionByTypeId">The serialization optimized read action.</param>
		///// <returns>The ObjectTypeId.</returns>
		//[CLSCompliant(false)]
		//public static int SetNewReadAction(Func<ISerializationReader, object> readActionByTypeId, Func<ISerializationReader, object> readActionOptimizedByTypeId, Func<ISerializationReader, object> readActionDefaultOptimizedByTypeId)
		//{
		//	int objectTypeId = ReadFuncsByTypeId.MaxIndex + 1;

		//	ReadFuncsByTypeId[objectTypeId] = readActionByTypeId;
		//	ReadFuncsOptimizedByTypeId[objectTypeId] = readActionOptimizedByTypeId;
		//	ReadFuncsDefaultOptimizedByTypeId[objectTypeId] = readActionDefaultOptimizedByTypeId;

		//	return objectTypeId;
		//}

		#endregion |   Helper Methods   |

		#region |   Additional Helper Methods  |

		protected T? ReadNullable<T>(Func<T> readNonNullableFunc) where T : struct
		{
			if (this.ReadBoolean())
				return default;
			else
				return readNonNullableFunc(); 
		}

		#endregion |   Additional Helper Methods  |

		#region |   Public Static Helper Methods   |

		/// <summary>
		/// Returns a Int16 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int16 value.</returns>
		public static short Read7BitEncodedInt16(ISequenceReader reader) => BinaryEndiannessReader.Read7BitEncodedInt16(reader);

		/// <summary>
		/// Returns a UInt16 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A UInt16 value.</returns>
		public static ushort Read7BitEncodedUInt16(ISequenceReader reader) => BinaryEndiannessReader.Read7BitEncodedUInt16(reader);

		/// <summary>
		/// Returns a Int32 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int32 value.</returns>
		public static int Read7BitEncodedInt32(ISequenceReader reader) => BinaryEndiannessReader.Read7BitEncodedInt32(reader);

		/// <summary>
		/// Returns a UInt32 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A UInt32 value.</returns>
		public static uint Read7BitEncodedUInt32(ISequenceReader reader) => BinaryEndiannessReader.Read7BitEncodedUInt32(reader);

		/// <summary>
		/// Returns a Int64 value from the stream that was stored optimized.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <returns>A Int64 value.</returns>
		public static long Read7BitEncodedInt64(ISequenceReader reader) => BinaryEndiannessReader.Read7BitEncodedInt64(reader);

		/// <summary>
		/// Reads in a 64-bit unigned long in compressed format.
		/// </summary>
		/// <returns> A 64-bit unsigned long in compressed format.</returns>
		public static ulong Read7BitEncodedUInt64(ISequenceReader reader) => BinaryEndiannessReader.Read7BitEncodedUInt64(reader);


		// TODO: Check this out

		///// <summary>
		/////	Deserialize object from the <see cref="Stream"/> when object type is unknown or dynamic.
		///// </summary>
		///// <param name="stream">Source <see cref="Stream"/>.</param>
		///// <returns>Deserialized object.</returns>
		///// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
		///// <exception cref="SerializationException"><typeparamref name="T"/> is not serializable etc.</exception>
		//public static object ReadObject(Stream stream)
		//{
		//	BinaryReader reader = new BinaryReader(stream);
		//	Type objectType = Type.GetType(reader.ReadString());

		//	return MessagePackSerializer.Get(objectType).Unpack(stream);
		//}

		///// <summary>
		/////	Deserialize object from the <see cref="Stream"/>.
		///// </summary>
		///// <typeparam name="T">The return object type.</typeparam>
		///// <param name="stream">Source <see cref="Stream"/>.</param>
		///// <returns>Deserialized object.</returns>
		///// <exception cref="ArgumentNullException"><paramref name="stream"/> is <c>null</c>.</exception>
		///// <exception cref="SerializationException"><typeparamref name="T"/> is not serializable etc.</exception>
		//public static object ReadObject<T>(Stream stream)
		//{
		//	return MessagePackSerializer.Get<T>().Unpack(stream);
		//}

		///// <summary>
		/////	Deserialize object from the <see cref="Stream"/>.
		///// </summary>
		///// <param name="stream">Destination <see cref="Stream"/>.</param>
		///// <param name="objectType">The value object type.</param>
		///// <returns>Deserialized object.</returns>
		///// <exception cref="ArgumentNullException"><paramref name="packer"/> is <c>null</c>.</exception>
		///// <exception cref="ArgumentException"><paramref name="objectTree"/> is not compatible for this serializer.</exception>
		///// <exception cref="SerializationException">The type of <paramref name="value"/> is not serializable etc.</exception>
		//public static object ReadObject(Stream stream, Type objectType)
		//{
		//	return MessagePackSerializer.Get(objectType).Unpack(stream);
		//}

		#endregion |   Public Static Helper Methods   |

		#region |   Extras   |

		/// <summary>
		/// Returns a Type or null from the stream.
		/// Throws an exception if the Type cannot be found and throwOnError is true.
		/// </summary>
		/// <returns>A Type instance.</returns>
		public Type? ReadType() => this.ReadType(throwOnError: true);

		/// <summary>
		/// Returns a Type or null from the stream.
		/// 
		/// Throws an exception if the Type cannot be found and throwOnError is true.
		/// </summary>
		/// <param name="throwOnError">If true throw an error if occur.</param>
		/// <returns>A Type instance.</returns>
		public Type? ReadType(bool throwOnError)
		{
			Type? type = Type.GetType(this.ReadString(), throwOnError);

			if (type != null)
				return type;

			return default;
		}

		/// <summary>
		/// Returns an Int16 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An Int16 value.</returns>
		public static short ReadNBitEncodedInt16(SerialReader reader, int n) => unchecked((short)ReadNBitEncodedUInt64(reader, n));

		/// <summary>
		/// Returns an UInt16 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An UInt16 value.</returns>
		public static ushort ReadNBitEncodedUInt16(SerialReader reader, int n) => unchecked((ushort)ReadNBitEncodedUInt64(reader, n));

		/// <summary>
		/// Returns an Int32 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An Int32 value.</returns>
		public static int ReadNBitEncodedInt32(SerialReader reader, int n) => unchecked((int)ReadNBitEncodedUInt64(reader, n));

		/// <summary>
		/// Returns an UInt32 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An UInt32 value.</returns>
		public static uint ReadNBitEncodedUInt32(SerialReader reader, int n) => unchecked((uint)ReadNBitEncodedUInt64(reader, n));

		/// <summary>
		/// Returns an Int64 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An Int64 value.</returns>
		public static long ReadNBitEncodedInt64(SerialReader reader, int n) => unchecked((long)ReadNBitEncodedUInt64(reader, n));

		/// <summary>
		/// Returns an UInt64 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An UInt64 value.</returns>
		public static ulong ReadNBitEncodedUInt64(SerialReader reader, int n)
		{
			ulong result = 0;
			var bitMask = SerialWriter.BitMask[n];
			var singleBit = SerialWriter.SingleBits[n];
			var bitShift = 0;
			var numOfBits = n + 1;

			while (true)
			{
				var nextByte = reader.ReadBits(numOfBits);

				result |= ((ulong)nextByte & bitMask) << bitShift;
				bitShift += n;

				if ((nextByte & singleBit) == 0)
					return result;
			}
		}

		public static int ReadBitEncodedInt16(SerialReader reader, int numOfBits) => unchecked((short)ReadBitEncodedUInt64(reader, numOfBits));

		public static uint ReadBitEncodedUInt16(SerialReader reader, int numOfBits) => (ushort)ReadBitEncodedUInt64(reader, numOfBits);

		public static int ReadBitEncodedInt32(SerialReader reader, int numOfBits) => unchecked((int)ReadBitEncodedUInt64(reader, numOfBits));

		public static uint ReadBitEncodedUInt32(SerialReader reader, int numOfBits) => (uint)ReadBitEncodedUInt64(reader, numOfBits);

		public static long ReadBitEncodedInt64(SerialReader reader, int numOfBits) => unchecked((long)ReadBitEncodedUInt64(reader, numOfBits));
		
		public static ulong ReadBitEncodedUInt64(SerialReader reader, int numOfBits)
		{
			ulong value = 0;
			byte bitMask = SerialWriter.BitMask[numOfBits];
			byte segment;

			do
			{
				segment = reader.ReadBits(numOfBits);
				value += segment;
			}
			while (segment == bitMask);

			return value;
		}

		#endregion |   Extras   |

		#region |   Private Properties  |

		private List<string> StringTokenList
		{
			get
			{
				if (this.stringTokenList == null)
					this.stringTokenList = new List<string>();

				return this.stringTokenList;
			}
		}

		#endregion |   Private Properties  |

		#region |   Private Methods  |

		///// <summary>
		///// Returns a generic IList populated with values using generic read element function.
		///// The list type must be the SAME as it is written.
		///// </summary>
		///// <typeparam name="T">The list Type.</typeparam>
		///// <param name="readElementFunc">Single element read function.</param>
		///// <returns>Returns a generic IList that was read from the reader.</returns>
		//private IList<T> ReadList<T>(Func<T> readElementFunc) => this.ReadArray(readElementFunc);
		////{
		////	return this.ReadNullableClass(readNonNullableFunc: () =>
		////	{
		////		T[] result = new T[this.ReadInt32Optimized()];

		////		for (int i = 0; i < result.Length; i++)
		////			result[i] = readElementFunc();

		////		return result;
		////	});
		////}

		///// <summary>
		///// Returns a generic IDictionary populated with key and value pairs using generic read elemnt function.
		///// The list type must be the SAME as it is written.
		///// </summary>
		///// <typeparam name="TKey">The key Type.</typeparam>
		///// <typeparam name="TValue">The value Type.</typeparam>
		///// <param name="readKeyFunc">Single key element read function.</param>
		///// <param name="readValueFunc">Single value element read function.</param>
		///// <returns>Returns a generic <see cref="IDictionary {TKey, TValue}"/> that was read from the reader.</returns>
		//private IDictionary<TKey, TValue> ReadDictionary<TKey, TValue>(Func<TKey> readKeyFunc, Func<TValue> readValueFunc) where TKey : notnull
		//{
		//	return this.ReadNullableClass(readNonNullableFunc: () =>
		//	{
		//		int length = this.ReadInt32Optimized();
		//		Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>(length);

		//		for (int i = 0; i < length; i++)
		//			result.Add(readKeyFunc(), readValueFunc());

		//		return result;
		//	});
		//}

		/// <summary>
		/// Returns a TimeSpan decoded from packed data.
		/// This routine is called from ReadOptimizedDateTime() and ReadOptimizedTimeSpan().
		/// <remarks>
		/// This routine uses a parameter to allow ReadOptimizedDateTime() to 'peek' at the
		/// next byte and extract the DateTimeKind from bits one and two (IsNegative and HasDays)
		/// which are never set for a Time portion of a DateTime.
		/// </remarks>
		/// </summary>
		/// <param name="reader">The serialization reader.</param>
		/// <param name="initialByte">The first of two always-present bytes.</param>
		/// <returns>A decoded TimeSpan</returns>
		private TimeSpan DecodeTimeSpan(ISequenceReader reader, byte initialByte)
		{
			var packedData = new BitVector32(initialByte | (reader.ReadByte() << 8)); // Read first two bytes
			var hasTime = packedData[SerialWriter.HasTimeSection] == 1;
			var hasSeconds = packedData[SerialWriter.HasSecondsSection] == 1;
			var hasMilliseconds = packedData[SerialWriter.HasMillisecondsSection] == 1;
			long ticks = 0;

			if (hasMilliseconds)
			{
				packedData = new BitVector32(packedData.Data | (reader.ReadByte() << 16) | (reader.ReadByte() << 24));
			}
			else if (hasTime && hasSeconds)
			{
				packedData = new BitVector32(packedData.Data | (reader.ReadByte() << 16));
			}

			if (hasTime)
			{
				ticks += packedData[SerialWriter.HoursSection] * TimeSpan.TicksPerHour;
				ticks += packedData[SerialWriter.MinutesSection] * TimeSpan.TicksPerMinute;
			}

			if (hasSeconds)
				ticks += packedData[(!hasTime && !hasMilliseconds) ? SerialWriter.MinutesSection : SerialWriter.SecondsSection] * TimeSpan.TicksPerSecond;

			if (hasMilliseconds)
				ticks += packedData[SerialWriter.MillisecondsSection] * TimeSpan.TicksPerMillisecond;

			if (packedData[SerialWriter.HasDaysSection] == 1)
				ticks += BinaryEndiannessReader.Read7BitEncodedInt32(reader) * TimeSpan.TicksPerDay; // (int)Read7BitEncodedSignedInt64(reader) = reader.ReadInt32Optimized()

			if (packedData[SerialWriter.IsNegativeSection] == 1)
				ticks = -ticks;

			return new TimeSpan(ticks);
		}

		#endregion |   Private Methods  |
	}
}
