
using System;
using System.IO;
using System.Net.Sockets;

namespace Thought.Net.Telnet
{

    /// <summary>
    ///     Provides a stream that implements the Telnet protocol.
    /// </summary>
    /// <seealso cref="TelnetClient"/>
    public class TelnetStream : Stream, IDisposable
    {

        /// <summary>
        ///     Privately tracks the read capability of the stream.
        /// </summary>
        private bool canRead;


        /// <summary>
        ///     Privately tracks the write capability of the stream.
        /// </summary>
        private bool canWrite;


        /// <summary>
        ///     The underlying TelnetClient wrapped by the stream.
        /// </summary>
        private TelnetClient client;


        /// <summary>
        ///     Privately tracks the ownership status.
        /// </summary>
        /// <remarks>
        ///     If owned, the client will close (dispose) the socket when
        ///     the class itself is closed.  Otherwise the client will
        ///     simply release its reference to the socket.
        /// </remarks>
        private bool ownsSocket;


        /// <summary>
        ///     The default constructor is marked as private to
        ///     ensure the parameterized constructor is called.
        /// </summary>
        private TelnetStream()
        {
        }


        /// <summary>
        ///     Creates a Telnet stream against the specified socket.
        /// </summary>
        /// <param name="socket">
        ///     The socket that the <see cref="TelnetStream"/> will use to send and receive data.
        /// </param>
        public TelnetStream(Socket socket)
            : this(socket, FileAccess.ReadWrite, false)
        {
        }


        /// <summary>
        ///     Creates a telnet stream against the specified
        ///     socket with designated file access and ownership
        ///     permissions.
        /// </summary>
        /// <param name="socket">
        ///     The socket that the <see cref="TelnetStream"/> will use to send and receive data.
        /// </param>
        /// <param name="access">
        ///     A bitwise combination of the FileAccess values that specifies the
        ///     type of access given to the TelnetStream over the provided Socket.
        /// </param>
        /// <param name="ownsSocket">
        ///     True to indicate that the TelnetStream will take ownership of the
        ///     Socket; otherwise False.
        /// </param>
        public TelnetStream(
          Socket socket,
          FileAccess access,
          bool ownsSocket)
        {

            // Create a telnet client.  The client will
            // validate the socket -- it requires an initialized,
            // stream, blocking socket and will raise if error
            // if any conditions are unmet.

            this.client = new TelnetClient(socket);
            this.canRead = (access & FileAccess.Read) == FileAccess.Read;
            this.canWrite = (access & FileAccess.Write) == FileAccess.Write;
            this.ownsSocket = ownsSocket;

        }


        public TelnetStream(TelnetClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            this.client = client;
            this.canRead = true;
            this.canWrite = true;
            this.ownsSocket = false;
        }


        /// <summary>
        ///     Gets a value indicating whether the TelnetStream supports reading.
        /// </summary>
        /// <value>
        ///     True if the stream supports reading; False if the stream is closed
        ///     or does not support reading.
        /// </value>
        public override bool CanRead
        {
            get
            {
                return this.canRead;
            }
        }


        /// <summary>
        ///     Gets a value indicating whether the TelnetStream supports seeking.
        /// </summary>
        /// <value>
        ///     Always false.
        /// </value>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        ///     Gets a value indicating whether the stream supports writing.
        /// </summary>
        /// <value>
        ///     True if the stream supports reading; false if the stream is closed
        ///     or does not support reading.
        /// </value>
        public override bool CanWrite
        {
            get
            {
                return canWrite;
            }
        }


        /// <summary>
        ///     Closes the Telnet stream and network connection.
        /// </summary>
        public override void Close()
        {

            // Call the IDisposable.Dispose method that
            // is implemented in this class.  It will close
            // down the object and inform the garbage collector
            // that the Finalize method is not needed.

            ((IDisposable)this).Dispose();
        }


        /// <summary>
        ///     Disposes (closes) the telnet stream.
        /// </summary>
        void IDisposable.Dispose()
        {

            // Call the dispose method of this class.
            // The method will close the stream.

            if (this.client != null)
            {

                // Mark the object as unable to read and write.

                this.canRead = false;
                this.canWrite = false;

                if (this.ownsSocket)
                {
                    this.client.Close();
                }

                this.client = null;

            }

            // Classes that implement IDisposable can call
            // the SuppressFinalize method to inform the
            // garbage collector that it should not call the
            // Finalize method.

            GC.SuppressFinalize(this);

        }


        /// <summary>
        ///     Ignored.
        /// </summary>
        public override void Flush()
        {
        }


        /// <summary>
        ///     The stream length (not supported).
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        ///     Indicates whether or not the stream owns the socket.
        /// </summary>
        /// <remarks>
        ///     If the stream owns the socket, then the socket will be closed
        ///     when the stream is closed.  Otherwise the socket will be released
        ///     without disposing.  This is useful when the stream is being used
        ///     temporarily against an existing socket that is needed after the
        ///     stream is finished.
        /// </remarks>
        public bool OwnsSocket
        {
            get
            {
                return this.ownsSocket;
            }
            set
            {
                this.ownsSocket = value;
            }
        }


        /// <summary>
        ///     The stream position (not supported).
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }


        /// <summary>
        ///     Copies bytes from the stream into an array.
        /// </summary>
        /// <param name="buffer">
        ///     The buffer to which bytes are copied.
        /// </param>
        /// <param name="offset">
        ///     The byte offset in the buffer at which to begin copying bytes.
        /// </param>
        /// <param name="count">
        ///     The number of bytes to read.
        /// </param>
        /// <returns>The number of bytes read into the buffer.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {

            if (!this.canRead)
                throw new NotSupportedException();

            return client.Read(buffer, offset, count);
        }


        /// <summary>
        ///     Not supported.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        ///     Set the length of the stream (not supported).
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }


        /// <summary>
        ///     The underlying <see cref="TelnetClient"/> of the stream.
        /// </summary>
        /// <seealso cref="TelnetClient"/>
        public TelnetClient TelnetClient
        {
            get
            {
                return this.client;
            }
        }


        /// <summary>
        ///     Copies bytes to the telnet stream.
        /// </summary>
        /// <param name="buffer">
        ///     The byte array from which to copy bytes to the stream.
        /// </param>
        /// <param name="offset">
        ///     The offset in the buffer at which to begin copying bytes.
        /// </param>
        /// <param name="count">
        ///     The number of bytes to copy to the stream.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count)
        {

            if (!this.canWrite)
                throw new NotSupportedException();

            client.Write(buffer, offset, count);
        }

    }
}
