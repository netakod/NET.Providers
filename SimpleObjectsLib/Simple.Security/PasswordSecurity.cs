using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Simple.Security
{
    public class PasswordSecurity
    {
        private static SHA512 hash = SHA512.Create();  //new SHA512Managed();

		public static string Encrypt(string plainText, ICryptoTransform encryptor)
		{
            if (plainText.IsNullOrEmpty())
                return String.Empty;
            
            //byte[] iv = new byte[16];
			byte[] array;

			//using (Aes aes = Aes.Create())
			//{
			//	aes.Key = Encoding.UTF8.GetBytes(key);
			//	aes.IV = iv;

			//ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
				{
					using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
					{
						streamWriter.Write(plainText);
					}

					array = memoryStream.ToArray();
				}
			}
			//}

			return Convert.ToBase64String(array);
		}

		public static string Decrypt(string cipherText, ICryptoTransform decryptor)
		{
            if (cipherText.IsNullOrEmpty())
                return String.Empty;
            
            //byte[] iv = new byte[16];
			byte[] buffer = Convert.FromBase64String(cipherText);

			//using (Aes aes = Aes.Create())
			//{
			//	aes.Key = Encoding.UTF8.GetBytes(key);
			//	aes.IV = iv;
			//	ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

			using (MemoryStream memoryStream = new MemoryStream(buffer))
			{
				using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
				{
					using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
					{
						return streamReader.ReadToEnd();
					}
				}
			}
			//}
		}


		//public static string EncryptPassword(object value, ICryptoTransform encryptor, int blockSize)
  //      {
  //          if (value != null)
  //              return EncryptPassword(value.ToString(), encryptor, blockSize);

  //          return null;
  //      }

  //      public static string EncryptPassword(string password, ICryptoTransform encryptor, int blockSize)
  //      {
  //          if (password.IsNullOrEmpty())
  //              return null;

  //          string encrypted;
  //          byte[] encryptedArray;

  //          SymmetricEncryption symmetricEncryption = new SymmetricEncryption();
            
  //          encryptedArray = symmetricEncryption.Encrypt(Encoding.Unicode.GetBytes(password), encryptor, blockSize);
  //          encrypted = ByteArrayToHexString(encryptedArray);

  //          return encrypted;
  //      }


  //      public static string DecryptPassword(object encryptedValue, ICryptoTransform decryptor, int blockSize)
		//{
  //          if (encryptedValue == null)
  //              return null;

  //          return DecryptPassword(encryptedValue.ToString(), decryptor, blockSize);
		//}

  //      public static string DecryptPassword(string encryptedPassword, ICryptoTransform decryptor, int blockSize)
  //      {
  //          string decrypted;
  //          byte[] decryptedArray;

  //          if (String.IsNullOrEmpty(encryptedPassword))
  //              return encryptedPassword;

  //          SymmetricEncryption symmetricEncryption = new SymmetricEncryption();
            
  //          decryptedArray = symmetricEncryption.Decrypt(HexStringToByteArray(encryptedPassword), decryptor, blockSize);
  //          decrypted = Encoding.Unicode.GetString(decryptedArray);

  //          return decrypted;
  //      }

        private static byte[] EncryptUnicodeText(string text, ICryptoTransform encryptor, int blockSize)
        {
            SymmetricEncryption symmetricEncryption = new SymmetricEncryption();
            
            return symmetricEncryption.Encrypt(Encoding.Unicode.GetBytes(text), encryptor, blockSize);
        }

        private static string DecryptToUnicodeText(byte[] encryptedArray, ICryptoTransform decryptor, int blockSize)
        {
            SymmetricEncryption symmetricEncryption = new SymmetricEncryption();
            
            return Encoding.Unicode.GetString(symmetricEncryption.Decrypt(encryptedArray, decryptor, blockSize));
        }

        public static string HashPassword(string password)
        {
            byte[] tempArray = hash.ComputeHash(Encoding.Unicode.GetBytes(password));

            return Convert.ToBase64String(tempArray); // ByteArrayToHexString(tempArray);
        }

        private static byte[] HexStringToByteArray(string inString)
        {
            int size = inString.Length / 2;
            byte[] outArray = new byte[size];

            for (int i = 0; i < size; i++)
                outArray[i] = Convert.ToByte(inString.Substring(i * 2, 2), 16);

            return outArray;
        }

        private static string ByteArrayToHexString(byte[] inArray)
        {
            // BitConverter.ToString() returns hypen separated hex bytes ("F9-E2-24").
            return BitConverter.ToString(inArray).Replace("-", String.Empty);
        }
    }
}