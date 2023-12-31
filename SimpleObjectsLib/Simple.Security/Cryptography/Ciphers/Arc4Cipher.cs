﻿using System;

namespace Simple.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements ARCH4 cipher algorithm
    /// </summary>
    public sealed class Arc4Cipher : StreamCipher
    {
        private static readonly int STATE_LENGTH = 256;

        /// <summary>
        ///  Holds the state of the RC4 engine
        /// </summary>
        private byte[] _engineState;

        private int _x;

        private int _y;

        private byte[] _workingKey;

        /// <summary>
        /// Gets the minimum data size.
        /// </summary>
        /// <value>
        /// The minimum data size.
        /// </value>
        public override byte MinimumSize
        {
            get { return 0; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arc4Cipher" /> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dischargeFirstBytes">if set to <c>true</c> will disharged first 1536 bytes.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public Arc4Cipher(byte[] key, bool dischargeFirstBytes)
            : base(key)
        {
            this._workingKey = key;
            SetKey(this._workingKey);
            //   The first 1536 bytes of keystream
            //   generated by the cipher MUST be discarded, and the first byte of the
            //   first encrypted packet MUST be encrypted using the 1537th byte of
            //   keystream.
            if (dischargeFirstBytes)
                this.Encrypt(new byte[1536]);
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to encrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write encrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return this.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to decrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write decrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes decrypted.
        /// </returns>
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return this.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// Encrypted data.
        /// </returns>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            var output = new byte[length];
            this.ProcessBytes(input, offset, length, output, 0);
            return output;
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Decrypted data.
        /// </returns>
        public override byte[] Decrypt(byte[] input)
        {
            var output = new byte[input.Length];
            this.ProcessBytes(input, 0, input.Length, output, 0);
            return output;
        }

        private int ProcessBytes(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if ((inputOffset + inputCount) > inputBuffer.Length)
            {
                throw new IndexOutOfRangeException("input buffer too short");
            }

            if ((outputOffset + inputCount) > outputBuffer.Length)
            {
                throw new IndexOutOfRangeException("output buffer too short");
            }

            for (int i = 0; i < inputCount; i++)
            {
                this._x = (this._x + 1) & 0xff;
                this._y = (this._engineState[this._x] + this._y) & 0xff;

                // swap
                byte tmp = this._engineState[this._x];
                this._engineState[this._x] = this._engineState[this._y];
                this._engineState[this._y] = tmp;

                // xor
                outputBuffer[i + outputOffset] = (byte)(inputBuffer[i + inputOffset] ^ this._engineState[(this._engineState[this._x] + this._engineState[this._y]) & 0xff]);
            }
            return inputCount;
        }

        private void SetKey(byte[] keyBytes)
        {
            this._workingKey = keyBytes;

            this._x = 0;
            this._y = 0;

            if (this._engineState == null)
            {
                this._engineState = new byte[STATE_LENGTH];
            }

            // reset the state of the engine
            for (var i = 0; i < STATE_LENGTH; i++)
            {
                this._engineState[i] = (byte) i;
            }

            int i1 = 0;
            int i2 = 0;

            for (var i = 0; i < STATE_LENGTH; i++)
            {
                i2 = ((keyBytes[i1] & 0xff) + this._engineState[i] + i2) & 0xff;
                // do the byte-swap inline
                byte tmp = this._engineState[i];
                this._engineState[i] = this._engineState[i2];
                this._engineState[i2] = tmp;
                i1 = (i1 + 1) % keyBytes.Length;
            }
        }
    }
}
