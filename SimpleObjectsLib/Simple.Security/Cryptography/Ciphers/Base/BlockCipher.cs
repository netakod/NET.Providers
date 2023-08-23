﻿using System;

namespace Simple.Security.Cryptography
{
    /// <summary>
    /// Base class for block cipher implementations.
    /// </summary>
    public abstract class BlockCipher : SymmetricCipher
    {
        private readonly CipherMode _mode;

        private readonly CipherPadding _padding;

        /// <summary>
        /// Gets the size of the block in bytes.
        /// </summary>
        /// <value>
        /// The size of the block in bytes.
        /// </value>
        private readonly byte _blockSize;

        /// <summary>
        /// Gets the minimum data size.
        /// </summary>
        /// <value>
        /// The minimum data size.
        /// </value>
        public override byte MinimumSize
        {
            get { return BlockSize; }
        }

        /// <summary>
        /// Gets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public byte BlockSize
        {
            get
            {
                return _blockSize;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="mode">Cipher mode.</param>
        /// <param name="padding">Cipher padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        protected BlockCipher(byte[] key, byte blockSize, CipherMode mode, CipherPadding padding)
            : base(key)
        {
            _blockSize = blockSize;
            _mode = mode;
            _padding = padding;

            if (_mode != null)
                _mode.Init(this);
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="offset">The zero-based offset in <paramref name="data"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="data"/>.</param>
        /// <returns>Encrypted data</returns>
        public override byte[] Encrypt(byte[] data, int offset, int length)
        {
            if (length % _blockSize > 0)
            {
                if (_padding == null)
                {
                    throw new ArgumentException("data");
                }
                var paddingLength = _blockSize - (length % _blockSize);
                data = _padding.Pad(data, paddingLength);
                length += paddingLength;
            }

            var output = new byte[length];
            var writtenBytes = 0;

            for (var i = 0; i < length / _blockSize; i++)
            {
                if (_mode == null)
                {
                    writtenBytes += EncryptBlock(data, offset + (i * _blockSize), _blockSize, output, i * _blockSize);
                }
                else
                {
                    writtenBytes += _mode.EncryptBlock(data, offset + (i * _blockSize), _blockSize, output, i * _blockSize);
                }
            }

            if (writtenBytes < length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Decrypted data</returns>
        public override byte[] Decrypt(byte[] data)
        {
            if (data.Length % _blockSize > 0)
            {
                if (_padding == null)
                {
                    throw new ArgumentException("data");
                }
                data = _padding.Pad(_blockSize, data);
            }

            var output = new byte[data.Length];

            var writtenBytes = 0;
            for (var i = 0; i < data.Length / _blockSize; i++)
            {
                if (_mode == null)
                {
                    writtenBytes += DecryptBlock(data, i * _blockSize, _blockSize, output, i * _blockSize);
                }
                else
                {
                    writtenBytes += _mode.DecryptBlock(data, i * _blockSize, _blockSize, output, i * _blockSize);
                }
            }

            if (writtenBytes < data.Length)
            {
                throw new InvalidOperationException("Encryption error.");
            }

            return output;
        }
    }
}
