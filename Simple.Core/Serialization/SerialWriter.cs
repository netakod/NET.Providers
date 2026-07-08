using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipelines;
using System.Buffers;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security;
using Simple;
using Simple.Collections;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
//using MsgPack;
//using MsgPack.Serialization;

namespace Simple.Serialization
{
	/// <summary>
	/// A SerializationWriter is used to store values and objects to a byte array.
	/// Once an instance is created, use the various methods to write the required data.
	/// The data read MUST be exactly the same type and in the same order as it was written.
	/// </summary>
	public class SerialWriter //: ISerialWriter
	{
		#region |   Private Members   |

		//private ISequenceWriter baseWriter = null;
		private BitWriter bitWriter;
		private BinaryEndiannessWriter writer;
		private Encoding characterEncoding = DefaultEncoding;
		private UniqueStringList? stringLookup = null;

		private const int MaxArrayPoolRentalSize = 64 * 1024; // try to keep rentals to a reasonable size
		private readonly bool preserveDecimalScale = false;
		private readonly bool optimizeForSize = true;
		private readonly bool useStringLookupOptimization = false;
		private readonly bool useFastUtf8;
        private byte[]? largeByteBuffer = null;
        private int maxChars;

        internal static HashArray<Action<SerialWriter, object?>> WriteActionsByTypeId = new HashArray<Action<SerialWriter, object?>>(PropertyTypes.Count);
		internal static HashArray<Action<SerialWriter, object?>> WriteOptimizedActionsByTypeId = new HashArray<Action<SerialWriter, object?>>(PropertyTypes.Count);

		#endregion |   Private Members   |

		#region |   Public Static Members   |

		/// <summary>
		/// Default capacity for the underlying MemoryStream
		/// </summary>
		public static int DefaultCapacity = 1024;

		/// <summary>
		/// Gets the default character encoding.
		/// </summary>
		public static readonly Encoding DefaultEncoding = Encoding.UTF8; //new UTF8Encoding(false);

		/// <summary>
		/// Holds the highest Int16 that can be optimized into less than the normal 2 bytes
		/// </summary>
		public const short HighestOptimizable16BitValue = 127; // 0x7F

		/// <summary>
		/// Holds the highest Int32 that can be optimized into less than the normal 4 bytes
		/// </summary>
		public const int HighestOptimizable32BitValue = 2097151; // 0x001FFFFF

		/// <summary>
		/// Holds the highest Int64 that can be optimized into less than the normal 8 bytes
		/// </summary>
		public const long HighestOptimizable64BitValue = 562949953421311; // 0x0001FFFFFFFFFFFF

		#endregion |   Public Static Members   |

		#region |   Internal Static Members   |

		/// <summary>
		/// Section masks used for packing DateTime values
		/// </summary>
		internal static readonly BitVector32.Section DateYearMask = BitVector32.CreateSection(9999); //14 bits
		internal static readonly BitVector32.Section DateMonthMask = BitVector32.CreateSection(12, DateYearMask); // 4 bits
		internal static readonly BitVector32.Section DateDayMask = BitVector32.CreateSection(31, DateMonthMask); // 5 bits
		internal static readonly BitVector32.Section DateHasTimeOrKindMask = BitVector32.CreateSection(1, DateDayMask); // 1 bit  total= 3 bytes

		/// <summary>
		/// Section masks used for packing TimeSpan values
		/// </summary>
		internal static readonly BitVector32.Section IsNegativeSection = BitVector32.CreateSection(1); //1 bit
		internal static readonly BitVector32.Section HasDaysSection = BitVector32.CreateSection(1, IsNegativeSection); //1 bit
		internal static readonly BitVector32.Section HasTimeSection = BitVector32.CreateSection(1, HasDaysSection); //1 bit
		internal static readonly BitVector32.Section HasSecondsSection = BitVector32.CreateSection(1, HasTimeSection); //1 bit
		internal static readonly BitVector32.Section HasMillisecondsSection = BitVector32.CreateSection(1, HasSecondsSection); //1 bit
		internal static readonly BitVector32.Section HoursSection = BitVector32.CreateSection(23, HasMillisecondsSection); // 5 bits
		internal static readonly BitVector32.Section MinutesSection = BitVector32.CreateSection(59, HoursSection); // 6 bits  total = 2 bytes
		internal static readonly BitVector32.Section SecondsSection = BitVector32.CreateSection(59, MinutesSection); // 6 bits total = 3 bytes
		internal static readonly BitVector32.Section MillisecondsSection = BitVector32.CreateSection(1024, SecondsSection); // 10 bits - total 31 bits = 4 bytes

		/// <summary>
		/// Section masks used for packing n-bit encoding
		/// </summary>
		internal static byte[] BitMask = new byte[9];
		internal static byte[] SingleBits = new byte[8];

		#endregion |   Internal Static Members   |

		#region |   Constructors and Initialization   |

		///// <summary>
		///// Initializes a new instance of the <see cref="SimpleWriter"/> class with an initial <see cref="MemoryStream"/> expandable capacity initialized to zero.
		///// </summary>
		//public SerializationWriter() : this(new MemoryStream(DefaultCapacity)) { }

		///// <summary>
		///// Initializes a new instance of the <see cref="SimpleWriter"/> class with an initial <see cref="MemoryStream"/> with initial capacity as specified.
		///// </summary>
		///// <param name="capacity">The initial size of the internal array in bytes.</param>
		//public SerializationWriter(int capacity) : this(new MemoryStream(capacity)) { }

		///// <summary>
		///// Creates a SimpleWriter using a byte[] of pre-existing data.
		///// A MemoryStream is used to write the data without making a copy of it.
		///// </summary>
		///// <param name="data">The byte[] already containining serialized data.</param>
		//public SerializationWriter(byte[] data) : this(new MemoryStream(data)) { }

		public SerialWriter() 
			: this((ISequenceWriter)new ArraySegmentListWriter())	
		{ 
		}

