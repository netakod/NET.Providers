using System.IO;

namespace Simple.Compression
{
    /// <summary>
    /// Implements Zlib compression algorithm.
    /// </summary>
    public class ZlibStream
    {
        private readonly Ionic.Zlib.ZlibStream baseStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="ZlibStream" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mode">The mode.</param>
        public ZlibStream(Stream stream, Ionic.Zlib.CompressionMode mode)
        {
            switch (mode)
            {
                case Ionic.Zlib.CompressionMode.Compress:
                    this.baseStream = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.Default);
                    break;
                case Ionic.Zlib.CompressionMode.Decompress:
                    this.baseStream = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress, Ionic.Zlib.CompressionLevel.Default);
                    break;
                default:
                    break;
            }

            this.baseStream.FlushMode = Ionic.Zlib.FlushType.Partial;
        }

        /// <summary>
        /// Writes the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        public void Write(byte[] buffer, int offset, int count)
        {
            this.baseStream.Write(buffer, offset, count);
        }
    }
}
