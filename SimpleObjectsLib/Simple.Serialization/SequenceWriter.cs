using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security;

namespace Simple.Serialization
{
	public ref struct SequenceWriter
	{
		#region |   Private Members   |

		private readonly Span<byte> span;
		private int spanCount;
		private byte[][]? buffers = null;
		private int[]? bufferCounts = null;
		private int bufferIndex = 0;
		private int bufferSize;
		private uint bitBuffer = 0;
		private Span<byte> bitSpan = Span<byte>.Empty;
		private int bitPosition = 0;

		private Encoding characterEncoding; // = UTF8Encoding.Default;
		private readonly bool preserveDecimalScale = false;
		private readonly bool optimizeForSize = true;
		private readonly bool useStringLookupOptimization = false;
		private bool useFastUtf8;
		private char[]? charBuffer = null;
		private byte[]? byteBuffer = null;
		private int maxChars;
		private UniqueStringList? stringLookup = null;

		private const int MaxArrayPoolRentalSize = 64 * 1024; // try to keep rentals to a reasonable size
															  //public static int MaxStackSize = 1024;
		internal static readonly uint[] BitMask = new uint[9];
		internal static readonly byte[] SingleBits = new byte[8];

		#endregion |   Private Members   |

		#region |   Public Static Members   |

		public static int DefaultCapacity = 64;
		public static int MinimalBufferSize = 16;
		public static int DefaultBufferArrayCapacity = 4;

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

		#endregion |   Internal Static Members   |

		#region |   Constructors and Initialization   |

		public SequenceWriter()
			: this(Span<byte>.Empty)
		{
		}

		public SequenceWriter(Encoding characterEncoding)
			: this(Span<byte>.Empty, characterEncoding)
		{
		}

		public SequenceWriter(Span<byte> span, int start = 0)
			: this(span, UTF8Encoding.Default, start)
		{
		}

		public SequenceWriter(Span<byte> span, Encoding characterEncoding, int start = 0)
		{
			this.span = span;
			//this.currentSpan = span;
			this.spanCount = start;
			this.bufferSize = (span.Length > 0) ? span.Length * 2 : DefaultCapacity;
			this.characterEncoding = characterEncoding;
			this.useFastUtf8 = characterEncoding.CodePage == Encoding.UTF8.CodePage && characterEncoding.EncoderFallback.MaxCharCount <= 1;
		}

		static SequenceWriter()
		{
			BitMask[0] = 0x00; // 00000000
			BitMask[1] = 0x01; // 00000001
			BitMask[2] = 0x03; // 00000011
			BitMask[3] = 0x07; // 00000111
			BitMask[4] = 0x0F; // 00001111
			BitMask[5] = 0x1F; // 00011111
			BitMask[6] = 0x3F; // 00111111
			BitMask[7] = 0x7F; // 01111111
			BitMask[8] = 0xFF; // 11111111

			SingleBits[0] = 0x01; // 00000001
			SingleBits[1] = 0x02; // 00000010
			SingleBits[2] = 0x04; // 00000100
			SingleBits[3] = 0x08; // 00001000
			SingleBits[4] = 0x10; // 00010000
			SingleBits[5] = 0x20; // 00100000
			SingleBits[6] = 0x40; // 01000000
			SingleBits[7] = 0x80; // 10000000
		}

		#endregion |   Constructors and Initialization   |

		#region |   Public Properties   |

		public int BytesWritten { get; private set; }

		public Encoding CharacterEncoding
		{
			get => this.characterEncoding;

			set
			{
				this.characterEncoding = value;
				this.useFastUtf8 = value.CodePage == Encoding.UTF8.CodePage && value.EncoderFallback.MaxCharCount <= 1;
			}
		}

		public bool UserStringLookupOptimisation { get; set; } = false;
		public bool IsSingleSegment => this.buffers is null;


		#endregion |   Public Properties   |

		#region |   Public Methods  |

		public void WriteBinary(byte[] buffer) => this.Write(buffer, 0, buffer.Length);

		public void WriteBinary(byte[] buffer, int start, int count) => this.Write(buffer, start, count);

		public void Write(ArraySegment<byte> arraySegment) => this.Write(arraySegment.Array!, arraySegment.Offset, arraySegment.Count);

		public void Write(byte[] buffer) => this.Write(buffer, 0, buffer.Length);

		public void Write(byte[] buffer, int start, int count)
		{
			//if (!this.IsSingleSegment && count >= MinimalBufferSize && start == 0)
			//{
			//	this.AddNewBuffer(buffer, count);
			//	this.BytesWritten += count;
			//}
			//else
			//{
				this.Write(buffer.AsSpan(start, count));
			//}
		}

		public void Write(Span<byte> span) => this.Write(span, span.Length);

		public void Write(Span<byte> span, int count) => this.Write((ReadOnlySpan<byte>)span, count);

		public void Write(ReadOnlySpan<byte> span) => this.Write(span, span.Length);

		public void Write(ReadOnlySpan<byte> span, int count)
		{
			if (this.IsSingleSegment)
			{
				int currentLeft = this.span.Length - this.spanCount;
				var destination = this.span.Slice(this.spanCount);

				if (count <= currentLeft)
				{
					span.Slice(0, count).CopyTo(destination);
					this.spanCount += count;
				}
				else // Must split data into span and buffers
				{
					span.Slice(0, currentLeft).CopyTo(destination);
					this.spanCount = this.span.Length;

					this.CreateBufferArray();

					int rest = count - currentLeft;
					byte[] buffer = new byte[Math.Max(rest, DefaultCapacity)];

					span.Slice(currentLeft).CopyTo(buffer);
					this.buffers![0] = buffer;
					this.bufferCounts![0] = rest;
				}
			}
			else
			{
				byte[] currentBuffer = this.buffers![this.bufferIndex];
				int currentCount = this.bufferCounts![this.bufferIndex];
				int currentLeft = currentBuffer.Length - currentCount;

				if (count <= currentLeft)
				{
					span.CopyTo(currentBuffer.AsSpan(startIndex: currentCount));
					this.bufferCounts![this.bufferIndex] += count;
				}
				else // Must split into two parts
				{
					span.Slice(0, currentLeft).CopyTo(currentBuffer.AsSpan(startIndex: currentCount));
					this.bufferCounts![this.bufferIndex] = currentBuffer.Length;
					
					int rest = count - currentLeft;
					byte[] newBuffer = new byte[Math.Max(rest, this.bufferSize)];
					
					span.Slice(currentLeft).CopyTo(newBuffer);
					this.AddNewBuffer(newBuffer, count: rest);
				}
			}

			this.BytesWritten += count;
		}


		private void ResizeBufferArray()
		{
			int newSize = this.buffers!.Length * 2; // Double the size
			
			Array.Resize<byte[]>(ref this.buffers, newSize); 
			Array.Resize<int>(ref this.bufferCounts, newSize);
		}

		/// <summary>
		/// Writes all data to given <see cref="IBufferWriter{byte}"/>
		/// </summary>
		/// <param name="writer">The writer to write data to.</param>
		public void WriteDataTo(IBufferWriter<byte> writer)
		{
			if (this.spanCount > 0)
				writer.Write(this.span.Slice(0, this.spanCount));

			if (this.buffers != null)
				for (int i = 0; i <= this.bufferIndex; i++)
					writer.Write(this.buffers[i].AsSpan(0, this.bufferCounts![i]));
		}


		/// <summary>
		/// Copy all data to single byte array.
		/// </summary>
		/// <returns></returns>
		public byte[] ToArray() => this.ToArray(new byte[this.BytesWritten], 0);

		/// <summary>
		/// Copy all data into given byte array starting from position defined by start value.
		/// </summary>
		/// <param name="buffer">The buffer where to write data</param>
		/// <param name="start">The beggining position within buffer where data will be written.</param>
		/// <returns>Resulting array with data</returns>
		public byte[] ToArray(byte[] buffer, int start = 0)
		{
			this.span.Slice(0, this.spanCount).CopyTo(buffer.AsSpan().Slice(start));

			int position = start + this.spanCount;

			if (this.buffers != null)
			{
				for (int i = 0; i <= this.bufferIndex; i++)
				{
					int count = this.bufferCounts![i];

					Array.Copy(sourceArray: this.buffers[i], sourceIndex: 0, destinationArray: buffer, destinationIndex: position, length: count);
					position += count;
				}
			}

			return buffer;
		}

		//private int GetLength()
		//{
		//	int length = this.spanCount;

		//	if (this.buffers != null)
		//		length += this.bufferCounts.Sum();

		//	return length;
		//}

		public void Dispose()
		{
			//GC.SuppressFinalize((object)this);
		}

		#endregion |   Public Methods  |

		#region |   Primitive Types: bool, sbyte, short, int, long   |

		/// <summary>
		/// Writes a one-byte Boolean value to the current stream, with 0 representing false and 1 representing true.
		/// Stored Size: 1 bit.
		/// </summary>
		/// <param name="value">The Boolean value to write (0 or 1).</param>
		public void WriteBoolean(bool value) => this.WriteBits(value ? (byte)1 : (byte)0, 1);

		/// <summary>
		/// Writes a Nullable Boolean to the stream.
		/// Stored Size: 2 bits.
		/// </summary>
		/// <param name="value">The Nullable Boolean value to write.</param>
		public void WriteNullableBoolean(bool? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteBoolean((bool)value); } }

