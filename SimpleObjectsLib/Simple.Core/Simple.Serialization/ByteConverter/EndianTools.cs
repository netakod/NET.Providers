using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Serialization
{
    public static class EndianTools
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>The <see cref="Endian"/> representing system endianness.</summary>
        public static readonly Endian SystemEndian = BitConverter.IsLittleEndian ? Endian.Little : Endian.Big;
        /// <summary>The <see cref="Endian"/> not representing system endianness.</summary>
        public static readonly Endian NonSystemEndian = BitConverter.IsLittleEndian ? Endian.Big : Endian.Little;
    }
}
