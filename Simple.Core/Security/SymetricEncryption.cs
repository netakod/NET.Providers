using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Simple.Security
{
    public class SymmetricEncryption
    {
        // Key example: 
        
        //private static byte[] key = { 151, 121, 98, 229, 232, 207, 122, 259, 25, 198, 49, 79, 112, 108, 152, 183, 23, 15, 66, 175, 88, 138, 228, 89, 123, 47, 49, 191, 120, 35, 144, 154 };
        //private static byte[] iv = { 253, 220, 244, 208, 144, 173, 163, 121, 13, 19, 148, 168, 54, 163, 10, 8 };

        //private RijndaelManaged rijndael;
        //private ICryptoTransform encryptor;
        //private ICryptoTransform decryptor;
        //int blockSize;

		private static object lockObject = new object();

        public SymmetricEncryption()
        {
            //this.rijndael = new RijndaelManaged();

            //this.rijndael.Key = key;
            //this.rijndael.IV = iv;
            //this.blockSize = this.rijndael.BlockSize;

            //this.encryptor = this.rijndael.CreateEncryptor();
            //this.decryptor = this.rijndael.CreateDecryptor();
        }

        public byte[] Encrypt(byte[] data, ICryptoTransform encryptor, int blockSize)
        {
			lock (lockObject)
			{
				int bufferSize = data.Length + (blockSize / 8);
				MemoryStream stream = new MemoryStream(bufferSize);

				CryptoStream cryptoStream = new CryptoStream(stream, encryptor, CryptoStreamMode.Write);
				cryptoStream.Write(data, 0, data.Length);
				cryptoStream.FlushFinalBlock();

				return stream.ToArray();
			}
        }

        public byte[] Decrypt(byte[] data, ICryptoTransform decryptor, int blockSize)
        {
			lock (lockObject)
			{
				MemoryStream stream = new MemoryStream(data);

				int bufferSize = data.Length + (blockSize / 8);
				byte[] buffer = new byte[bufferSize];

				CryptoStream cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
				int size = cryptoStream.Read(buffer, 0, bufferSize);

				byte[] resultData = new byte[size];
				Array.Copy(buffer, 0, resultData, 0, size);
				
				return resultData;
			}
        }

		public static string EncryptString(string clearText, string password)
		{
			SymmetricAlgorithm algorithm = GetAlgorithm(password);
			byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, algorithm.CreateEncryptor(), CryptoStreamMode.Write);
			cs.Write(clearBytes, 0, clearBytes.Length);
			cs.FlushFinalBlock();
			cs.Close();
			return Convert.ToBase64String(ms.ToArray());
		}

		/*
		 * decryptString
		 * provides simple decryption of a string, with a given password
		 */
		public static string DecryptString(string cipherText, string password)
		{
			SymmetricAlgorithm algorithm = GetAlgorithm(password);
			byte[] cipherBytes = Convert.FromBase64String(cipherText);
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, algorithm.CreateDecryptor(), CryptoStreamMode.Write);
			cs.Write(cipherBytes, 0, cipherBytes.Length);
			cs.FlushFinalBlock();
			cs.Close();
			return System.Text.Encoding.Unicode.GetString(ms.ToArray());
		}

		// create and initialize a crypto algorithm
		private static SymmetricAlgorithm GetAlgorithm(string password)
		{
			SymmetricAlgorithm algorithm = Rijndael.Create();
			Rfc2898DeriveBytes rdb = new Rfc2898DeriveBytes(
				password, new byte[] {	0x53,0x6f,0x64,0x69,0x75,0x6d,0x20,             // salty goodness
										0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65
									 });
			algorithm.Padding = PaddingMode.ISO10126;
			algorithm.Key = rdb.GetBytes(32);
			algorithm.IV = rdb.GetBytes(16);
			return algorithm;
		}
    }
}