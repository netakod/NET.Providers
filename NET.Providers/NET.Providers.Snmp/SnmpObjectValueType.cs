using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace NET.Tools.Snmp
{
	/// <summary>
	///  The ASN.1 object value type codes. The values are specified in RFC1213 as a subset of ASN.1
	/// </summary>
	public enum SnmpObjectValueType
	{
		//Integer = 2,
		//OctetString = 4,
		//Null = 5,
		//ObjectId = 6,
		//IPAddress = 64,
		//Counter32 = 65,
		//Gauge32 = 66,
		//TimeTicks = 67,
		//Opaque = 68,
		//NSAP = 69,
		//Counter64 = 70,
		//UnsignedInteger32 = 71,
		//NoSuchObject = 128,
		//NoSuchInstance = 129,
		//EndOfMibView = 130,

		EndMarker = 0x00,

		/// <summary>
		/// INTEGER type. (SMIv1, SMIv2)
		/// </summary>
		Integer32 = 0x02,

		/// <summary>
		/// OCTET STRING type.
		/// </summary>
		OctetString = 0x04, // X690.OctetString

		/// <summary>
		/// NULL type. (SMIv1)
		/// </summary>
		Null = 0x05,

		/// <summary>
		/// OBJECT IDENTIFIER type. (SMIv1)
		/// </summary>
		ObjectIdentifier = 0x06,

		/// <summary>
		/// RFC1213 sequence for whole SNMP packet beginning
		/// </summary>
		Sequence = 0x30,  // RFC1213 sequence for whole SNMP packet beginning

		/// <summary>
		/// IpAddress type. (SMIv1)
		/// </summary>
		[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
		IPAddress = 0x40,

		/// <summary>
		/// Counter32 type. (SMIv1, SMIv2)
		/// </summary>
		Counter32 = 0x41,

		/// <summary>
		/// Gauge32 type. (SMIv1, SMIv2)
		/// </summary>
		Gauge32 = 0x42,

		/// <summary>
		/// TimeTicks type. (SMIv1)
		/// </summary>
		TimeTicks = 0x43,

		/// <summary>
		/// Opaque type. (SMIv1)
		/// </summary>
		Opaque = 0x44,

		/// <summary>
		/// Network Address. (SMIv1)
		/// </summary>
		NetAddress = 0x45,

		/// <summary>
		/// Counter64 type. (SMIv2)
		/// </summary>
		Counter64 = 0x46,

		/// <summary>
		/// Unsigned Integer32.
		/// </summary>
		UnsignedInteger32 = 0x47,

		/// <summary>
		/// No such object exception.
		/// </summary>
		NoSuchObject = 0x80,

		/// <summary>
		/// No such instance exception.
		/// </summary>
		NoSuchInstance = 0x81,

		/// <summary>
		/// End of MIB view exception.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Mib")]
		EndOfMibView = 0x82,

		/// <summary>
		/// Get request PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		GetRequestPdu = 0xA0,

		/// <summary>
		/// Get Next request PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		GetNextRequestPdu = 0xA1,

		/// <summary>
		/// Response PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		ResponsePdu = 0xA2,

		/// <summary>
		/// Set request PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		SetRequestPdu = 0xA3,

		/// <summary>
		/// Trap v1 PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		TrapV1Pdu = 0xA4,

		/// <summary>
		/// Get Bulk PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		GetBulkRequestPdu = 0xA5,

		/// <summary>
		/// Inform PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		InformRequestPdu = 0xA6,

		/// <summary>
		/// Trap v2 PDU.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		TrapV2Pdu = 0xA7,

		/// <summary>
		/// Report PDU. SNMP v3.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Pdu")]
		ReportPdu = 0xA8,

		/// <summary>
		/// Defined by #SNMP for unknown type.
		/// </summary>
		Unknown = 0xFFFF
	}
}
