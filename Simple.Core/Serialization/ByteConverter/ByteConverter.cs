using System;

namespace Simple.Serialization
{
    /// <summary>
    /// Represents a <see cref="BitConverter"/> which handles a specific endianness.
    /// </summary>
    public abstract class ByteConverter
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The exception thrown if a conversion buffer is too small or <see langword="null"/>.
        /// </summary>
        protected static readonly Exception BufferException = new Exception("Buffer null or too small.");

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ByteConverter"/> class.
        /// </summary>
        static ByteConverter()
        {
            if (BitConverter.IsLittleEndian)
            {
                Little = new ByteConverterSystem();
                Big = new ByteConverterBig();
                System = Little;
            }
            else
            {
                Little = new ByteConverterLittle();
                Big = new ByteConverterSystem();
                System = Big;
            }
        }

        private protected ByteConverter() { }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a <see cref="ByteConverter"/> instance converting data stored in little endian byte order.
        /// </summary>
        public static ByteConverter Little { get; }

        /// <summary>
        /// Gets a <see cref="ByteConverter"/> instance converting data stored in big endian byte order.
        /// </summary>
        public static ByteConverter Big { get; }

        /// <summary>
        /// Gets a <see cref="ByteConverter"/> instance converting data stored in the byte order of the system
        /// executing the assembly.
        /// </summary>
        public static ByteConverter System { get; }

        /// <summary>
        /// Gets the <see cref="Core.Endian"/> in which data is stored as converted by this instance.
        /// </summary>
        public abstract Endian Endian { get; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Returns a <see cref="ByteConverter"/> for the given <paramref name="byteOrder"/>.
        /// </summary>
        /// <param name="byteOrder">The <see cref="Endian"/> to retrieve a converter for.</param>
        /// <returns>The corresponding <see cref="ByteConverter"/> instance.</returns>
        public static ByteConverter GetConverter(Endian byteOrder) => byteOrder switch
        {
            Endian.Big => Big,
            Endian.Little => Little,
            Endian.System => System,
            _ => throw new ArgumentException($"Invalid {nameof(Endian)}.", nameof(byteOrder)),
        };

        /// <summary>
        /// Stores the specified <see cref="Decimal"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public static void GetBytes(Decimal value, Span<byte> buffer)
        {
            if (buffer.Length < sizeof(Decimal))
                throw BufferException;

            // Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
            int[] parts = Decimal.GetBits(value);
            for (int i = 0; i < 4; i++)
            {
                int offset = i * sizeof(int);
                int part = parts[i];
                buffer[offset] = (byte)part;
                buffer[offset + 1] = (byte)(part >> 8);
                buffer[offset + 2] = (byte)(part >> 16);
                buffer[offset + 3] = (byte)(part >> 24);
            }
        }

        /// <summary>
        /// Stores the specified <see cref="Double"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(Double value, Span<byte> buffer);

        /// <summary>
        /// Stores the specified <see cref="Int16"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(Int16 value, Span<byte> buffer);

        /// <summary>
        /// Stores the specified <see cref="Int32"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(Int32 value, Span<byte> buffer);

        /// <summary>
        /// Stores the specified <see cref="Int64"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(Int64 value, Span<byte> buffer);

        /// <summary>
        /// Stores the specified <see cref="Single"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(Single value, Span<byte> buffer);

        /// <summary>
        /// Stores the specified <see cref="UInt16"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(UInt16 value, Span<byte> buffer);

        /// <summary>
        /// Stores the specified <see cref="UInt32"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(UInt32 value, Span<byte> buffer);

        /// <summary>
        /// Stores the specified <see cref="UInt64"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        public abstract void GetBytes(UInt64 value, Span<byte> buffer);

        /// <summary>
        /// Returns an <see cref="Decimal"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public static Decimal ToDecimal(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length < sizeof(Decimal))
                throw BufferException;

            // Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
            int[] parts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                int offset = i * sizeof(int);
                parts[i] = buffer[offset]
                    | buffer[offset + 1] << 8
                    | buffer[offset + 2] << 16
                    | buffer[offset + 3] << 24;
            }
            return new Decimal(parts);
        }

        /// <summary>
        /// Returns an <see cref="Double"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract Double ToDouble(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Returns an <see cref="Int16"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract Int16 ToInt16(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Returns an <see cref="Int32"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract Int32 ToInt32(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Returns an <see cref="Int64"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract Int64 ToInt64(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Returns an <see cref="Single"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract Single ToSingle(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Returns a <see cref="UInt16"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract UInt16 ToUInt16(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Returns a <see cref="UInt32"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract UInt32 ToUInt32(ReadOnlySpan<byte> buffer);

        /// <summary>
        /// Returns a <see cref="UInt64"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <returns>The converted value.</returns>
        public abstract UInt64 ToUInt64(ReadOnlySpan<byte> buffer);
    }
}