        public SerialWriter(Encoding characterEncoding) 
			: this((ISequenceWriter)new ArraySegmentListWriter(), characterEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="SpanSequenceWriter"/> 
		/// </summary>
		/// <param name="sequenceWriter">The buffer sequencer in whitch to write data.</param>
		public SerialWriter(SpanSequenceWriter sequenceWriter) 
			: this(sequenceWriter, DefaultEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="SpanSequenceWriter"/> and character encoding
		/// </summary>
		/// <param name="sequenceWriter">The buffer sequencer in whitch to write data.</param>
		public SerialWriter(SpanSequenceWriter sequenceWriter, Encoding characterEncoding) 
			: this((ISequenceWriter)sequenceWriter, characterEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="List{ArraySegment{byte}}"/> 
		/// </summary>
		/// <param name="sequenceWriter">The buffer sequencer in whitch to write data.</param>
		public SerialWriter(List<ArraySegment<byte>> arraySegmentList) 
			: this(arraySegmentList, DefaultEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="List{ArraySegment{byte}}"/> and character encoding
		/// </summary>
		/// <param name="sequenceWriter">The buffer sequencer in whitch to write data.</param>
		public SerialWriter(List<ArraySegment<byte>> arraySegmentList, Encoding characterEncoding) 
			: this((ISequenceWriter)new ArraySegmentListWriter(arraySegmentList), characterEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="IList{ArraySegment{byte}}"/> segments.
		/// </summary>
		/// <param name="segments"></param>
		public SerialWriter(ArraySegmentStream arraySegmentStream) 
			: this(arraySegmentStream, DefaultEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="IList{ArraySegment{byte}}"/> segments and character encoding.
		/// </summary>
		/// <param name="segments"></param>
		public SerialWriter(ArraySegmentStream arraySegmentStream, Encoding characterEncoding) 
			: this((Stream)arraySegmentStream, characterEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> 
		/// and a default character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="Simple.Serialization.BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as its underlying.
		/// </summary>
		/// <param name="stream">The stream in which to write data.</param>
		public SerialWriter(Stream stream) 
			: this(stream, DefaultEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SerialWriter"/> class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> and character encoding.
		/// and a default character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="Simple.Serialization.BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as its underlying.
		/// </summary>
		/// <param name="stream">The stream in which to write data.</param>
		public SerialWriter(Stream stream, Encoding characterEncoding) 
			: this((ISequenceWriter)new StreamSequenceWriter(stream), characterEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleWriter"/> class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> 
		/// and a specific character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="Simple.Serialization.BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as a underlying.
		/// </summary>
		/// <param name="writer">The equence writer in which to write data.</param>
		public SerialWriter(ISequenceWriter writer) 
			: this(writer, DefaultEncoding) 
		{ 
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleWriter"/> class based on the supplied <see cref="Stream"/>, <see cref="Simple.Serialization.BitStream"/> 
		/// and a specific character <see cref="System.Text.Encoding"/>.
		/// A <see cref="Stream"/> is used to access the data without making a copy of it.
		/// A <see cref="Simple.Serialization.BitStream"/> is used to access the bit data without making a copy of it and must be based this <see cref="Stream"/> as a underlying.
		/// </summary>
		/// <param name="writer">The equence writer in which to write data.</param>
		/// <param name="characterEncoding">The character/string encoding.</param>
		public SerialWriter(ISequenceWriter writer, Encoding characterEncoding)
		{
			this.writer = BinaryEndiannessWriter.Create(writer);
			this.bitWriter = new BitWriter(writer);
			this.characterEncoding = characterEncoding;
			this.useFastUtf8 = characterEncoding.CodePage == Encoding.UTF8.CodePage && characterEncoding.EncoderFallback.MaxCharCount <= 1;
		}

		static SerialWriter()
		{
			BitMask[0] = 0x00;
			BitMask[1] = 0x01;
			BitMask[2] = 0x03;
			BitMask[3] = 0x07;
			BitMask[4] = 0x0F;
			BitMask[5] = 0x1F;
			BitMask[6] = 0x3F;
			BitMask[7] = 0x7F;
			BitMask[8] = 0xFF;

			SingleBits[0] = 0x01;
			SingleBits[1] = 0x02;
			SingleBits[2] = 0x04;
			SingleBits[3] = 0x08;
			SingleBits[4] = 0x10;
			SingleBits[5] = 0x20;
			SingleBits[6] = 0x40;
			SingleBits[7] = 0x80;

			WriteActionsByTypeId[(int)PropertyTypeId.String] = (writer, value) => writer.WriteString((String)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.Boolean] = (writer, value) => writer.WriteBoolean((Boolean)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableBoolean] = (writer, value) => writer.WriteNullableBoolean((Boolean?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.SByte] = (writer, value) => writer.WriteSByte((SByte)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableSByte] = (writer, value) => writer.WriteNullableSByte((SByte?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Int16] = (writer, value) => writer.WriteInt16((Int16)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableInt16] = (writer, value) => writer.WriteNullableInt16((Int16?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Int32] = (writer, value) => writer.WriteInt32((Int32)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableInt32] = (writer, value) => writer.WriteNullableInt32((Int32?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Int64] = (writer, value) => writer.WriteInt64((Int64)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableInt64] = (writer, value) => writer.WriteNullableInt64((Int64?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Byte] = (writer, value) => writer.WriteByte((Byte)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableByte] = (writer, value) => writer.WriteNullableByte((Byte?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.UInt16] = (writer, value) => writer.WriteUInt16((UInt16)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableUInt16] = (writer, value) => writer.WriteNullableUInt16((UInt16?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.UInt32] = (writer, value) => writer.WriteUInt32((UInt32)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableUInt32] = (writer, value) => writer.WriteNullableUInt32((UInt32?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.UInt64] = (writer, value) => writer.WriteUInt64((UInt64)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableUInt64] = (writer, value) => writer.WriteNullableUInt64((UInt64?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Single] = (writer, value) => writer.WriteSingle((Single)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableSingle] = (writer, value) => writer.WriteNullableSingle((Single?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Double] = (writer, value) => writer.WriteDouble((Double)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableDouble] = (writer, value) => writer.WriteNullableDouble((Double?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Decimal] = (writer, value) => writer.WriteDecimal((Decimal)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableDecimal] = (writer, value) => writer.WriteNullableDecimal((Decimal?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.DateTime] = (writer, value) => writer.WriteDateTime((DateTime)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableDateTime] = (writer, value) => writer.WriteNullableDateTime((DateTime?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.TimeSpan] = (writer, value) => writer.WriteTimeSpan((TimeSpan)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableTimeSpan] = (writer, value) => writer.WriteNullableTimeSpan((TimeSpan?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.BitArray] = (writer, value) => writer.WriteBitArray((BitArray)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.BitVector32] = (writer, value) => writer.WriteBitVector32((BitVector32)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableBitVector32] = (writer, value) => writer.WriteNullableBitVector32((BitVector32?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Guid] = (writer, value) => writer.WriteGuid((Guid)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableGuid] = (writer, value) => writer.WriteNullableGuid((Guid?)value);
			WriteActionsByTypeId[(int)PropertyTypeId.Char] = (writer, value) => writer.WriteChar((Char)value!);
			WriteActionsByTypeId[(int)PropertyTypeId.NullableChar] = (writer, value) => writer.WriteNullableChar((Char?)value);

			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.String] = (writer, value) => writer.WriteString((String)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Boolean] = (writer, value) => writer.WriteBoolean((Boolean)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableBoolean] = (writer, value) => writer.WriteNullableBoolean((Boolean?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.SByte] = (writer, value) => writer.WriteSByte((SByte)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableSByte] = (writer, value) => writer.WriteNullableSByte((SByte?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Int16] = (writer, value) => writer.WriteInt16Optimized((Int16)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableInt16] = (writer, value) => writer.WriteNullableInt16Optimized((Int16?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Int32] = (writer, value) => writer.WriteInt32Optimized((Int32)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableInt32] = (writer, value) => writer.WriteNullableInt32Optimized((Int32?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Int64] = (writer, value) => writer.WriteInt64Optimized((Int64)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableInt64] = (writer, value) => writer.WriteNullableInt64Optimized((Int64?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Byte] = (writer, value) => writer.WriteByte((Byte)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableByte] = (writer, value) => writer.WriteNullableByte((Byte?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.UInt16] = (writer, value) => writer.WriteUInt16Optimized((UInt16)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableUInt16] = (writer, value) => writer.WriteNullableUInt16Optimized((UInt16?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.UInt32] = (writer, value) => writer.WriteUInt32Optimized((UInt32)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableUInt32] = (writer, value) => writer.WriteNullableUInt32Optimized((UInt32?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.UInt64] = (writer, value) => writer.WriteUInt64Optimized((UInt64)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableUInt64] = (writer, value) => writer.WriteNullableUInt64Optimized((UInt64?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Single] = (writer, value) => writer.WriteSingle((Single)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableSingle] = (writer, value) => writer.WriteNullableSingle((Single?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Double] = (writer, value) => writer.WriteDouble((Double)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableDouble] = (writer, value) => writer.WriteNullableDouble((Double?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Decimal] = (writer, value) => writer.WriteDecimalOptimized((Decimal)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableDecimal] = (writer, value) => writer.WriteNullableDecimalOptimized((Decimal?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.DateTime] = (writer, value) => writer.WriteDateTimeOptimized((DateTime)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableDateTime] = (writer, value) => writer.WriteNullableDateTimeOptimized((DateTime?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.TimeSpan] = (writer, value) => writer.WriteTimeSpanOptimized((TimeSpan)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableTimeSpan] = (writer, value) => writer.WriteNullableTimeSpanOptimized((TimeSpan?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.BitArray] = (writer, value) => writer.WriteBitArray((BitArray)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.BitVector32] = (writer, value) => writer.WriteBitVector32Optimized((BitVector32)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableBitVector32] = (writer, value) => writer.WriteNullableBitVector32Optimized((BitVector32?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Guid] = (writer, value) => writer.WriteGuid((Guid)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableGuid] = (writer, value) => writer.WriteNullableGuid((Guid?)value);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.Char] = (writer, value) => writer.WriteChar((Char)value!);
			WriteOptimizedActionsByTypeId[(int)PropertyTypeId.NullableChar] = (writer, value) => writer.WriteNullableChar((Char?)value);
		}


		#endregion |   Constructors and Initialization   |

		#region |   Private Properties   |

		///// <summary>
		///// Gets the bit stream in which to write bit data.
		///// </summary>
		//private BitStream BitStream
		//{
		//	get { return this.bitStream; }
		//}

		//private UniqueStringList StringLookup
		//{
		//	get
		//	{
		//		if (this.stringLookup == null)
		//			this.stringLookup = new UniqueStringList();

		//		return this.stringLookup;
		//	}
		//}


		#endregion |   Private Properties   |

		#region |   Public Properties   |

		/// <summary>
		/// Gets the specified character encoding.
		/// </summary>
		public Encoding CharacterEncoding { get => this.characterEncoding; set => this.characterEncoding = value; }

		/// <summary>
		/// Gats tha underlying writer that holds the written data.
		/// </summary>
		public BinaryEndiannessWriter BaseWriter => this.writer;

		#endregion |   Public Properties   |

		#region |   Public Methods  |

		///// <summary>
		///// Gets the underlying buffer of the written data.
		///// </summary>
		///// <returns></returns>
		//public ReadOnlySequence<byte> GetBuffer() => this.writer.BaseSequenceWriter.GetBuffer();

		public List<ArraySegment<byte>> ToArraySegmentList() => this.writer.Provider.ToArraySegmentList();


		public void WriteDataTo(IBufferWriter<byte> writer) => this.writer.Provider.WriteTo(writer);

		///// <summary>
		///// Returns a byte[] containing all of the serialized data from a stream.
		///// Only call this method once all of the data has been serialized.
		///// </summary>
		///// <returns>A byte[] containing all serialized data.</returns>
		//public byte[] ToArray() => this.writer.BaseWriter.ToArray();

		//public SpanSegment<byte> AsSpanSequence() => this.writer.Provider.AsSpanSequence();


		#endregion |   Public Methods  |

		#region |   ISerializationWriter - Genaral Part Implementation  |

		public long BytesWritten => this.writer.Provider.BytesWritten;

		public object GetCurrentPositionToken() => this.writer.Provider.GetCurrentPositionToken();

		public void WriteAt(object positionToken, byte value) => this.writer.Provider.WriteAt(positionToken, value);

		#endregion |   ISerializationWriter - Genaral Part Implementation  |

		#region |   ISerializationWriter Implementation   |

		#region |   Primitive Signed Types: bool, sbyte, short, int, long   |

		/// <summary>
		/// Writes a one-byte Boolean value to the current stream, with 0 representing false and 1 representing true.
		/// Stored Size: 1 bit.
		/// </summary>
		/// <param name="value">The Boolean value to write (0 or 1).</param>
		public void WriteBoolean(bool value) => this.bitWriter.WriteBit(value);

		/// <summary>
		/// Writes a Nullable Boolean to the stream.
		/// Stored Size: 2 bits.
		/// </summary>
		/// <param name="value">The Nullable Boolean value to write.</param>
		public void WriteNullableBoolean(bool? value) => this.WriteNullable<bool>(value, (item) => this.WriteBoolean(item));

		/// <summary>
		/// Writes a signed byte to the current stream and advances the stream position by one byte.
		/// </summary>
		/// <param name="value">The signed byte to write.</param>
		public void WriteSByte(sbyte value) => this.writer.WriteByte(unchecked((byte)value));

		/// <summary>
		/// Writes a Nullable SByte to the stream.
		/// </summary>
		/// <param name="value">The Nullable SByte value to write.</param>
		public void WriteNullableSByte(sbyte? value) => this.WriteNullable<sbyte>(value, (item) => this.WriteSByte(item));

		/// <summary>
		/// Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.
		/// Stored Size: 2 bytes.
		/// </summary>
		/// <param name="value">The two-byte signed integer to write.</param>
		public void WriteInt16(short value) => this.writer.WriteInt16(value);

		/// <summary>
		/// Writes a Nullable Int16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int16 value to write.</param>
		public void WriteNullableInt16(short? value) => this.WriteNullable<short>(value, (item) => this.WriteInt16(item));

		/// <summary>
		/// Write an Int16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Int16 to store.</param>
		public void WriteInt16Optimized(short value) => this.writer.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Write an Nullable Int16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable Int16 to store.</param>
		public void WriteNullableInt16Optimized(short? value) => this.WriteNullable<short>(value, (item) => this.WriteInt16Optimized(item));

		/// <summary>
		/// Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.
		/// Stored Size: 4 bytes.
		/// </summary>
		/// <param name="value">The four-byte signed integer to write.</param>
		public void WriteInt32(int value) => this.writer.WriteInt32(value);

		/// <summary>
		/// Writes a Nullable Int32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int32 value to write.</param>
		public void WriteNullableInt32(int? value) => this.WriteNullable<int>(value, (item) => this.WriteInt32(item));

		/// <summary>
		/// Write an Int32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Int32 to store.</param>
		public void WriteInt32Optimized(int value) => this.writer.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Write an Nullable Int32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable Int32 to store.</param>
		public void WriteNullableInt32Optimized(int? value) => this.WriteNullable<int>(value, (item) => this.WriteInt32Optimized(item));

		/// <summary>
		/// Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.
		/// Stored Size: 4 bytes.
		/// </summary>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public void WriteInt64(long value) => this.writer.WriteInt64(value);

		/// <summary>
		/// Writes a Nullable Int64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int64 value to write.</param>
		public void WriteNullableInt64(long? value) => this.WriteNullable<long>(value, (item) => this.WriteInt64(item));

		/// <summary>
		/// Write an Int64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Int64 to store.</param>
		public void WriteInt64Optimized(long value) => this.writer.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Write an Nullable Int64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable Int64 to store.</param>
		public void WriteNullableInt64Optimized(long? value) => this.WriteNullable<long>(value, (item) => this.WriteInt64Optimized(item));

		#endregion |   Primitive Signed Types: bool, sbyte, short, int, long   |

		#region |   Unsigned Primitives: byte, ushort, uint, ulong   |

		/// <summary>
		/// Writes an unsigned byte to the current stream and advances the stream position by one byte.
		/// Stored Size: 1 byte.
		/// </summary>
		/// <param name="value">The unsigned byte to write.</param>
		public void WriteByte(byte value) => this.writer.WriteByte(value);

		/// <summary>
		/// Writes a Nullable Byte to the stream.
		/// </summary>
		/// <param name="value">The Nullable Byte value to write.</param>
		public void WriteNullableByte(byte? value) => this.WriteNullable<byte>(value, (item) => this.WriteByte(item));

		/// <summary>
		/// Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public void WriteUInt16(ushort value) => this.writer.WriteUInt16(value);

		/// <summary>
		/// Writes a Nullable UInt16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt16 value to write.</param>
		public void WriteNullableUInt16(ushort? value) => this.WriteNullable<ushort>(value, (item) => this.WriteUInt16(item));

		/// <summary>
		/// Write an UInt16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The UInt16 to store.</param>
		public void WriteUInt16Optimized(ushort value) => this.writer.Write7BitEncodedUInt64(value);

		/// <summary>
		/// Write an Nullable UInt16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable UInt16 to store.</param>
		public void WriteNullableUInt16Optimized(ushort? value) => this.WriteNullable<ushort>(value, (item) => this.WriteUInt16Optimized(item));

		/// <summary>
		/// Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public void WriteUInt32(uint value) => this.writer.WriteUInt32(value);

		/// <summary>
		/// Writes a Nullable UInt32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt32 value to write.</param>
		public void WriteNullableUInt32(uint? value) => this.WriteNullable<uint>(value, (item) => this.WriteUInt32(item));

		/// <summary>
		/// Write an UInt32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The UInt32 to store.</param>
		public void WriteUInt32Optimized(uint value) => this.writer.Write7BitEncodedUInt64(value);

		/// <summary>
		/// Write an Nullable UInt32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable UInt32 to store.</param>
		public void WriteNullableUInt32Optimized(uint? value) => this.WriteNullable<uint>(value, (item) => this.WriteUInt32Optimized(item));

		/// <summary>
		/// Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public void WriteUInt64(ulong value) => this.writer.WriteUInt64(value);

		/// <summary>
		/// Writes a Nullable UInt64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt64 value to write.</param>
		public void WriteNullableUInt64(ulong? value) => this.WriteNullable<ulong>(value, (item) => this.WriteUInt64(item));

		/// <summary>
		/// Write an UInt64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The UInt64 to store.</param>
		public void WriteUInt64Optimized(ulong value) => this.writer.Write7BitEncodedUInt64(value);

		/// <summary>
		/// Write an Nullable UInt64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable UInt64 to store.</param>
		public void WriteNullableUInt64Optimized(ulong? value) => this.WriteNullable<ulong>(value, (item) => this.WriteUInt64Optimized(item));

		#endregion |   Unsigned Primitives: byte, ushort, uint, ulong   |

		#region |   Floating Point & Decimal Types: Half, float, double, decimal   |


		/// <summary>
		/// Write a <see cref="Half" /> to the sequence (2 bytes).
		/// </summary>
		/// <param name="value">The Half value</param>
		public void WriteHalf(Half value) => this.WriteUInt16((ushort)value);

		/// <summary>
		/// Write a Nullable <see cref="Half" /> to the sequence.
		/// </summary>
		/// <param name="value">The Half value</param>
		public void WriteNullableHalf(Half? value) => this.WriteNullable<Half>(value, (item) => this.WriteHalf(item));

		/// <summary>
		/// Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public void WriteSingle(float value) => this.writer.WriteSingle(value);

		/// <summary>
		/// Writes a Nullable Single to the stream.
		/// </summary>
		/// <param name="value">The Nullable Single value to write.</param>
		public void WriteNullableSingle(float? value) => this.WriteNullable<float>(value, (item) => this.WriteSingle(item));

		/// <summary>
		///  Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public void WriteDouble(double value) => this.writer.WriteDouble(value);

		/// <summary>
		/// Writes a Nullable Double to the stream.
		/// </summary>
		/// <param name="value">The Nullable Double value to write.</param>
		public void WriteNullableDouble(double? value) => this.WriteNullable<double>(value, (item) => this.WriteDouble(item));

		/// <summary>
		/// Writes a decimal value to the current stream and advances the stream position by sixteen bytes.
		/// </summary>
		/// <param name="value">The decimal value to write.</param>
		public void WriteDecimal(decimal value) => this.writer.WriteDecimal(value);

		/// <summary>
		/// Writes a Nullable Decimal to the stream.
		/// </summary>
		/// <param name="value">The Nullable Decimal value to write.</param>
		public void WriteNullableDecimal(decimal? value) => this.WriteNullable<decimal>(value, (item) => this.WriteDecimal(item));

		/// <summary>
		/// Writes a Decimal value into the stream using the fewest number of bytes possible.
		/// Stored Size: 1 byte to 14 bytes (.Net is 16 bytes)
		/// Restrictions: None
		/// </summary>
		/// <param name="value">The Decimal value to store</param>
		public void WriteDecimalOptimized(decimal value)
		{
			var data = Decimal.GetBits(value);
			var scale = (byte)(data[3] >> 16);
			byte flags = 0;

			if (scale != 0 && !this.preserveDecimalScale && this.optimizeForSize)
			{
				var normalized = Decimal.Truncate(value);

				if (normalized == value)
				{
					data = Decimal.GetBits(normalized);
					scale = 0;
				}
			}

			if ((data[3] & -2147483648) != 0)
				flags |= 0x01;

			if (scale != 0)
				flags |= 0x02;

			if (data[0] == 0)
				flags |= 0x04;
			else if (data[0] <= HighestOptimizable32BitValue && data[0] >= 0)
				flags |= 0x20;

			if (data[1] == 0)
				flags |= 0x08;
			else if (data[1] <= HighestOptimizable32BitValue && data[1] >= 0)
				flags |= 0x40;

			if (data[2] == 0)
				flags |= 0x10;
			else if (data[2] <= HighestOptimizable32BitValue && data[2] >= 0)
				flags |= 0x80;

			this.WriteByte(flags);

			if (scale != 0)
				this.WriteByte(scale);

			if ((flags & 0x04) == 0)
			{
				if ((flags & 0x20) != 0)
					this.WriteInt32Optimized(data[0]);
				else
					this.WriteInt32(data[0]);
			}

			if ((flags & 0x08) == 0)
			{
				if ((flags & 0x40) != 0)
					this.WriteInt32Optimized(data[1]);
				else
					this.WriteInt32(data[1]);
			}

			if ((flags & 0x10) == 0)
			{
				if ((flags & 0x80) != 0)
					this.WriteInt32Optimized(data[2]);
				else
					this.WriteInt32(data[2]);
			}
		}

		/// <summary>
		/// Writes a Nullable Decimal value into the stream using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable Decimal value to store.</param>
		public void WriteNullableDecimalOptimized(decimal? value) => this.WriteNullable<decimal>(value, (item) => this.WriteDecimalOptimized(item));

		#endregion |   Floating Point & Decimal Types: Half, float, double, decimal   |

		#region |   Date & Time: DateTime, TimeSpan   |

		/// <summary>
		/// Writes a DateTime value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void WriteDateTime(DateTime value) => this.WriteInt64(value.ToBinary());

		/// <summary>
		/// Writes a Nullable DateTime value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The Nullable DateTime value to write.</param>
		public void WriteNullableDateTime(DateTime? value) => this.WriteNullable<DateTime>(value, (item) => this.WriteDateTime(item));

		/// <summary>
		/// Writes a DateTime value into the stream using the fewest number of bytes possible.
		/// Stored Size: 3 bytes to 7 bytes (.Net is 8 bytes)
		/// Notes:
		/// A DateTime containing only a date takes 3 bytes
		/// (except a .NET 2.0 Date with a specified DateTimeKind which will take a minimum
		/// of 5 bytes - no further optimization for this situation felt necessary since it
		/// is unlikely that a DateTimeKind would be specified without hh:mm also)
		/// Date plus hh:mm takes 5 bytes.
		/// Date plus hh:mm:ss takes 6 bytes.
		/// Date plus hh:mm:ss.fff takes 7 bytes.
		/// </summary>
		/// <param name="value">The DateTime value to store. Must not contain sub-millisecond data.</param>
		public void WriteDateTimeOptimized(DateTime value)
		{
			var dateMask = new BitVector32();
			dateMask[DateYearMask] = value.Year;
			dateMask[DateMonthMask] = value.Month;
			dateMask[DateDayMask] = value.Day;

			var initialData = (int)value.Kind;
			var writeAdditionalData = value != value.Date;

			writeAdditionalData |= initialData != 0;
			dateMask[DateHasTimeOrKindMask] = writeAdditionalData ? 1 : 0;

			// Store 3 bytes of Date information
			var dateMaskData = dateMask.Data;
			this.WriteByte((byte)dateMaskData);
			this.WriteByte((byte)(dateMaskData >> 8));
			this.WriteByte((byte)(dateMaskData >> 16));

			if (writeAdditionalData)
				this.EncodeTimeSpan(value.TimeOfDay, true, initialData);
		}

		/// <summary>
		/// Writes a Nullable DateTime value into the stream using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable DateTime value to write.</param>
		public void WriteNullableDateTimeOptimized(DateTime? value) => this.WriteNullable<DateTime>(value, (item) => this.WriteDateTimeOptimized(item));

		/// <summary>
		/// Writes a TimeSpan value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The TimeSpan value to write.</param>
		public void WriteTimeSpan(TimeSpan value) => this.WriteInt64(value.Ticks);

		/// <summary>
		/// Writes a Nullable TimeSpan value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The Nullable TimeSpan value to write.</param>
		public void WriteNullableTimeSpan(TimeSpan? value) => this.WriteNullable<TimeSpan>(value, (item) => this.WriteTimeSpan(item));

		/// <summary>
		/// Writes a TimeSpan value into the stream using the fewest number of bytes possible.
		/// Stored Size: 2 bytes to 8 bytes (.Net is 8 bytes)
		/// Notes:
		/// hh:mm (time) are always stored together and take 2 bytes.
		/// If seconds are present then 3 bytes unless (time) is not present in which case 2 bytes
		/// since the seconds are stored in the minutes position.
		/// If milliseconds are present then 4 bytes.
		/// In addition, if days are present they will add 1 to 4 bytes to the above.
		/// </summary>
		/// <param name="value">The TimeSpan value to store. Must not contain sub-millisecond data.</param>
		public void WriteTimeSpanOptimized(TimeSpan value) => this.EncodeTimeSpan(value, false, 0);

		/// <summary>
		/// Writes a Nullable TimeSpan value into the stream using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable TimeSpan value to write.</param>
		public void WriteNullableTimeSpanOptimized(TimeSpan? value) => this.WriteNullable<TimeSpan>(value, (item) => this.WriteTimeSpanOptimized(item));

		#endregion |   Date & Time: DateTime, TimeSpan   |

		#region |   Specific Types: BitArray, BitVector32, Guid   |

		/// <summary>
		/// Writes a BitArray value into the stream using the fewest number of bytes possible.
		/// Stored Size: 1 byte upwards depending on data content
		/// Notes:
		/// A null BitArray takes 1 byte.
		/// An empty BitArray takes 2 bytes.
		/// </summary>
		/// <param name="value">The BitArray value to write.</param>
		public void WriteBitArray(BitArray value)
		{
			//this.WriteNullable(
			//    value,
			//    (item) =>
			//    {
			this.WriteInt32Optimized(value.Length);

			//               var data = new byte[(item.Length + 7) / 8];

			//item.CopyTo(data, 0);

			//for (int i = 0; i < data.Length; i++)
			//	this.WriteByte(data[i]);

			for (int i = 0; i < value.Length; i++)
				this.WriteBoolean(value[i]);
			//});
		}

		/// <summary>
		/// Writes a BitArray value into the stream using the fewest number of bytes possible.
		/// Stored Size: 1 byte upwards depending on data content
		/// Notes:
		/// A null BitArray takes 1 byte.
		/// An empty BitArray takes 2 bytes.
		/// </summary>
		/// <param name="value">The BitArray value to write.</param>
		public void WriteBitArrayOptimized(BitArray value)
		{
			//this.writer.Write7BitEncodedUInt64(unchecked((ulong)(value.Length)));
			this.WriteInt32Optimized(value.Length);

			if (value.Length > 0)
			{
				var data = new byte[(value.Length + 7) / 8];

				value.CopyTo(data, 0);
				this.writer.WriteByteArray(data);
			}
		}

		/// <summary>
		/// Writes a BitVector32 into the stream.
		/// Stored Size: 4 bytes.
		/// </summary>
		/// <param name="value">The BitVector32 to write.</param>
		public void WriteBitVector32(BitVector32 value) => this.WriteInt32(value.Data);

		/// <summary>
		/// Writes a Nullable BitVector32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable BitVector32 value to write.</param>
		public void WriteNullableBitVector32(BitVector32? value) => this.WriteNullable<BitVector32>(value, (item) => this.WriteBitVector32(item));

		/// <summary>
		/// Writes a BitVector32 into the stream using the fewest number of bytes possible.
		/// Stored Size: 1 to 4 bytes. (.Net is 4 bytes)
		///  1 to  7 bits takes 1 byte
		///  8 to 14 bits takes 2 bytes
		/// 15 to 21 bits takes 3 bytes
		/// 22 to 28 bits takes 4 bytes
		/// -------------------------------------------------------------------
		/// 29 to 32 bits takes 5 bytes - use Write(BitVector32) method instead
		/// 
		/// Try to order the BitVector32 masks so that the highest bits are least-likely
		/// to be set.
		/// </summary>
		/// <param name="value">The BitVector32 to store. Must not use more than 28 bits.</param>
		public void WriteBitVector32Optimized(BitVector32 value) => this.WriteInt32Optimized(value.Data);

		/// <summary>
		/// Writes a Nullable BitVector32 to the stream using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable BitVector32 value to write.</param>
		public void WriteNullableBitVector32Optimized(BitVector32? value) => this.WriteNullable<BitVector32>(value, (item) => this.WriteBitVector32Optimized(item));

		/// <summary>
		/// Writes a Guid into the stream.
		/// Stored Size: 16 bytes.
		/// </summary>
		/// <param name="value"></param>
		public void WriteGuid(Guid value) => this.writer.WriteByteArray(value.ToByteArray());

		/// <summary>
		/// Writes a Nullable Guid to the stream.
		/// </summary>
		/// <param name="value">The Nullable Guid value to write.</param>
		public void WriteNullableGuid(Guid? value) => this.WriteNullable<Guid>(value, (item) => this.WriteGuid(item));

		#endregion |   Specific Types: BitArray, BitVector32, Guid   |

		#region |   Char & String   |

		/// <summary>
		///  Writes a Unicode character to the current stream and advances the current position of the stream in accordance with the Encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="value">The non-surrogate, Unicode character to write.</param>
		public virtual void WriteChar(char value)
        {
#if NETSTANDARD
			if (!MinimalisticRune.TryCreate(value, out MinimalisticRune rune)) // optimistically assume UTF-8 code path (which uses Rune) will be hit
#else
			if (!Rune.TryCreate(value, out Rune rune)) // optimistically assume UTF-8 code path (which uses Rune) will be hit
#endif
                throw new ArgumentException("Surrogates Not Allowed As Single Char");
            
			Span<byte> span = stackalloc byte[8];

			if (this.useFastUtf8)
			{
				int length = rune.EncodeToUtf8(span);

				this.WriteSpan(span.Slice(0, length));
			}
			else
			{
				byte[]? array = null;
				int maxByteCount = this.CharacterEncoding.GetMaxByteCount(1);

				if (maxByteCount > span.Length)
				{
					array = ArrayPool<byte>.Shared.Rent(maxByteCount);
					span = array;
				}

				int actualByteCount = this.CharacterEncoding.GetBytes(MemoryMarshal.CreateReadOnlySpan(ref value, 1), span);

				this.WriteSpan(span.Slice(0, actualByteCount));
				
				//int actualByteCount = this.CharacterEncoding.GetBytes(new ReadOnlySpan<char>(new char[] { value }), buffer);
				//this.WriteSpanDirect(buffer[..actualByteCount]); // this.WriteSpanDirect(buffer.Slice(0, actualByteCount));

				if (array != null)
					ArrayPool<byte>.Shared.Return(array);
			}
        }

		/// <summary>
		/// Writes a Nullable Char to the stream.
		/// </summary>
		/// <param name="value">The Nullable Char value to write.</param>
		public void WriteNullableChar(char? value) => this.WriteNullable<char>(value, (item) => this.WriteChar(item));

		public unsafe void WriteString2(string value)
		{
			try
			{
				if (value.IsNullOrEmpty()) // value != null || value.Length == 0;
				{
					this.WriteBoolean(true);
				}
				else
				{
					this.WriteBoolean(false);

					int byteCount = this.CharacterEncoding.GetByteCount(value);

					this.WriteInt32Optimized(byteCount); //this.Write7BitEncodedInt(byteCount);

					if (this.largeByteBuffer == null)
					{
						this.largeByteBuffer = new byte[256];
						this.maxChars = this.largeByteBuffer.Length / this.CharacterEncoding.GetMaxByteCount(1);
					}

					if (byteCount <= this.largeByteBuffer.Length)
					{
						this.CharacterEncoding.GetBytes(value, 0, value.Length, this.largeByteBuffer, 0);
						this.writer.WriteByteArray(this.largeByteBuffer, 0, byteCount);

						return;
					}

					int num = 0;
					int num2 = value.Length;

					while (num2 > 0)
					{
						int num3 = ((num2 > this.maxChars) ? this.maxChars : num2);

						if (num < 0 || num3 < 0 || num > checked(value.Length - num3))
							throw new ArgumentOutOfRangeException("charCount");

						int bytes2;

						fixed (char* ptr = value)
						{
							fixed (byte* bytes = &this.largeByteBuffer[0])
							{
								bytes2 = this.CharacterEncoding.GetBytes((char*)checked(unchecked((nuint)ptr) + unchecked((nuint)checked(unchecked((nint)num) * (nint)2))), num3, bytes, this.largeByteBuffer.Length); //, flush: num3 == num2);
							}
						}

						this.writer.WriteByteArray(this.largeByteBuffer, 0, bytes2);

						num += num3;
						num2 -= num3;
					}
				}
			}
			catch (Exception ex)
			{
				value = value;
			}
		}

		/// <summary>
		/// Writes a length-prefixed string to this stream in the current encoding and advances the current position of the stream in accordance
		/// with the encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="value">The string value to write.</param>
		public void WriteStringX(string value)
		{
			////StreamSequenceWriter ssw = new StreamSequenceWriter()
			//BinaryWriter bw = new BinaryWriter(new MemoryStream());

			//bw.Write(value);

			//this.WriteNullable<string>(value, (item) => this.writer.WriteBytes(this.CharacterEncoding.GetBytes(value)));

			if (value.IsNullOrEmpty()) // value != null || value.Length == 0;
			{
				this.WriteBoolean(true);
			}
			else
			{
				this.WriteBoolean(false);

				// Common: UTF-8, small string, avoid 2-pass calculation
				// Less common: UTF-8, large string, avoid 2-pass calculation
				// Uncommon: excessively large string or not UTF-8

				int actualByteCount;

                if (this.useFastUtf8)
				{
					if (value.Length <= 127 / 3)
					{
						// Max expansion: each char -> 3 bytes, so 127 bytes max of data, +1 for length prefix
						Span<byte> buffer = stackalloc byte[128];
						actualByteCount = this.CharacterEncoding.GetBytes(value, buffer.Slice(1));
						
						buffer[0] = (byte)actualByteCount; // bypass call to Write7BitEncodedInt
						this.WriteSpan(buffer.Slice(0, actualByteCount + 1 /* length prefix */));

						return;
					}
					else if (value.Length <= MaxArrayPoolRentalSize / 3)
					{
						byte[] rented = ArrayPool<byte>.Shared.Rent(value.Length * 3); // max expansion: each char -> 3 bytes
						actualByteCount = this.CharacterEncoding.GetBytes(value, rented);
						
						this.WriteInt32Optimized(actualByteCount);
						this.writer.WriteByteArray(rented, 0, actualByteCount);
						ArrayPool<byte>.Shared.Return(rented);

						return;
					}
				}

				// Slow path: not fast UTF-8, or data is very large. We need to fall back
				// to a 2-pass mechanism so that we're not renting absurdly large arrays.
				actualByteCount = this.CharacterEncoding.GetByteCount(value);
				
				this.WriteInt32Optimized(actualByteCount);
				this.WriteCharsCommonWithoutLengthPrefix(value);
			}
		}

		public unsafe void WriteString3(string value)
		{
			if (value.IsNullOrEmpty()) // value != null || value.Length == 0;
			{
				this.WriteBoolean(true);
			}
			else
			{
				this.WriteBoolean(false);

				int length = this.CharacterEncoding.GetByteCount(value);
				var b64span = value.AsSpan(); // convert string into a "span"

				//prepare resulting buffer that receives the data
				//in base64 every char encodes 6 bits, so 4 chars = 3 bytes
				Span<byte> buffer = stackalloc byte[((b64span.Length * 3) + 3) / 4];

				//call TryFromBase64Chars which accepts Span as input
				if (Convert.TryFromBase64Chars(b64span, buffer, out int bytesWritten))
				{
					this.WriteInt32Optimized(bytesWritten);
					this.WriteByteArray(buffer.Slice(0, bytesWritten).ToArray());
				}
				else
				{
					this.WriteInt32Optimized(0); // <- String.Empty
				}
			}
		}


		[SecuritySafeCritical]
		//[__DynamicallyInvokable]
		public unsafe void WriteString(string value)
		{
			if (value.IsNullOrEmpty()) // value != null || value.Length == 0;
			{
				this.WriteBoolean(true);
			}
			else
			{
				this.WriteBoolean(false);

				int byteCount = this.CharacterEncoding.GetByteCount(value);

				if (byteCount > 6000)
					byteCount = byteCount;

				this.Write7BitEncodedInt(byteCount);

				if (this.largeByteBuffer == null)
				{
					this.largeByteBuffer = new byte[256];
					this.maxChars = this.largeByteBuffer.Length / this.CharacterEncoding.GetMaxByteCount(1);
				}

				if (byteCount <= this.largeByteBuffer.Length)
				{
					this.CharacterEncoding.GetBytes(value, 0, value.Length, this.largeByteBuffer, 0);
					this.writer.WriteByteArray(this.largeByteBuffer, 0, byteCount);

					return;
				}

				int num = 0;
				int num2 = value.Length;

				while (num2 > 0)
				{
					int num3 = ((num2 > this.maxChars) ? this.maxChars : num2);

					if (num < 0 || num3 < 0 || checked(num + num3) > value.Length)
						throw new ArgumentOutOfRangeException("charCount");

					int bytes2;

					fixed (char* ptr = value)
					{
						fixed (byte* bytes = this.largeByteBuffer)
						{
							bytes2 = this.CharacterEncoding.GetBytes((char*)checked(unchecked((nuint)ptr) + unchecked((nuint)checked(unchecked((nint)num) * (nint)2))), num3, bytes, this.largeByteBuffer.Length); //, flush: num3 == num2);
						}
					}

					this.writer.WriteByteArray(this.largeByteBuffer, 0, bytes2);
					num += num3;
					num2 -= num3;
				}
			}
		}

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// </summary>
		/// <param name="value">The 32-bit integer to be written</param>
		private void Write7BitEncodedInt(int value)
		{
			uint num;
			
			for (num = (uint)value; num >= 128; num >>= 7)
				Write((byte)(num | 0x80u));

			Write((byte)num);
		}



		/// <summary>
		/// Writes a length-prefixed string to this stream using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The string value to write.</param>
		public void WriteStringOptimized(string value)
		{
			if (this.useStringLookupOptimization)
				this.WriteStringLookupOptimized(value);
			else
				this.WriteString(value);
		}

		/// <summary>
		/// Writes a length-prefixed string to this stream using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The string value to write.</param>
		public void WriteStringLookupOptimized(string value)
		{
			if (value.IsNullOrEmpty())
			{
				this.bitWriter.Write(0, 3); // 0 -> Null or Empty
				
				return;
			}

			//if (value.Length == 0)
			//{
			//	this.bitWriter.Write(1, 3); // 1 -> String.Empty 
				
			//	return;
			//}

			if (value.Length == 1)
			{
				var singleChar = value[0];

				switch (singleChar)
				{
					case 'Y':
						
						this.bitWriter.Write(1, 3); // 1 -> 'Y'
						
						return;

					case 'N':
						
						this.bitWriter.Write(2, 3); // 2 -> 'N'
						
						return;

					case ' ':
						
						this.bitWriter.Write(3, 3); // 3 -> ' '
						
						return;

					default:
						
						this.bitWriter.Write(4, 3); // 4 -> single char
						this.WriteChar(singleChar);
						
						return;
				}
			}

			int stringIndex;

			if (this.StringLookup.Add(value, out stringIndex))
			{
				// string added into lookup
				this.bitWriter.Write(5, 3); // 7 -> string is new, write it
				this.WriteString(value);
			}
			else
			{
				// string already exists
				this.bitWriter.Write(6, 3); // 6 -> string already exists in lookup, write string index
				this.WriteInt32Optimized(stringIndex);
			}
		}

		#endregion |   Char & String   |

		#region |   Bits, Array   |

		/// <summary>
		/// Write single bits, up to 8 bits. Number of to write reaed is specified by count property.
		/// </summary>
		/// <param name="value">The bits stored in a byte.</param>
		/// <param name="count">The number of bits to read, reange 1-8.</param>
		public void WriteBits(byte value, int count) => this.bitWriter.Write(value, count);

		/// <summary>
		/// Writes a Byte[] into the stream.
		/// </summary>
		/// <param name="value">The byte array to write.</param>
		public void WriteByteArray(byte[] values) => this.writer.WriteByteArray(values); // this.WriteArray<byte>(values, this.WriteByte);


		/// <summary>
		/// Write single bits, up to 8 bits. Number of to write reaed is specified by count property.
		/// </summary>
		/// <param name="values">The byte array to write.</param>
		/// <param name="offset">The array offset</param>
		/// <param name="count">Tne number of bytes to write</param>
		public void WriteByteArray(byte[] values, int offset, int count) => this.writer.WriteByteArray(values, offset, count);
	
		#endregion |   Bits, Array   |

		#region |   Span  |

		public void WriteSpan(Span<byte> span) => this.writer.WriteSpan(span, span.Length);

		public void WriteSpan(Span<byte> span, int count) => this.writer.WriteSpan(span, count);

		//public void WriteSpanSequence(ReadOnlySpanSegment<byte> first) => this.writer.WriteSpanSequence(first);

		//public void WriteSpanSequence(SpanSegment<byte> first) => this.writer.WriteSpanSequence(first);

		#endregion |   Span  |

		#region |   Type & Object  |


		/// <summary>
		/// Stores a Type object into the stream.
		/// Stored Size: Depends on the length of the Type's name and whether the fullyQualified parameter is set.
		/// A null Type takes 1 byte.
		/// </summary>
		/// <param name="value">The Type to write.</param>
		/// <param name="fullyQualified">true to write the AssemblyQualifiedName or false to write the FullName. </param>
		public void WriteType(Type value) => this.WriteType(value, fullyQualified: true);

		/// <summary>
		/// Stores a Type object into the stream.
		/// Stored Size: Depends on the length of the Type's name and whether the fullyQualified parameter is set.
		/// A null Type takes 1 byte.
		/// </summary>
		/// <param name="value">The Type to write.</param>
		/// <param name="fullyQualified">true to write the AssemblyQualifiedName or false to write the FullName. </param>
		public void WriteType(Type value, bool fullyQualified) => this.WriteNullable<Type>(value, (item) => this.WriteString((fullyQualified) ? item.AssemblyQualifiedName : item.FullName));

		//public void WriteTypeOptimized(Type value) => this.WriteType(value); // No any optimization on Type

		///// <summary>
		///// Stores a non-null Type object into the stream.
		///// Stored Size: Depends on the length of the Type's name.
		///// If the type is a System type (mscorlib) then it is stored without assembly name information,
		///// otherwise the Type's AssemblyQualifiedName is used.
		///// </summary>
		///// <param name="value">The Type to store. Must not be null.</param>
		///// <param name="fullyQualified">true to write the AssemblyQualifiedName or false to write the FullName. </param>
		//public void WriteTypeOptimized(Type value) => this.WriteNullable<Type>(value, (item) => this.WriteStringOptimized((item.AssemblyQualifiedName.IndexOf(", mscorlib,") == -1) ? item.AssemblyQualifiedName : item.FullName));

		///// <summary>
		///// Stores an object into the stream. Use this method only if cannot use any of other method available 
		///// or if you for any reason not able to manualy serialize it using serializating its properties.
		///// </summary>
		///// <param name="value">The object to store.</param>
		//public void WriteObject(object value)
		//{
		//	this.WriteNullable<object>(value,
		//							   (item) =>
		//							   {
		//								   this.WriteTypeOptimized(item.GetType());
		//								   SimpleWriter.WriteObject(this.BaseStream, item.GetType(), item);
		//							   });
		//}

		///// <summary>
		///// Stores an object into the stream. Use this method only if cannot use any of other method available 
		///// or if you for any reason not able to manualy serialize it using serializating its properties.
		///// </summary>
		///// <param name="objectType">The type of the object.</param>
		///// <param name="value">The object to store.</param>
		//public void WriteObject(Type objectType, object value)
		//{
		//	SimpleWriter.WriteObject(this.BaseStream, objectType, value);
		//}

		///// <summary>
		///// Writes a not-null object[] into the stream.
		///// </summary>
		///// <param name="values">The object[] to store. Must not be null.</param>
		//public void WriteObjectArray(object[] values)
		//{
		//	this.WriteArray<object>(values, this.WriteObject);
		//}

		#endregion |   Type & Object   |

		#endregion |   ISerializationWriter Implementation   |

		#region |   Write((dynamic) value) Methods Variant   |

		/// <summary>
		/// Writes a value to the current stream.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to write.</param>
		public void Write<T>(T value) => this.Write((dynamic)value!);
		//public void Write<T>(T value) where T : notnull => this.Write((dynamic)value);
		//public void Write<T>(object? value) where T : notnull => this.Write((dynamic)value!);

		/// <summary>
		/// Writes a value to the current stream using the fewest number of bytes possible.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		/// <param name="value">The value to write.</param>
		public void WriteOptimized<T>(T value) => this.WriteOptimized((dynamic)value!);

		#region |   Primitive Signed Types: bool, sbyte, short, int, long   |

		/// <summary>
		/// Writes a one-byte Boolean value to the current stream, with 0 representing false and 1 representing true.
		/// </summary>
		/// <param name="value">The Boolean value to write (0 or 1).</param>
		public void Write(bool value) => this.WriteBoolean(value);

		/// <summary>
		/// Writes a Nullable Boolean to the stream.
		/// </summary>
		/// <param name="value">The Nullable Boolean value to write.</param>
		public void Write(bool? value) => this.WriteNullableBoolean(value);

		///// <summary>
		///// Writes a one-byte Boolean value to the current stream, with 0 representing false and 1 representing true.
		///// </summary>
		///// <param name="value">The Boolean value to write (0 or 1).</param>
		//public void WriteOptimized(bool value) => this.WriteBoolean(value);

		///// <summary>
		///// Writes a Nullable Boolean to the stream.
		///// </summary>
		///// <param name="value">The Nullable Boolean value to write.</param>
		//public void WriteOptimized(bool? value) => this.WriteBoolean(value);

		/// <summary>
		/// Writes a SByte to the stream.
		/// </summary>
		/// <param name="value">The SByte value to write.</param>
		public void Write(sbyte value) => this.WriteSByte(value);

		/// <summary>
		/// Writes a Nullable SByte to the stream.
		/// </summary>
		/// <param name="value">The Nullable SByte value to write.</param>
		public void Write(sbyte? value) => this.WriteNullableSByte(value);

		///// <summary>
		///// Writes a signed byte to the current stream and advances the stream position by one byte.
		///// </summary>
		///// <param name="value">The signed byte to write.</param>
		//public void WriteOptimized(sbyte value) => this.WriteSByte(value);

		///// <summary>
		///// Writes a Nullable SByte to the stream.
		///// </summary>
		///// <param name="value">The Nullable SByte value to write.</param>
		//public void WriteOptimized(sbyte? value) => this.WriteSByte(value);

		/// <summary>
		/// Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte signed integer to write.</param>
		public void Write(short value) => this.WriteInt16(value);

		/// <summary>
		/// Writes a Nullable Int16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int16 value to write.</param>
		public void Write(short? value) => this.WriteNullableInt16(value);

		/// <summary>
		/// Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte signed integer to write.</param>
		public void WriteOptimized(short value) => this.WriteInt16Optimized(value);

		/// <summary>
		/// Writes a Nullable Int16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int16 value to write.</param>
		public void WriteOptimized(short? value) => this.WriteNullableInt16Optimized(value);

		/// <summary>
		/// Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte signed integer to write.</param>
		public void Write(int value) => this.WriteInt32(value);

		/// <summary>
		/// Writes a Nullable Int32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int32 value to write.</param>
		public void Write(int? value) => this.WriteNullableInt32(value);

		/// <summary>
		/// Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte signed integer to write.</param>
		public void WriteOptimized(int value) => this.WriteInt32Optimized(value);

		/// <summary>
		/// Writes a Nullable Int32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int32 value to write.</param>
		public void WriteOptimized(int? value) => this.WriteNullableInt32Optimized(value);

		/// <summary>
		/// Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public void Write(long value) => this.WriteInt64(value);

		/// <summary>
		/// Writes a Nullable Int64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int64 value to write.</param>
		public void Write(long? value) => this.WriteNullableInt64(value);

		/// <summary>
		/// Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public void WriteOptimized(long value) => this.WriteInt64Optimized(value);

		/// <summary>
		/// Writes a Nullable Int64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int64 value to write.</param>
		public void WriteOptimized(long? value) => this.WriteNullableInt64Optimized(value);

		#endregion |   Primitive Signed Types: bool, sbyte, short, int, long   |

		#region |   Unsigned Primitive Types: byte, ushort, uint, ulong   |

		/// <summary>
		/// Writes an unsigned byte to the current stream and advances the stream position by one byte.
		/// </summary>
		/// <param name="value">The unsigned byte to write.</param>
		public void Write(byte value) => this.WriteByte(value);

		/// <summary>
		/// Writes a Nullable Byte to the stream.
		/// </summary>
		/// <param name="value">The Nullable Byte value to write.</param>
		public void Write(byte? value) => this.WriteNullableByte(value);

		///// <summary>
		///// Writes an unsigned byte to the current stream and advances the stream position by one byte.
		///// </summary>
		///// <param name="value">The unsigned byte to write.</param>
		//public void WriteOptimized(byte value) => this.WriteByte(value);

		///// <summary>
		///// Writes a Nullable Byte to the stream.
		///// </summary>
		///// <param name="value">The Nullable Byte value to write.</param>
		//public void WriteOptimized(byte? value) => this.WriteByte(value);

		/// <summary>
		/// Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public void Write(ushort value) => this.WriteUInt16(value);

		/// <summary>
		/// Writes a Nullable UInt16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt16 value to write.</param>
		public void Write(ushort? value) => this.WriteNullableUInt16(value);

		/// <summary>
		/// Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public void WriteOptimized(ushort value) => this.WriteUInt16Optimized(value);

		/// <summary>
		/// Writes a Nullable UInt16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt16 value to write.</param>
		public void WriteOptimized(ushort? value) => this.WriteNullableUInt16Optimized(value);

		/// <summary>
		/// Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public void Write(uint value) => this.WriteUInt32(value);

		/// <summary>
		/// Writes a Nullable UInt32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt32 value to write.</param>
		public void Write(uint? value) => this.WriteNullableUInt32(value);

		/// <summary>
		/// Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public void WriteOptimized(uint value) => this.WriteUInt32Optimized(value);

		/// <summary>
		/// Writes a Nullable UInt32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt32 value to write.</param>
		public void WriteOptimized(uint? value) => this.WriteNullableUInt32Optimized(value);


		/// <summary>
		/// Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public void Write(ulong value) => this.WriteUInt64(value);

		/// <summary>
		/// Writes a Nullable UInt64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt64 value to write.</param>
		public void Write(ulong? value) => this.WriteNullableUInt64(value);

		/// <summary>
		/// Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public void WriteOptimized(ulong value) => this.WriteUInt64Optimized(value);

		/// <summary>
		/// Writes a Nullable UInt64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt64 value to write.</param>
		public void WriteOptimized(ulong? value) => this.WriteNullableUInt64Optimized(value);

		#endregion |   Unsigned Primitive Types: byte, ushort, uint, ulong   |

		#region |   Floating Point & Decimal Types: float, double, decimal   |

		/// <summary>
		/// Write a <see cref="Half" /> to the sequence (2 bytes).
		/// </summary>
		/// <param name="value">The Half value</param>
		public void Write(Half value) => this.WriteHalf(value);

		/// <summary>
		/// Write a Nullable <see cref="Half" /> to the sequence.
		/// </summary>
		/// <param name="value">The Nullable Half value</param>
		public void Write(Half? value) => this.WriteNullableHalf(value);

		/// <summary>
		/// Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public void Write(float value) => this.WriteSingle(value);

		/// <summary>
		/// Writes a Nullable Single to the stream.
		/// </summary>
		/// <param name="value">The Nullable Single value to write.</param>
		public void Write(float? value) => this.WriteNullableSingle(value);

		///// <summary>
		///// Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.
		///// </summary>
		///// <param name="value">The four-byte floating-point value to write.</param>
		//public void WriteOptimized(float value) => this.WriteSingle(value);

		///// <summary>
		///// Writes a Nullable Single to the stream.
		///// </summary>
		///// <param name="value">The Nullable Single value to write.</param>
		//public void WriteOptimized(float? value) => this.WriteSingle(value);


		/// <summary>
		///  Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public void Write(double value) => this.WriteDouble(value);

		/// <summary>
		/// Writes a Nullable Double to the stream.
		/// </summary>
		/// <param name="value">The Nullable Double value to write.</param>
		public void Write(double? value) => this.WriteNullableDouble(value);

		///// <summary>
		/////  Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.
		///// </summary>
		///// <param name="value">The eight-byte floating-point value to write.</param>
		//public void WriteOptimized(double value) => this.WriteDouble(value);

		///// <summary>
		///// Writes a Nullable Double to the stream.
		///// </summary>
		///// <param name="value">The Nullable Double value to write.</param>
		//public void WriteOptimized(double? value) => this.WriteDouble(value);

		/// <summary>
		/// Writes a decimal value to the current stream and advances the stream position by sixteen bytes.
		/// </summary>
		/// <param name="value">The decimal value to write.</param>
		public void Write(decimal value) => this.WriteDecimal(value);

		/// <summary>
		/// Writes a Nullable Decimal to the stream.
		/// </summary>
		/// <param name="value">The Nullable Decimal value to write.</param>
		public void Write(decimal? value) => this.WriteNullableDecimal(value);

		/// <summary>
		/// Writes a decimal value to the current stream and advances the stream position by sixteen bytes.
		/// </summary>
		/// <param name="value">The decimal value to write.</param>
		public void WriteOptimized(decimal value) => this.WriteDecimalOptimized(value);

		/// <summary>
		/// Writes a Nullable Decimal to the stream.
		/// </summary>
		/// <param name="value">The Nullable Decimal value to write.</param>
		public void WriteOptimized(decimal? value) => this.WriteNullableDecimalOptimized(value);

		#endregion |   Floating Point & Decimal Types: float, double, decimal   |

		#region |   Date & Time: DateTime, TimeSpan   |

		/// <summary>
		/// Writes a DateTime value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void Write(DateTime value) => this.WriteDateTime(value);

		/// <summary>
		/// Writes a Nullable DateTime value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The Nullable DateTime value to write.</param>
		public void Write(DateTime? value) => this.WriteNullableDateTime(value);

		/// <summary>
		/// Writes a DateTime value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void WriteOptimized(DateTime value) => this.WriteDateTimeOptimized(value);

		/// <summary>
		/// Writes a Nullable DateTime value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The DateTime value to write.</param>
		public void WriteOptimized(DateTime? value) => this.WriteNullableDateTimeOptimized(value);

		/// <summary>
		/// Writes a TimeSpan value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The TimeSpan value to write.</param>
		public void Write(TimeSpan value) => this.WriteTimeSpan(value);

		/// <summary>
		/// Writes a Nullable TimeSpan value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The Nullable TimeSpan value to write.</param>
		public void Write(TimeSpan? value) => this.WriteNullableTimeSpan(value);

		/// <summary>
		/// Writes a TimeSpan value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The TimeSpan value to write.</param>
		public void WriteOptimized(TimeSpan value) => this.WriteTimeSpanOptimized(value);

		/// <summary>
		/// Writes a Nullable TimeSpan value into the stream.
		/// Stored Size: 8 bytes
		/// </summary>
		/// <param name="value">The Nullable TimeSpan value to write.</param>
		public void WriteOptimized(TimeSpan? value) => this.WriteNullableTimeSpanOptimized(value);

		#endregion |   Date & Time: DateTime, TimeSpan   |

		#region |   Specific Types: BitArray, BitVector32, Guid   |

		/// <summary>
		/// Writes a BitArray value into the stream using the fewest number of bytes possible.
		/// Stored Size: 1 byte upwards depending on data content
		/// Notes:
		/// A null BitArray takes 1 byte.
		/// An empty BitArray takes 2 bytes.
		/// </summary>
		/// <param name="value">The BitArray value to write.</param>
		public void Write(BitArray value) => this.WriteBitArray(value);

		/// <summary>
		/// Writes a BitArray value into the stream using the fewest number of bytes possible.
		/// Stored Size: 1 byte upwards depending on data content
		/// Notes:
		/// A null BitArray takes 1 byte.
		/// An empty BitArray takes 2 bytes.
		/// </summary>
		/// <param name="value">The BitArray value to write.</param>
		public void WriteOptimized(BitArray value) => this.WriteBitArrayOptimized(value);

		/// <summary>
		/// Writes a BitVector32 into the stream.
		/// Stored Size: 4 bytes.
		/// </summary>
		/// <param name="value">The BitVector32 to write.</param>
		public void Write(BitVector32 value) => this.WriteBitVector32(value);

		/// <summary>
		/// Writes a Nullable BitVector32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable BitVector32 value to write.</param>
		public void Write(BitVector32? value) => this.WriteNullableBitVector32(value);

		/// <summary>
		/// Writes a BitVector32 into the stream.
		/// Stored Size: 4 bytes.
		/// </summary>
		/// <param name="value">The BitVector32 to write.</param>
		public void WriteOptimized(BitVector32 value) => this.WriteBitVector32Optimized(value);

		/// <summary>
		/// Writes a Nullable BitVector32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable BitVector32 value to write.</param>
		public void WriteOptimized(BitVector32? value) => this.WriteNullableBitVector32Optimized(value);

		/// <summary>
		/// Writes a Guid into the stream.
		/// Stored Size: 16 bytes.
		/// </summary>
		/// <param name="value"></param>
		public void Write(Guid value) => this.WriteGuid(value);

		/// <summary>
		/// Writes a Nullable Guid to the stream.
		/// </summary>
		/// <param name="value">The Nullable Guid value to write.</param>
		public void Write(Guid? value) => this.WriteNullableGuid(value);

		/// <summary>
		/// Writes a Guid into the stream.
		/// Stored Size: 16 bytes.
		/// </summary>
		/// <param name="value"></param>
		public void WriteOptimized(Guid value) => this.WriteGuid(value);

		/// <summary>
		/// Writes a Nullable Guid to the stream.
		/// </summary>
		/// <param name="value">The Nullable Guid value to write.</param>
		public void WriteOptimized(Guid? value) => this.WriteNullableGuid(value);

		#endregion |   Specific Types: BitArray, BitVector32, Guid   |

		#region |   Char & String   |

		/// <summary>
		///  Writes a Unicode character to the current stream and advances the current position of the stream in accordance with the Encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="value">The non-surrogate, Unicode character to write.</param>
		public void Write(char value) => this.WriteChar(value);

		/// <summary>
		/// Writes a Nullable Char to the stream.
		/// </summary>
		/// <param name="value">The Nullable Char value to write.</param>
		public void Write(char? value) => this.WriteNullableChar(value);

		/// <summary>
		/// Writes a length-prefixed string to this stream in the current encoding and advances the current position of the stream in accordance
		/// with the encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="value">The string value to write.</param>
		public void Write(string value) => this.WriteString(value);

		/// <summary>
		/// Writes a length-prefixed string to this stream in the current encoding and advances the current position of the stream in accordance
		/// with the encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="value">The string value to write.</param>
		public void WriteOptimized(string value) => this.WriteStringOptimized(value);

		#endregion |   Char & String   |

		#region |   Type & Object   |

		/// <summary>
		/// Stores a Type object into the stream.
		/// Stored Size: Depends on the length of the Type's name and whether the fullyQualified parameter is set.
		/// A null Type takes 1 byte.
		/// </summary>
		/// <param name="value">The Type to write.</param>
		public void Write(Type value) => this.WriteType(value, fullyQualified: true);

		///// <summary>
		///// Stores an object into the stream. Use this method only if cannot use any of other method available 
		///// or if you for any reason not able to manualy serialize it using serializating its properties.
		///// </summary>
		///// <param name="value">The object to store.</param>
		//public void Write(object value) => this.WriteObject(value);

		///// <summary>
		///// Stores an object into the stream. Use this method only if cannot use any of other method available 
		///// or if you for any reason not able to manualy serialize it using serializating its properties.
		///// </summary>
		///// <typeparam name="T">The type of the object.</typeparam>
		///// <param name="value">The object to store.</param>
		//public void Write<T>(object value) => this.Writer.WriteObject(typeof(T), value);

		#endregion |   Type & Object   |

		#endregion |   Write((dynamic) value) Methods Variant   |

		#region |   Helper Methods   |

		/// <summary>
		/// Stores the object specified by objectType. 
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray, ArrayList and Type.
		/// </summary>
		/// <param name="objectType">The <see cref="Type"/> of the object.</param>
		/// <param name="value">The object value.</param>
		public void Write(Type objectType, object? value)
		{
			int objectTypeId = PropertyTypes.GetPropertyTypeId(objectType);

			if (objectTypeId >= 0)
				this.Write(objectTypeId, value);
			else
				throw new NotSupportedException(String.Format("The object type '{0}' is not supperted for serialization.", objectType.ToString()));
		}

		/// <summary>
		/// Stores the object specified by objectTypeId (<see cref="Simple.PropertyTypes"/> class).
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray, ArrayList and Type.
		/// </summary>
		/// <param name="objectTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <param name="value">The object value.</param>
		public void Write(int objectTypeId, object? value) => WriteActionsByTypeId[objectTypeId](this, value);

		/// <summary>
		/// Stores the object specified by objectType using the fewest number of bytes possible. 
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray, ArrayList and Type.
		/// </summary>
		/// <param name="objectType">The <see cref="Type"/> of the object.</param>
		/// <param name="value">The object value.</param>
		public void WriteOptimized(Type objectType, object? value)
		{
			int objectTypeId = PropertyTypes.GetPropertyTypeId(objectType);

			if (objectTypeId >= 0)
				this.WriteOptimized(objectTypeId, value);
			else
				throw new NotSupportedException(String.Format("The object type '{0}' is not supperted for serialization.", objectType.ToString()));
		}

		/// <summary>
		/// Stores the object specified by objectTypeId (<see cref="Simple.PropertyTypes"/> class), using the fewest number of bytes possible.
		/// Available object types are: bool, byte, short, int, long, bool[], byte[], short[], int[], long[], bool?, byte?, short?, int?, long?, bool?[], byte?[], short?[], int?[], long?[], 
		/// sbyte, ushort, uint, ulong, sbyte[], ushort[], uint[], ulong[], sbyte?, ushort?, uint?, ulong?, sbyte?[], ushort?[], uint?[], ulong?[],
		/// float, double, decimal, float[], double[], decimal[], float?, double?, decimal?, float?[], double?[], decimal?[], 
		/// DateTime, TimeSpan, DateTime[], TimeSpan[], DateTime?, TimeSpan?, DateTime?[], TimeSpan?[], 
		/// BitVector32, Guid, BitVector32[], Guid[], BitVector32?, Guid?, BitVector32?[], Guid?[], 
		/// char, char?, string, char?[], string[], 
		/// BitArray, ArrayList and Type.
		/// </summary>
		/// <param name="objectTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <param name="value">The object value.</param>
		public void WriteOptimized(int objectTypeId, object? value) => WriteOptimizedActionsByTypeId[objectTypeId](this, value);

		/// <summary>
		/// Gets the write action method that writes to given sream by specified object type.
		/// </summary>
		/// <param name="objectType">The object type identifier.</param>
		/// <returns>The action method for writing to stream.</returns>
		public static Action<SerialWriter, object> GetWriterAction(int objectTypeId) => WriteActionsByTypeId[objectTypeId];

		/// <summary>
		/// Gets the optimized write action method that writes to given sream by specified object type.
		/// </summary>
		/// <param name="objectType">The object type identifier.</param>
		/// <returns>The action method for optimized writing to stream.</returns>
		public static Action<SerialWriter, object> GetWriterActionOptimized(int objectTypeId) => WriteOptimizedActionsByTypeId[objectTypeId];

		///// <summary>
		///// Sets the new write custom actions. The related ObjectTypeId is given as result value.
		///// </summary>
		///// <param name="writerActionByTypeId">The serialization write action.</param>
		///// <param name="writerActionOptimizedByTypeId">The serialization optimized write action.</param>
		///// <returns>The ObjectTypeId.</returns>
		//[CLSCompliant(false)]
		//public static int SetNewWriteAction(Action<ISerializationWriter, object> writerActionByTypeId, Action<ISerializationWriter, object> writerActionOptimizedByTypeId, Action<ISerializationWriter, object> writerActionDefaultOptimizedByTypeId)
		//{
		//	int objectTypeId = WriteActionsByTypeId.MaxIndex + 1;

		//	WriteActionsByTypeId[objectTypeId] = writerActionByTypeId;
		//	WriteActionsOptimizedByTypeId[objectTypeId] = writerActionOptimizedByTypeId;
		//	WriteActionsDefaultOptimizedByTypeId[objectTypeId] = writerActionDefaultOptimizedByTypeId;

		//	return objectTypeId;
		//}

		#endregion |   Helper Methods   |

		#region |   Additional Helper Methods  |

		protected void WriteNullable<T>(object? value, Action<T> writeNonNullableFunc) where T : notnull
		{
			if (value is null)
			{
				this.WriteBoolean(true); // null
			}
			else
			{
				this.WriteBoolean(false); // non null
				writeNonNullableFunc((T)value);
			}
		}
		
		#endregion |   Additional Helper Methods  |

		#region |   Public Static Helper Methods   |

		/// <summary>
		/// Writes a 16-bit short integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The Int64 value to be writen</param>
		public static void Write7BitEncodedInt16(ISequenceWriter writer, short value) => BinaryEndiannessWriter.Write7BitEncodedUInt64(writer, unchecked((ulong)value));

		/// <summary>
		/// Writes a 16-bit unsigned short integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The Int64 value to be writen</param>
		public static void Write7BitEncodedUInt16(ISequenceWriter writer, ushort value) => BinaryEndiannessWriter.Write7BitEncodedUInt64(writer, unchecked(value));

		/// <summary>
		/// Writes a 32-bit integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The Int64 value to be writen</param>
		public static void Write7BitEncodedInt32(ISequenceWriter writer, int value) => BinaryEndiannessWriter.Write7BitEncodedUInt64(writer, unchecked((ulong)value));

		/// <summary>
		/// Writes a 32-bit unsigned integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The Int64 value to be writen</param>
		public static void Write7BitEncodedUInt32(ISequenceWriter writer, uint value) => BinaryEndiannessWriter.Write7BitEncodedUInt64(writer, unchecked(value));

		/// <summary>
		/// Writes a 64-bit long integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The Int64 value to be writen</param>
		public static void Write7BitEncodedInt64(ISequenceWriter writer, long value) => BinaryEndiannessWriter.Write7BitEncodedUInt64(writer, unchecked((ulong)value));

		/// <summary>
		/// Writes a 64-bit unsigned long integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The UInt64 value to be writen</param>
		public static void Write7BitEncodedUInt64(ISequenceWriter writer, ulong value) => BinaryEndiannessWriter.Write7BitEncodedUInt64(writer, value);

		#endregion |   Public Static Helper Methods   |

		#region |   Extras   |

		/// <summary>
		/// Stores a 16-bit signed value into the stream using N-bit encoding.
		/// 
		/// The value is written n bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// </summary>
		/// <param name="writer">The serialization writer.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The Int16 value to encode.</param>
		public static void WriteNBitEncodedInt16(SerialWriter writer, int n, short value) => WriteNBitEncodedUInt64(writer, n, unchecked((ulong)value));

		/// <summary>
		/// Stores a 16-bit unsigned value into the stream using N-bit encoding.
		/// 
		/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// 
		/// See Write(ULong) for details of the values that are optimizable.
		/// </summary>
		/// <param name="writer">The serialization writer.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The UInt16 value to encode.</param>
		public static void WriteNBitEncodedUInt16(SerialWriter writer, int n, ushort value) => WriteNBitEncodedUInt64(writer, n, unchecked(value));

		/// <summary>
		/// Stores a 32-bit signed value into the stream using N-bit encoding.
		/// 
		/// The value is written n bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// </summary>
		/// <param name="writer">The serialization writer.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The Int32 value to encode.</param>
		public static void WriteNBitEncodedInt32(SerialWriter writer, int n, int value) => WriteNBitEncodedUInt64(writer, n, unchecked((ulong)value));

		/// <summary>
		/// Stores a 32-bit unsigned value into the stream using N-bit encoding.
		/// 
		/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// 
		/// See Write(ULong) for details of the values that are optimizable.
		/// </summary>
		/// <param name="writer">The serialization writer.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The ULong64 value to encode.</param>
		public static void WriteNBitEncodedUInt32(SerialWriter writer, int n, uint value) => WriteNBitEncodedUInt64(writer, n, unchecked(value));

		/// <summary>
		/// Stores a 64-bit signed value into the stream using N-bit encoding.
		/// 
		/// The value is written n bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// </summary>
		/// <param name="writer">The serialization writer.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The Int64 value to encode.</param>
		public static void WriteNBitEncodedInt64(SerialWriter writer, int n, long value) => WriteNBitEncodedUInt64(writer, n, unchecked((ulong)value));

		/// <summary>
		/// Stores a 64-bit unsigned value into the stream using N-bit encoding.
		/// 
		/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// 
		/// See Write(ULong) for details of the values that are optimizable.
		/// </summary>
		/// <param name="writer">The serialization writer.</param>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The ULong64 value to encode.</param>
		public static void WriteNBitEncodedUInt64(SerialWriter writer, int n, ulong value) // <- TODO: Check is this method endianness !!!
		{
			var singleBit = SingleBits[n];
			var numOfBits = n + 1;

			while (value >= singleBit)
			{
				writer.WriteBits((byte)(value | singleBit), numOfBits);
				value >>= n;
			}

			writer.WriteBits((byte)value, numOfBits);
		}

		public static void WriteBitEncodedSignedInt16(SerialWriter writer, int numOfBits, short value) => WriteBitEncodedUnsignedInt64(writer, numOfBits, unchecked((ulong)value));

		public static void WriteBitEncodedUnsignedInt16(SerialWriter writer, int numOfBits, ushort value) => WriteBitEncodedUnsignedInt64(writer, numOfBits, unchecked(value));

		public static void WriteBitEncodedSignedInt32(SerialWriter writer, int numOfBits, int value) => WriteBitEncodedUnsignedInt64(writer, numOfBits, unchecked((ulong)value));

		public static void WriteBitEncodedUnsignedInt32(SerialWriter writer, int numOfBits, uint value) => WriteBitEncodedUnsignedInt64(writer, numOfBits, unchecked(value));

		public static void WriteBitEncodedSignedInt64(SerialWriter writer, int numOfBits, long value) => WriteBitEncodedUnsignedInt64(writer, numOfBits, unchecked((ulong)value));

		public static void WriteBitEncodedUnsignedInt64(SerialWriter writer, int numOfBits, ulong value)
		{
			byte bitMask = BitMask[numOfBits];

			while (value >= bitMask)
			{
				writer.WriteBits(bitMask, numOfBits);
				value -= bitMask;
			}

			writer.WriteBits((byte)value, numOfBits);
		}

		#endregion |   Extras   |

		#region |   Private Properties  |

		private UniqueStringList StringLookup
		{
			get
			{
				if (this.stringLookup == null)
					this.stringLookup = new UniqueStringList();

				return this.stringLookup;
			}
		}

		#endregion |   Private Properties  |

		#region |   Private Methods  |

		///// <summary>
		///// Writes a generic IList populated with values using generic write element action.
		///// </summary>
		///// <typeparam name="T">The list Type.</typeparam>
		///// <param name="value">The generic list to write.</param>
		///// <param name="writeElementAction">Single write value element action.</param>
		//private void WriteList<T>(IEnumerable<T> value, Action<T> writeElementAction)
		//{
		//	this.WriteNullable(value, (item) =>
		//	{
		//		this.WriteInt32Optimized(value.Count());

		//		foreach (T element in value)
		//			writeElementAction(element);
		//	});
		//}

		///// <summary>
		///// Writes a generic IDictionary populated with key and value pairs using generic write key and value element action.
		///// </summary>
		///// <typeparam name="TKey">The key Type.</typeparam>
		///// <typeparam name="TValue">The value Type.</typeparam>
		///// <param name="value">The generic dictionary to write.</param>
		///// <param name="writeKeyAction">Single write key element action.</param>
		///// <param name="writeValueAction">Single write value element action.</param>
		//private void WriteDictionary<TKey, TValue>(IDictionary<TKey, TValue> value, Action<TKey> writeKeyAction, Action<TValue> writeValueAction)
		//{
		//	this.WriteNullable(value, (item) =>
		//	{
		//		this.WriteInt32Optimized(value.Count);

		//		foreach (KeyValuePair<TKey, TValue> element in value)
		//		{
		//			writeKeyAction(element.Key);
		//			writeValueAction(element.Value);
		//		}
		//	});
		//}

		/// <summary>
		/// Encodes a TimeSpan into the fewest number of bytes.
		/// Has been separated from the WriteOptimized(TimeSpan) method so that WriteOptimized(DateTime)
		/// can also use this for .NET 2.0 DateTimeKind information.
		/// By taking advantage of the fact that a DateTime's TimeOfDay portion will never use the IsNegative
		/// and HasDays flags, we can use these 2 bits to store the DateTimeKind and, since DateTimeKind is
		/// unlikely to be set without a Time, we need no additional bytes to support a .NET 2.0 DateTime.
		/// </summary>
		/// <param name="writer">The serialization writer.</param>
		/// <param name="value">The TimeSpan to store.</param>
		/// <param name="partOfDateTime">True if the TimeSpan is the TimeOfDay from a DateTime; False if a real TimeSpan.</param>
		/// <param name="initialData">The intial data for the BitVector32 - contains DateTimeKind or 0</param>
		private void EncodeTimeSpan(TimeSpan value, bool partOfDateTime, int initialData)
		{
			var packedData = new BitVector32(initialData);
			int days;
			var hours = Math.Abs(value.Hours);
			var minutes = Math.Abs(value.Minutes);
			var seconds = Math.Abs(value.Seconds);
			var milliseconds = Math.Abs(value.Milliseconds);
			var hasTime = hours != 0 || minutes != 0;
			var optionalBytes = 0;

			if (partOfDateTime)
			{
				days = 0;
			}
			else
			{
				days = Math.Abs(value.Days);
				packedData[IsNegativeSection] = value.Ticks < 0 ? 1 : 0;
				packedData[HasDaysSection] = days != 0 ? 1 : 0;
			}

			if (hasTime)
			{
				packedData[HasTimeSection] = 1;
				packedData[HoursSection] = hours;
				packedData[MinutesSection] = minutes;
			}

			if (seconds != 0)
			{
				packedData[HasSecondsSection] = 1;

				if (!hasTime && (milliseconds == 0)) // If only seconds are present then we can use the minutes slot to save a byte
				{
					packedData[MinutesSection] = seconds;
				}
				else
				{
					packedData[SecondsSection] = seconds;
					optionalBytes++;
				}
			}

			if (milliseconds != 0)
			{
				packedData[HasMillisecondsSection] = 1;
				packedData[MillisecondsSection] = milliseconds;
				optionalBytes = 2;
			}

			var data = packedData.Data;

			this.WriteByte((byte)data);
			this.WriteByte((byte)(data >> 8)); // Always write minimum of two bytes

			if (optionalBytes > 0)
				this.WriteByte((byte)(data >> 16));

			if (optionalBytes > 1)
				this.WriteByte((byte)(data >> 24));

			if (days != 0)
				this.WriteInt32Optimized(days);
		}

        private void WriteCharsCommonWithoutLengthPrefix(ReadOnlySpan<char> chars)
		{
			// If our input is truly enormous, the call to GetMaxByteCount might overflow,
			// which we want to avoid. Theoretically, any Encoding could expand from chars -> bytes
			// at an enormous ratio and cause us problems anyway given small inputs, but this is so
			// unrealistic that we needn't worry about it.

			byte[] rented;

			if (chars.Length <= MaxArrayPoolRentalSize)
			{
				// GetByteCount may walk the buffer contents, resulting in 2 passes over the data.
				// We prefer GetMaxByteCount because it's a constant-time operation.

				int maxByteCount = this.CharacterEncoding.GetMaxByteCount(chars.Length);

				if (maxByteCount <= MaxArrayPoolRentalSize)
				{
					rented = ArrayPool<byte>.Shared.Rent(maxByteCount);
					int actualByteCount = this.CharacterEncoding.GetBytes(chars, rented);

					this.WriteByteArray(rented, 0, actualByteCount);
					ArrayPool<byte>.Shared.Return(rented);

					return;
				}
			}

			// We're dealing with an enormous amount of data, so acquire an Encoder.
			// It should be rare that callers pass sufficiently large inputs to hit
			// this code path, and the cost of the operation is dominated by the transcoding
			// step anyway, so it's ok for us to take the allocation here.

			rented = ArrayPool<byte>.Shared.Rent(MaxArrayPoolRentalSize);
			Encoder encoder = this.CharacterEncoding.GetEncoder();
			bool completed;

			do
			{
				encoder.Convert(chars, rented, flush: true, out int charsConsumed, out int bytesWritten, out completed);

				if (bytesWritten != 0)
					this.WriteByteArray(rented, 0, bytesWritten);

				chars = chars.Slice(charsConsumed);
			}
			while (!completed);

			ArrayPool<byte>.Shared.Return(rented);
		}

		#endregion |   Private Methods  |

		#region |   Private Classes  |

		/// <summary>
		/// Private class used to wrap an object that is to be tokenized, and recreated at deserialization by its type.
		/// </summary>
		class SingletonTypeWrapper
		{
			readonly Type wrappedType;

			/// <summary>
			/// Initializes a new instance of the <see cref="SingletonTypeWrapper"/> class.
			/// </summary>
			/// <param name="value">The value.</param>
			public SingletonTypeWrapper(object value)
			{
				wrappedType = value.GetType();
			}

			/// <summary>
			/// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
			/// </summary>
			/// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
			/// <returns>
			/// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
			/// </returns>
			public override bool Equals(object? obj)
			{
				if (obj is SingletonTypeWrapper singletonTypeWrapper)
					return wrappedType.Equals(singletonTypeWrapper.wrappedType);

				return false;
			}

			/// <summary>
			/// Serves as a hash function for a particular type.
			/// </summary>
			/// <returns>
			/// A hash code for the current <see cref="T:System.Object"></see>.
			/// </returns>
			public override int GetHashCode()
			{
				return wrappedType.GetHashCode();
			}

			/// <summary>
			/// Gets the type of the wrapped.
			/// </summary>
			/// <value>The type of the wrapped.</value>
			public Type WrappedType
			{
				get { return wrappedType; }
			}
		}

		#endregion |   Private Classes  |
	}
}
