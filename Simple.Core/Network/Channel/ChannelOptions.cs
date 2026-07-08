using System.Text;
using System.IO.Pipelines;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;

namespace Simple.Network
{
    public class ChannelOptions
    {
        public static readonly ChannelOptions Default = new ChannelOptions();
        
        public const int DefaultTimeout = 15000;
        public const int DefaultBufferSize = 255;

        // 1M by default
        public int MaxPackageLength { get; set; } = 1024 * 1024;

        public int ReceiveBufferSize { get; set; } = DefaultBufferSize;

        public int SendBufferSize { get; set; } = DefaultBufferSize;

        // trigger the read only when the stream is being consumed
        public bool ReadAsDemand { get; set; }


        /// <summary>
        /// Connect timeout in milliseconds
        /// </summary>
        public int ConnectTimeout { get; set; } = DefaultTimeout;

        /// <summary>
        /// Receive timeout in milliseconds
        /// </summary>
        /// <value></value>
        public int ReceiveTimeout { get; set; } = DefaultTimeout;

        /// <summary>
        /// Send timeout in milliseconds
        /// </summary>
        /// <value></value>
        public int SendTimeout { get; set; } = DefaultTimeout;
        
        /// <summary>
        /// Gets or sets Socket NoDelay property
        /// </summary>
        public bool NoDelay { get; set; } = true;

        public Encoding Encoding { get; set; } = new UTF8Encoding(false);

        public ILogger Logger { get; set; }

        public Pipe In { get; set; }

        public Pipe Out { get; set; }

        public Dictionary<string, string> Values { get; set; }
    }
}
