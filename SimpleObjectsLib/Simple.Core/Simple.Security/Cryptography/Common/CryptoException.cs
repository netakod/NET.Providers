using System;

namespace Simple.Security.Cryptography.Common
{
    /// <summary>
    /// The exception that is thrown when SSH exception occurs.
    /// </summary>
    public partial class CryptoException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoException"/> class.
        /// </summary>
        public CryptoException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public CryptoException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public CryptoException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
