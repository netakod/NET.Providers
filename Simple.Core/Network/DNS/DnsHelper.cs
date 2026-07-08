using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Simple;

namespace Simple.Network
{
	public static class DnsHelper
	{

		/// <summary>
		/// Method for retrieving the IP address from the hostname or IP address provided
		/// </summary>
		/// <param name="hostnameOrAddress">The host we need the address for</param>
		/// <returns>The resolved IPAddress class.</returns>
		public static IPAddress ResolveIPAddressFromHostname(string hostnameOrAddress)
		{
			return ResolveIPAddressFromHostname(hostnameOrAddress, AddressFamily.InterNetwork); // IPv4 as default
		}

		/// <summary>
		/// Method for retrieving the IP address from the hostname or IP address provided
		/// </summary>
		/// <param name="hostnameOrAddress">The host we need the address for</param>
		/// <param name="preferedAddressFamily">The preferred address family</param>
		/// <returns>The resolved IPAddress class.</returns>
		public static IPAddress ResolveIPAddressFromHostname(string hostnameOrAddress, AddressFamily preferedAddressFamily)
		{
			if (hostnameOrAddress.IsNullOrEmpty() || hostnameOrAddress.Trim().Length == 0)
			{
				return IPAddress.None;
			}
			else if (hostnameOrAddress.Trim() == "0.0.0.0")
			{
				return IPAddress.Any;
			}

			IPAddress ipAddress = null;

			try
			{
				IPAddress[] ipAddresses = Dns.GetHostAddresses(hostnameOrAddress);

				ipAddress = ipAddresses.FirstOrDefault(a => a.AddressFamily == preferedAddressFamily); // try to get an IPv4 IP address first

				if (ipAddress == null)
				{
					if (preferedAddressFamily == AddressFamily.InterNetwork) // InterNetwork is IPv4
					{
						ipAddress = ipAddresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetworkV6); // Try to find IPv6 IPAddress
					}
					else if (preferedAddressFamily == AddressFamily.InterNetworkV6)
					{
						ipAddress = ipAddresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork); // Try to find IPv4 IPAddress
					}
				}
			}
			catch
			{
				try
				{
					ipAddress = IPAddress.Parse(hostnameOrAddress);
				}
				catch
				{
					ipAddress = IPAddress.None;
				}
			}

			return ipAddress ?? IPAddress.None;
		}

		public static string ResolveHostnameFromIpAddress(string ipAddress)
		{
			string result = String.Empty;

			try
			{
				IPHostEntry ipHostEntry = Dns.GetHostEntry(ipAddress);
				result = ipHostEntry.AddressList.Length > 0 ? ipHostEntry.HostName : String.Empty;
			}
			catch
			{
				if (IpHelper.ValidateIpAddress(ipAddress))
					result = ipAddress;
			}

			return result;
		}
	}
}
