// AES privacy provider
// Copyright (C) 2009-2010 Lex Li, Milan Sinadinovic
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

/*
 * Created by SharpDevelop.
 * User: lextm
 * Date: 5/30/2009
 * Time: 8:06 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Lextm.SharpSnmpLib.Security
{
    /// <summary>
    /// Privacy provider base for AES.
    /// </summary>
    /// <remarks>
    /// This is an experimental port from SNMP#NET project. As AES is not part of SNMP RFC, this class is provided as it is.
    /// If you want other AES providers, you can port them from SNMP#NET in a similar manner.
    /// </remarks>
    public abstract class AESPrivacyProviderBase : IPrivacyProvider
    {
        private readonly SaltGenerator _salt = new();
        private readonly OctetString _phrase;

        /// <summary>
        /// Verifies if the provider is supported.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
#if NETSTANDARD2_0
                return Helper.AESSupported;
#else
                return true;
#endif
            }
        }

#if NET6_0
        /// <summary>
        /// Flag to force using legacy encryption/decryption code on .NET 6.
        /// </summary>
        public static bool UseLegacy { get; set; }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="AESPrivacyProviderBase"/> class.
        /// </summary>
        /// <param name="keyBytes">Key bytes.</param>
        /// <param name="phrase">The phrase.</param>
        /// <param name="auth">The authentication provider.</param>
        protected AESPrivacyProviderBase(int keyBytes, OctetString phrase, IAuthenticationProvider auth)
        {
            if (keyBytes != 16 && keyBytes != 24 && keyBytes != 32)
            {
                throw new ArgumentOutOfRangeException(nameof(keyBytes), "Valid key sizes are 16, 24 and 32 bytes.");
            }

            if (auth == null)
            {
                throw new ArgumentNullException(nameof(auth));
            }

            KeyBytes = keyBytes;

            // IMPORTANT: in this way privacy cannot be non-default.
            if (auth == DefaultAuthenticationProvider.Instance)
            {
                throw new ArgumentException("If authentication is off, then privacy cannot be used.", nameof(auth));
            }

            _phrase = phrase ?? throw new ArgumentNullException(nameof(phrase));
            AuthenticationProvider = auth;
        }

        /// <summary>
        /// Corresponding <see cref="IAuthenticationProvider"/>.
        /// </summary>
        public IAuthenticationProvider AuthenticationProvider { get; private set; }

        /// <summary>
        /// Engine IDs.
        /// </summary>
        /// <remarks>This is an optional field, and only used by TRAP v2 authentication.</remarks>
        public ICollection<OctetString>? EngineIds { get; set; }

        /// <summary>
        /// Encrypt scoped PDU using AES encryption protocol
        /// </summary>
        /// <param name="unencryptedData">Unencrypted scoped PDU byte array</param>
        /// <param name="key">Encryption key. Key has to be at least 32 bytes is length</param>
        /// <param name="engineBoots">Engine boots.</param>
        /// <param name="engineTime">Engine time.</param>
        /// <param name="privacyParameters">Privacy parameters out buffer. This field will be filled in with information
        /// required to decrypt the information. Output length of this field is 8 bytes and space has to be reserved
        /// in the USM header to store this information</param>
        /// <returns>Encrypted byte array</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when encryption key is null or length of the encryption key is too short.</exception>
        internal byte[] Encrypt(byte[] unencryptedData, byte[] key, int engineBoots, int engineTime, byte[] privacyParameters)
        {
            if (!IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            // check the key before doing anything else
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length < KeyBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(key), "Invalid key length.");
            }

            if (unencryptedData == null)
            {
                throw new ArgumentNullException(nameof(unencryptedData));
            }

            var iv = GetIV(engineBoots, engineTime, privacyParameters);
            var pkey = GetKey(key);
#if NET6_0
            return UseLegacy ? LegacyEncrypt(pkey, iv, unencryptedData) : Net6Encrypt(pkey, iv, unencryptedData);
#else
            return LegacyEncrypt(pkey, iv, unencryptedData);
#endif
        }

#if NET6_0
        internal byte[] Net6Encrypt(byte[] key, byte[] iv, byte[] unencryptedData)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;

            var length = (unencryptedData.Length % MinimalBlockSize == 0) 
                ? unencryptedData.Length
                : ((unencryptedData.Length / MinimalBlockSize) + 1) * MinimalBlockSize;
            var result = new byte[length];
            var buffer = result.AsSpan();
            var encryptedData = aes.EncryptCfb(unencryptedData.AsSpan(), iv.AsSpan(), buffer, PaddingMode.Zeros, 128);
            
            // check if encrypted data is the same length as source data
            if (encryptedData != unencryptedData.Length)
            {
                // cut out the padding
                return buffer.Slice(0, unencryptedData.Length).ToArray();
            }

            return result;
        }
#endif

        internal byte[] LegacyEncrypt(byte[] key, byte[] iv, byte[] unencryptedData)
        {
#if NET471
            using (var rm = Rijndael.Create())
#else
            using (var rm = Aes.Create())
#endif
            {
                rm.KeySize = KeyBytes * 8;
                rm.FeedbackSize = 128;
                rm.BlockSize = 128;

                // we have to use Zeros padding otherwise we get encrypt buffer size exception
                rm.Padding = PaddingMode.Zeros;
                rm.Mode = CipherMode.CFB;

                rm.Key = key;
                rm.IV = iv;
                using (var cryptor = rm.CreateEncryptor())
                {
                    var encryptedData = cryptor.TransformFinalBlock(unencryptedData, 0, unencryptedData.Length);

                    // check if encrypted data is the same length as source data
                    if (encryptedData.Length != unencryptedData.Length)
                    {
                        // cut out the padding
                        var tmp = new byte[unencryptedData.Length];
                        Buffer.BlockCopy(encryptedData, 0, tmp, 0, unencryptedData.Length);
                        return tmp;
                    }

                    return encryptedData;
                }
            }
        }

        /// <summary>
        /// Decrypt AES encrypted scoped PDU.
        /// </summary>
        /// <param name="encryptedData">Source data buffer</param>
        /// <param name="engineBoots">Engine boots.</param>
        /// <param name="engineTime">Engine time.</param>
        /// <param name="key">Decryption key. Key length has to be 32 bytes in length or longer (bytes beyond 32 bytes are ignored).</param>
        /// <param name="privacyParameters">Privacy parameters extracted from USM header</param>
        /// <returns>Decrypted byte array</returns>
        /// <exception cref="ArgumentNullException">Thrown when encrypted data is null or length == 0</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when encryption key length is less then 32 byte or if privacy parameters
        /// argument is null or length other then 8 bytes</exception>
        internal byte[] Decrypt(byte[] encryptedData, byte[] key, int engineBoots, int engineTime, byte[] privacyParameters)
        {
            if (!IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (encryptedData == null)
            {
                throw new ArgumentNullException(nameof(encryptedData));
            }

            if (key.Length < KeyBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(key), "Invalid key length.");
            }

            var iv = GetIV(engineBoots, engineTime, privacyParameters);

            var finalKey = GetKey(key);
#if NET6_0
            return UseLegacy ? LegacyDecrypt(finalKey, iv, encryptedData) : Net6Decrypt(finalKey, iv, encryptedData);
#else
            return LegacyDecrypt(finalKey, iv, encryptedData);
#endif
        }

#if NET6_0
        internal byte[] Net6Decrypt(byte[] key, byte[] iv, byte[] encryptedData)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            if ((encryptedData.Length % MinimalBlockSize) != 0)
            {
                var div = encryptedData.Length / MinimalBlockSize;
                var newLength = (div + 1) * MinimalBlockSize;
                var decryptBuffer = new byte[newLength];
                Buffer.BlockCopy(encryptedData, 0, decryptBuffer, 0, encryptedData.Length);
                var buffer = new byte[newLength].AsSpan();
                var decryptedData = aes.DecryptCfb(decryptBuffer.AsSpan(), iv.AsSpan(), buffer, PaddingMode.Zeros, 128);

                // now remove padding
                return buffer.Slice(0, encryptedData.Length).ToArray();
            }

            return aes.DecryptCfb(encryptedData, iv, PaddingMode.Zeros, 128);
        }
#endif

        internal byte[] LegacyDecrypt(byte[] key, byte[] iv, byte[] encryptedData)
        {
            // now do CFB decryption of the encrypted data
#if NET471
            using (var rm = Rijndael.Create())
#else
            using (var rm = Aes.Create())
#endif
            {
                rm.KeySize = KeyBytes * 8;
                rm.FeedbackSize = 128;
                rm.BlockSize = 128;
                rm.Padding = PaddingMode.Zeros;
                rm.Mode = CipherMode.CFB;

                rm.Key = key;
                rm.IV = iv;
                using (var cryptor = rm.CreateDecryptor())
                {
                    // We need to make sure that cryptedData is a collection of 128 byte blocks
                    byte[] decryptedData;
                    if ((encryptedData.Length % MinimalBlockSize) != 0)
                    {
                        var div = encryptedData.Length / MinimalBlockSize;
                        var newLength = (div + 1) * MinimalBlockSize;
                        var decryptBuffer = new byte[newLength];
                        Buffer.BlockCopy(encryptedData, 0, decryptBuffer, 0, encryptedData.Length);
                        decryptedData = cryptor.TransformFinalBlock(decryptBuffer, 0, decryptBuffer.Length);

                        // now remove padding
                        var buffer = new byte[encryptedData.Length];
                        Buffer.BlockCopy(decryptedData, 0, buffer, 0, encryptedData.Length);
                        return buffer;
                    }

                    decryptedData = cryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                    return decryptedData;
                }
            }
        }

        /// <summary>
        /// Returns the length of privacyParameters USM header field. For AES, field length is 8.
        /// </summary>
        private static int PrivacyParametersLength
        {
            get { return 8; }
        }

        /// <summary>
        /// Returns minimum encryption/decryption key length. For DES, returned value is 16.
        /// 
        /// DES protocol itself requires an 8 byte key. Additional 8 bytes are used for generating the
        /// encryption IV. For encryption itself, first 8 bytes of the key are used.
        /// </summary>
        private int MinimumKeyLength
        {
            get { return KeyBytes; }
        }

        /// <summary>
        /// Return maximum encryption/decryption key length. For DES, returned value is 16
        /// 
        /// DES protocol itself requires an 8 byte key. Additional 8 bytes are used for generating the
        /// encryption IV. For encryption itself, first 8 bytes of the key are used.
        /// </summary>
        public int MaximumKeyLength
        {
            get { return KeyBytes; }
        }

#region IPrivacyProvider Members

        /// <summary>
        /// Decrypts the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public ISnmpData Decrypt(ISnmpData data, SecurityParameters parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var code = data.TypeCode;
            if (code != SnmpType.OctetString)
            {
                throw new ArgumentException($"Cannot decrypt the scope data: {code}.", nameof(data));
            }

            if (parameters.EngineId == null)
            {
                throw new ArgumentException("Invalid security parameters", nameof(parameters));
            }

            var octets = (OctetString)data;
            var bytes = octets.GetRaw();
            var pkey = PasswordToKey(_phrase.GetRaw(), parameters.EngineId.GetRaw());

            // decode encrypted packet
            var decrypted = Decrypt(bytes, pkey, parameters.EngineBoots!.ToInt32(), parameters.EngineTime!.ToInt32(), parameters.PrivacyParameters!.GetRaw());
            return DataFactory.CreateSnmpData(decrypted);
        }

        /// <summary>
        /// Encrypts the specified scope.
        /// </summary>
        /// <param name="data">The scope data.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public ISnmpData Encrypt(ISnmpData data, SecurityParameters parameters)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (data.TypeCode != SnmpType.Sequence && !(data is ISnmpPdu))
            {
                throw new ArgumentException("Invalid data type.", nameof(data));
            }

            if (parameters.EngineId == null)
            {
                throw new ArgumentException("Invalid security parameters", nameof(parameters));
            }

            var pkey = PasswordToKey(_phrase.GetRaw(), parameters.EngineId.GetRaw());
            var bytes = data.ToBytes();
            var reminder = bytes.Length % 8;
            var count = reminder == 0 ? 0 : 8 - reminder;
            using (var stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                for (var i = 0; i < count; i++)
                {
                    stream.WriteByte(1);
                }

                bytes = stream.ToArray();
            }

            var encrypted = Encrypt(bytes, pkey, parameters.EngineBoots!.ToInt32(), parameters.EngineTime!.ToInt32(), parameters.PrivacyParameters!.GetRaw());
            return new OctetString(encrypted);
        }

        /// <summary>
        /// Gets the salt.
        /// </summary>
        /// <value>The salt.</value>
        public OctetString Salt
        {
            get { return new OctetString(_salt.GetSaltBytes()); }
        }

        /// <summary>
        /// Gets the key bytes.
        /// </summary>
        /// <value>The key bytes.</value>
        public int KeyBytes { get; private set; } = 16;

        private const int MinimalBlockSize = 16;

        /// <summary>
        /// Passwords to key.
        /// </summary>
        /// <param name="secret">The secret.</param>
        /// <param name="engineId">The engine identifier.</param>
        /// <returns></returns>
        public byte[] PasswordToKey(byte[] secret, byte[] engineId)
        {
            var pkey = AuthenticationProvider.PasswordToKey(secret, engineId);
            if (pkey.Length < MinimumKeyLength)
            {
                pkey = ExtendShortKey(pkey, engineId, AuthenticationProvider);
            }

            return pkey;
        }

#endregion

        private byte[] GetKey(byte[] key)
        {
            if (key.Length > KeyBytes)
            {
                var normKey = new byte[KeyBytes];
                Buffer.BlockCopy(key, 0, normKey, 0, KeyBytes);
                return normKey;
            }

            return key;
        }

        private byte[] GetIV(int engineBoots, int engineTime, byte[] privacyParameters)
        {
            var iv = new byte[16];
            var bootsBytes = BitConverter.GetBytes(engineBoots);
            iv[0] = bootsBytes[3];
            iv[1] = bootsBytes[2];
            iv[2] = bootsBytes[1];
            iv[3] = bootsBytes[0];
            var timeBytes = BitConverter.GetBytes(engineTime);
            iv[4] = timeBytes[3];
            iv[5] = timeBytes[2];
            iv[6] = timeBytes[1];
            iv[7] = timeBytes[0];

            // Copy salt value to the iv array
            Buffer.BlockCopy(privacyParameters, 0, iv, 8, PrivacyParametersLength);
            return iv;
        }

        /// <summary>
        /// Some protocols support a method to extend the encryption or decryption key when supplied key
        /// is too short.
        /// </summary>
        /// <param name="shortKey">Key that needs to be extended</param>
        /// <param name="engineID">Authoritative engine id. Value is retrieved as part of SNMP v3 discovery procedure</param>
        /// <param name="authProtocol">Authentication protocol class instance cast as <see cref="IAuthenticationProvider"/></param>
        /// <returns>Extended key value</returns>
        internal byte[] ExtendShortKey(byte[] shortKey, byte[] engineID, IAuthenticationProvider authProtocol)
        {
            byte[] extKey = new byte[MinimumKeyLength];
            byte[] lastKeyBuf = new byte[shortKey.Length];
            Array.Copy(shortKey, lastKeyBuf, shortKey.Length);
            int keyLen = shortKey.Length > MinimumKeyLength ? MinimumKeyLength : shortKey.Length;
            Array.Copy(shortKey, extKey, keyLen);
            while (keyLen < MinimumKeyLength)
            {
                byte[] tmpBuf = authProtocol.PasswordToKey(lastKeyBuf, engineID);
                if (tmpBuf.Length <= (MinimumKeyLength - keyLen))
                {
                    Array.Copy(tmpBuf, 0, extKey, keyLen, tmpBuf.Length);
                    keyLen += tmpBuf.Length;
                }
                else
                {
                    Array.Copy(tmpBuf, 0, extKey, keyLen, MinimumKeyLength - keyLen);
                    keyLen += (MinimumKeyLength - keyLen);
                }

                lastKeyBuf = new byte[tmpBuf.Length];
                Array.Copy(tmpBuf, lastKeyBuf, tmpBuf.Length);
            }

            return extKey;
        }
    }
}
