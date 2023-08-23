using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Simple;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Xml.XPath;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Simple.Network
{
    public class IpHelper
    {
		private const int NumberOfLabelsIPv4 = 4;
		private const int NumberOfLabelsIPv6 = 8;

		public static string[] SubnetMasks = new string[33];
        private static string[] SubnetMaskWildCards = new string[33];

    	static IpHelper()
        {
            SubnetMasks[ 0] = "0.0.0.0";         SubnetMaskWildCards[0] = "255.255.255.255";
            SubnetMasks[ 1] = "128.0.0.0";       SubnetMaskWildCards[1] = "127.255.255.255";
            SubnetMasks[ 2] = "192.0.0.0";       SubnetMaskWildCards[2] = "63.255.255.255";
            SubnetMasks[ 3] = "224.0.0.0";       SubnetMaskWildCards[3] = "31.255.255.255";
            SubnetMasks[ 4] = "240.0.0.0";       SubnetMaskWildCards[4] = "15.255.255.255";
            SubnetMasks[ 5] = "248.0.0.0";       SubnetMaskWildCards[5] = "7.255.255.255";
            SubnetMasks[ 6] = "252.0.0.0";       SubnetMaskWildCards[6] = "3.255.255.255";
            SubnetMasks[ 7] = "254.0.0.0";       SubnetMaskWildCards[7] = "1.255.255.255";
            SubnetMasks[ 8] = "255.0.0.0";       SubnetMaskWildCards[8] = "0.255.255.255";
            SubnetMasks[ 9] = "255.128.0.0";     SubnetMaskWildCards[9] = "0.127.255.255";
            SubnetMasks[10] = "255.192.0.0";     SubnetMaskWildCards[10] = "0.63.255.255";
            SubnetMasks[11] = "255.224.0.0";     SubnetMaskWildCards[11] = "0.31.255.255";
            SubnetMasks[12] = "255.240.0.0";     SubnetMaskWildCards[12] = "0.15.255.255";
            SubnetMasks[13] = "255.248.0.0";     SubnetMaskWildCards[13] = "0.7.255.255";
            SubnetMasks[14] = "255.252.0.0";     SubnetMaskWildCards[14] = "0.3.255.255";
            SubnetMasks[15] = "255.254.0.0";     SubnetMaskWildCards[15] = "0.1.255.255";
            SubnetMasks[16] = "255.255.0.0";     SubnetMaskWildCards[16] = "0.0.255.255";
            SubnetMasks[17] = "255.255.128.0";   SubnetMaskWildCards[17] = "0.0.127.255";
            SubnetMasks[18] = "255.255.192.0";   SubnetMaskWildCards[18] = "0.0.63.255";
            SubnetMasks[19] = "255.255.224.0";   SubnetMaskWildCards[19] = "0.0.31.255";
            SubnetMasks[20] = "255.255.240.0";   SubnetMaskWildCards[20] = "0.0.15.255";
            SubnetMasks[21] = "255.255.248.0";   SubnetMaskWildCards[21] = "0.0.7.255";
            SubnetMasks[22] = "255.255.252.0";   SubnetMaskWildCards[22] = "0.0.3.255";
            SubnetMasks[23] = "255.255.254.0";   SubnetMaskWildCards[23] = "0.0.1.255";
            SubnetMasks[24] = "255.255.255.0";   SubnetMaskWildCards[24] = "0.0.0.255";
            SubnetMasks[25] = "255.255.255.128"; SubnetMaskWildCards[25] = "0.0.0.127";
            SubnetMasks[26] = "255.255.255.192"; SubnetMaskWildCards[26] = "0.0.0.63";
            SubnetMasks[27] = "255.255.255.224"; SubnetMaskWildCards[27] = "0.0.0.31";
            SubnetMasks[28] = "255.255.255.240"; SubnetMaskWildCards[28] = "0.0.0.15";
            SubnetMasks[29] = "255.255.255.248"; SubnetMaskWildCards[29] = "0.0.0.7";
            SubnetMasks[30] = "255.255.255.252"; SubnetMaskWildCards[30] = "0.0.0.3";
            SubnetMasks[31] = "255.255.255.254"; SubnetMaskWildCards[31] = "0.0.0.1";
            SubnetMasks[32] = "255.255.255.255"; SubnetMaskWildCards[32] = "0.0.0.0";
        }

        public static string GetSubnetMask(int subnetMaskPrefix)
        {
            string value = subnetMaskPrefix >= 0 && subnetMaskPrefix < SubnetMasks.Count() ? SubnetMasks[subnetMaskPrefix] : String.Empty;
            
            return value;
        }

        public static string GetSubnetMaskWildCard(int subnetMaskPrefix)
        {
            string value = subnetMaskPrefix >= 0 && subnetMaskPrefix < SubnetMaskWildCards.Count() ? SubnetMaskWildCards[subnetMaskPrefix] : String.Empty;
            
            return value;
        }

        public static int GetSubnetPrefix(string subnetMask)
        {
            int value = SubnetMasks.Contains(subnetMask) ? IndexOf(SubnetMasks, subnetMask) : -1;
            
            return value;
        }

        public static int GetSubnetPrefixByWildCard(string subnetMaskWildCard)
        {
            int value = SubnetMaskWildCards.Contains(subnetMaskWildCard) ? IndexOf(SubnetMaskWildCards, subnetMaskWildCard) : 32;
            
            return value;
        }


        public static bool ValidateIpAddress(string? ipV4OrV6AddressText)
        {
            if (IPAddress.TryParse(ipV4OrV6AddressText, out IPAddress address))
                return address.AddressFamily == AddressFamily.InterNetwork || address.AddressFamily == AddressFamily.InterNetworkV6;

            return false;
           
            bool result = false;

            if (ipV4OrV6AddressText != null && ipV4OrV6AddressText != String.Empty && ipV4OrV6AddressText.Trim().Length > 0)
            {
				ipV4OrV6AddressText = ipV4OrV6AddressText.Trim();
                string[] ipAddressArray = ipV4OrV6AddressText.Split('.');

                if (ipAddressArray.Length == 4)
                {
                    result = true;

                    for (int i = 0; i <= 3; i++)
                    {
                        int a = -1;

                        try
                        {
                            a = Convert.ToInt32(ipAddressArray[i]);
                        }
                        catch
                        {
                            result = false;
                            
                            break;
                        }

                        if (a > 255 || a < 0)
                        {
                            result = false;
                            
                            break;
                        }
                    }
                }
            }

            return result;
        }

		//public static byte[] ParseIpAddress(string iPv4Orv6AddressText)
		//{
		//	var chars = iPv4Orv6AddressText.AsSpan();

		//	if (chars.Contains(':')) // chars.IndexOf(new ReadOnlySpan<char>(new char[1] { ':' }), StringComparison.CurrentCulture) >= 0)
		//		return IpHelper.ParseIPv6(ref chars);
		//	else
		//		return IpHelper.ParseIPv4(ref chars);
		//}

		/// <summary>
		/// Parse IPv4 text with ASCII address decimal bytes separated by comma. Example "192.168.1.0"
		/// </summary>
		/// <param name="ipV4AddressText">IPv4 text with ASCII address decimal bytes separated by comma</param>
		/// <returns></returns>
		public static byte[] ParseIPv4(string ipV4AddressText)
		{
			var chars = ipV4AddressText.AsSpan();
            byte[] result = ParseIPv4(chars);

			return result;
		}

        public static byte[] ParseIPv4(ReadOnlySpan<char> chars)
        {
			int currentValue = 0;
			int dotCount = 0; // Limit 3
			char ch;
			byte[] result = new byte[4];

			for (int i = 0; i < chars.Length; i++)
			{
				ch = chars[i]; // ipAddressText[i];

				if (ch != '.')
				{
					currentValue = (currentValue * 10) + (ch - '0');
				}
				else
				{
					result[dotCount++] = unchecked((byte)currentValue);
					currentValue = 0;
				}
			}

			result[dotCount] = unchecked((byte)currentValue);

			return result;
		}

  //      public static byte[] ParseIPv6(string ipV6AddressText)
  //      {
		//	var chars = ipV6AddressText.AsSpan();
		//	byte[] result = ParseIPv6(ref chars);

		//	return result;
		//}

		//public static byte[] ParseIPv6(ref ReadOnlySpan<char> chars)
		//{
		//	string? scopeId = null;

		//	byte[] result = ParseIPv6(chars, ref scopeId, out int prefixLength);

		//	return result;
		//}

		///// <summary>
		///// Convert this IPv6 address into a sequence 16 bytes array
		///// </summary>
		///// <param name="address">The validated IPv6 address</param>
		///// <param name="numbers"></param>
		///// <param name="start"></param>
		///// <param name="scopeId"></param>
		//public static byte[] ParseIPv6(ReadOnlySpan<char> address, ref string? scopeId, out int prefixLength, int start = 0)
		//{
		//	int number = 0;
		//	int index = 0;
		//	int compressorIndex = -1;
		//	bool numberIsValid = true;
		//	Span<ushort> numbers = stackalloc ushort[8];
		//	byte[] result = new byte[16];

		//	//This used to be a class instance member but have not been used so far
		//	prefixLength = 0;

		//	if (address[start] == '[')
		//		++start;

		//	for (int i = start; i < address.Length && address[i] != ']';)
		//	{
		//		switch (address[i])
		//		{
		//			case '%':

		//				if (numberIsValid)
		//				{
		//					numbers[index++] = (ushort)number;
		//					numberIsValid = false;
		//				}

		//				start = i;

		//				for (++i; i < address.Length && address[i] != ']' && address[i] != '/'; ++i)
		//				{
		//				}

		//				scopeId = new string(address.Slice(start, i - start));

		//				// ignore prefix if any
		//				for (; i < address.Length && address[i] != ']'; ++i)
		//				{
		//				}

		//				break;

		//			case ':':

		//				numbers[index++] = (ushort)number;
		//				number = 0;
		//				++i;

		//				if (address[i] == ':')
		//				{
		//					compressorIndex = index;
		//					++i;
		//				}
		//				else if ((compressorIndex < 0) && (index < 6))
		//				{
		//					// no point checking for IPv4 address if we don't
		//					// have a compressor or we haven't seen 6 16-bit
		//					// numbers yet
		//					break;
		//				}

		//				// check to see if the upcoming number is really an IPv4
		//				// address. If it is, convert it to 2 ushort numbers
		//				for (int j = i; j < address.Length &&
		//								(address[j] != ']') &&
		//								(address[j] != ':') &&
		//								(address[j] != '%') &&
		//								(address[j] != '/') &&
		//								(j < i + 4); ++j)
		//				{

		//					if (address[j] == '.')
		//					{
		//						// we have an IPv4 address. Find the end of it:
		//						// we know that since we have a valid IPv6
		//						// address, the only things that will terminate
		//						// the IPv4 address are the prefix delimiter '/'
		//						// or the end-of-string (which we conveniently
		//						// delimited with ']')
		//						while (j < address.Length && (address[j] != ']') && (address[j] != '/') && (address[j] != '%'))
		//						{
		//							++j;
		//						}

		//						//number = IPv4ParseHostNumber(address, i, j);
		//						//
		//						// Parse IPv4 host numbers
		//						//
		//						int begin = i, end = j;
		//						Span<byte> ipV4Numbers = stackalloc byte[NumberOfLabelsIPv4];

		//						for (int k = 0; k < ipV4Numbers.Length; ++k)
		//						{
		//							int b = 0;
		//							char ch;

		//							for (; (begin < end) && (ch = address[begin]) != '.' && ch != ':'; ++begin)
		//								b = (b * 10) + ch - '0';

		//							ipV4Numbers[k] = (byte)b;
		//							++begin;
		//						}

		//						number = BinaryPrimitives.ReadInt32BigEndian(ipV4Numbers);

		//						numbers[index++] = (ushort)(number >> 16);
		//						numbers[index++] = (ushort)number;
		//						i = j;

		//						// set this to avoid adding another number to
		//						// the array if there's a prefix
		//						number = 0;
		//						numberIsValid = false;

		//						break;
		//					}
		//				}

		//				break;

		//			case '/':

		//				if (numberIsValid)
		//				{
		//					numbers[index++] = (ushort)number;
		//					numberIsValid = false;
		//				}

		//				// since we have a valid IPv6 address string, the prefix
		//				// length is the last token in the string
		//				for (++i; address[i] != ']'; ++i)
		//					prefixLength = prefixLength * 10 + (address[i] - '0');

		//				break;

		//			default:

		//				number = number * 16 + Uri.FromHex(address[i++]);

		//				break;
		//		}
		//	}

		//	// add number to the array if its not the prefix length or part of
		//	// an IPv4 address that's already been handled
		//	if (numberIsValid)
		//		numbers[index++] = (ushort)number;

		//	// if we had a compressor sequence ("::") then we need to expand the
		//	// numbers array
		//	if (compressorIndex > 0)
		//	{
		//		int toIndex = NumberOfLabelsIPv6 - 1;
		//		int fromIndex = index - 1;

		//		// if fromIndex and toIndex are the same, it means that "zero bits" are already in the correct place
		//		// it happens for leading and trailing compression
		//		if (fromIndex != toIndex)
		//		{
		//			for (int i = index - compressorIndex; i > 0; --i)
		//			{
		//				numbers[toIndex--] = numbers[fromIndex];
		//				numbers[fromIndex--] = 0;
		//			}
		//		}
		//	}

		//	// TODO: Implement directly writing into result and remove numbers
		//	int pos = 0;

		//	for (int i = 0; i < numbers.Length; i++)
		//	{
		//		result[pos++] = (byte)(numbers[i] >> 8);
		//		result[pos++] = (byte)numbers[i];
		//	}

		//	return result;
		//}

		public static string GetIPv4AddressText(byte[] address) => $"{address[0].ToString()}.{address[1].ToString()}.{address[2].ToString()}.{address[3].ToString()}";

		//public static string GetNetworkIpAddress(IpAddressInfo ipAddress)
		//{
		//    return GetNetworkIpAddress(ipAddress.IpAddress, ipAddress.IpSubnetMask);
		//}

		public static string GetNetworkIpAddress(string ipAddress, int ipSubnetMaskNumberOfBits)
        {
            string ipSubnetMask = GetSubnetMask(ipSubnetMaskNumberOfBits);
            
            return GetNetworkIpAddress(ipAddress, ipSubnetMask);
        }

        public static string GetNetworkIpAddress(string ipAddress, string ipSubnetMask)
        {
            if (ipAddress == null || ipAddress.Trim() == String.Empty || ipSubnetMask.Trim() == String.Empty)
                return String.Empty;

            IPAddress address = IPAddress.Parse(ipAddress);
            IPAddress subnetMask = IPAddress.Parse(ipSubnetMask);
            IPAddress networkAddress = address.GetNetworkAddress(subnetMask);

            return networkAddress.ToString();
        }

        public static string GetBroadcastIpAddress(string ipAddress, string ipSubnetMask)
        {
            if (ipAddress.Trim() == String.Empty || ipSubnetMask.Trim() == String.Empty)
                return String.Empty;

            IPAddress address = IPAddress.Parse(ipAddress);
            IPAddress subnetMask = IPAddress.Parse(ipSubnetMask);
            IPAddress broadcastAddress = address.GetBroadcastAddress(subnetMask);

            return broadcastAddress.ToString();
        }

		public static bool IsInSameSubnet(string ipAddress, int subnetMaskPrefix, string ipAddressSameSubnetCandidate) => IsInSameSubnet(ipAddress, IpHelper.GetSubnetMask(subnetMaskPrefix), ipAddressSameSubnetCandidate);

        public static bool IsInSameSubnet(string ipAddress, string ipSubnetMask, string ipAddressSameSubnetCandidate)
        {
            if (ipAddress.Trim() == String.Empty || ipSubnetMask.Trim() == String.Empty || ipAddressSameSubnetCandidate.Trim() == String.Empty)
                return false;


            IPAddress address = IPAddress.Parse(ipAddress);
            IPAddress subnetMask = IPAddress.Parse(ipSubnetMask);
            IPAddress sameSubnetCandidate = IPAddress.Parse(ipAddressSameSubnetCandidate);

            bool result = address.IsInSameSubnet(sameSubnetCandidate, subnetMask);

            return result;
        }

        public static string CreateNetworkIpAddressText(string ipAddress, int ipSubnetMaskNumberOfBits)
        {
            string result = String.Empty;

            if (ipAddress == null || ipAddress.Trim().Length == 0)
            {
                result = String.Empty;
            }
            else if (ipSubnetMaskNumberOfBits >= 0 && ipSubnetMaskNumberOfBits <= 32)
            {
                result = ipSubnetMaskNumberOfBits < 32 ? String.Format("{0}/{1}", ipAddress, ipSubnetMaskNumberOfBits) : ipAddress;
            }

            return result;
        }

        /// <summary>
        /// Parse endpoint IP address or hostname text with or without port info.
        /// Example: wwww.myserver.com
        ///			 https://myserver.com
        ///			 myserver.com 8080
        ///			 myserver.com:8080
        ///			 myserver.com, 8080
        ///			 192.168.1.0 9000
        /// </summary>
        /// <param name="endpointText">The endpoint text.</param>
        public static IPEndPoint ParseIpEndPoint(string endpointText, int defaultPort)
        {
            string hostname = endpointText;
            int port = defaultPort;
            string[] list = endpointText.Split(new char[] { ':', ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (list.Length == 2)
            {
                hostname = list[0];
                port = Conversion.TryChangeType<int>(list[1], defaultPort);
            }

            IPAddress ipAddress = DnsHelper.ResolveIPAddressFromHostname(hostname);

            return new IPEndPoint(ipAddress, port);
        }

        public static string GetIpAddressOrHostname(EndPoint endPoint)
        {
            var dnsEndPoint = endPoint as DnsEndPoint;

            if (dnsEndPoint != null)
                return dnsEndPoint.Host;

            var ipEndPoint = endPoint as IPEndPoint;

            if (ipEndPoint != null)
                return ipEndPoint.Address.ToString();

            return String.Empty;
        }

        public static int GetPort(EndPoint endPoint)
        {
            var dnsEndPoint = endPoint as DnsEndPoint;

            if (dnsEndPoint != null)
                return dnsEndPoint.Port;

            var ipEndPoint = endPoint as IPEndPoint;

            if (ipEndPoint != null)
                return ipEndPoint.Port;

            return -1;
        }

        private static int IndexOf(string[] stringArray, string item)
        {
            for (int i = 0; i < stringArray.Length; i++)
                if (stringArray[i] == item)
                    return i;

            return -1;
        }
    }
}