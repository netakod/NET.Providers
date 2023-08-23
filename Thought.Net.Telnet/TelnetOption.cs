
using System;

namespace Thought.Net.Telnet
{

    /// <summary>
    ///     Common telnet options and their corresponding byte values.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Options defined here do not imply support in the Telnet
    ///         class library.  They are defined for reference purposes
    ///         only by developers implementing the option in a Telnet
    ///         client.
    ///     </para>
    ///     <para>
    ///         Member names generally correspond to the command names
    ///         defined in RFC document.  Names are converted to Pascal
    ///         capitalization with dashes stripped.  Some abbreviations
    ///         are expanded.  For example, the TRANSMIT-BINARY telnet
    ///         option is listed as TransmitBinary.
    ///     </para>
    ///     <para>
    ///         This enumeration will be updated as new Telnet-related
    ///         RFC documents are published on the Internet.  Updates
    ///         may be sent to dave@thoughtproject.com.
    ///     </para>
    /// </remarks>
    public enum TelnetOption : byte 
    {

        /// <summary>
        ///     Binary Transmission (RFC 856)
        /// </summary>
        TransmitBinary = 0,


        /// <summary>
        ///     Echo (RFC 857)
        /// </summary>
        Echo = 1,


        /// <summary>
        ///     Reconnection (NIC 50005)
        /// </summary>
        Reconnection = 2,


        /// <summary>
        ///     Suppress Go Ahead (RFC 858)
        /// </summary>
        SuppressGoAhead = 3,


        /// <summary>
        ///     Approximate Message Size Negotiation
        /// </summary>
        ApproximateMessageSizeNegotiation = 4,


        /// <summary>
        ///     Status (RFC 859)
        /// </summary>
        Status = 5,


        /// <summary>
        ///     Timing Mark (RFC 860)
        /// </summary>
        TimingMark = 6,


        /// <summary>
        ///     Remote Controlled Transmission and Echoing (RFC 726)
        /// </summary>
        Rcte = 7,


        /// <summary>
        ///     Output Carriage-Return Disposition (RFC 652)
        /// </summary>
        OutputCarriageReturnDisposition = 10,


        /// <summary>
        ///     Output Horizontal Tab Stops (RFC 653)
        /// </summary>
        OutputHorizontalTabStops = 11,


        /// <summary>
        ///     Output Horizontal Tab Disposition (RFC 654)
        /// </summary>
        OutputHorizontalTabDisposition = 12,


        /// <summary>
        ///     Output formfeed disposition (RFC 655)
        /// </summary>
        OutputFormfeedDisposition = 13,


        /// <summary>
        ///     Output Vertical Tab Stops (RFC 656)
        /// </summary>
        OutputVerticalTabStops = 14,


        /// <summary>
        ///     Output Vertical Tab Disposition (RFC 657)
        /// </summary>
        OutputVerticalTabDisposition = 15,


        /// <summary>
        ///     Output Linefeed Disposition (RFC 658)
        /// </summary>
        OutputLinefeedDisposition = 16,


        /// <summary>
        ///     Extended ASCII (RFC 698)
        /// </summary>
        ExtendedAscii = 17,


        /// <summary>
        ///     Logout (RFC 727)
        /// </summary>
        Logout = 18,


        /// <summary>
        ///     Byte Macro (RFC 735)
        /// </summary>
        ByteMacro = 19,


        /// <summary>
        ///     Data Entry Terminal (RRC 1043)
        /// </summary>
        DataEntryTerminal = 20,


        /// <summary>
        ///     Send Location (RFC 779)
        /// </summary>
        SendLocation = 23,


        /// <summary>
        ///     Terminal type (RFC 1091)
        /// </summary>
        TerminalType = 24,


        /// <summary>
        ///   End of Record (RFC 885)
        /// </summary>
        EndOfRecord = 25,


        /// <summary>
        ///     TACAS User Identification (RFC 927)
        /// </summary>
        TacacsUserId = 26,


        /// <summary>
        ///     Output Marking (RFC 933)
        /// </summary>
        OutputMarking = 27,


        /// <summary>
        ///     Terminal Location (RFC 946)
        /// </summary>
        TerminalLocation = 28,


        /// <summary>
        ///     IBM 3270 Regime (RFC 1041)
        /// </summary>
        Ibm3270Regime = 29,


        /// <summary>
        ///     X.3-PAD (RFC 1053)
        /// </summary>
        X3Pad = 30,


        /// <summary>
        ///     NAWS (Negotiate About Window Size), RFC 1073.
        /// </summary>
        WindowSize = 31,


        /// <summary>
        ///     Terminal Speed (RFC 1079)
        /// </summary>
        TerminalSpeed = 32,


        /// <summary>
        ///     Toggle Flow Control (RFC 1372)
        /// </summary>
        ToggleFlowControl = 33,


        /// <summary>
        ///     Line Mode (RFC 1184)
        /// </summary>
        LineMode = 34,


        /// <summary>
        ///     X Display Location (RFC 1096)
        /// </summary>
        XDisplayLocation = 35,


        /// <summary>
        ///     Environment (RFC 1408)
        /// </summary>
        Environment = 36,


        /// <summary>
        ///     Authentication (RFC 2941)
        /// </summary>
        Authentication = 37,


        /// <summary>
        ///     Encrypt (RFC 2946)
        /// </summary>
        Encrypt = 38,

        /// <summary>
        ///     New Environment (RFC 1572)
        /// </summary>
        NewEnvironment = 39,


        /// <summary>
        ///     TN3270 Enhancements (RFC 2355)
        /// </summary>
        TN3270E = 40,


        /// <summary>
        ///     Character Set (RFC 2066)
        /// </summary>
        CharacterSet = 42,


        /// <summary>
        ///     Communications Port (RFC 2217)
        /// </summary>
        ComPort = 44,


        /// <summary>
        ///     Kermit (RFC 2840)
        /// </summary>
        Kermit = 47

        /*
        OutputLineWidth = 8,
        OutputPageSize = 9,

        */

    }

}
