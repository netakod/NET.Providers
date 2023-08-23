using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Routrek.Info
{
	public class ErrorInfo
	{
		private static Dictionary<ErrorSshMessage, string> errorMessageDictionaryEng = new Dictionary<ErrorSshMessage, string>()
		{
			{ ErrorSshMessage.AuthenticationFailed, "User authentication failed." },
			{ ErrorSshMessage.BrokenKeyFile, "{0} is not valid private key file." },
			{ ErrorSshMessage.NotSSHServer, "The destination would not be a correct SSH server." },
			{ ErrorSshMessage.ServerNotSupportedPassword, "Server does not support the plain password authentication." },
			{ ErrorSshMessage.ServerNotSupportedRSA, "Server does not support the RSA authentication." },
			{ ErrorSshMessage.ServerNotSupportedX, "Server does not support {0}." },
			{ ErrorSshMessage.ServerRefusedRSA, "Server refused the RSA authentication." },
			{ ErrorSshMessage.UnexpectedEOF, "An unexpected end of stream is found." },
			{ ErrorSshMessage.UnexpectedResponse, "Received unexpected response [{0}]." },
			{ ErrorSshMessage.WrongPassphrase, "The passphrase is wrong." },
		};

		private static Dictionary<ErrorSshMessage, string> errorMessageDictionaryJa = new Dictionary<ErrorSshMessage, string>()
		{
			{ ErrorSshMessage.AuthenticationFailed, "ユーザ認証に失敗しました。" },
			{ ErrorSshMessage.BrokenKeyFile, "{0} は正しい鍵ファイルではありません。" },
			{ ErrorSshMessage.NotSSHServer, "接続先はSSHのサーバではありません。" },
			{ ErrorSshMessage.ServerNotSupportedPassword, "サーバはパスワード認証をサポートしません。" },
			{ ErrorSshMessage.ServerNotSupportedRSA, "サーバはRSA認証をサポートしません。" },
			{ ErrorSshMessage.ServerNotSupportedX, "サーバは{0}をサポートしません。" },
			{ ErrorSshMessage.ServerRefusedRSA, "サーバはRSA認証を拒否しました。" },
			{ ErrorSshMessage.UnexpectedEOF, "予期しないストリーム終端に到達しました。" },
			{ ErrorSshMessage.UnexpectedResponse, "予期しない応答 {0} を受信しました。" },
			{ ErrorSshMessage.WrongPassphrase, "パスフレーズが誤っています" },
		};

		public static string GetErrorMessageText(ErrorSshMessage errorMessage)
		{
			string result = String.Empty;
			Dictionary<ErrorSshMessage, string> errorMessageDictionary;
			CultureInfo cultureInfo = System.Threading.Thread.CurrentThread.CurrentUICulture;
			
			if(cultureInfo.Name.StartsWith("ja"))
			{
				errorMessageDictionary = errorMessageDictionaryJa;
			}
			else
			{
				errorMessageDictionary = errorMessageDictionaryEng;
			}

			errorMessageDictionary.TryGetValue(errorMessage, out result);

			return result;
		}
	}
	
	public enum ErrorSshMessage
	{
		AuthenticationFailed,
		BrokenKeyFile,
		NotSSHServer,
		ServerNotSupportedPassword,
		ServerNotSupportedRSA,
		ServerNotSupportedX,
		ServerRefusedRSA,
		UnexpectedEOF,
		UnexpectedResponse,
		WrongPassphrase
	}
}
