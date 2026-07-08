using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using Simple;

namespace Simple.Network
{
	public static class MacAddressHelper
	{
		public static long MaxMacAddress = (long)System.Math.Pow(2, 48) - 1; // ParseMacAddress("FF:FF:FF:FF:FF:FF");


		public static bool ValidateMacAddress(long macAddress)
		{
			return macAddress >= 0 && macAddress <= MaxMacAddress; // ParseMacAddress("FF:FF:FF:FF:FF:FF"); // Math.Pow(2, 48);
		}

		/// <summary>
		/// Determines whether if input is a valid MAC address.
		/// </summary>
		/// <param name="macAddressText">MAC address as string to convert. Possible format is 000000000000, 00 00 00 00 00 00, 00:00:00:00:00:00, 00-00-00-00-00-00</param>
		/// <returns>True if MAC address is valid, false otherwise</returns>
		public static bool ValidateMacAddress(string macAddressText)
		{
			if (macAddressText.IsNullOrEmpty())
				return false;

			string macText = RemoveMacAddressSeparators(macAddressText);

			if (macText.Length != 12)
				return false;

			return new Regex(@"^[0-9a-fA-F]{12}$").IsMatch(macText);
		}

		///// <summary>
		///// Determines whether if input is a valid MAC address.
		///// </summary>
		///// <param name="macAddress">MAC address as string to convert. Possible format is 000000000000, 00 00 00 00 00 00, 00:00:00:00:00:00, 00-00-00-00-00-00</param>
		///// <returns>True if MAC address is valid, false otherwise</returns>
		//public static bool ValidateMacAddress2(string macAddress)
		//{
		//	bool result;

		//	if (String.IsNullOrEmpty(macAddress) || macAddress.Trim().Length == 0)
		//	{
		//		result = true;
		//	}
		//	else if (macAddress.Length < 12 || macAddress.Length > 17)
		//	{
		//		result = false;
		//	}
		//	else if (macAddress.Length == 12)
		//	{
		//		result = new Regex(@"^[0-9a-fA-F]{12}$").IsMatch(macAddress);
		//	}
		//	else
		//	{
		//		result = new Regex(@"^([0-9a-fA-F]{2}[:-]){5}([0-9a-fA-F]{2})$").IsMatch(macAddress);
		//	}

		//	return result;
		//}

		public static long? ParseMacAddress(string? macAddressText)
		{
			if (macAddressText == null)
				return null;
			
			if (ValidateMacAddress(macAddressText))
				return long.Parse(RemoveMacAddressSeparators(macAddressText), NumberStyles.HexNumber);

			return null;
		}

		public static string CreateMacAddressText(long? macAddress, string octetSplitter = "-")
		{
			string result = String.Empty;

			if (macAddress == null)
				return result;

			result = String.Format("{0:X2}{1}{2:X2}{3}{4:X2}{5}{6:X2}{7}{8:X2}{9}{10:X2}",
				(macAddress >> 40) & 0xFF, octetSplitter, 
				(macAddress >> 32) & 0xFF, octetSplitter,
				(macAddress >> 24) & 0xFF, octetSplitter,
				(macAddress >> 16) & 0xFF, octetSplitter,
				(macAddress >> 8) & 0xFF, octetSplitter,
				 macAddress & 0xFF);

			return result;
		}

		//public static string CreateMacAddressText2(long? macAddress)
		//{
		//	string result = String.Empty;

		//	if (macAddress == null)
		//		return result;

		//	byte[] parts = BitConverter.GetBytes((long)macAddress);

		//	if (BitConverter.IsLittleEndian)
		//	{
		//		parts = parts.Reverse().ToArray();
		//		result = BitConverter.ToString(parts).Substring(6); // Skip first two octet
		//	}
		//	else
		//	{
		//		result = BitConverter.ToString(parts).Substring(0, 17); // first 17 chars (6 octet by 2 + 5 splitters "-")
		//	}

		//	return result;
		//}

		private static string RemoveMacAddressSeparators(string macAddress)
		{
			return macAddress.Replace(":", "").Replace("-", "").Replace(".", "").Replace(" ", "");
		}
	}
}
