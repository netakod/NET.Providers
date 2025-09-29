﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    public enum Endian : ushort
    {
        /// <summary>Indicates that the endianness will not be changed for this operation.</summary>
        None,
        /// <summary>Indicates the byte order of the system executing the assembly.</summary>
        System = 1,
        /// <summary>Indicates big endian byte order.</summary>
        Big = 0xFEFF,
        /// <summary>Indicates little endian byte order.</summary>
        Little = 0xFFFE
    }

    /// <summary>
    /// Represents extension methods for <see cref="Endian"/> instances.
    /// </summary>
    public static class EndianExtensions
    {
        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a value indicating whether the <see cref="Endian"/> equals the system's native endianness.
        /// </summary>
        /// <param name="self">The extended <see cref="Endian"/> instance.</param>
        /// <returns><see langword="true"/> if this is the system endianness; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsSystem(this Endian self)
        {
            return self != Endian.Big && BitConverter.IsLittleEndian
                || self != Endian.Little && !BitConverter.IsLittleEndian;
        }
    }
}
