using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Simple.Collections;
using Simple.Serialization;
using Simple.Network;

namespace Simple.Network
{
	/// <summary>
	/// Collect ACL rule infos. The base is Cisco style extended ACL 
	/// 
	/// </summary>
	public class AclInfo
	{
		//private string ciscoAcl = null;
		//private string ciscoAclWithoutPortSimbols = null;
		//private bool isParsed = false;

		private byte protocol;
		//private string sourceIpAddressText; // <- remove this
		private IpAddress sourceIpAddress;
		private int sourceSubnetMaskPrefix = 0;
		private AclPortOperator sourcePortOperator = 0;
		private ushort sourcePort = 0;
		private ushort sourcePort2 = 0;
		//private string destinationIpAddressText; // <- remove this
		private IpAddress destinationIpAddress;
		private int destinationSubnetMaskPrefix = 0;
		private AclPortOperator destinationPortOperator = 0;
		private ushort destinationPort = 0;
		private ushort destinationPort2 = 0;
		private byte? icmpType = null;
		private byte? icmpTypeCode = null;
		private byte? dscp = null;
		private bool established = false;
		private bool logging = false;

		private string? aclText = null;
		private bool useSimbolsForPortAndCodes = false;


		public static Dictionary<byte, string> ProtocolKeywordsByProtocolNumber = new Dictionary<byte, string>();
		public static Dictionary<string, byte> ProtocolNumbersByProtocolKeyword = new Dictionary<string, byte>();

		public static Dictionary<ushort, string> TcpKeywordsByPortNumber = new Dictionary<ushort, string>();
		public static Dictionary<string, ushort> PortNumbersByTcpKeyword = new Dictionary<string, ushort>();

		public static Dictionary<ushort, string> UdpKeywordsByPortNumber = new Dictionary<ushort, string>();
		public static Dictionary<string, ushort> PortNumbersByUdpKeyword = new Dictionary<string, ushort>();

		//public static NullableDictionary<byte, string> IcmpMsgKeywordsByIcmpMsg = new NullableDictionary<byte, string>();
		//public static NullableDictionary<string, byte> IcmpMsgByIcmpMsgKeywords = new NullableDictionary<string, byte>();
		public static string?[,] IcmpMessageKeywordByIcmpTypeAndCode = new string?[256, 257]; // the last one code index 256 is for null value
		private static Dictionary<string, IcmpTypeAndCode> IcmpTypeAndCodesByIcmpMessage = new Dictionary<string, IcmpTypeAndCode>();

		public static NullableDictionary<byte?, string> DscpKeywordsByNumber = new NullableDictionary<byte?, string>();
		public static NullableDictionary<string, byte?> NumbersByDscpKeyword = new NullableDictionary<string, byte?>();

		//public static NullableDictionary<byte, string> IcmpMsgCodeKeywordsByIcmpMsgCode = new NullableDictionary<byte, string>();
		//public static NullableDictionary<string, byte> IcmpMsgCodeByIcmpMsgCodeKeywords = new NullableDictionary<string, byte>();

		public static Dictionary<byte, string> PortOperatorsByCode = new Dictionary<byte, string>();
		public static Dictionary<string, byte> CodesByPortOperator = new Dictionary<string, byte>();

		public static readonly AclInfo IpAnyAny;
		public static readonly AclInfo IpAnyAnyLog;

