
using System;

namespace Thought.Net.Telnet
{

    /// <summary>
    ///     The type of data received by a remote system.
    /// </summary>
    /// <remarks>
    ///     The Telnet protocol does not have "packets" in the formal sense.
    ///     The <see cref="TelnetParser"/> class packages the incoming raw bytes
    ///     into their appropriate types (e.g. data, commands, etc).  Each 
    ///     chunk is called a packet by the class library.
    /// </remarks>
    public enum TelnetPacketType
    {

        /// <summary>
        ///     A packet containing data.
        /// </summary>
        Data,


        /// <summary>
        ///     A packet containing an option command.
        /// </summary>
        Command,


        /// <summary>
        ///     A packet containing subnegotiation data.
        /// </summary>
        Subnegotiation

    }
}
