
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Thought.Net.Telnet
{

    /// <summary>
    ///     Parses incoming data from a Telnet connection.
    /// </summary>
    public class TelnetParser
    {

        /// <summary>
        ///     Marks the beginning of any telnet command or option.
        /// </summary>
        /// <remarks>
        ///     The IAC character marks the beginning of any telnet command
        ///     or option.  It is ASCII #255.  When this character is part of
        ///     regular data (that is, not intended as an IAC character), it
        ///     must be doubled (sent as two 0xFF characters).
        /// </remarks>
        public const byte IAC = 255;

        /// <summary>
        ///     The socket to use for communications.
        /// </summary>
        private Socket socket;

        /// <summary>
        ///     The state of the protocol parser.
        /// </summary>
        private TelnetParserState state;

        /// <summary>
        ///     Byte that have been received but are still unprocessed.
        /// </summary>
        private byte[] unprocessed;

        /// <summary>
        ///     The number of unprocessed bytes (the array might be larger).
        /// </summary>
        private int unprocessedCount;

        /// <summary>
        ///     The start of valid bytes in the unprocessed byte array.
        /// </summary>
        private int unprocessedIndex;



        public TelnetParser(Socket socket)
        {

            if (socket == null)
                throw new ArgumentNullException("socket");

            // This class requires the socket to operate in
            // blocking mode (calls do not return until data
            // is available).

            if (!socket.Blocking)
                throw new IOException();

            // Obviously the socket must be connected, for
            // otherwise nothing can happen!

            if (!socket.Connected)
                throw new IOException();

            // A stream-type socket is required (e.g., not
            // a UDP or other type of socket).

            if (socket.SocketType != SocketType.Stream)
                throw new IOException();

            this.socket = socket;
            this.unprocessed = new byte[1024];
        
        }


        public void Close()
        {
            socket.Close();
        }


        /// <summary>
        ///     Reads the next <see cref="TelnetPacket"/> from the socket.
        /// </summary>
        /// <remarks>
        ///     The next packet from the remote system.  If the packet regular
        ///     or subnegotiation data, then the function will attempt to 
        ///     load as much data as possible.
        /// </remarks>
        public TelnetPacket ReadNext()
        {
            return ReadNext(null);
        }


        /// <summary>
        ///     Reads the next <see cref="TelnetPacket"/> while limiting
        ///     the number of bytes returned for a data or subnegotiation
        ///     packet.
        /// </summary>
        /// <param name="maximumDataBytes">
        ///     The maximum number of bytes to return in a data or
        ///     subnegotiation packet.  Ignored for other packet types.
        /// </param>
        /// <returns>
        ///     The next <see cref="TelnetPacket"/> from the remote system.
        /// </returns>
        public TelnetPacket ReadNext(int? maximumDataBytes)
        {

            // The general algorithm (below) is to examine each
            // incoming character until a valid packet is formed.
            // For efficiency, a chunk of incoming data is loaded
            // as needed into an array (this.unprocessed).  For
            // regular data and subnegotiation data, the examined
            // bytes are put into buffer until the full packet
            // is ready to be returned.
 
            ByteBuffer packetData = new ByteBuffer();

            // The loop below will continue executing until
            // the packet field is initialized.

            TelnetPacket packet = null;

            do
            {

                // For efficiency, the code loads chunks of data
                // from the underlying socket.  The unprocessedIndex
                // field tracks the location of the next
                // byte to examine; the unprocessedCount field 
                // tracks the number of valid bytes in the array.
                // The array is never resized; it is initialized by
                // the constructor.

                if (this.unprocessedIndex >= this.unprocessedCount)
                {

                    // There are no unprocessed bytes that can be scanned
                    // by the parser.  Ask the socket for another chunk.

                    this.unprocessedCount = this.socket.Receive(
                        this.unprocessed,
                        this.unprocessed.Length,
                        SocketFlags.None);

                    this.unprocessedIndex = 0;

                }

                // Grab the next character from the array of
                // unprocessed characters, then increment the index.

                byte currentByte =
                    this.unprocessed[this.unprocessedIndex++];

                switch (this.state)
                {

                    case TelnetParserState.Data:

                        #region [ Data State ]

                        // The parser is waiting for data or has been reading
                        // data.  The next step is to look for several
                        // special characters (such as the prefix for a 
                        // telnet command).  These characters require special
                        // processing; other characters are data bytes.

                        switch (currentByte)
                        {

                            case AsciiByte.CR:

                                // The current character is a carriage-return.
                                // If this CR is not followed by a line feed,
                                // then the Telnet protocol requires it to be
                                // followed by a NUL (ASCII 0) character, which
                                // is stripped upon being received.  This
                                // mechanism simplifies newline handling for
                                // some types of terminals.

                                packetData.Append(AsciiByte.CR);
                                this.state = TelnetParserState.ReceivedCarriageReturn;
                                break;

                            case (byte)TelnetCommand.Iac:

                                // An IAC character marks the beginning of
                                // a Telnet command.  The next character will
                                // identify the command, or be another IAC
                                // character, which is an escape code to insert
                                // a single IAC into the data.

                                this.state = TelnetParserState.ReceivedIAC;
                                break;

                            default:

                                // The current character is not a carriage
                                // return nor an IAC character.  Therefore
                                // it is a data byte.

                                packetData.Append(currentByte);
                                break;
                        }
                        break;

                        #endregion

                    case TelnetParserState.ReceivedCarriageReturn:

                        #region [ Received Carriage Return State ]

                        // The parser previously loaded a carriage-return
                        // and placed it in the pending buffer.  The
                        // next character is stripped if it is a NUL
                        // character.

                        if (currentByte != AsciiByte.NUL)
                            packetData.Append(currentByte);
                        this.state = TelnetParserState.Data;
                        break;

                        #endregion

                    case TelnetParserState.ReceivedIAC:

                        #region [ Received IAC State ]

                        // The parser previously received an IAC character.
                        // An IAC character marks the start of a Telnet
                        // command.

                        if (currentByte == (byte)TelnetCommand.Iac)
                        {

                            // A double IAC character is an escape sequence
                            // to allow an IAC character to exist in the
                            // data.  Append an IAC to the pending buffer.

                            packetData.Append((byte)TelnetCommand.Iac);
                            this.state = TelnetParserState.Data;

                        }
                        else
                        {

                            // The current character identifies the command type.
                            // If any pending data exist, then package it up.

                            if (packetData.Count > 0)
                                packet = new TelnetPacket(packetData.ToArray());

                            switch ((TelnetCommand)currentByte)
                            {

                                case TelnetCommand.AbortOutput:
                                case TelnetCommand.AreYouThere:
                                case TelnetCommand.Break:
                                case TelnetCommand.DataMark:
                                case TelnetCommand.EraseCharacter:
                                case TelnetCommand.EraseLine:
                                case TelnetCommand.GoAhead:
                                case TelnetCommand.InterruptProcess:
                                case TelnetCommand.NoOperation:

                                    // These are all simple commands consisting
                                    // of an IAC character followed by a single
                                    // byte (e.g. the go-ahead command is #249).

                                    packet = new TelnetPacket((TelnetCommand)currentByte);
                                    this.state = TelnetParserState.Data;
                                    break;

                                case TelnetCommand.Do:
                                    this.state = TelnetParserState.ReceivedDo;
                                    break;

                                case TelnetCommand.Dont:
                                    this.state = TelnetParserState.ReceivedDont;
                                    break;

                                case TelnetCommand.Will:
                                    this.state = TelnetParserState.ReceivedWill;
                                    break;

                                case TelnetCommand.Wont:
                                    this.state = TelnetParserState.ReceivedWont;
                                    break;

                                default:
                                    throw new NotSupportedException();

                            }
                        }

                        break;

                        #endregion

                    case TelnetParserState.ReceivedDo:
                        packet = new TelnetPacket(TelnetCommand.Do, currentByte);
                        this.state = TelnetParserState.Data;
                        break;

                    case TelnetParserState.ReceivedDont:
                        packet = new TelnetPacket(TelnetCommand.Dont, currentByte);
                        this.state = TelnetParserState.Data;
                        break;

                    case TelnetParserState.ReceivedWill:
                        packet = new TelnetPacket(TelnetCommand.Will, currentByte);
                        this.state = TelnetParserState.Data;
                        break;

                    case TelnetParserState.ReceivedWont:
                        packet = new TelnetPacket(TelnetCommand.Wont, currentByte);
                        this.state = TelnetParserState.Data;
                        break;

                }


                // The current character has been processed.  The next step
                // is to determine if the loop should stop collecting bytes
                // for data packets and subnegotiation packets.

                if ((packet == null) && (packetData.Count > 0))
                {

                    // The packet should be finalized if a maximum count
                    // was specified AND the pending data has reached
                    // this count.  The packet is returned even if
                    // more data is available in the unprocessed array
                    // or the socket.

                    bool returnNow =
                        (maximumDataBytes != null) &&
                        (packetData.Count == maximumDataBytes);

                    // Also, the packet should be finalized if the
                    // entire unprocessed array has been scanned and
                    // it appears that no more data is available from
                    // the socket.

                    if (!returnNow)
                        returnNow =
                            (this.unprocessedIndex == this.unprocessedCount) &&
                            (this.socket.Available == 0);

                    if(returnNow)
                    {
                        return new TelnetPacket(packetData.ToArray());
                    }

                }

            } while (packet == null);

            return packet;

        }


        /// <summary>
        ///     Sends a Telnet command that requires no additional parameters.
        /// </summary>
        /// <param name="command">
        ///     The Telnet command to send to the remote system.
        /// </param>
        public void SendCommand(TelnetCommand command)
        {

            switch (command)
            {
                case TelnetCommand.AbortOutput:
                case TelnetCommand.AreYouThere:
                case TelnetCommand.Break:
                case TelnetCommand.DataMark:
                case TelnetCommand.EraseCharacter:
                case TelnetCommand.EraseLine:
                case TelnetCommand.GoAhead:
                case TelnetCommand.Iac:
                case TelnetCommand.InterruptProcess:
                case TelnetCommand.NoOperation:
                
                    // These are all simple Telnet commands that do not
                    // require any additional parameters.

                    SendRaw(new byte[] { IAC, (byte)command });
                    break;

                case TelnetCommand.Do:
                case TelnetCommand.Dont:
                case TelnetCommand.Will:
                case TelnetCommand.Wont:

                    // These commands require an option byte.

                    throw new InvalidOperationException();

                default:
                    throw new NotSupportedException();

            }

        }


        /// <summary>
        ///     Sends a Telnet command that requires an option parameter.
        /// </summary>
        public void SendCommand(TelnetCommand command, TelnetOption option)
        {

            switch (command)
            {

                case TelnetCommand.Do:
                case TelnetCommand.Dont:
                case TelnetCommand.Will:
                case TelnetCommand.Wont:

                    SendRaw(new byte[] { IAC, (byte)command, (byte)option });
                    break;

                default:

                    throw new NotSupportedException();

            }

        }


        /// <summary>
        ///     Sends an IAC DO command.
        /// </summary>
        public void SendDo(byte option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Do, option });
        }


        /// <summary>
        ///     Sends an IAC DO command.
        /// </summary>
        public void SendDo(TelnetOption option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Do, (byte)option });
        }


        /// <summary>
        ///     Sends an IAC DONT dommand.
        /// </summary>
        public void SendDont(byte option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Dont, option });
        }


        /// <summary>
        ///     Sends an IAC DONT command.
        /// </summary>
        public void SendDont(TelnetOption option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Dont, (byte)option });
        }


        /// <summary>
        ///     Sends a byte array after processing carriage-returns
        ///     and IAC characters.
        /// </summary>
        /// <param name="buffer">
        ///     A byte array containing the data to send to the remote system.
        /// </param>
        public void SendEscaped(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            SendEscaped(buffer, 0, buffer.Length);
        }


        /// <summary>
        ///     Sends a portion of a byte array after processing
        ///     carriage-returns and IAC characters.
        /// </summary>
        /// <param name="buffer">
        ///     A byte array containing data to send to the remote system.
        /// </param>
        /// <param name="offset">
        ///     The offset at which to begin copying bytes.
        /// </param>
        /// <param name="count">
        ///     The number of bytes to copy from the array.
        /// </param>
        public void SendEscaped(byte[] buffer, int offset, int count)
        {
            ByteBuffer escaped = new ByteBuffer(buffer, offset, count);
            escaped.Double(IAC);
            SendRaw(escaped.ToArray());
        }


        /// <summary>
        ///     Sends an IAC GA (go ahead) command.
        /// </summary>
        public void SendGoAhead()
        {
            SendCommand(TelnetCommand.GoAhead);
        }


        /// <summary>
        ///     Sends raw data to the remote system without processing.
        /// </summary>
        public void SendRaw(byte[] buffer)
        {

            if (buffer == null)
                throw new ArgumentNullException("data");

            this.socket.Send(buffer);

        }


        /// <summary>
        ///     Sends raw data to the remote system without processing.
        /// </summary>
        public void SendRaw(byte[] buffer, int offset, int size)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            this.socket.Send(buffer, offset, size, SocketFlags.None);

        }


        /// <summary>
        ///     Sends an IAC WILL option.
        /// </summary>
        public void SendWill(byte option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Will, option });
        }


        /// <summary>
        ///     Sends an IAC WILL option.
        /// </summary>
        /// <param name="option"></param>
        public void SendWill(TelnetOption option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Will, (byte)option });
        }


        /// <summary>
        ///     Sends an IAC WONT option.
        /// </summary>
        public void SendWont(byte option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Wont, option });
        }


        /// <summary>
        ///     Sends an IAC WONT sequence.
        /// </summary>
        public void SendWont(TelnetOption option)
        {
            SendRaw(new byte[] { IAC, (byte)TelnetCommand.Wont, (byte)option });
        }


        /// <summary>
        ///     The underlying socket of the Telnet connection.
        /// </summary>
        public Socket Socket
        {
            get
            {
                return this.socket;
            }
        }


        /// <summary>
        ///     The state of the incoming data parser.
        /// </summary>
        public TelnetParserState State
        {
            get
            {
                return this.state;
            }
        }

    }
}
