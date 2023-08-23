using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Serialization;

namespace Simple.Network
{
	public class IpAddress
	{
		private byte[]? ipAddressV4;
		private IPAddress? ipAddressV6;
		private string? ipAddressText;

		public IpAddress(byte[] address, uint scopeId = 0)
		{
			if (address.Length != 4 && address.Length != 16)
				throw new ArgumentException("Bad IP address data.");

			if (address.Length == 4)
			{
				this.ipAddressV4 = address;
				this.ipAddressV6 = null;
			}
			else if (address.Length == 16)
			{
				this.ipAddressV4 = null;
				this.ipAddressV6 = new IPAddress(address, scopeId);
			}
			else
			{
				throw new ArgumentException("Bad IP address data.");
			}

			this.ipAddressText = null;
		}

		public IpAddress(string iPv4Orv6AddressText)
		{
			var chars = iPv4Orv6AddressText.AsSpan();

			if (chars.Contains(':'))
			{
				this.ipAddressV4 = null;
				this.ipAddressV6 = IPAddress.Parse(chars);
			}
			else
			{
				this.ipAddressV4 = IpHelper.ParseIPv4(chars);
				this.ipAddressV6 = null;
			}

			this.ipAddressText = iPv4Orv6AddressText;
		}

		public IpAddress(ref SequenceReader reader)
		{
			bool isIpV4 = reader.ReadBoolean();
			int length = isIpV4 ? 4 : 16;
			byte[] ipAddressBytes = reader.ReadBinary(length).ToArray();

			if (isIpV4)
			{
				this.ipAddressV4 = ipAddressBytes;
			}
			else
			{
				uint scopeId = reader.ReadUInt32();

				this.ipAddressV6 = new IPAddress(ipAddressBytes, scopeId);
			}

			this.ipAddressText = null;
		}


		public static readonly IpAddress Any = new IpAddress(new byte[4] { 0, 0, 0, 0 });

		public static readonly IpAddress Loopback = new IpAddress(new byte[4] { 127, 0, 0, 1 });

		public static readonly IpAddress Broadcast = new IpAddress(new byte[4] { 255, 255, 255, 255 });

		public static readonly IpAddress None = Broadcast;

		public static readonly IpAddress IPv6Any = new IpAddress(new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

		public static readonly IpAddress IPv6Loopback = new IpAddress(new byte[16]	{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 });

		public static readonly IpAddress IPv6None = IPv6Any;

		public bool IsIPv4 => this.ipAddressV4 != null;
		public bool IsIPv6 => this.ipAddressV4 == null;

		public byte[] GetAddressBytes()
		{
			if (this.IsIPv4)
				return this.ipAddressV4!;
			else
				return this.ipAddressV6!.GetAddressBytes();
		}

		public long GetIpV6ScopeId() => this.ipAddressV6!.ScopeId;

		public void WriteTo(ref SequenceWriter writer)
		{
			byte[] ipAddressBytes = this.GetAddressBytes();

			writer.WriteBoolean(this.IsIPv4);
			writer.WriteBinary(ipAddressBytes);

			if (this.IsIPv6)
				writer.WriteUInt32((uint)this.GetIpV6ScopeId());
		}

		public override bool Equals(object? ipAddressObject) => (ipAddressObject is IpAddress ipAddress) ? this.Equals(ipAddress) : false;

		public bool Equals(IpAddress? ipAddress) => this == ipAddress;

		public static bool operator ==(IpAddress? a, IpAddress? b)
		{
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
				return true;

			// If one is null, but not both, return false.
			if ((object?)a == null ^ (object?)b == null)
				return false;

			if (a.IsIPv4)
			{
				if (!b.IsIPv4)
					return false;

				if (!a.ipAddressV4!.IsEqual(b.ipAddressV4))
					return false;
			}
			else
			{
				if (!b.IsIPv6)
					return false;

				if (!a.ipAddressV6!.Equals(b.ipAddressV6)) 
					return false;
			}

			return true;
		}

		public override int GetHashCode() => (this.IsIPv4) ? this.ipAddressV4!.GetHashCode() : this.ipAddressV6!.GetHashCode();

		public static bool operator !=(IpAddress? a, IpAddress? b) => !(a == b);


		public override string ToString()
		{
			if (this.ipAddressText == null)
			{
				if (this.ipAddressV4 != null)
					this.ipAddressText = IpHelper.GetIPv4AddressText(this.ipAddressV4);
				else
					this.ipAddressText = this.ipAddressV6!.ToString();
			}

			return this.ipAddressText;
		}
	}
}
