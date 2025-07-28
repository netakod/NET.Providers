using System;
using System.Collections;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using Simple.Collections;
using System.IO;
using Simple;

namespace Simple.Serialization
{
	// Link https://stackoverflow.com/questions/49297751/should-readonlyspant-parameters-use-the-in-modifier
//	A key factor here is size; Span<T> / ReadOnlySpan<T> are deliberately very small, so the difference between a span and a reference-to-a-span is tiny.One key usage for in here is
//	for larger readonly structs, to avoid a significant stack copy; note that there's a trade-off: the in is really a ref, so you're adding an extra layer of indirection to all access,
//	unless the JIT sees what you're doing and works some voodoo. And of course: if the type doesn't declare itself as readonly then a stack copy is automatically added before the call
//	to preserve the semantics.
//	Share
//	
//  Marc Gravell

//	I had an impression that ref readonly structs, such as Span and ReadOnlySpan already passed by reference (they even named like "ref-like types"), that seems not true? –
//	
//	Evk Mar 15, 2018 at 12:10
//  @Evk the ref in ref struct just means "only allowed on the stack" (and therefore: allowed to hold ref-like types and managed pointers as fields); it doesn't mean it is passed by reference;
//  and some of the most useful ref struct types are actually deliberately mutable. The readonly modifier enforces certain semantics allowing it to be used reliably in terms of no unexpected changes,
//  which allows in to perform a copy-free ref call while guaranteeing (at least at the language level, not at the IL level) that the value will not be modified. – 
//  Marc Gravell
//
//	Mar 15, 2018 at 12:21 
//
//  @Evk the key key thing is that it allows you to have(for example) a span field, which isn't allowed otherwise - or any other ref-like type (which muse likewise only ever exist on the stack) – 
//	
//	Marc Gravell Mar 15, 2018 at 12:22
//	Yes I think I understand the intention, but for some reason I also had impression they are also passed by ref. Maybe that "ref" in definition confused me.They could use some more meaningful name,
//	like "stackonly" or something. – 
//
//  Evk Mar 15, 2018 at 12:25
//  @Evk adding keywords - even contextual keywords - is harder than re-purposing and overloading an existing reserved keyword :) – 
//
//  Marc Gravell
//  Mar 15, 2018 at 12:30

