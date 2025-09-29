using System;
using System.IO;

namespace Simple
{
    /// <summary>
    /// Wraps another stream and provides events reporting for when bytes are read or written to the stream.
    /// </summary>
    public class StreamWithEvents : Stream
    {
        #region Private Data Members
        
        private Stream innerStream;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new ProgressStream supplying the stream for it to report on.
        /// </summary>
        /// <param name="streamToReportOn">The underlying stream that will be reported on when bytes are read or written.</param>
        public StreamWithEvents(Stream streamToReportOn)
        {
            if (streamToReportOn != null)
            {
                this.innerStream = streamToReportOn;
            }
            else
            {
                throw new ArgumentNullException("streamToReportOn");
            }
        }
        
        #endregion

        #region Events
        
        /// <summary>
        /// Raised when bytes are read from the stream.
        /// </summary>
        public event StreamActionDelegate BytesRead;

        /// <summary>
        /// Raised when bytes are written to the stream.
        /// </summary>
        public event StreamActionDelegate BytesWritten;

        /// <summary>
        /// Raised when bytes are either read or written to the stream.
        /// </summary>
        public event TransferStreamActionDelegate BytesMoved;

        #endregion

        #region Stream Members

        public override bool CanRead
        {
            get { return this.innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return this.innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return this.innerStream.CanWrite; }
        }

        public override void Flush()
        {
            this.innerStream.Flush();
        }

        public override long Length
        {
            get { return innerStream.Length; }
        }

        public override long Position
        {
            get { return this.innerStream.Position; }
            set { this.innerStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = this.innerStream.Read(buffer, offset, count);

            this.OnBytesRead(buffer, bytesRead);
            this.OnBytesMoved(buffer, bytesRead, isRead: true, isWrite: false);

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.innerStream.Write(buffer, offset, count);

            this.OnBytesWritten(buffer, count);
            this.OnBytesMoved(buffer, count, isRead: false, isWrite: true);
        }

        public override void Close()
        {
            this.innerStream.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            this.innerStream.Dispose();
            base.Dispose(disposing);
        }
        
        #endregion
        
        #region Protected Members

        protected virtual void OnBytesRead(byte[] data, int bytesMoved)
        {
            if (this.BytesRead != null)
            {
                this.BytesRead(this, new StreamActionEventArgs(data, bytesMoved, this.innerStream.Length, this.innerStream.Position));
            }
        }

        protected virtual void OnBytesWritten(byte[] data, int bytesMoved)
        {
            if (this.BytesWritten != null)
            {
                this.BytesWritten(this, new StreamActionEventArgs(data, bytesMoved, this.innerStream.Length, this.innerStream.Position));
            }
        }

        protected virtual void OnBytesMoved(byte[] data, int bytesMoved, bool isRead, bool isWrite)
        {
            if (this.BytesMoved != null)
            {
                this.BytesMoved(this, new TransferStreamActionEventArgs(data, bytesMoved, this.innerStream.Length, this.innerStream.Position, isRead, isWrite));
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Contains the pertinent data for a Bytes Read/Written event.
    /// </summary>
    public class StreamActionEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">The data buffer read/written to/from the stream.</param>
        /// <param name="bytesMoved">The number of bytes that were read/written to/from the stream.</param>
        /// <param name="streamLength">The total length of the stream in bytes.</param>
        /// <param name="streamPosition">The current position in the stream.</param>
        public StreamActionEventArgs(byte[] data, int bytesMoved, long streamLength, long streamPosition)
        {
            this.Data = data;
            this.BytesMoved = bytesMoved;
            this.StreamLength = streamLength;
            this.StreamPosition = streamPosition;
        }
        
        /// <summary>
        /// The data read or written to/from the stream.
        /// </summary>
        public byte[] Data { get; private set; }
        
        /// <summary>
        /// The number of bytes that were read/written to/from the stream.
        /// </summary>
        public int BytesMoved { get; private set; }

        /// <summary>
        /// The total length of the stream in bytes.
        /// </summary>
        public long StreamLength { get; private set; }

        /// <summary>
        /// The current position in the stream.
        /// </summary>
        public long StreamPosition { get; private set; }
    }

    /// <summary>
    /// Contains the pertinent data for a ProgressStream Report event.
    /// </summary>
    public class TransferStreamActionEventArgs : StreamActionEventArgs
    {
        /// <summary>
        /// True if the bytes were read from the stream, false if they were written.
        /// </summary>
        public bool WasRead { get; private set; }

        /// <summary>
        /// True if the bytes were written from the stream, false if they were read.
        /// </summary>
        public bool WasWrite { get; private set; }

        /// <summary>
        /// Creates a new ProgressStreamReportEventArgs initializing its members.
        /// </summary>
        /// <param name="data">The data buffer read/written to/from the stream.</param>
        /// <param name="bytesMoved">The number of bytes that were read/written to/from the stream.</param>
        /// <param name="streamLength">The total length of the stream in bytes.</param>
        /// <param name="streamPosition">The current position in the stream.</param>
        /// <param name="wasRead">True if the bytes were read from the stream, false if they were written.</param>
        /// <param name="wasWrite">True if the bytes were written from the stream, false if they were read.</param>
        public TransferStreamActionEventArgs(byte[] data, int bytesMoved, long streamLength, long streamPosition, bool wasRead, bool wasWrite)
            : base(data, bytesMoved, streamLength, streamPosition)
        {
            this.WasRead = wasRead;
            this.WasWrite = wasWrite;
        }
    }

    /// <summary>
    /// The delegate for handling a ProgressStreamAction event.
    /// </summary>
    /// <param name="sender">The object that raised the event, should be a StreamWithEvents.</param>
    /// <param name="args">The arguments raised with the event.</param>
    public delegate void StreamActionDelegate(object sender, StreamActionEventArgs e);

    /// <summary>
    /// The delegate for handling a ProgressStreamAction event.
    /// </summary>
    /// <param name="sender">The object that raised the event, should be a StreamWithEvents.</param>
    /// <param name="args">The arguments raised with the event.</param>
    public delegate void TransferStreamActionDelegate(object sender, TransferStreamActionEventArgs e);

}