		/// <summary>
		/// Writes a signed byte to the current stream and advances the stream position by one byte.
		/// </summary>
		/// <param name="value">The signed byte to write.</param>
		public void WriteSByte(sbyte value) => this.WriteByte(unchecked((byte)value));

		/// <summary>
		/// Writes a Nullable SByte to the stream.
		/// </summary>
		/// <param name="value">The Nullable SByte value to write.</param>
		public void WriteNullableSByte(sbyte? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteSByte((sbyte)value); } }

		/// <summary>
		/// Writes a two-byte signed integer to the current stream and advances the stream position by two bytes.
		/// Stored Size: 2 bytes.
		/// </summary>
		/// <param name="value">The two-byte signed integer to write.</param>
		public void WriteInt16(short value)
		{
			if (BitConverter.IsLittleEndian)
			{
				this.WriteByte((byte)value);
				this.WriteByte((byte)(value >> 8));
			}
			else
			{
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)value);
			}
		}

		/// <summary>
		/// Writes a Nullable Int16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int16 value to write.</param>
		public void WriteNullableInt16(short? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteInt16((short)value); } }

		/// <summary>
		/// Write an Int16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Int16 to store.</param>
		public void WriteInt16Optimized(short value) => this.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Write an Nullable Int16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable Int16 to store.</param>
		public void WriteNullableInt16Optimized(short? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteInt16Optimized((short)value); } }

		/// <summary>
		/// Writes a four-byte signed integer to the current stream and advances the stream position by four bytes.
		/// Stored Size: 4 bytes.
		/// </summary>
		/// <param name="value">The four-byte signed integer to write.</param>
		public void WriteInt32(int value)
		{
			if (BitConverter.IsLittleEndian)
			{
				this.WriteByte((byte)value);
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)(value >> 16));
				this.WriteByte((byte)(value >> 24));
			}
			else
			{
				this.WriteByte((byte)(value >> 24));
				this.WriteByte((byte)(value >> 16));
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)value);
			}
		}

		/// <summary>
		/// Writes a Nullable Int32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int32 value to write.</param>
		public void WriteNullableInt32(int? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteInt32((int)value); } }

		/// <summary>
		/// Write an Int32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Int32 to store.</param>
		public void WriteInt32Optimized(int value) => this.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Write an Nullable Int32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable Int32 to store.</param>
		public void WriteNullableInt32Optimized(int? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteInt32Optimized((int)value); } }

		/// <summary>
		/// Writes an eight-byte signed integer to the current stream and advances the stream position by eight bytes.
		/// Stored Size: 4 bytes.
		/// </summary>
		/// <param name="value">The eight-byte signed integer to write.</param>
		public void WriteInt64(long value)
		{
			if (BitConverter.IsLittleEndian)
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
			else
			{
				this.WriteByte((byte)(value >> 56));
				this.WriteByte((byte)(value >> 48));
				this.WriteByte((byte)(value >> 40));
				this.WriteByte((byte)(value >> 32));
				this.WriteByte((byte)(value >> 24));
				this.WriteByte((byte)(value >> 16));
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)value);
			}
		}

		/// <summary>
		/// Writes a Nullable Int64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable Int64 value to write.</param>
		public void WriteNullableInt64(long? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteInt64((long)value); } }

		/// <summary>
		/// Write an Int64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Int64 to store.</param>
		public void WriteInt64Optimized(long value) => this.Write7BitEncodedUInt64(unchecked((ulong)value));

		/// <summary>
		/// Write an Nullable Int64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable Int64 to store.</param>
		public void WriteNullableInt64Optimized(long? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteInt64Optimized((long)value); } }

		#endregion |   Primitive Types: bool, sbyte, short, int, long   |

		#region |   Unsigned Primitives: byte, ushort, uint, ulong   |

		/// <summary>
		/// Writes an unsigned byte to the current stream and advances the stream position by one byte.
		/// Stored Size: 1 byte.
		/// </summary>
		/// <param name="value">The unsigned byte to write.</param>
		public void WriteByte(byte value)
		{
			//byte[] buffer = new byte[1];
			//
			//buffer[0] = value;
			//
			//this.WriteSequence(buffer.AsSpan());
			
			if (this.IsSingleSegment)
			{
				if (this.spanCount < this.span.Length)
				{
					this.span[this.spanCount++] = value;
				}
				else // Not enough space in span, create buffers array and fill it with first byte of data
				{
					byte[] buffer = new byte[this.bufferSize];

					this.CreateBufferArray();
					
					this.buffers![0] = buffer;
					buffer[0] = value;
					this.bufferCounts![0] = 1;
					this.bufferSize *= 2;
				}
			}
			else
			{
				byte[] buffer;

				if (this.bufferCounts![this.bufferIndex] == this.buffers![this.bufferIndex].Length)
				{
					buffer = new byte[this.bufferSize];
					buffer[0] = value;
					this.AddNewBuffer(buffer, count: 1);
				}
				else
				{
					buffer = this.buffers[this.bufferIndex];
					buffer[this.bufferCounts[this.bufferIndex]++] = value;
				}
			}

			this.BytesWritten++;
		}

		/// <summary>
		/// Writes a Nullable Byte to the stream.
		/// </summary>
		/// <param name="value">The Nullable Byte value to write.</param>
		public void WriteNullableByte(byte? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteByte((byte)value); } }

		/// <summary>
		/// Writes a two-byte unsigned integer to the current stream and advances the stream position by two bytes.
		/// </summary>
		/// <param name="value">The two-byte unsigned integer to write.</param>
		public void WriteUInt16(ushort value)
		{
			if (BitConverter.IsLittleEndian)
			{
				this.WriteByte((byte)value);
				this.WriteByte((byte)(value >> 8));
			}
			else
			{
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)value);
			}
		}

		/// <summary>
		/// Writes a Nullable UInt16 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt16 value to write.</param>
		public void WriteNullableUInt16(ushort? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteUInt16((ushort)value); } }

		/// <summary>
		/// Write an UInt16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The UInt16 to store.</param>
		public void WriteUInt16Optimized(ushort value) => this.Write7BitEncodedUInt64(value);

		/// <summary>
		/// Write an Nullable UInt16 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable UInt16 to store.</param>
		public void WriteNullableUInt16Optimized(ushort? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteUInt16Optimized((ushort)value); } }

		/// <summary>
		/// Writes a four-byte unsigned integer to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte unsigned integer to write.</param>
		public void WriteUInt32(uint value)
		{
			if (BitConverter.IsLittleEndian)
			{
				this.WriteByte((byte)value);
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)(value >> 16));
				this.WriteByte((byte)(value >> 24));
			}
			else
			{
				this.WriteByte((byte)(value >> 24));
				this.WriteByte((byte)(value >> 16));
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)value);
			}
		}

		/// <summary>
		/// Writes a Nullable UInt32 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt32 value to write.</param>
		public void WriteNullableUInt32(uint? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteUInt32((uint)value); } }

		/// <summary>
		/// Write an UInt32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The UInt32 to store.</param>
		public void WriteUInt32Optimized(uint value) => this.Write7BitEncodedUInt64(value);

		/// <summary>
		/// Write an Nullable UInt32 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable UInt32 to store.</param>
		public void WriteNullableUInt32Optimized(uint? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteUInt32Optimized((uint)value); } }

		/// <summary>
		/// Writes an eight-byte unsigned integer to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte unsigned integer to write.</param>
		public void WriteUInt64(ulong value)
		{
			if (BitConverter.IsLittleEndian)
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
			else
			{
				this.WriteByte((byte)(value >> 56));
				this.WriteByte((byte)(value >> 48));
				this.WriteByte((byte)(value >> 40));
				this.WriteByte((byte)(value >> 32));
				this.WriteByte((byte)(value >> 24));
				this.WriteByte((byte)(value >> 16));
				this.WriteByte((byte)(value >> 8));
				this.WriteByte((byte)value);
			}
		}

		/// <summary>
		/// Writes a Nullable UInt64 to the stream.
		/// </summary>
		/// <param name="value">The Nullable UInt64 value to write.</param>
		public void WriteNullableUInt64(ulong? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteUInt64((ulong)value); } }

		/// <summary>
		/// Write an UInt64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The UInt64 to store.</param>
		public void WriteUInt64Optimized(ulong value) => this.Write7BitEncodedUInt64(value);

		/// <summary>
		/// Write an Nullable UInt64 value using the fewest number of bytes possible.
		/// </summary>
		/// <param name="value">The Nullable UInt64 to store.</param>
		public void WriteNullableUInt64Optimized(ulong? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteUInt64Optimized((ulong)value); } }

		#endregion |   Unsigned Primitives: byte, ushort, uint, ulong   |

		#region |   Floating Point & Decimal Types: Half, float, double, decimal   |

		/// <summary>
		/// Write a <see cref="Half" /> to the sequence (2 bytes).
		/// </summary>
		/// <param name="value">The Half value</param>
		public void WriteHalf(Half value) => this.WriteUInt16((ushort)value);

		/// <summary>
		/// Write a Nullable <see cref="Half" /> to the sequence
		/// </summary>
		/// <param name="value">The Half value</param>
		public void WriteNullableHalf(Half? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteHalf((Half)value); } }

		/// <summary>
		/// Writes a four-byte floating-point value to the current stream and advances the stream position by four bytes.
		/// </summary>
		/// <param name="value">The four-byte floating-point value to write.</param>
		public unsafe void WriteSingle(float value)
		{
			UInt32 raw = *(UInt32*)&value;

			if (BitConverter.IsLittleEndian)
			{
				this.WriteByte((byte)raw);
				this.WriteByte((byte)(raw >> 8));
				this.WriteByte((byte)(raw >> 16));
				this.WriteByte((byte)(raw >> 24));
			}
			else
			{
				this.WriteByte((byte)(raw >> 24));
				this.WriteByte((byte)(raw >> 16));
				this.WriteByte((byte)(raw >> 8));
				this.WriteByte((byte)raw);
			}
		}

		/// <summary>
		/// Writes a Nullable Single to the stream.
		/// </summary>
		/// <param name="value">The Nullable Single value to write.</param>
		public void WriteNullableSingle(float? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteSingle((float)value); } }

		/// <summary>
		///  Writes an eight-byte floating-point value to the current stream and advances the stream position by eight bytes.
		/// </summary>
		/// <param name="value">The eight-byte floating-point value to write.</param>
		public unsafe void WriteDouble(double value)
		{
			UInt64 raw = *(UInt64*)&value;

			if (BitConverter.IsLittleEndian)
			{
				this.WriteByte((byte)raw);
				this.WriteByte((byte)(raw >> 8));
				this.WriteByte((byte)(raw >> 16));
				this.WriteByte((byte)(raw >> 24));
				this.WriteByte((byte)(raw >> 32));
				this.WriteByte((byte)(raw >> 40));
				this.WriteByte((byte)(raw >> 48));
				this.WriteByte((byte)(raw >> 56));
			}
			else
			{
				this.WriteByte((byte)(raw >> 56));
				this.WriteByte((byte)(raw >> 48));
				this.WriteByte((byte)(raw >> 40));
				this.WriteByte((byte)(raw >> 32));
				this.WriteByte((byte)(raw >> 24));
				this.WriteByte((byte)(raw >> 16));
				this.WriteByte((byte)(raw >> 8));
				this.WriteByte((byte)raw);
			}
		}

		/// <summary>
		/// Writes a Nullable Double to the stream.
		/// </summary>
		/// <param name="value">The Nullable Double value to write.</param>
		public void WriteNullableDouble(double? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteDouble((double)value); } }

		/// <summary>
		/// Writes a decimal value to the current stream and advances the stream position by sixteen bytes.
		/// </summary>
		/// <param name="value">The decimal value to write.</param>
		public void WriteDecimal(decimal value)
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

		/// <summary>
		/// Writes a Nullable Decimal to the stream.
		/// </summary>
		/// <param name="value">The Nullable Decimal value to write.</param>
		public void WriteNullableDecimal(decimal? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteDecimal((decimal)value); } }

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
		public void WriteNullableDecimalOptimized(decimal? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteDecimalOptimized((decimal)value); } }

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
		public void WriteNullableDateTime(DateTime? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteDateTime((DateTime)value); } }

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
		public void WriteNullableDateTimeOptimized(DateTime? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteDateTimeOptimized((DateTime)value); } }

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
		public void WriteNullableTimeSpan(TimeSpan? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteTimeSpan((TimeSpan)value); } }

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
		public void WriteNullableTimeSpanOptimized(TimeSpan? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteTimeSpanOptimized((TimeSpan)value); } }

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
			this.WriteInt32Optimized(value.Length);

			for (int i = 0; i < value.Length; i++)
				this.WriteBoolean(value[i]);
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
				this.Write(data);
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
		public void WriteNullableBitVector32(BitVector32? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteBitVector32((BitVector32)value); } }

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
		public void WriteNullableBitVector32Optimized(BitVector32? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteBitVector32((BitVector32)value); } }

		/// <summary>
		/// Writes a Guid into the stream.
		/// Stored Size: 16 bytes.
		/// </summary>
		/// <param name="value"></param>
		public void WriteGuid(Guid value) => this.Write(value.ToByteArray());

		/// <summary>
		/// Writes a Nullable Guid to the stream.
		/// </summary>
		/// <param name="value">The Nullable Guid value to write.</param>
		public void WriteNullableGuid(Guid? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteGuid((Guid)value); } }

		#endregion |   Specific Types: BitArray, BitVector32, Guid   |

		#region |   Char & String   |

		/// <summary>
		///  Writes a Unicode character to the current stream and advances the current position of the stream in accordance with the Encoding used and the specific characters being written to the stream.
		/// </summary>
		/// <param name="value">The non-surrogate, Unicode character to write.</param>
		public void WriteChar(char value)
		{
#if NETSTANDARD
			if (!MinimalisticRune.TryCreate(value, out MinimalisticRune rune)) // optimistically assume UTF-8 code path (which uses Rune) will be hit
#else
			if (!Rune.TryCreate(value, out Rune rune)) // optimistically assume UTF-8 code path (which uses Rune) will be hit
#endif
				throw new ArgumentException("Surrogates Not Allowed As Single Char");

			Span<byte> span = new byte[8];

			if (this.useFastUtf8)
			{
				int length = rune.EncodeToUtf8(span);

				this.Write(span.Slice(0, length));
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

				this.Write(span.Slice(0, actualByteCount));

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
		public void WriteNullableChar(char? value) { if (value is null) this.WriteBoolean(true); else { this.WriteBoolean(false); this.WriteChar((char)value); } }

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

					if (this.byteBuffer == null)
					{
						this.byteBuffer = new byte[256];
						this.maxChars = this.byteBuffer.Length / this.CharacterEncoding.GetMaxByteCount(1);
					}

					if (byteCount <= this.byteBuffer.Length)
					{
						this.CharacterEncoding.GetBytes(value, 0, value.Length, this.byteBuffer, 0);
						this.Write(this.byteBuffer, 0, byteCount);

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
							fixed (byte* bytes = &this.byteBuffer[0])
							{
								bytes2 = this.CharacterEncoding.GetBytes((char*)checked(unchecked((nuint)ptr) + unchecked((nuint)checked(unchecked((nint)num) * (nint)2))), num3, bytes, this.byteBuffer.Length); //, flush: num3 == num2);
							}
						}

						this.Write(this.byteBuffer, 0, bytes2);

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
						Span<byte> stringSpan = new byte[128]; // staclalloc byte[128] is not allowed in ref struct (this.WriteSequence has error CS8350)

						actualByteCount = this.CharacterEncoding.GetBytes(value, stringSpan.Slice(1));
						stringSpan[0] = (byte)actualByteCount; // bypass call to Write7BitEncodedInt
						this.Write(stringSpan.Slice(0, actualByteCount + 1 /* length prefix */));

						return;
					}
					else if (value.Length <= MaxArrayPoolRentalSize / 3)
					{
						byte[] rented = ArrayPool<byte>.Shared.Rent(value.Length * 3); // max expansion: each char -> 3 bytes
						
						actualByteCount = this.CharacterEncoding.GetBytes(value, rented);
						this.WriteInt32Optimized(actualByteCount);
						this.Write(rented, 0, actualByteCount);
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
					this.Write(buffer.Slice(0, bytesWritten).ToArray());
				}
				else
				{
					this.WriteInt32Optimized(0); // <- String.Empty
				}
			}
		}


		[SecuritySafeCritical]
		//[__DynamicallyInvokable]
		public unsafe void WriteString(string? value)
		{
			if (value.IsNullOrEmpty()) // value != null || value.Length == 0;
			{
				//this.WriteBoolean(true);
				this.Write7BitEncodedInt(0);
			}
			else
			{
				//this.WriteBoolean(false);

				int byteCount = this.CharacterEncoding.GetByteCount(value);

				if (byteCount > 6000)
					byteCount = byteCount;

				this.Write7BitEncodedInt(byteCount);

				if (this.byteBuffer == null)
				{
					this.byteBuffer = new byte[256];
					this.maxChars = this.byteBuffer.Length / this.CharacterEncoding.GetMaxByteCount(1);
				}

				if (byteCount <= this.byteBuffer.Length)
				{
					this.CharacterEncoding.GetBytes(value, 0, value.Length, this.byteBuffer, 0);
					this.Write(this.byteBuffer, 0, byteCount);

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
						fixed (byte* bytes = this.byteBuffer)
						{
							bytes2 = this.CharacterEncoding.GetBytes((char*)checked(unchecked((nuint)ptr) + unchecked((nuint)checked(unchecked((nint)num) * (nint)2))), num3, bytes, this.byteBuffer.Length); //, flush: num3 == num2);
						}
					}

					this.Write(this.byteBuffer, 0, bytes2);
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
				this.WriteByte((byte)(num | 0x80u));

			this.WriteByte((byte)num);
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
				this.WriteBits(0, 3); // 0 -> Null or Empty

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

						this.WriteBits(1, 3); // 1 -> 'Y'

						return;

					case 'N':

						this.WriteBits(2, 3); // 2 -> 'N'

						return;

					case ' ':

						this.WriteBits(3, 3); // 3 -> ' '

						return;

					default:

						this.WriteBits(4, 3); // 4 -> single char
						this.WriteChar(singleChar);

						return;
				}
			}

			int stringIndex;
			this.stringLookup ??= new UniqueStringList();

			if (this.stringLookup.Add(value, out stringIndex))
			{
				// string added into lookup
				this.WriteBits(5, 3); // 7 -> string is new, write it
				this.WriteString(value);
			}
			else
			{
				// string already exists
				this.WriteBits(6, 3); // 6 -> string already exists in lookup, write string index
				this.WriteInt32Optimized(stringIndex);
			}
		}

		#endregion |   Char & String   |

		#region |   Type  |

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
		public void WriteType(Type value, bool fullyQualified) => this.WriteString((fullyQualified) ? value.AssemblyQualifiedName! : value.FullName!);

		#endregion |   Type   |

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

		#region |   Floating Point & Decimal Types: Half, float, double, decimal   |

		/// <summary>
		/// Write a <see cref="Half" /> to the sequence (2 bytes).
		/// </summary>
		/// <param name="value">The Half value</param>
		public void Write(Half value) => this.WriteHalf(value);

		/// <summary>
		/// Write a Nullable <see cref="Half" /> to the sequence (2 bytes).
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

		#endregion |   Floating Point & Decimal Types: Half, float, double, decimal   |

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
		/// <param name="propertyTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <param name="value">The object value.</param>
		public void Write(int propertyTypeId, object? value)
		{
			switch ((PropertyTypeId)propertyTypeId)
			{
				case PropertyTypeId.String :			  this.WriteString((string)value!); break;
				case PropertyTypeId.Boolean :			  this.WriteBoolean((bool)value!); break;
				case PropertyTypeId.NullableBoolean :	  this.WriteNullableBoolean((bool?)value); break;
				case PropertyTypeId.SByte :				  this.WriteSByte((sbyte)value!); break;
				case PropertyTypeId.NullableSByte :		  this.WriteNullableSByte((sbyte?)value); break;
				case PropertyTypeId.Int16 :				  this.WriteInt16((short)value!); break;
				case PropertyTypeId.NullableInt16 :		  this.WriteNullableInt16((short?)value); break;
				case PropertyTypeId.Int32 :				  this.WriteInt32((int)value!); break;
				case PropertyTypeId.NullableInt32 :		  this.WriteNullableInt32((int?)value); break;
				case PropertyTypeId.Int64 :				  this.WriteInt64((long)value!); break;
				case PropertyTypeId.NullableInt64 :		  this.WriteNullableInt64((long?)value); break;
				case PropertyTypeId.Byte :				  this.WriteByte((byte)value!); break;
				case PropertyTypeId.NullableByte :		  this.WriteNullableByte((byte?)value); break;
				case PropertyTypeId.UInt16 :			  this.WriteUInt16((ushort)value!); break;
				case PropertyTypeId.NullableUInt16 :	  this.WriteNullableUInt16((ushort?)value); break;
				case PropertyTypeId.UInt32 :			  this.WriteUInt32((uint)value!); break;
				case PropertyTypeId.NullableUInt32 :	  this.WriteNullableUInt32((uint?)value); break;
				case PropertyTypeId.UInt64 :			  this.WriteUInt64((ulong)value!); break;
				case PropertyTypeId.NullableUInt64 :	  this.WriteNullableUInt64((ulong?)value); break;
				case PropertyTypeId.Half :				  this.WriteHalf((Half)value!); break;
				case PropertyTypeId.NullableHalf :		  this.WriteNullableHalf((Half?)value); break;
				case PropertyTypeId.Single :			  this.WriteSingle((float)value!); break;
				case PropertyTypeId.NullableSingle :	  this.WriteNullableSingle((float?)value); break;
				case PropertyTypeId.Double :			  this.WriteDouble((double)value!); break;
				case PropertyTypeId.NullableDouble :	  this.WriteNullableDouble((double?)value); break;
				case PropertyTypeId.Decimal :			  this.WriteDecimal((decimal)value!); break;
				case PropertyTypeId.NullableDecimal :	  this.WriteNullableDecimal((decimal?)value); break;
				case PropertyTypeId.DateTime :			  this.WriteDateTime((DateTime)value!); break;
				case PropertyTypeId.NullableDateTime :	  this.WriteNullableDateTime((DateTime?)value); break;
				case PropertyTypeId.TimeSpan :			  this.WriteTimeSpan((TimeSpan)value!); break;
				case PropertyTypeId.NullableTimeSpan :	  this.WriteNullableTimeSpan((TimeSpan?)value); break;
				case PropertyTypeId.BitArray :			  this.WriteBitArray((BitArray)value!); break;
				case PropertyTypeId.BitVector32 :		  this.WriteBitVector32((BitVector32)value!); break;
				case PropertyTypeId.NullableBitVector32 : this.WriteNullableBitVector32((BitVector32?)value); break;
				case PropertyTypeId.Guid :				  this.WriteGuid((Guid)value!); break;
				case PropertyTypeId.NullableGuid :		  this.WriteNullableGuid((Guid?)value); break;
				case PropertyTypeId.Char :				  this.WriteChar((char)value!); break;
				case PropertyTypeId.NullableChar :		  this.WriteNullableChar((char?)value); break;
				case PropertyTypeId.Binary :

					if (value is null || ((byte[])value).Length == 0)
						this.WriteInt32Optimized(0);
					else
					{
						this.WriteInt32Optimized(((byte[])value).Length);
						this.WriteBinary((byte[])value);
					}

					break;

				default: throw new ArgumentOutOfRangeException($"PropertyTypeId {propertyTypeId.ToString()} is not supported");
			}
		}

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
		/// <param name="propertyTypeId">The object typeId that uniquely identify writen object type.</param>
		/// <param name="value">The object value.</param>
		public void WriteOptimized(int propertyTypeId, object? value)
		{
			switch ((PropertyTypeId)propertyTypeId)
			{
				case PropertyTypeId.String :			  this.WriteStringOptimized((string)value!); break;
				case PropertyTypeId.Boolean :			  this.WriteBoolean((bool)value!); break;
				case PropertyTypeId.NullableBoolean :	  this.WriteNullableBoolean((bool?)value); break;
				case PropertyTypeId.SByte :				  this.WriteSByte((sbyte)value!); break;
				case PropertyTypeId.NullableSByte :		  this.WriteNullableSByte((sbyte?)value); break;
				case PropertyTypeId.Int16 :				  this.WriteInt16Optimized((short)value!); break;
				case PropertyTypeId.NullableInt16 :		  this.WriteNullableInt16Optimized((short?)value); break;
				case PropertyTypeId.Int32 :				  this.WriteInt32Optimized((int)value!); break;
				case PropertyTypeId.NullableInt32 :		  this.WriteNullableInt32Optimized((int?)value); break;
				case PropertyTypeId.Int64 :				  this.WriteInt64Optimized((long)value!); break;
				case PropertyTypeId.NullableInt64 :		  this.WriteNullableInt64Optimized((long?)value); break;
				case PropertyTypeId.Byte :				  this.WriteByte((byte)value!); break;
				case PropertyTypeId.NullableByte :		  this.WriteNullableByte((byte?)value); break;
				case PropertyTypeId.UInt16 :			  this.WriteUInt16Optimized((ushort)value!); break;
				case PropertyTypeId.NullableUInt16 :	  this.WriteNullableUInt16Optimized((ushort?)value); break;
				case PropertyTypeId.UInt32 :			  this.WriteUInt32Optimized((uint)value!); break;
				case PropertyTypeId.NullableUInt32 :	  this.WriteNullableUInt32Optimized((uint?)value); break;
				case PropertyTypeId.UInt64 :			  this.WriteUInt64Optimized((ulong)value!); break;
				case PropertyTypeId.NullableUInt64 :	  this.WriteNullableUInt64Optimized((ulong?)value); break;
				case PropertyTypeId.Half :				  this.WriteHalf((Half)value!); break;
				case PropertyTypeId.NullableHalf :		  this.WriteNullableHalf((Half?)value); break;
				case PropertyTypeId.Single :			  this.WriteSingle((float)value!); break;
				case PropertyTypeId.NullableSingle :	  this.WriteNullableSingle((float?)value); break;
				case PropertyTypeId.Double :			  this.WriteDouble((double)value!); break;
				case PropertyTypeId.NullableDouble :	  this.WriteNullableDouble((double?)value); break;
				case PropertyTypeId.Decimal :			  this.WriteDecimalOptimized((decimal)value!); break;
				case PropertyTypeId.NullableDecimal :	  this.WriteNullableDecimalOptimized((decimal?)value); break;
				case PropertyTypeId.DateTime :			  this.WriteDateTimeOptimized((DateTime)value!); break;
				case PropertyTypeId.NullableDateTime :    this.WriteNullableDateTimeOptimized((DateTime?)value); break;
				case PropertyTypeId.TimeSpan :			  this.WriteTimeSpanOptimized((TimeSpan)value!); break;
				case PropertyTypeId.NullableTimeSpan :	  this.WriteNullableTimeSpanOptimized((TimeSpan?)value); break;
				case PropertyTypeId.BitArray :			  this.WriteBitArrayOptimized((BitArray)value!); break;
				case PropertyTypeId.BitVector32 :		  this.WriteBitVector32Optimized((BitVector32)value!); break;
				case PropertyTypeId.NullableBitVector32 : this.WriteNullableBitVector32Optimized((BitVector32?)value); break;
				case PropertyTypeId.Guid :				  this.WriteGuid((Guid)value!); break;
				case PropertyTypeId.NullableGuid :		  this.WriteNullableGuid((Guid?)value); break;
				case PropertyTypeId.Char :				  this.WriteChar((char)value!); break;
				case PropertyTypeId.NullableChar :		  this.WriteNullableChar((char?)value); break;
				case PropertyTypeId.Binary :

					if (value is null)
						this.WriteInt32Optimized(0);
					else
					{
						this.WriteInt32Optimized(((byte[])value).Length);
						this.WriteBinary((byte[])value);
					}

					break;

				default: throw new ArgumentOutOfRangeException($"PropertyTypeId {propertyTypeId.ToString()} is not supported");
			}
		}

		#endregion |   Helper Methods   |

		#region |   Extras   |

		/// <summary>
		/// Writes data bits to the bit stream.
		/// Number of bits writen from data is specified by count property.
		/// The data should NOT excceed the max value possible by number of bit specified by count.
		/// </summary>
		/// <param name="data">The bits data to be writen.</param>
		/// <param name="count">Number of bits from data to be written. Available value range is: 1-8.</param>
		public void WriteBits(byte data, int count) // Packing order of bit segments: | 5 | 4  | 3|2| 1 |  (The 1 is first written bit segment, 2 is second and so on.
		{
			if (count > 8)
				throw new ArgumentOutOfRangeException("count ig greater than 8");
			
			uint dataToWrite = unchecked(data & BitMask[count]); // Just to make shure that no more bits exists than specified by count

			if (this.bitPosition == 0)
			{
				this.bitBuffer = dataToWrite;
				this.bitPosition = count;
				this.WriteByte(unchecked((byte)dataToWrite));
				this.bitSpan = this.GetCurrentSingleByteSpan();

				if (this.bitPosition == 8)
					this.bitPosition = 0;
			}
			else
			{
				this.bitBuffer |= (dataToWrite << this.bitPosition);
				this.bitSpan[0] = (byte)this.bitBuffer;
				this.bitPosition += count;

				if (this.bitPosition == 8)
				{
					this.bitPosition = 0;
				}
				else if (this.bitPosition > 8) // byte overflow, update bit buffer where it is placed inside base stream
				{
					this.bitBuffer >>= 8;
					this.WriteByte(unchecked((byte)this.bitBuffer));
					this.bitSpan = this.GetCurrentSingleByteSpan();
					this.bitPosition -= 8;
				}
			}
		}

		/// <summary>
		/// Stores a 16-bit signed value into the stream using N-bit encoding.
		/// 
		/// The value is written n bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The Int16 value to encode.</param>
		public void WriteNBitEncodedInt16(int n, short value) => this.WriteNBitEncodedUInt64(n, unchecked((ulong)value));

		/// <summary>
		/// Stores a 16-bit unsigned value into the stream using N-bit encoding.
		/// 
		/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// 
		/// See Write(ULong) for details of the values that are optimizable.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The UInt16 value to encode.</param>
		public void WriteNBitEncodedUInt16(int n, ushort value) => this.WriteNBitEncodedUInt64(n, unchecked(value));

		/// <summary>
		/// Stores a 32-bit signed value into the stream using N-bit encoding.
		/// 
		/// The value is written n bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The Int32 value to encode.</param>
		public void WriteNBitEncodedInt32(int n, int value) => this.WriteNBitEncodedUInt64(n, unchecked((ulong)value));

		/// <summary>
		/// Stores a 32-bit unsigned value into the stream using N-bit encoding.
		/// 
		/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// 
		/// See Write(ULong) for details of the values that are optimizable.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The ULong64 value to encode.</param>
		public void WriteNBitEncodedUInt32(int n, uint value) => this.WriteNBitEncodedUInt64(n, unchecked(value));

		/// <summary>
		/// Stores a 64-bit signed value into the stream using N-bit encoding.
		/// 
		/// The value is written n bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The Int64 value to encode.</param>
		public void WriteNBitEncodedInt64(int n, long value) => this.WriteNBitEncodedUInt64(n, unchecked((ulong)value));

		/// <summary>
		/// Stores a 64-bit unsigned value into the stream using N-bit encoding.
		/// 
		/// The value is written 7 bits at a time (starting with the least-significant bits) until there are no more bits to write.
		/// The eighth bit of each byte stored is used to indicate whether there are more bytes following this one.
		/// 
		/// See Write(ULong) for details of the values that are optimizable.
		/// </summary>
		/// <param name="n">Encoding number of bits.</param>
		/// <param name="value">The ULong64 value to encode.</param>
		public void WriteNBitEncodedUInt64(int n, ulong value) // <- TODO: Check is this method endianness !!!
		{
			var singleBit = SingleBits[n];
			var numOfBits = n + 1;

			while (value >= singleBit)
			{
				this.WriteBits((byte)(value | singleBit), numOfBits);
				value >>= n;
			}

			this.WriteBits((byte)value, numOfBits);
		}


		#endregion |   Extras   |

		#region |   Private Methods  |

		/// <summary>
		/// Encodes a TimeSpan into the fewest number of bytes.
		/// Has been separated from the WriteOptimized(TimeSpan) method so that WriteOptimized(DateTime)
		/// can also use this for .NET 2.0 DateTimeKind information.
		/// By taking advantage of the fact that a DateTime's TimeOfDay portion will never use the IsNegative
		/// and HasDays flags, we can use these 2 bits to store the DateTimeKind and, since DateTimeKind is
		/// unlikely to be set without a Time, we need no additional bytes to support a .NET 2.0 DateTime.
		/// </summary>
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
					packedData[SequenceWriter.MinutesSection] = seconds;
				}
				else
				{
					packedData[SequenceWriter.SecondsSection] = seconds;
					optionalBytes++;
				}
			}

			if (milliseconds != 0)
			{
				packedData[SequenceWriter.HasMillisecondsSection] = 1;
				packedData[SequenceWriter.MillisecondsSection] = milliseconds;
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

					this.Write(rented, 0, actualByteCount);
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
					this.Write(rented, 0, bytesWritten);

				chars = chars.Slice(charsConsumed);
			}
			while (!completed);

			ArrayPool<byte>.Shared.Return(rented);
		}

		private void CreateBufferArray()
		{
			this.buffers = new byte[DefaultBufferArrayCapacity][];
			this.bufferCounts = new int[DefaultBufferArrayCapacity];
		}

		private void AddNewBuffer(byte[] buffer, int count)
		{
			this.bufferIndex++;

			if (this.bufferIndex == this.buffers!.Length)
				this.ResizeBufferArray();

			this.buffers[this.bufferIndex] = buffer;
			this.bufferCounts![this.bufferIndex] = count;
			this.bufferSize *= 2;
		}

		private Span<byte> GetCurrentSingleByteSpan()
		{
			if (this.IsSingleSegment)
				return this.span.Slice(this.spanCount - 1, 1);
			else
				return new Span<byte>(this.buffers![this.bufferIndex], this.bufferCounts![this.bufferIndex] - 1, 1);
		}

		/// <summary>
		/// Writes a 64-bit unsigned integer in a compressed format.
		/// Writes a 7-bit encoded integer from the stream. This is stored with the least significant information first, 
		/// with 7 bits of information per byte of value, and the top bit as a continuation flag.  
		/// </summary>
		/// <param name="writer">The sequencial writer to write data to</param>
		/// <param name="value">The value to be writen</param>
		public void Write7BitEncodedUInt64(ulong value)
		{
			// Write out an int 7 bits at a time. The high bit of the byte,
			// when on, tells reader to continue reading more bytes.
			//
			// Using the constants 0x7F and ~0x7F below offers smaller
			// codegen than using the constant 0x80.

			while (value > 0x7Fu)
			{
				this.WriteByte((byte)((uint)value | ~0x7Fu));
				value >>= 7;
			}

			this.WriteByte((byte)value);
		}

		#endregion |   Private Methods  |

	}
}