		/// <summary>
		/// Access Control Linst info contains all relevant data to generate and aplly single ACL
		/// </summary>
		/// <param name="protocol"></param>
		/// <param name="sourceIpAddress"></param>
		/// <param name="sourceSubnetMaskPrefix"></param>
		/// <param name="sourcePortOperator"></param>
		/// <param name="sourcePort"></param>
		/// <param name="sourcePort2"></param>
		/// <param name="destinationIpAddress"></param>
		/// <param name="destinationSubnetMaskPrefix"></param>
		/// <param name="destinationPortOperator"></param>
		/// <param name="destinationPort"></param>
		/// <param name="destinationPort2"></param>
		/// <param name="icmpType"></param>
		/// <param name="icmpTypeCode"></param>
		/// <param name="dscp"></param>
		/// <param name="established"></param>
		/// <param name="logging"></param>
		public AclInfo(byte protocol, IpAddress sourceIpAddress,	  byte sourceSubnetMaskPrefix,		AclPortOperator sourcePortOperator,		 ushort sourcePort,		 ushort sourcePort2,
									  IpAddress destinationIpAddress, byte destinationSubnetMaskPrefix, AclPortOperator destinationPortOperator, ushort destinationPort, ushort destinationPort2,
									  byte? icmpType, byte? icmpTypeCode, byte? dscp, bool established, bool logging)
		{
			this.protocol = protocol;
			this.sourceIpAddress = sourceIpAddress;
			//this.sourceIpAddressText = sourceIpAddress.ToString();
			this.sourceSubnetMaskPrefix = sourceSubnetMaskPrefix;
			this.sourcePortOperator = sourcePortOperator;
			this.sourcePort = sourcePort;
			this.sourcePort2 = sourcePort2;
			this.destinationIpAddress = destinationIpAddress;
			//this.destinationIpAddressText = destinationIpAddress.ToString();
			this.destinationSubnetMaskPrefix = destinationSubnetMaskPrefix;
			this.destinationPortOperator = destinationPortOperator;
			this.destinationPort = destinationPort;
			this.destinationPort2 = destinationPort2;
			this.icmpType = icmpType;
			this.icmpTypeCode = icmpTypeCode;
			this.dscp = dscp;
			this.established = established;
			this.logging = logging;

			this.aclText = null;
			this.useSimbolsForPortAndCodes = false;
			//this.isParsed = true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="aclText"></param>
		public AclInfo(string aclText)
		{
			string[] lineArray = aclText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			int index = 0;

			string? next = GetNext(lineArray, ref index);

			//if (next == "permit" || next == "deny")
			//	next = GetNext(lineArray, ref index); // skip permition if exists

			this.protocol = GetProtocolNumber(next!); // {gre | icmp | tcp | udp | ip | proto-num}

			// Source IP
			next = GetNext(lineArray, ref index); // {source-ip [wildcard] | host source - ip | any}

			if (next == "any")
			{
				this.sourceIpAddress = IpAddress.Any;
				this.sourceSubnetMaskPrefix = 0;
			}
			else if (next == "host")
			{
				next = GetNext(lineArray, ref index);
				this.sourceIpAddress = new IpAddress(next!);
				this.sourceSubnetMaskPrefix = 32;
			}
			else if (IpHelper.ValidateIpAddress(next))
			{
				this.sourceIpAddress = new IpAddress(next!);
				next = GetNext(lineArray, ref index);
				this.sourceSubnetMaskPrefix = (byte)IpHelper.GetSubnetPrefixByWildCard(next);
			}
			else
			{
				this.sourceIpAddress = IpAddress.None;
			}

			next = GetNext(lineArray, ref index);

			if (this.protocol == 6 || this.protocol == 17) // TCP or UDP
			{
				byte? portOperator = GetPortOperatorCode(next);

				if (portOperator != null)
				{
					this.sourcePortOperator = (AclPortOperator)portOperator;
					next = GetNext(lineArray, ref index);
					this.sourcePort = (this.protocol == 6) ? GetTcpPortNumber(next!) : GetUdpPortNumber(next!);

					if (this.sourcePortOperator == AclPortOperator.Range)
					{
						next = GetNext(lineArray, ref index);
						this.sourcePort2 = (this.protocol == 6) ? GetTcpPortNumber(next!) : GetUdpPortNumber(next!);
					}

					next = GetNext(lineArray, ref index);
				}
			}

			// Destination IP
			//next = lineArray[index++]; // {dest-ip [wildcard] | host dest - ip | any}

			if (next == "any")
			{
				this.destinationIpAddress = IpAddress.Any;
				this.destinationSubnetMaskPrefix = 0;
			}
			else if (next == "host")
			{
				next = GetNext(lineArray, ref index);
				this.destinationIpAddress = new IpAddress(next!);
				this.destinationSubnetMaskPrefix = 32;
			}
			else if (IpHelper.ValidateIpAddress(next))
			{
				this.destinationIpAddress = new IpAddress(next!);
				next = GetNext(lineArray, ref index);
				this.destinationSubnetMaskPrefix = (byte)IpHelper.GetSubnetPrefixByWildCard(next!);
			}
			else
			{
				this.destinationIpAddress = IpAddress.None;
			}

			next = GetNext(lineArray, ref index);

			if (this.protocol == 6 || this.protocol == 17) // TCP or UDP
			{
				byte? portOperator = GetPortOperatorCode(next);

				if (portOperator != null)
				{
					this.destinationPortOperator = (AclPortOperator)portOperator;
					next = GetNext(lineArray, ref index);
					this.destinationPort = (this.protocol == 6) ? GetTcpPortNumber(next!) : GetUdpPortNumber(next!);

					if (this.destinationPortOperator == AclPortOperator.Range)
					{
						next = GetNext(lineArray, ref index);
						this.destinationPort2 = (this.protocol == 6) ? GetTcpPortNumber(next!) : GetUdpPortNumber(next!);
					}

					next = GetNext(lineArray, ref index);
				}

				if (this.protocol == 6) // TCP
				{
					if (next == "established")
					{
						this.established = true;
						next = GetNext(lineArray, ref index);
					}
				}
			}
			else if (this.protocol == 1) // ICMP
			{
				if (next != null)
				{
					IcmpTypeAndCode icmpTypeAndCode;

					if (!IcmpTypeAndCodesByIcmpMessage.TryGetValue(next, out icmpTypeAndCode))
					{
						int icmpType; // try with icmp type and code numbers

						if (int.TryParse(next, out icmpType))
						{
							this.icmpType = (byte)icmpType;
							next = GetNext(lineArray, ref index);

							int icmpTypeCode;

							if (int.TryParse(next, out icmpTypeCode))
							{
								this.icmpTypeCode = (byte)icmpTypeCode;
								next = GetNext(lineArray, ref index);
							}
						}
					}
					else
					{
						this.icmpType = icmpTypeAndCode.IcmpType;
						this.icmpTypeCode = icmpTypeAndCode.IcmpCode;
						next = GetNext(lineArray, ref index);
					}
				}
			}

			if (next == "dscp" || next == "precedence" || next == "tos")
			{
				next = GetNext(lineArray, ref index);
				this.dscp = GetDscpNumber(next!);
				next = GetNext(lineArray, ref index);
			}

			if (next == "log")
				this.logging = true;

			this.aclText = aclText;
			this.useSimbolsForPortAndCodes = false;
		}

		public AclInfo(ref SequenceReader reader) //=> this.ReadFrom(ref reader);
		{
			this.protocol = reader.ReadByte();
			this.sourceIpAddress = new IpAddress(ref reader);
			this.sourceSubnetMaskPrefix = reader.ReadInt32Optimized();
			this.destinationIpAddress = new IpAddress(ref reader);
			this.destinationSubnetMaskPrefix = reader.ReadInt32Optimized();

			if (this.protocol == 6 || this.protocol == 17) // TCP or UDP
			{
				this.sourcePortOperator = (AclPortOperator)reader.ReadByte();

				if (this.sourcePortOperator != AclPortOperator.Any)
				{
					this.sourcePort = reader.ReadUInt16();

					if (this.sourcePortOperator == AclPortOperator.Range)
						this.sourcePort2 = reader.ReadUInt16();
				}

				this.destinationPortOperator = (AclPortOperator)reader.ReadByte();

				if (this.destinationPortOperator != AclPortOperator.Any)
				{
					this.destinationPort = reader.ReadUInt16();

					if (destinationPortOperator == AclPortOperator.Range)
						this.destinationPort2 = reader.ReadUInt16();
				}

				if (this.protocol == 6) // TCP
					this.established = reader.ReadBoolean();
			}
			else if (this.protocol == 1) // ICMP
			{
				this.icmpType = reader.ReadNullableByte();

				if (icmpType != null)
					this.icmpTypeCode = reader.ReadNullableByte();
			}

			this.dscp = reader.ReadNullableByte();
			this.logging = reader.ReadBoolean();
		}

		static AclInfo()
		{
			// Protocol Keywords
			// permit { gre | icmp | tcp | udp | ip | proto - num}
			AddProtocolKeyword(1, "icmp"); // Internet Control Message Protocol
			AddProtocolKeyword(6, "tcp");  // Transmission Control Protocol
			AddProtocolKeyword(17, "udp");  // User Datagram Protocol
			AddProtocolKeyword(4, "ip");   // IP-in-IP IPv4 encapsulation
			AddProtocolKeyword(51, "ahp");   // Authentication Header Protocol 
			AddProtocolKeyword(88, "eigrp");   // Cisco's EIGRP routing protocol
			AddProtocolKeyword(50, "esp");   // Encapsulation Security Payload
			AddProtocolKeyword(47, "gre");   // Cisco's GRE tunneling
			AddProtocolKeyword(2, "igmp");   // Internet Gateway Message Protocol
			AddProtocolKeyword(89, "ospf");   // OSPF routing protocol
			AddProtocolKeyword(108, "pcp");   // Payload Compression Protocol
			AddProtocolKeyword(103, "pim");   // Protocol Independent Multicast
			AddProtocolKeyword(132, "sctp");   // Stream Control Transmission Protocol

			// TCP port keywords
			//AddTcpKeyword(null, "any");
			AddTcpKeyword(19, "chargen"); // Character generator
			AddTcpKeyword(53, "domain"); // Domain Name Service
			AddTcpKeyword(13, "daytime"); // Daytime
			AddTcpKeyword(179, "bgp"); // Border Gateway Protocol
			AddTcpKeyword(514, "cmd"); // Remote commands
			AddTcpKeyword(9, "discard"); // Discard
			AddTcpKeyword(512, "exec"); // Exec(rsh)
			AddTcpKeyword(21, "ftp"); // File Transfer Protocol
			AddTcpKeyword(20, "ftp-data"); // FTP data connections
			AddTcpKeyword(443, "https");
			AddTcpKeyword(1755, "mms");
			AddTcpKeyword(2049, "nfs");
			AddTcpKeyword(22, "ssh");
			AddTcpKeyword(49, "tacacs"); // TAC Access Control System
			AddTcpKeyword(23, "telnet"); // Telnet
			AddTcpKeyword(80, "www"); // World Wide Web(HTTP)
			AddTcpKeyword(3949, "drip"); // Dynamic Routing Information Protocol
			AddTcpKeyword(7, "echo"); // Echo
			AddTcpKeyword(79, "finger"); // Finger
			AddTcpKeyword(70, "gopher"); // Gopher
			AddTcpKeyword(101, "hostname"); //hostname NIC hostname server
			AddTcpKeyword(113, "ident"); // Ident Protocol
			AddTcpKeyword(194, "irc"); // Internet Relay Chat
			AddTcpKeyword(543, "klogin"); // Kerberos login
			AddTcpKeyword(544, "kshell"); // Kerberos shell
			AddTcpKeyword(513, "login"); // Login(rlogin)
			AddTcpKeyword(515, "lpd"); // Printer service
			AddTcpKeyword(119, "nntp"); // Network News Transport Protocol
			AddTcpKeyword(15001, "onep-plain"); // ONEP Cleartext
			AddTcpKeyword(15002, "onep-tls"); // ONEP TLS
			AddTcpKeyword(496, "pim-auto-rp"); //  PIM Auto-RP
			AddTcpKeyword(109, "pop2"); // Post Office Protocol v2
			AddTcpKeyword(110, "pop3"); // Post Office Protocol v3
			AddTcpKeyword(25, "smtp"); // Simple Mail Transport Protocol
			AddTcpKeyword(111, "sunrpc"); // Sun Remote Procedure Call
			AddTcpKeyword(517, "talk"); // Talk
			AddTcpKeyword(37, "time"); // Time
			AddTcpKeyword(540, "uucp"); // Unix-to - Unix Copy Program
			AddTcpKeyword(43, "whois"); // Nicname(43)

			// UDP port keywords
			//AddUdpKeyword(null, "any");
			AddUdpKeyword(68, "bootpc");
			AddUdpKeyword(67, "bootps");
			AddUdpKeyword(53, "domain"); // Domain Name Service(DNS)
			AddUdpKeyword(1755, "mms");
			AddUdpKeyword(138, "netbios-dgm"); // NetBios datagram service
			AddUdpKeyword(137, "netbios-ns"); // NetBios name service
			AddUdpKeyword(139, "netbios-ss"); // NetBios session service
			AddUdpKeyword(2049, "nfs");
			AddUdpKeyword(123, "ntp"); // Network Time Protocol
			AddUdpKeyword(161, "snmp"); // Simple Network Management Protocol
			AddUdpKeyword(162, "snmptrap"); // SNMP Traps
			AddUdpKeyword(49, "tacacs"); // TAC Access Control System
			AddUdpKeyword(69, "tftp"); // Trivial File Transfer Protocol
			AddUdpKeyword(2048, "wccp");
			AddUdpKeyword(512, "biff"); //	biff Biff(mail notification, comsat)
			AddUdpKeyword(9, "discard"); // Discard
			AddUdpKeyword(195, "dnsix"); // DNSIX security protocol auditing
			AddUdpKeyword(7, "echo"); // Echo
			AddUdpKeyword(500, "isakmp"); // Internet Security Association and Key Management Protocol
			AddUdpKeyword(434, "mobile-ip"); // Mobile IP registration
			AddUdpKeyword(42, "nameserver"); // IEN116 name service(obsolete)
			AddUdpKeyword(4500, "non500-isakmp"); // Internet Security Association and Key Management Protocol
			AddUdpKeyword(496, "pim-auto-rp"); // PIM Auto-RP
			AddUdpKeyword(520, "rip"); // Routing Information Protocol(router, in.routed)
			AddUdpKeyword(111, "sunrpc"); // Sun Remote Procedure Call
			AddUdpKeyword(514, "syslog"); // System Logger
			AddUdpKeyword(517, "talk"); // Talk
			AddUdpKeyword(37, "time"); // Time
			AddUdpKeyword(513, "who"); // Who service(rwho)
			AddUdpKeyword(177, "xdmcp"); // X Display Manager Control Protocol

			// ICMP message keywords as combination of ICMP message type and code types, as expressed by the keywords (Cisco extended ACL)
			SetIcmpMessageKeywords("administratively-prohibited", 3, 13);
			SetIcmpMessageKeywords("alternate-address", 6, null);
			SetIcmpMessageKeywords("conversion-error", 31, null);
			SetIcmpMessageKeywords("dod-host-prohibited", 3, 10);
			SetIcmpMessageKeywords("dod-net-prohibited", 3, 9);
			SetIcmpMessageKeywords("echo", 8, null);
			SetIcmpMessageKeywords("echo-reply", 0, null);
			SetIcmpMessageKeywords("general-parameter-problem", 3, 8);
			SetIcmpMessageKeywords("host-isolated", 3, 8);
			SetIcmpMessageKeywords("host-precedence-unreachable", 3, 14);
			SetIcmpMessageKeywords("host-redirect", 5, 1); // Check is maybe 32-MobileHostRedirect
			SetIcmpMessageKeywords("host-tos-redirect", 5, 3);
			SetIcmpMessageKeywords("host-tos-unreachable", 3, 12);
			SetIcmpMessageKeywords("host-unknown", 3, 7);
			SetIcmpMessageKeywords("host-unreachable", 3, 1);
			SetIcmpMessageKeywords("information-reply", 16, null);
			SetIcmpMessageKeywords("information-request", 15, null);
			SetIcmpMessageKeywords("mask-reply", 18, null);
			SetIcmpMessageKeywords("mask-request", 17, null);
			SetIcmpMessageKeywords("mobile-redirect", 32, null);
			SetIcmpMessageKeywords("net-redirect", 5, 0);
			SetIcmpMessageKeywords("net-tos-redirect", 5, 2);
			SetIcmpMessageKeywords("net-tos-unreachable", 3, 11);
			SetIcmpMessageKeywords("net-unreachable", 3, 0);
			SetIcmpMessageKeywords("network-unknown", 3, 6);
			SetIcmpMessageKeywords("no-room-for-option", 12, 1); // Need to be checked
			SetIcmpMessageKeywords("option-missing", 12, 1);
			SetIcmpMessageKeywords("packet-too-big", 2, null); // Need to be checked - type 2 is not present (unassigned)
			SetIcmpMessageKeywords("parameter-problem", 12, null);
			SetIcmpMessageKeywords("port-unreachable", 3, 3);
			SetIcmpMessageKeywords("precedence-unreachable", 3, 15); // Need check
			SetIcmpMessageKeywords("protocol-unreachable", 3, 2);
			SetIcmpMessageKeywords("reassembly-timeout", 11, 1);
			SetIcmpMessageKeywords("redirect", 5, null);
			SetIcmpMessageKeywords("router-advertisement", 9, null);
			SetIcmpMessageKeywords("router-solicitation", 9, null);
			SetIcmpMessageKeywords("source-quench", 4, null);
			SetIcmpMessageKeywords("source-route-failed", 3, 5);
			SetIcmpMessageKeywords("time-exceeded", 11, null);
			SetIcmpMessageKeywords("timestamp-reply", 14, null);
			SetIcmpMessageKeywords("timestamp-request", 13, null);
			SetIcmpMessageKeywords("traceroute", 30, null);
			SetIcmpMessageKeywords("ttl-exceeded", 11, 0);
			SetIcmpMessageKeywords("unreachable", 3, null);

			// Port operators
			AddPortOperator(0, "any");
			AddPortOperator(1, "eq");
			AddPortOperator(2, "ne");
			AddPortOperator(3, "gt");
			AddPortOperator(4, "lt");
			AddPortOperator(5, "range");

			// DSCP keywords
			AddDscpKeyword("Any", null);
			AddDscpKeyword("CS0", 0);
			AddDscpKeyword("CS1", 8);
			AddDscpKeyword("CS2", 16);
			AddDscpKeyword("CS3", 24);
			AddDscpKeyword("CS4", 32);
			AddDscpKeyword("CS5", 40);
			AddDscpKeyword("CS6", 48);
			AddDscpKeyword("CS7", 56);
			AddDscpKeyword("AF11", 10);
			AddDscpKeyword("AF12", 12);
			AddDscpKeyword("AF13", 14);
			AddDscpKeyword("AF21", 18);
			AddDscpKeyword("AF22", 20);
			AddDscpKeyword("AF23", 22);
			AddDscpKeyword("AF31", 26);
			AddDscpKeyword("AF32", 28);
			AddDscpKeyword("AF33", 30);
			AddDscpKeyword("AF41", 34);
			AddDscpKeyword("AF42", 36);
			AddDscpKeyword("AF43", 38);

			IpAnyAny = new AclInfo("ip any any");
			IpAnyAnyLog = new AclInfo("ip any any log");
		}

		public void WriteTo(ref SequenceWriter writer)
		{
			writer.WriteByte(this.protocol);
			this.sourceIpAddress.WriteTo(ref writer);
			writer.WriteInt32Optimized(this.sourceSubnetMaskPrefix);
			this.destinationIpAddress.WriteTo(ref writer);
			writer.WriteInt32Optimized(this.destinationSubnetMaskPrefix);

			if (this.protocol == 6 || this.protocol == 17) // TCP or UDP
			{
				writer.WriteByte((byte)this.sourcePortOperator);

				if (this.sourcePortOperator != AclPortOperator.Any)
				{
					writer.WriteUInt16(this.sourcePort!);

					if (this.sourcePortOperator == AclPortOperator.Range)
						writer.WriteUInt16(this.sourcePort2);
				}

				writer.WriteByte((byte)this.destinationPortOperator);

				if (this.destinationPortOperator != AclPortOperator.Any)
				{
					writer.WriteUInt16(this.destinationPort);

					if (this.destinationPortOperator == AclPortOperator.Range)
						writer.WriteUInt16(this.destinationPort2);
				}

				if (this.protocol == 6) // TCP
					writer.WriteBoolean(this.established);
			}
			else if (this.protocol == 1) // ICMP
			{
				writer.WriteNullableByte(this.icmpType);

				if (this.icmpType != null)
					writer.WriteNullableByte(this.icmpTypeCode);
			}

			writer.WriteNullableByte(this.dscp);
			writer.WriteBoolean(this.logging);
		}


		// *** TEST ***

		//string acl = "permit tcp any any eq www";
		//AclRuleInfo aclRuleInfo = new AclRuleInfo(acl);
		//string aclX = aclRuleInfo.ToCiscoAcl();

		//string acl2 = "permit tcp host 10.1.1.5 any eq ssh";
		//AclRuleInfo aclRuleInfo2 = new AclRuleInfo(acl2);
		//string acl2X = aclRuleInfo2.ToCiscoAcl();

		public byte Protocol => this.protocol;

		public IpAddress SourceIpAddress => this.sourceIpAddress;

		public int SourceSubnetMaskPrefix => this.sourceSubnetMaskPrefix;

		public AclPortOperator SourcePortOperator => this.sourcePortOperator;

		public ushort SourcePort => this.sourcePort;

		public ushort SourcePort2 => this.sourcePort2;

		public int DestinationSubnetMaskPrefix => this.destinationSubnetMaskPrefix;

		public IpAddress DestinationIpAddress => this.destinationIpAddress;

		public AclPortOperator DestinationPortOperator => this.destinationPortOperator;

		public ushort DestinationPort => this.destinationPort;

		public ushort DestinationPort2 => this.destinationPort2;

		public byte? IcmpType => this.icmpType;

		public byte? IcmpTypeCode => this.icmpTypeCode;

		public byte? Dscp => this.dscp;

		public bool Established => this.established;

		public bool Logging => this.logging;


		public override bool Equals(object? obj) => this.Equals((AclInfo)obj);
		public override int GetHashCode() => base.GetHashCode();

		public bool Equals(AclInfo? aclRuleInfo) => this == aclRuleInfo;

		public static bool operator ==(AclInfo? a, AclInfo? b)
		{
			int i = 0;

			if (a.ToString().Trim() == "tcp 192.168.0.0 0.0.0.255 host 192.168.1.2" && b.ToString().Trim() == "tcp 192.168.0.0 0.0.0.255 host 192.168.1.2")
				i = 1;
			

		
			// If both are null, or both are same instance, return true.
			if (System.Object.ReferenceEquals(a, b))
				return true;

			// If one is null, but not both, return false.
			if ((object?)a == null ^ (object?)b == null)
				return false;

			// Return true if the key fields match.

			if (a.Protocol != b.Protocol) // || a.Permit != b.Permit
				return false;

			if (a.Protocol == 6 || a.Protocol == 17) // TCP or UDP
			{
				if (!a.SourceIpAddress.Equals(b.SourceIpAddress) || a.SourceSubnetMaskPrefix != b.SourceSubnetMaskPrefix || !a.DestinationIpAddress.Equals(b.DestinationIpAddress) || a.DestinationSubnetMaskPrefix != b.DestinationSubnetMaskPrefix)
					return false;

				if (a.SourcePortOperator == AclPortOperator.Any)
				{
					if (a.SourcePortOperator != AclPortOperator.Any)
						return false;
				}
				else if (a.SourcePortOperator != b.SourcePortOperator)
				{
					return false;
				}
				else if (a.SourcePort != b.SourcePort)
				{
					return false;
				}
				else if (a.SourcePortOperator == AclPortOperator.Range && a.SourcePort2 != b.SourcePort2)
				{
					return false;
				}

				if (a.DestinationPortOperator == AclPortOperator.Any)
				{
					if (b.DestinationPortOperator != AclPortOperator.Any)
						return false;
				}
				else if (a.DestinationPortOperator != b.DestinationPortOperator)
				{
					return false;
				}
				else if (a.DestinationPort != b.DestinationPort)
				{
					return false;
				}
				else if (a.DestinationPortOperator == AclPortOperator.Range && a.DestinationPort2 != b.DestinationPort2)
				{
					return false;
				}

				if (a.Protocol == 6 && a.Established != b.Established) // TCP
					return false;
			}
			else if (a.Protocol == 1) // ICMP
			{
				if (a.IcmpType != b.IcmpType)
					return false;

				if (a.IcmpType != null && a.IcmpTypeCode != b.IcmpTypeCode)
					return false;
			}

			if (a.Dscp != b.Dscp || a.Logging != b.Logging)
				return false;

			return true;
		}

		public static bool operator !=(AclInfo? a, AclInfo? b) => !(a == b);


		//private static IcmpTypeAndCode? GetIcmpTypeAndCode(string icmpMessage)
		//{
		//	IcmpTypeAndCode result;

		//	if (!IcmpTypeAndCodesByIcmpMessage.TryGetValue(icmpMessage, out result))
		//	{
		//		// Try parse if string is consist of one or two numbers (type and code optional)

		//		TODO:

		//		return null;
		//	}

		//	return result;
		//}



		public override string ToString() => this.ToCiscoAcl();

		public string ToString(bool useSimbolsForPortAndCodes = true) => this.ToCiscoAcl(useSimbolsForPortAndCodes);


		public string ToCiscoAcl(bool useSimbolsForPortAndCodes = true)
		{
			if (this.aclText != null && this.useSimbolsForPortAndCodes == useSimbolsForPortAndCodes)
				return this.aclText;

			this.useSimbolsForPortAndCodes = useSimbolsForPortAndCodes;

			StringBuilder stringBuilder = new StringBuilder();
			//string result = String.Empty; // (this.permit) ? "permit" : "deny";

			//result += " ";
			//  ICMP                    IP                   TCP                    UDP (only this simbols allowed when not using simbols for ports and codes)
			//if (useSimbolsForPortAndCodes || this.protocol == 1 || this.protocol == 4 || this.protocol == 6 || this.protocol == 17)
			//	result += GetProtocolKeyword(this.protocol);
			//else
			//	result += this.protocol.ToString();

			stringBuilder.Append(GetProtocolKeyword(this.Protocol));
			stringBuilder.Append(" " + GetIpAddressWildCardKeywords(this.SourceIpAddress.ToString(), this.SourceSubnetMaskPrefix));

			if (this.Protocol == 6 || this.Protocol == 17) // TCP or UDP
			{
				if (this.SourcePortOperator != AclPortOperator.Any)
				{
					stringBuilder.Append(" " + GetPortOperatorKeyword((byte)this.SourcePortOperator) + " ");

					if (useSimbolsForPortAndCodes)
						stringBuilder.Append((this.Protocol == 6) ? GetTcpPortKeyword(this.SourcePort) : GetUdpPortKeyword(this.SourcePort));
					else
						stringBuilder.Append(this.SourcePort.ToString());

					if (this.SourcePortOperator == AclPortOperator.Range)
						if (useSimbolsForPortAndCodes)
							stringBuilder.Append(" " + ((this.Protocol == 6) ? GetTcpPortKeyword(this.SourcePort2) : GetUdpPortKeyword(this.SourcePort2)));
						else
							stringBuilder.Append(" " + this.SourcePort2.ToString());
				}
			}

			stringBuilder.Append(" " + GetIpAddressWildCardKeywords(this.DestinationIpAddress.ToString(), this.DestinationSubnetMaskPrefix));

			if (this.Protocol == 6 || this.Protocol == 17) // TCP or UDP
			{
				if (this.DestinationPortOperator != AclPortOperator.Any)
				{
					stringBuilder.Append(" " + GetPortOperatorKeyword((byte)this.DestinationPortOperator) + " ");

					if (useSimbolsForPortAndCodes)
						stringBuilder.Append((this.Protocol == 6) ? GetTcpPortKeyword(this.DestinationPort) : GetUdpPortKeyword(this.DestinationPort));
					else
						stringBuilder.Append(this.DestinationPort.ToString());

					if (this.DestinationPortOperator == AclPortOperator.Range)
						if (useSimbolsForPortAndCodes)
							stringBuilder.Append(" " + ((this.Protocol == 6) ? GetTcpPortKeyword(this.DestinationPort2) : GetUdpPortKeyword(this.DestinationPort2)));
						else
							stringBuilder.Append(" " + this.DestinationPort2.ToString());
				}

				if (this.Protocol == 6 && this.Established)
					stringBuilder.Append(" established");
			}
			else if (this.Protocol == 1) // ICMP
			{
				string icmpMessage = GetIcmpMessage(this.IcmpType, this.IcmpTypeCode);

				if (icmpMessage.Length > 0)
					stringBuilder.Append(" " + icmpMessage);
			}

			if (this.Dscp != null)
				stringBuilder.Append(" dsxp " + this.Dscp.ToString()); // " precedance"

			if (this.Logging)
				stringBuilder.Append(" log");

			this.aclText = stringBuilder.ToString();

			return this.aclText;
		}

		private static string? GetNext(string[] lineArray, ref int index)
		{
			if (index < lineArray.Length)
				return lineArray[index++];

			return null;
		}

		public static byte GetProtocolNumber(string protoText) // permit {gre | icmp | tcp | udp | ip | proto-num}
		{
			byte result;

			if (!ProtocolNumbersByProtocolKeyword.TryGetValue(protoText, out result))
				result = Conversion.TryChangeType<byte>(protoText);

			return result;
		}

		public static string GetProtocolKeyword(byte protocol) // permit {gre | icmp | tcp | udp | ip | proto-num}
		{
			string result = String.Empty;

			if (!ProtocolKeywordsByProtocolNumber.TryGetValue(protocol, out result!))
				result = protocol.ToString();

			return result;
		}

		public static ushort GetTcpPortNumber(string tcpPortKeyword)
		{
			ushort result;

			if (!PortNumbersByTcpKeyword.TryGetValue(tcpPortKeyword, out result))
				result = Conversion.TryChangeType<ushort>(tcpPortKeyword);

			return result;
		}

		public static string GetTcpPortKeyword(ushort tcpPort)
		{
			string result;

			if (!TcpKeywordsByPortNumber.TryGetValue(tcpPort, out result!))
				result = tcpPort.ToString();

			return result;
		}

		public static ushort GetUdpPortNumber(string udpPortKeyword)
		{
			ushort result;

			if (!PortNumbersByUdpKeyword.TryGetValue(udpPortKeyword, out result))
				result = Conversion.TryChangeType<ushort>(udpPortKeyword);

			return result;
		}

		public static string GetUdpPortKeyword(ushort udpPort)
		{
			string result;

			if (!UdpKeywordsByPortNumber.TryGetValue(udpPort, out result))
				result = udpPort.ToString();

			return result;
		}

		public static byte? GetPortOperatorCode(string? portOperatorKeyword)
		{
			if (portOperatorKeyword is null)
				return null;
			
			byte result;

			if (!CodesByPortOperator.TryGetValue(portOperatorKeyword, out result))
				return null;

			return result;
		}

		public static string GetPortOperatorKeyword(byte portOperatorCode)
		{
			string result;

			if (!PortOperatorsByCode.TryGetValue(portOperatorCode, out result!))
				result = String.Empty;

			return result;
		}

		public static string GetIpAddressWildCardKeywords(string ipAddress, int ipMaskNumOfBits)
		{
			string result;

			if (ipMaskNumOfBits == 0)
				result = "any";
			else if (ipMaskNumOfBits == 32)
				result = "host " + ipAddress;
			else
				result = ipAddress + " " + IpHelper.GetSubnetMaskWildCard(ipMaskNumOfBits);

			return result;
		}

		public static string GetIcmpMessage(byte? icmpType, byte? icmpCode)
		{
			if (icmpType is null)
			{
				return String.Empty;
			}
			else
			{
				int icmpCodeValue = icmpCode ?? 256;
				string? result = IcmpMessageKeywordByIcmpTypeAndCode[(byte)icmpType, icmpCodeValue];

				if (result is null)
				{
					result = icmpType.ToString();

					if (icmpCode != null)
						result += " " + icmpCode.ToString();
				}

				return result;
			}
		}

		public static string GetDscpKeyword(byte? dscpNumber)
		{
			string result;

			if (!DscpKeywordsByNumber.TryGetValue(dscpNumber, out result))
				result = dscpNumber.ToString() ?? String.Empty;

			return result;
		}

		public static byte? GetDscpNumber(string dscpKeyword)
		{
			byte? result;

			if (!NumbersByDscpKeyword.TryGetValue(dscpKeyword, out result))
				result = Conversion.TryChangeType<byte>(dscpKeyword);

			return result;
		}

		private static void AddProtocolKeyword(byte protocol, string protocolWord)
		{
			ProtocolKeywordsByProtocolNumber.Add(protocol, protocolWord);
			ProtocolNumbersByProtocolKeyword.Add(protocolWord, protocol);
		}

		private static void AddTcpKeyword(ushort port, string tcpKeyword)
		{
			TcpKeywordsByPortNumber.Add(port, tcpKeyword);
			PortNumbersByTcpKeyword.Add(tcpKeyword, port);
		}

		private static void AddUdpKeyword(ushort port, string udpKeyword)
		{
			UdpKeywordsByPortNumber.Add(port, udpKeyword);
			PortNumbersByUdpKeyword.Add(udpKeyword, port);
		}

		private static void AddPortOperator(byte code, string portOperator)
		{
			PortOperatorsByCode.Add(code, portOperator);
			CodesByPortOperator.Add(portOperator, code);
		}

		private static void AddDscpKeyword(string keyword, byte? number)
		{
			DscpKeywordsByNumber.Add(number, keyword);
			NumbersByDscpKeyword.Add(keyword, number);
		}

		//private static void AddIcmpKeyword(byte icmpMsgType, string icmpTypeKeyword)
		//{
		//	IcmpMsgKeywordsByIcmpMsg.Add(icmpMsgType, icmpTypeKeyword);
		//	IcmpMsgByIcmpMsgKeywords.Add(icmpTypeKeyword, icmpMsgType);
		//}

		private static void SetIcmpMessageKeywords(string icmpMessage, byte icmpType, byte? icmpCode)
		{
			int icmpCodeIndex = icmpCode ?? 256; // (icmpCode is null) ? 256 : (byte)icmpCode;

			IcmpTypeAndCodesByIcmpMessage.Add(icmpMessage, new IcmpTypeAndCode(icmpType, icmpCode));
			IcmpMessageKeywordByIcmpTypeAndCode[icmpType, icmpCodeIndex] = icmpMessage;
		}
	}

	public enum AclDirection
	{
		In = 0,
		Out = 1
	}

	public enum AclPermission
	{
		Permit,
		Deny
	}

	public enum AclPortOperator
	{
		Any = 0,
		Equal = 1,
		NotEqual = 2,
		GreaterThan = 3,
		LessThan = 4,
		Range = 5
	}
}