	/// <summary>
	/// The binary reader that use comination of stack and heap memory to do the serialzization transformation of bytes
	/// </summary>
	public ref struct SequenceReader
	{
		#region |   Private Members   |

		private SequenceReader<byte> reader;
		private uint bitBuffer;
		private int bitPosition;

		//private SequenceBitReader bitReader;
		private Encoding characterEncoding = UTF8Encoding.Default;
		private char[]? charBuffer = null;
		private byte[]? bytesBuffer = null;
		private List<string>? stringTokenList = null;

		private const int MaxCharBytesSize = 128;
		private static readonly BitArray FullyOptimizableTypedArray = new BitArray(0);

		private int maxCharsSize;

		#endregion |   Private Members   |

		#region |   Constructors and Initialization   |

		public SequenceReader(ref SequenceReader<byte> reader)
			: this(ref reader, UTF8Encoding.Default)
		{
		}

		public SequenceReader(ref SequenceReader<byte> reader, Encoding characterEncoding)
        {
            this.reader = reader;
			this.CharacterEncoding = characterEncoding;
		}

		public SequenceReader(byte[] buffer)
			: this(buffer, UTF8Encoding.Default)
		{
		}

		public SequenceReader(byte[] buffer, Encoding characterEncoding)
        {
			var ros = new ReadOnlySequence<byte>(buffer);
			
			this.reader = new SequenceReader<byte>(ros);
			this.CharacterEncoding = characterEncoding;
		}

		#endregion  |   Constructors and Initialization   |

		#region |   Private Properties   |

		//private SequenceBitReader BitReader => this.bitReader ??= new();
		private List<string> StringTokenList => this.stringTokenList ??= new();

		#endregion |   Private Properties   |

		#region |   Public Properties   |

		/// <summary>
		/// The underlying <see cref="ReadOnlySequence{byte}"/> for the reader.
		/// </summary>
		public ReadOnlySequence<byte> Sequence => this.reader.Sequence;

		/// <summary>
		/// The total number of byte's processed by the reader.
		/// </summary>
		public long BytesConsumed => this.reader.Consumed;

		/// <summary>
		/// Specifie character <see cref="Encoding"/> used when reading string. 
		/// The same character encoding must be used whe writing.
		/// </summary>
		public Encoding CharacterEncoding 
		{
			get => this.characterEncoding;
			
			set
			{
				this.characterEncoding = value;
				this.maxCharsSize = value.GetMaxCharCount(MaxCharBytesSize);
			}
		}
		public bool UserStringLookupOptimisation { get; set; } = false;


		#endregion |   Public Properties   |

		#region |   Public Methods  |

		/// <summary>
		/// Reads the sequence of bytes. The number of bytes is specified by the count.
		/// </summary>
		/// <param name="count">The number of bytes to read</param>
		/// <returns>The <see cref="ReadOnlySequence{byte}"/> result.</returns>
		/// <exception cref="EndOfStreamException"></exception>
		public ReadOnlySequence<byte> ReadBinary(int count)
		{
			var sequence = this.reader.Sequence.Slice(this.reader.Position, count);

			//if (count != 0)
			this.reader.Advance(count);

			return sequence;


			if (this.reader.TryReadExact(count, out ReadOnlySequence<byte> sequence2))
				return sequence2;
			else
				throw new EndOfStreamException("End of sequence detected");
		}

		/// <summary>
		/// Move the reader ahead for the specified number of items.
		/// </summary>
		/// <param name="count">The number of bytes to move ahead.</param>
		public void AdvancePosition(long count) => this.reader.Advance(count);

		/// <summary>
		/// Reset the reading position from the begining
		/// </summary>
		public void ResetPosition()
		{
			this.reader = new SequenceReader<byte>(this.reader.Sequence);
			this.bitBuffer = 0;
			this.bitPosition = 0;
		}

		/// <summary>
		/// Converts the all data from <see cref="ReadOnlySequence{byte}"/> to an array
		/// </summary>
		/// <returns>Byte array</returns>
		public byte[] ToArray() => this.reader.Sequence.ToArray();

		/// <summary>
		/// Dispose method called when the object is disappearing
		/// </summary>
		public void Dispose()
		{
			//GC.SuppressFinalize((object)this);
		}

		#endregion |   Public Methods  |

		#region |   Primitive Types: bool, sbyte, short, int, long   |

		/// <summary>
		/// Reads a  <see cref="bool" /> value from the current squence and advances the current position by one byte.
		/// </summary>
		/// <returns>true if the byte is nonzero; otherwise, false.</returns>
		public Boolean ReadBoolean() => this.ReadBits(1) == (byte)1;

		/// <summary>
		/// Returns a Nullable <see cref="bool" /> from the stream.
		/// </summary>
		/// <returns>A Nullable Boolean.</returns>
		public Boolean? ReadNullableBoolean() => (this.ReadBoolean()) ? null : this.ReadBoolean();

		/// <summary>
		/// Reads a signed byte  <see cref="sbyte" /> from this sequence and advances the current sequence position by one byte.
		/// </summary>
		/// <returns>A signed byte read from the sequence.</returns>
		public SByte ReadSByte() => (SByte)this.ReadByte();

		/// <summary>
		/// Returns a Nullable <see cref="sbyte" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable SByte.</returns>
		public SByte? ReadNullableSByte() => (this.ReadBoolean()) ? null : this.ReadSByte();

		/// <summary>
		/// Reads a <see cref="short" /> from the sequence.
		/// </summary>
		public Int16 ReadInt16()
		{
			if (BitConverter.IsLittleEndian)
				return (Int16)(this.ReadByte() | this.ReadByte() << 8);
			else
				return (Int16)(this.ReadByte() << 8 | this.ReadByte());
		}

		/// <summary>
		/// Returns an <see cref="short" /> value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Int16 value.</returns>
		public Int16 ReadInt16Optimized() => unchecked((short)this.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns a Nullable <see cref="short" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable Int16.</returns>
		public Int16? ReadNullableInt16() => (this.ReadBoolean()) ? null : this.ReadInt16();

		/// <summary>
		/// Returns an Nullable <see cref="int" /> value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable Int16 value.</returns>
		public Int16? ReadNullableInt16Optimized() => (this.ReadBoolean()) ? null : this.ReadInt16Optimized();

		/// <summary>
		/// Reads a <see cref="int" /> from the sequence.
		/// </summary>
		public Int32 ReadInt32()
		{
			if (BitConverter.IsLittleEndian)
				return this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24;
			else
				return this.ReadByte() << 24 | this.ReadByte() << 16 | this.ReadByte() << 8 | this.ReadByte();
		}

		/// <summary>
		/// Returns a Nullable <see cref="int" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable Int16.</returns>
		public Int32? ReadNullableInt32() => (this.ReadBoolean()) ? null : this.ReadInt32();

		/// <summary>
		/// Returns an <see cref="int" /> value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>An Int32 value.</returns>
		public Int32 ReadInt32Optimized() => unchecked((int)this.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns an Nullable <see cref="int" /> value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>An Nullable Int32 value.</returns>
		public Int32? ReadNullableInt32Optimized() => (this.ReadBoolean()) ? null : this.ReadInt32Optimized();

		/// <summary>
		/// Reads a <see cref="long" /> from the sequence.
		/// </summary>
		public Int64 ReadInt64()
		{
			if (BitConverter.IsLittleEndian)
			{
				return (long)this.ReadByte()	   | (long)this.ReadByte() << 8  | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 24 |
					   (long)this.ReadByte() << 32 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 56;
			}
			else
			{
				return (Int64)((long)this.ReadByte() << 56 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 32 |
							   (long)this.ReadByte() << 24 | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 8  | this.ReadByte());
			}
		}

		/// <summary>
		/// Returns a Nullable <see cref="long" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable Int64.</returns>
		public Int64? ReadNullableInt64() => (this.ReadBoolean()) ? null : this.ReadInt64();

		/// <summary>
		/// Returns an <see cref="long" /> value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>An Int64 value.</returns>
		public Int64 ReadInt64Optimized() => unchecked((long)this.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns an Nullable <see cref="long" /> value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>An Nullable Int64 value.</returns>
		public Int64? ReadNullableInt64Optimized() => (this.ReadBoolean()) ? null : this.ReadInt64Optimized();

		#endregion |   Primitive Types: bool, sbyte, short, int, long   |

		#region |   Unsigned Primitive Types: byte, ushort, uint, ulong   |

		public Byte ReadByte()
        {
            this.reader.TryRead(out byte value);

            return value;
        }

		/// <summary>
		/// Returns a Nullable <see cref="byte" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable Byte.</returns>
		public Byte? ReadNullableByte() => (this.ReadBoolean()) ? null : this.ReadByte();

		/// <summary>
		/// Reads a <see cref="ushort" /> from the sequence.
		/// </summary>
		public UInt16 ReadUInt16()
		{
			if (BitConverter.IsLittleEndian)
				return (UInt16)(this.ReadByte()		 | this.ReadByte() << 8);
			else
				return (UInt16)(this.ReadByte() << 8 | this.ReadByte());
		}

		/// <summary>
		/// Returns an <see cref="ushort" /> value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An UInt16 value.</returns>
		public UInt16 ReadUInt16Optimized() => unchecked((ushort)this.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns a Nullable <see cref="ushort" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable UInt16.</returns>
		public UInt16? ReadNullableUInt16() => (this.ReadBoolean()) ? null : this.ReadUInt16();

		/// <summary>
		/// Returns an Nullable <see cref="uint" /> value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable UInt16 value.</returns>
		public UInt16? ReadNullableUInt16Optimized() => (this.ReadBoolean()) ? null : this.ReadUInt16Optimized();

		/// <summary>
		/// Reads a <see cref="uint" /> from the sequence.
		/// </summary>
		public UInt32 ReadUInt32()
		{
			if (BitConverter.IsLittleEndian)
				return (UInt32)(this.ReadByte()		  | this.ReadByte() << 8  | this.ReadByte() << 16 | this.ReadByte() << 24);
			else
				return (UInt32)(this.ReadByte() << 24 | this.ReadByte() << 16 | this.ReadByte() << 8  | this.ReadByte());
		}

		/// <summary>
		/// Returns a Nullable <see cref="uint" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable UInt32.</returns>
		public UInt32? ReadNullableUInt32() => (this.ReadBoolean()) ? null : this.ReadUInt32();

		/// <summary>
		/// Returns an <see cref="uint" /> value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An UInt32 value.</returns>
		public UInt32 ReadUInt32Optimized() => unchecked((uint)this.Read7BitEncodedUInt64());

		/// <summary>
		/// Returns an Nullable <see cref="uint" /> value from the stream that was stored optimized.
		/// </summary>
		/// <returns>An Nullable UInt16 value.</returns>
		public UInt32? ReadNullableUInt32Optimized() => (this.ReadBoolean()) ? null : this.ReadUInt32Optimized();

		/// <summary>
		/// Reads a <see cref="ulong" /> from the sequence.
		/// </summary>
		public UInt64 ReadUInt64()
		{
			if (BitConverter.IsLittleEndian)
			{
				return		  this.ReadByte()		| (ulong)this.ReadByte() << 8  | (ulong)this.ReadByte() << 16 | (ulong)this.ReadByte() << 24 |
					   (ulong)this.ReadByte() << 32 | (ulong)this.ReadByte() << 40 | (ulong)this.ReadByte() << 48 | (ulong)this.ReadByte() << 56;
			}
			else
			{
				return (ulong)this.ReadByte() << 56 | (ulong)this.ReadByte() << 48 | (ulong)this.ReadByte() << 40 | (ulong)this.ReadByte() << 32 |
					   (ulong)this.ReadByte() << 24 | (ulong)this.ReadByte() << 16 | (ulong)this.ReadByte() << 8  | this.ReadByte();
			}
		}

		/// <summary>
		/// Returns a Nullable <see cref="ulong" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable UInt64.</returns>
		public UInt64? ReadNullableUInt64() => (this.ReadBoolean()) ? null : this.ReadUInt64();

		/// <summary>
		/// Returns an <see cref="ulong" /> value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>An Int64 value.</returns>
		public UInt64 ReadUInt64Optimized() => this.Read7BitEncodedUInt64();

		/// <summary>
		/// Returns an Nullable <see cref="ulong" /> value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>An Nullable UInt64 value.</returns>
		public UInt64? ReadNullableUInt64Optimized() => (this.ReadBoolean()) ? null : this.ReadUInt64Optimized();

		#endregion |   Unsigned Primitive Types: byte, ushort, uint, ulong   |

		#region |   Floating Point Types & Decimal Types: Half, float, double, decimal   |

		/// <summary>
		/// Reads a <see cref="Half" /> from the sequence (2 bytes).
		/// </summary>
		/// <returns>The Half value.</returns>
		/// </exception>
		public Half ReadHalf() => (Half)this.ReadUInt16();

		/// <summary>
		/// Returns a Nullable <see cref="Half" /> from the sequence.
		/// </summary>
		/// <returns>A Nullable Single.</returns>
		public Half? ReadNullableHalf() => (this.ReadBoolean()) ? null : this.ReadHalf();

		/// <summary>
		/// Reads a <see cref="float" /> from the sequence.
		/// </summary>
		[SecuritySafeCritical]
		public unsafe Single ReadSingle()
		{
			Int32 raw;

			if (BitConverter.IsLittleEndian)
				raw = this.ReadByte()		| this.ReadByte() << 8  | this.ReadByte() << 16 | this.ReadByte() << 24;
			else
				raw = this.ReadByte() << 24 | this.ReadByte() << 16 | this.ReadByte() << 8  | this.ReadByte();

			return *(Single*)&raw;
		}

		/// <summary>
		/// Returns a Nullable Single from the sequence.
		/// </summary>
		/// <returns>A Nullable Single.</returns>
		public Single? ReadNullableSingle() => (this.ReadBoolean()) ? null : this.ReadSingle();


		/// <summary>
		/// Reads a <see cref="double" /> from the sequence.
		/// </summary>
		[SecuritySafeCritical]
		public unsafe Double ReadDouble()
		{
			Int64 raw;

			if (BitConverter.IsLittleEndian)
			{
				raw =		this.ReadByte()		  | (long)this.ReadByte() << 8  | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 24 |
					  (long)this.ReadByte() << 32 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 56;
			}
			else
			{
				raw = (long)this.ReadByte() << 56 | (long)this.ReadByte() << 48 | (long)this.ReadByte() << 40 | (long)this.ReadByte() << 32 |
					  (long)this.ReadByte() << 24 | (long)this.ReadByte() << 16 | (long)this.ReadByte() << 8  | this.ReadByte();

			}

			return *(Double*)&raw;
		}

		/// <summary>
		/// Returns a Nullable Double from the sequence.
		/// </summary>
		/// <returns>A Nullable Double.</returns>
		public Double? ReadNullableDouble() => (this.ReadBoolean()) ? null : this.ReadDouble();

		/// <summary>
		/// Returns an <see cref="decimal"/> instance converted from the bytes from the current stream>.
		/// Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
		/// </summary>
		/// <param name="buffer">The byte array storing the raw data.</param>
		/// <returns>The converted value.</returns>
		public decimal ReadDecimal()
		{
			int[] parts = new int[4];

			for (int i = 0; i < 4; i++)
				parts[i] = this.ReadByte() | this.ReadByte() << 8 | this.ReadByte() << 16 | this.ReadByte() << 24;

			return new Decimal(parts);
		}

		/// <summary>
		/// Returns a Nullable Decimal from the sequence.
		/// </summary>
		/// <returns>A Nullable Decimal.</returns>
		public Decimal? ReadNullableDecimal() => (this.ReadBoolean()) ? null : this.ReadDecimal();

		/// <summary>
		/// Returns a Decimal value from the sequence that was stored optimized.
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
		/// Returns a Nullable Decimal from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A Nullable Decimal.</returns>
		public Decimal? ReadNullableDecimalOptimized() => (this.ReadBoolean()) ? null : this.ReadDecimalOptimized();

		#endregion |   Floating Point Types & Decimal Types: float, double, decimal   |

		#region |   Date & Time: DateTime, TimeSpan   |

		/// <summary>
		/// Returns a DateTime value from the sequence.
		/// </summary>
		/// <returns>A DateTime value.</returns>
		public DateTime ReadDateTime() => DateTime.FromBinary(this.ReadInt64());

		/// <summary>
		/// Returns a Nullable DateTime from the sequence.
		/// </summary>
		/// <returns>A Nullable DateTime.</returns>
		public DateTime? ReadNullableDateTime() => (this.ReadBoolean()) ? null : this.ReadDateTime();

		/// <summary>
		/// Returns a DateTime value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A DateTime value.</returns>
		public DateTime ReadDateTimeOptimized()
		{
			// Read date information from first three bytes
			var dateMask = new BitVector32(this.ReadByte() | (this.ReadByte() << 8) | (this.ReadByte() << 16));
			var result = new DateTime(dateMask[SequenceWriter.DateYearMask],
									  dateMask[SequenceWriter.DateMonthMask],
									  dateMask[SequenceWriter.DateDayMask]);

			if (dateMask[SequenceWriter.DateHasTimeOrKindMask] == 1)
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
					result = result.Add(this.DecodeTimeSpan(initialByte: initialByte));
				}
			}

			return result;
		}

		/// <summary>
		/// Returns a Nullable DateTime from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A Nullable DateTime.</returns>
		public DateTime? ReadNullableDateTimeOptimized() => (this.ReadBoolean()) ? null : this.ReadDateTimeOptimized();

		/// <summary>
		/// Returns a TimeSpan value from the sequence.
		/// </summary>
		/// <returns>A TimeSpan value.</returns>
		public TimeSpan ReadTimeSpan() => new TimeSpan(this.ReadInt64());

		/// <summary>
		/// Returns a Nullable TimeSpan from the sequence.
		/// </summary>
		/// <returns>A Nullable TimeSpan.</returns>
		public TimeSpan? ReadNullableTimeSpan() => (this.ReadBoolean()) ? null : this.ReadTimeSpan();

		/// <summary>
		/// Returns a TimeSpan value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A TimeSpan value.</returns>
		public TimeSpan ReadTimeSpanOptimized() => this.DecodeTimeSpan(initialByte: this.ReadByte());

		/// <summary>
		/// Returns a Nullable TimeSpan from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A Nullable TimeSpan.</returns>
		public TimeSpan? ReadNullableTimeSpanOptimized() => (this.ReadBoolean()) ? null : this.ReadTimeSpanOptimized();

		#endregion |   Date & Time: DateTime, TimeSpan   |

		#region |   Specific Types: BitArray, BitVector32, Guid   |

		/// <summary>
		/// Returns a BitArray or null from the sequence.
		/// </summary>
		/// <returns>A BitArray instance.</returns>
		public BitArray ReadBitArray()
		{
			int length = this.ReadInt32Optimized();

			//return new BitArray(this.reader.ReadBytes(((length + 7) / 8))) { Length = length };
			var result = new BitArray(length);

			for (int i = 0; i < length; i++)
				result[i] = this.ReadBoolean();

			return result;
		}

		/// <summary>
		/// Returns a BitArray from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A BitArray instance.</returns>
		public BitArray ReadBitArrayOptimized() // Since Boolen require reads/writes on 1/8 of byte (1 bit) ReadBitArray could be more compressable than this
		{
			var length = this.ReadInt32Optimized();

			if (length == 0)
				return FullyOptimizableTypedArray;

			return new BitArray(this.ReadBinary((length + 7) / 8).ToArray()) { Length = length };
		}

		/// <summary>
		/// Returns a BitVector32 value from the sequence.
		/// </summary>
		/// <returns>A BitVector32 value.</returns>
		public BitVector32 ReadBitVector32() => new BitVector32(this.ReadInt32());

		/// <summary>
		/// Returns a Nullable BitVector32 from the sequence.
		/// </summary>
		/// <returns>A Nullable BitVector32.</returns>
		public BitVector32? ReadNullableBitVector32() => (this.ReadBoolean()) ? null : this.ReadBitVector32();
		
		/// <summary>
		/// Returns a BitVector32 value from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A BitVector32 value.</returns>
		public BitVector32 ReadBitVector32Optimized() => new BitVector32(this.ReadInt32Optimized());
		
		/// <summary>
		/// Returns a Nullable BitVector32 from the sequence that was stored optimized.
		/// </summary>
		/// <returns>A Nullable BitVector32.</returns>
		public BitVector32? ReadNullableBitVector32Optimized() => (this.ReadBoolean()) ? null : this.ReadBitVector32Optimized();

		/// <summary>
		/// Returns a Guid value from the sequence.
		/// </summary>
		/// <returns>A Guid value.</returns>
		public Guid ReadGuid() => new Guid(this.ReadBinary(16).ToArray());

		/// <summary>
		/// Returns a Nullable Guid from the sequence.
		/// </summary>
		/// <returns>A Nullable Guid.</returns>
		public Guid? ReadNullableGuid() => (this.ReadBoolean()) ? null : this.ReadGuid();

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
			bool twoBytesPerChar = this.CharacterEncoding is UnicodeEncoding;

			while (charsRead == 0)
			{
				// We really want to know what the minimum number of bytes per char
				// is for our encoding.  Otherwise for UnicodeEncoding we'd have to
				// do ~1+log(n) reads to read n characters.
				// Assume 1 byte can be 1 char unless _2BytesPerChar is true.
				numBytes = (twoBytesPerChar) ? 2 : 1;

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
		public Char? ReadNullableChar() => (this.ReadBoolean()) ? null : this.ReadChar();

		/// <summary>
		/// Reads a string from the current sequence. The string is prefixed with the length, encoded as an integer seven bits at a time.
		/// </summary>
		/// <returns>The string being read.</returns>
		public string ReadStringX()
		{
			int strLength;
			int numOfChars;
			byte[] charBytes;

			try
			{
				if (this.ReadBoolean())
					return String.Empty;

				//if (!reader.CanRead)
				//	strLength = 0;

				// Length of the string in bytes, not chars
				strLength = this.ReadInt32Optimized(); //  BinaryEndiannessReader.Read7BitEncodedInt32(this.reader.BaseReader); // Read7BitEncodedInt();

				//if (stringLength < 0)
				//	return null;
				//	//throw new Exception("Invalid string length: " + stringLength);

				//if (strLength == 0)
				//	return String.Empty;

				charBytes = this.ReadBinary(strLength).ToArray(); // new byte[strLength];
																   //this.reader.ReadToBuffer(charBytes, 0, strLength);

				//this.charBuffer ??= new char[this.maxCharsSize];
				if (this.charBuffer is null || this.charBuffer.Length < strLength)
				{
					int maxCharsSize = this.CharacterEncoding.GetMaxCharCount(MaxCharBytesSize);

					this.charBuffer = new char[Math.Max(maxCharsSize, strLength)];
				}

				numOfChars = this.CharacterEncoding.GetChars(charBytes, 0, strLength, this.charBuffer, 0);

				return new string(this.charBuffer, 0, numOfChars);
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		public string ReadString0()
		{
			if (this.ReadBoolean())
				return String.Empty;

			// Length of the string in bytes, not chars
			int strLength = this.ReadInt32Optimized();
			int charRead = 0;
			int maxCharsSize = this.CharacterEncoding.GetMaxCharCount(MaxCharBytesSize);
			byte[] charBytes = new byte[MaxCharBytesSize];
			StringBuilder? stringBuilder = null;

			this.charBuffer ??= new char[maxCharsSize];

			do
			{
				int count = ((strLength - charRead) > MaxCharBytesSize) ? MaxCharBytesSize : (strLength - charRead);
				charBytes = this.ReadBinary(count).ToArray();

				//if (count == 0)
				//	throw new Exception("Get end of file");

				int chars = this.CharacterEncoding.GetChars(charBytes, 0, count, this.charBuffer, 0);

				if (charRead == 0 && count == strLength)
					return new string(this.charBuffer, 0, chars);

				// Since we could be reading from an untrusted data source, limit the initial size of the
				// StringBuilder instance we're about to get or create. It'll expand automatically as needed.
				if (stringBuilder == null)
					stringBuilder = StringBuilderCache.Acquire(capacity: Math.Min(strLength, StringBuilderCache.MaxBuilderSize)); // Actual string length in chars may be smaller

				stringBuilder.Append(this.charBuffer, 0, chars);
				charRead += count;
			}
			while (charRead < strLength);

			return StringBuilderCache.GetStringAndRelease(stringBuilder);
		}

		/// <summary>
		/// Reads a string from the current stream. The string is prefixed with the length, encoded as an integer seven bits at a time.
		/// </summary>
		/// <returns>The string being read.</returns>
		/// <exception cref="IOException"></exception>
		public string ReadString()
		{
			//if (this.ReadBoolean())
			//	return String.Empty;

			int num = 0;
			int num2 = this.Read7BitEncodedInt();

			if (num2 < 0)
				throw new IOException("String deserialization error: IO.IO_InvalidStringLen, Len=" + num2);

			if (num2 == 0)
				return string.Empty;

			this.bytesBuffer ??= new byte[128];
			this.charBuffer ??= new char[this.maxCharsSize];

			StringBuilder? stringBuilder = null;

			do
			{
				int count = ((num2 - num > 128) ? 128 : (num2 - num));
				int num3 = count;

				this.ReadBinary(count).CopyTo(this.bytesBuffer);

				if (num3 == 0)
					throw new EndOfStreamException("String deserialization error: End of stream");

				int chars = this.CharacterEncoding.GetChars(this.bytesBuffer, 0, num3, this.charBuffer, 0);

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
		public string ReadStringOptimized() => (this.UserStringLookupOptimisation) ? this.ReadStringLookupOptimized() : this.ReadString();

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

		#region |   Type   |

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

		#endregion |   Type   |

		#region |   Helper Methods   |

		/// <summary>
		/// Reads a value from the current stream.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		public T? Read<T>() => (T?)this.Read(typeof(T));

		/// <summary>
		/// Reads a value from the current stream that MUST be the same as it is written, using the fewest number of bytes possible.
		/// </summary>
		/// <typeparam name="T">The type of the value.</typeparam>
		public T? ReadOptimized<T>() => (T?)this.ReadOptimized(typeof(T));

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
		/// <param name="propertyTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <returns>The object that is read from the stream.</returns>
		public object? Read(int propertyTypeId)
		{
			switch ((PropertyTypeId)propertyTypeId)
			{
				case PropertyTypeId.String :			  return this.ReadString();
				case PropertyTypeId.Boolean :			  return this.ReadBoolean();
				case PropertyTypeId.NullableBoolean :	  return this.ReadNullableBoolean();
				case PropertyTypeId.SByte :				  return this.ReadSByte();
				case PropertyTypeId.NullableSByte :		  return this.ReadNullableSByte();
				case PropertyTypeId.Int16 :				  return this.ReadInt16();
				case PropertyTypeId.NullableInt16 :		  return this.ReadNullableInt16();
				case PropertyTypeId.Int32 :				  return this.ReadInt32();
				case PropertyTypeId.NullableInt32 :		  return this.ReadNullableInt32();
				case PropertyTypeId.Int64 :				  return this.ReadInt64();
				case PropertyTypeId.NullableInt64:		  return this.ReadNullableInt64();
				case PropertyTypeId.Byte :				  return this.ReadByte();
				case PropertyTypeId.NullableByte :		  return this.ReadNullableByte();
				case PropertyTypeId.UInt16 :			  return this.ReadUInt16();
				case PropertyTypeId.NullableUInt16 :	  return this.ReadNullableUInt16();
				case PropertyTypeId.UInt32 :			  return this.ReadUInt32();
				case PropertyTypeId.NullableUInt32 :	  return this.ReadNullableUInt32();
				case PropertyTypeId.UInt64 :			  return this.ReadUInt64();
				case PropertyTypeId.NullableUInt64 :	  return this.ReadNullableUInt64();
				case PropertyTypeId.Half :				  return this.ReadHalf();
				case PropertyTypeId.NullableHalf :		  return this.ReadNullableHalf();
				case PropertyTypeId.Single :			  return this.ReadSingle();
				case PropertyTypeId.NullableSingle :	  return this.ReadNullableSingle();
				case PropertyTypeId.Double :			  return this.ReadDouble();
				case PropertyTypeId.NullableDouble :	  return this.ReadNullableDouble();
				case PropertyTypeId.Decimal :			  return this.ReadDecimal();
				case PropertyTypeId.NullableDecimal :	  return this.ReadNullableDecimal();
				case PropertyTypeId.DateTime :			  return this.ReadDateTime();
				case PropertyTypeId.NullableDateTime :	  return this.ReadNullableDateTime();
				case PropertyTypeId.TimeSpan :			  return this.ReadTimeSpan();
				case PropertyTypeId.NullableTimeSpan :	  return this.ReadNullableTimeSpan();
				case PropertyTypeId.BitArray :			  return this.ReadBitArray();
				case PropertyTypeId.BitVector32 :		  return this.ReadBitVector32();
				case PropertyTypeId.NullableBitVector32 : return this.ReadNullableBitVector32();
				case PropertyTypeId.Guid :				  return this.ReadGuid();
				case PropertyTypeId.NullableGuid :		  return this.ReadNullableGuid();
				case PropertyTypeId.Char :				  return this.ReadChar();
				case PropertyTypeId.NullableChar :		  return this.ReadNullableChar();
				case PropertyTypeId.Binary :			  return this.ReadBinary(this.ReadInt32Optimized()).ToArray();
									
				default: return null;
			}
		}

		/// <summary>
		/// Reads the object specified by objectType that MUST be the same as it is written, using the fewest number of bytes possible.
		/// Available object types are: bool, byte, short, int, long, bool?, byte?, short?, int?, long?, 
		/// sbyte, ushort, uint, ulong, sbyte?, ushort?, uint?, ulong?, 
		/// Half, float, double, decimal, float?, double?, decimal?, 
		/// DateTime, TimeSpan, DateTime?, TimeSpan?, 
		/// Guid, Guid?, BitArray, BitVector32 BitVector32?,
		/// char, char? and string, 
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
		/// Available object types are: bool, byte, short, int, long, bool?, byte?, short?, int?, long?, 
		/// sbyte, ushort, uint, ulong, sbyte?, ushort?, uint?, ulong?, 
		/// Half, float, double, decimal, float?, double?, decimal?, 
		/// DateTime, TimeSpan, DateTime?, TimeSpan?, 
		/// Guid, Guid?, BitArray, BitVector32 BitVector32?,
		/// char, char? and string, 
		/// </summary>
		/// <param name="propertyTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <returns>The object that is read from the stream.</returns>
		public object? ReadOptimized(int propertyTypeId)
		{
			switch ((PropertyTypeId)propertyTypeId)
			{
				case PropertyTypeId.String :			 return this.ReadStringOptimized();
				case PropertyTypeId.Boolean :			 return this.ReadBoolean();
				case PropertyTypeId.NullableBoolean :	 return this.ReadNullableBoolean();
				case PropertyTypeId.SByte :				 return this.ReadSByte();
				case PropertyTypeId.NullableSByte :		 return this.ReadNullableSByte();
				case PropertyTypeId.Int16 :				 return this.ReadInt16Optimized();
				case PropertyTypeId.NullableInt16 :		 return this.ReadNullableInt16Optimized();
				case PropertyTypeId.Int32 :				 return this.ReadInt32Optimized();
				case PropertyTypeId.NullableInt32 :		 return this.ReadNullableInt32Optimized();
				case PropertyTypeId.Int64 :				 return this.ReadInt64Optimized();
				case PropertyTypeId.NullableInt64 :		 return this.ReadNullableInt64Optimized();
				case PropertyTypeId.Byte :				 return this.ReadByte();
				case PropertyTypeId.NullableByte :		 return this.ReadNullableByte();
				case PropertyTypeId.UInt16 :			 return this.ReadUInt16Optimized();
				case PropertyTypeId.NullableUInt16 :	 return this.ReadNullableUInt16Optimized();
				case PropertyTypeId.UInt32 :			 return this.ReadUInt32Optimized();
				case PropertyTypeId.NullableUInt32 :	 return this.ReadNullableUInt32Optimized();
				case PropertyTypeId.UInt64 :			 return this.ReadUInt64Optimized();
				case PropertyTypeId.NullableUInt64 :	 return this.ReadNullableUInt64Optimized();
				case PropertyTypeId.Half :				 return this.ReadHalf();
				case PropertyTypeId.NullableHalf :		 return this.ReadNullableHalf();
				case PropertyTypeId.Single :			 return this.ReadSingle();
				case PropertyTypeId.NullableSingle :	 return this.ReadNullableSingle();
				case PropertyTypeId.Double :			 return this.ReadDouble();
				case PropertyTypeId.NullableDouble :	 return this.ReadNullableDouble();
				case PropertyTypeId.Decimal :			 return this.ReadDecimalOptimized();
				case PropertyTypeId.NullableDecimal :	 return this.ReadNullableDecimalOptimized();
				case PropertyTypeId.DateTime :			 return this.ReadDateTimeOptimized();
				case PropertyTypeId.NullableDateTime :	 return this.ReadNullableDateTimeOptimized();
				case PropertyTypeId.TimeSpan :			 return this.ReadTimeSpanOptimized();
				case PropertyTypeId.NullableTimeSpan :	 return this.ReadNullableTimeSpanOptimized();
				case PropertyTypeId.BitArray :			 return this.ReadBitArrayOptimized();
				case PropertyTypeId.BitVector32 :		 return this.ReadBitVector32Optimized();
				case PropertyTypeId.NullableBitVector32: return this.ReadNullableBitVector32Optimized();
				case PropertyTypeId.Guid :				 return this.ReadGuid();
				case PropertyTypeId.NullableGuid :		 return this.ReadNullableGuid();
				case PropertyTypeId.Char :				 return this.ReadChar();
				case PropertyTypeId.NullableChar :		 return this.ReadNullableChar();
				case PropertyTypeId.Binary :			 return this.ReadBinary(this.ReadInt32Optimized()).ToArray();

				default: return null;
			}
		}

		#endregion |   Helper Methods   |

		#region |   Extras   |

		/// <summary>
		/// Reads data bits from the sequence up to 8 bits. 
		/// Number of bits is specified by count property.
		/// </summary>
		/// <param name="count">Number of bits to be read. Available value range is: 1-8.</param>
		/// <returns>The bit sequence stored in byte.</returns>
		public byte ReadBits(int count) // Packing order of bit segments: | 5 | 4  | 3|2| 1 |  (The 1 is first written bit segment, 2 is second and so on.
		{
			uint result;

			if (this.bitPosition == 0)
			{
				this.bitBuffer = unchecked((uint)this.ReadByte()); // bitBuffer remains the same, only position is shifted
				result = this.bitBuffer & SequenceWriter.BitMask[count];

				this.bitPosition = (count < 8) ? count : 0;
			}
			else if (this.bitPosition + count > 8) // not enough bits in buffer -> read new byte from stream
			{
				this.bitBuffer |= unchecked((uint)this.ReadByte()) << 8; // read new byte from stream and place them to be higher 2nd byte
				result = (this.bitBuffer >> this.bitPosition) & SequenceWriter.BitMask[count];

				this.bitBuffer >>= 8;
				this.bitPosition += count - 8;
			}
			else
			{
				result = (this.bitBuffer >> this.bitPosition) & SequenceWriter.BitMask[count];
				this.bitPosition += count;
			}

			return unchecked((byte)result);
		}

		/// <summary>
		/// Returns an Int16 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An Int16 value.</returns>
		public short ReadNBitEncodedInt16(int n) => unchecked((short)ReadNBitEncodedUInt64(n));

		/// <summary>
		/// Returns an UInt16 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An UInt16 value.</returns>
		public ushort ReadNBitEncodedUInt16(int n) => unchecked((ushort)ReadNBitEncodedUInt64(n));

		/// <summary>
		/// Returns an Int32 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An Int32 value.</returns>
		public int ReadNBitEncodedInt32(int n) => unchecked((int)ReadNBitEncodedUInt64(n));

		/// <summary>
		/// Returns an UInt32 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An UInt32 value.</returns>
		public uint ReadNBitEncodedUInt32(int n) => unchecked((uint)ReadNBitEncodedUInt64(n));

		/// <summary>
		/// Returns an Int64 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An Int64 value.</returns>
		public long ReadNBitEncodedInt64(int n) => unchecked((long)ReadNBitEncodedUInt64(n));

		/// <summary>
		/// Returns an UInt64 value from the stream that was stored optimized using n-bit encoding.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <returns>An UInt64 value.</returns>
		public ulong ReadNBitEncodedUInt64(int n)
		{
			ulong result = 0;
			var bitMask = SequenceWriter.BitMask[n];
			var singleBit = SequenceWriter.SingleBits[n];
			var bitShift = 0;
			var numOfBits = n + 1;

			while (true)
			{
				var nextByte = this.ReadBits(numOfBits);

				result |= ((ulong)nextByte & bitMask) << bitShift;
				bitShift += n;

				if ((nextByte & singleBit) == 0)
					return result;
			}
		}


		#endregion |   Extras   |

		#region |   Private Methods  |

		/// <summary>
		/// Returns a TimeSpan decoded from packed data.
		/// This routine is called from ReadOptimizedDateTime() and ReadOptimizedTimeSpan().
		/// <remarks>
		/// This routine uses a parameter to allow ReadOptimizedDateTime() to 'peek' at the
		/// next byte and extract the DateTimeKind from bits one and two (IsNegative and HasDays)
		/// which are never set for a Time portion of a DateTime.
		/// </remarks>
		/// </summary>
		/// <param name="initialByte">The first of two always-present bytes.</param>
		/// <returns>A decoded TimeSpan</returns>
		private TimeSpan DecodeTimeSpan(byte initialByte)
		{
			var packedData = new BitVector32(initialByte | (this.ReadByte() << 8)); // Read first two bytes
			var hasTime = packedData[SequenceWriter.HasTimeSection] == 1;
			var hasSeconds = packedData[SequenceWriter.HasSecondsSection] == 1;
			var hasMilliseconds = packedData[SequenceWriter.HasMillisecondsSection] == 1;
			long ticks = 0;

			if (hasMilliseconds)
				packedData = new BitVector32(packedData.Data | (this.ReadByte() << 16) | (this.ReadByte() << 24));
			else if (hasTime && hasSeconds)
				packedData = new BitVector32(packedData.Data | (this.ReadByte() << 16));

			if (hasTime)
			{
				ticks += packedData[SequenceWriter.HoursSection] * TimeSpan.TicksPerHour;
				ticks += packedData[SequenceWriter.MinutesSection] * TimeSpan.TicksPerMinute;
			}

			if (hasSeconds)
				ticks += packedData[(!hasTime && !hasMilliseconds) ? SequenceWriter.MinutesSection : SequenceWriter.SecondsSection] * TimeSpan.TicksPerSecond;

			if (hasMilliseconds)
				ticks += packedData[SequenceWriter.MillisecondsSection] * TimeSpan.TicksPerMillisecond;

			if (packedData[SequenceWriter.HasDaysSection] == 1)
				ticks += this.ReadInt32Optimized() * TimeSpan.TicksPerDay; // (int)Read7BitEncodedSignedInt64(reader) = reader.ReadInt32Optimized()

			if (packedData[SequenceWriter.IsNegativeSection] == 1)
				ticks = -ticks;

			return new TimeSpan(ticks);
		}

		private ulong Read7BitEncodedUInt64()
		{
			if (BitConverter.IsLittleEndian)
				return this.ReadLittleEndian7BitEncodedUInt64();
			else
				return this.ReadBigEndian7BitEncodedUInt64();
		}


		private ulong ReadLittleEndian7BitEncodedUInt64()
		{
			ulong result = 0;
			var bitShift = 0;

			while (true)
			{
				ulong nextByte = this.ReadByte();

				result |= (nextByte & 0x7f) << bitShift;
				bitShift += 7;

				if ((nextByte & 0x80) == 0)
					return result;
			}
		}

		private ulong ReadBigEndian7BitEncodedUInt64()
		{
			ulong result = 0;
			ulong nextByte;
			var bitShift = 0;

			do
			{
				nextByte = this.ReadByte();
				result |= (nextByte & 0x7f) << bitShift;
				bitShift += 7;
			}
			while ((nextByte & 0x80) != 0);

			return result;
		}

		#endregion |   Private Methods  |
	}
}
