using System;
using System.Security;

namespace Simple.Serialization
{
    /// <summary>
    /// Represents a <see cref="ByteConverter"/> which handles little endianness.
    /// </summary>
    [SecuritySafeCritical]
    public sealed class ByteConverterLittle : ByteConverter
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        public override Endian Endian => Endian.Little;

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Double value, Span<byte> buffer)
        {
            UInt64 raw = *(UInt64*)&value;

            buffer[0] = (byte)raw;
            buffer[1] = (byte)(raw >> 8);
            buffer[2] = (byte)(raw >> 16);
            buffer[3] = (byte)(raw >> 24);
            buffer[4] = (byte)(raw >> 32);
            buffer[5] = (byte)(raw >> 40);
            buffer[6] = (byte)(raw >> 48);
            buffer[7] = (byte)(raw >> 56);
        }

        /// <inheritdoc/>
        public override void GetBytes(Int16 value, Span<byte> buffer)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
        }

        /// <inheritdoc/>
        public override void GetBytes(Int32 value, Span<byte> buffer)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
        }

        /// <inheritdoc/>
        public override void GetBytes(Int64 value, Span<byte> buffer)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);
        }

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Single value, Span<byte> buffer)
        {
            UInt32 raw = *(UInt32*)&value;
            buffer[0] = (byte)raw;
            buffer[1] = (byte)(raw >> 8);
            buffer[2] = (byte)(raw >> 16);
            buffer[3] = (byte)(raw >> 24);
        }

        /// <inheritdoc/>
        public override void GetBytes(UInt16 value, Span<byte> buffer)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
        }

        /// <inheritdoc/>
        public override void GetBytes(UInt32 value, Span<byte> buffer)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
        }

        /// <inheritdoc/>
        public override void GetBytes(UInt64 value, Span<byte> buffer)
        {
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);
        }

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe Double ToDouble(ReadOnlySpan<byte> buffer)
        {
            Int64 raw = buffer[0]
                | (long)buffer[1] << 8
                | (long)buffer[2] << 16
                | (long)buffer[3] << 24
                | (long)buffer[4] << 32
                | (long)buffer[5] << 40
                | (long)buffer[6] << 48
                | (long)buffer[7] << 56;
            return *(Double*)&raw;
        }

        /// <inheritdoc/>
        public override Int16 ToInt16(ReadOnlySpan<byte> buffer)
        {
            return (Int16)(buffer[0]
                | buffer[1] << 8);
        }

        /// <inheritdoc/>
        public override Int32 ToInt32(ReadOnlySpan<byte> buffer)
        {
            return buffer[0]
                | buffer[1] << 8
                | buffer[2] << 16
                | buffer[3] << 24;
        }

        /// <inheritdoc/>
        public override Int64 ToInt64(ReadOnlySpan<byte> buffer)
        {
            return buffer[0]
                | (long)buffer[1] << 8
                | (long)buffer[2] << 16
                | (long)buffer[3] << 24
                | (long)buffer[4] << 32
                | (long)buffer[5] << 40
                | (long)buffer[6] << 48
                | (long)buffer[7] << 56;
        }

        /// <inheritdoc/>
        [SecuritySafeCritical]
        public override unsafe Single ToSingle(ReadOnlySpan<byte> buffer)
        {
            Int32 raw = buffer[0]
                | buffer[1] << 8
                | buffer[2] << 16
                | buffer[3] << 24;
            return *(Single*)&raw;
        }

        /// <inheritdoc/>
        public override UInt16 ToUInt16(ReadOnlySpan<byte> buffer)
        {
            return (UInt16)(buffer[0]
                | buffer[1] << 8);
        }

        /// <inheritdoc/>
        public override UInt32 ToUInt32(ReadOnlySpan<byte> buffer)
        {
            return (UInt32)(buffer[0]
                | buffer[1] << 8
                | buffer[2] << 16
                | buffer[3] << 24);
        }

        /// <inheritdoc/>
        public override UInt64 ToUInt64(ReadOnlySpan<byte> buffer)
        {
            return buffer[0]
                | (ulong)buffer[1] << 8
                | (ulong)buffer[2] << 16
                | (ulong)buffer[3] << 24
                | (ulong)buffer[4] << 32
                | (ulong)buffer[5] << 40
                | (ulong)buffer[6] << 48
                | (ulong)buffer[7] << 56;
        }
    }
}
