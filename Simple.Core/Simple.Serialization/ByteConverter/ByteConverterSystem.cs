using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Simple.Serialization
{
    /// <summary>
    /// Represents a <see cref="ByteConverter"/> which handles system endianness.
    /// </summary>
    [SecuritySafeCritical]
    public sealed class ByteConverterSystem : ByteConverter
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        public override Endian Endian => EndianTools.SystemEndian;

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Double value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(Double)).CopyTo(buffer);

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Int16 value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(Int16)).CopyTo(buffer);

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Int32 value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(Int32)).CopyTo(buffer);

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Int64 value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(Int64)).CopyTo(buffer);

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Single value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(Single)).CopyTo(buffer);

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(UInt16 value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(UInt16)).CopyTo(buffer);

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(UInt32 value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(UInt32)).CopyTo(buffer);

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(UInt64 value, Span<byte> buffer)
            => new Span<byte>(&value, sizeof(UInt64)).CopyTo(buffer);

        /// <inheritdoc/>
        public override Double ToDouble(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, Double>(buffer)[0];

        /// <inheritdoc/>
        public override Int16 ToInt16(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, Int16>(buffer)[0];

        /// <inheritdoc/>
        public override Int32 ToInt32(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, Int32>(buffer)[0];

        /// <inheritdoc/>
        public override Int64 ToInt64(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, Int64>(buffer)[0];

        /// <inheritdoc/>
        public override Single ToSingle(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, Single>(buffer)[0];

        /// <inheritdoc/>
        public override UInt16 ToUInt16(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, UInt16>(buffer)[0];

        /// <inheritdoc/>
        public override UInt32 ToUInt32(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, UInt32>(buffer)[0];

        /// <inheritdoc/>
        public override UInt64 ToUInt64(ReadOnlySpan<byte> buffer) => MemoryMarshal.Cast<byte, UInt64>(buffer)[0];
    }
}
