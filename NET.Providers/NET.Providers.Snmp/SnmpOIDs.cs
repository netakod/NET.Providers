using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Tools.Snmp
{
    public struct SnmpOIDs
    {
        public struct System
        {
            public static readonly string system         = "1.3.6.1.2.1.1";
            public static readonly string sysDescription = "1.3.6.1.2.1.1.1";
            public static readonly string sysObjectID    = "1.3.6.1.2.1.1.2";
            public static readonly string sysUpTime      = "1.3.6.1.2.1.1.3";
            public static readonly string sysContact     = "1.3.6.1.2.1.1.4";
            public static readonly string sysName        = "1.3.6.1.2.1.1.5";
            public static readonly string sysLocation    = "1.3.6.1.2.1.1.6";
        }

        public struct Interfaces
        {
            public static readonly string ifNumber          = "1.3.6.1.2.1.2.1";
            public static readonly string ifTable           = "1.3.6.1.2.1.2.2";
            public static readonly string ifEntry           = "1.3.6.1.2.1.2.2.1";
            public static readonly string ifIndex           = "1.3.6.1.2.1.2.2.1.1";
            public static readonly string ifDescr           = "1.3.6.1.2.1.2.2.1.2";
            public static readonly string ifType            = "1.3.6.1.2.1.2.2.1.3";
            public static readonly string ifMtu             = "1.3.6.1.2.1.2.2.1.4";
            public static readonly string ifSpeed           = "1.3.6.1.2.1.2.2.1.5";
            public static readonly string ifPhysAddress     = "1.3.6.1.2.1.2.2.1.6";
            public static readonly string ifAdminStatus     = "1.3.6.1.2.1.2.2.1.7";
            public static readonly string ifOperStatus      = "1.3.6.1.2.1.2.2.1.8";
            public static readonly string ifLastChange      = "1.3.6.1.2.1.2.2.1.9";
            public static readonly string ifInOctets        = "1.3.6.1.2.1.2.2.1.10";
            public static readonly string ifInUcastPkts     = "1.3.6.1.2.1.2.2.1.11";
            public static readonly string ifInNUcastPkts    = "1.3.6.1.2.1.2.2.1.12";
            public static readonly string ifInDiscards      = "1.3.6.1.2.1.2.2.1.13";
            public static readonly string ifInErrors        = "1.3.6.1.2.1.2.2.1.14";
            public static readonly string ifInUnknownProtos = "1.3.6.1.2.1.2.2.1.15";
            public static readonly string ifOutOctets       = "1.3.6.1.2.1.2.2.1.16";
            public static readonly string ifOutUcastPkts    = "1.3.6.1.2.1.2.2.1.17";
            public static readonly string ifOutNUcastPkts   = "1.3.6.1.2.1.2.2.1.18";
            public static readonly string ifOutDiscards     = "1.3.6.1.2.1.2.2.1.19";
            public static readonly string ifOutErrors       = "1.3.6.1.2.1.2.2.1.20";
            public static readonly string ifOutQLen         = "1.3.6.1.2.1.2.2.1.21";
            public static readonly string ifSpecific        = "1.3.6.1.2.1.2.2.1.22";
        }

        public struct Tcp
        {
            public static readonly string tcpConnTable        = "1.3.6.1.2.1.6.13";
            public static readonly string tcpConnEntry        = "1.3.6.1.2.1.6.13.1";
            public static readonly string tcpConnState        = "1.3.6.1.2.1.6.13.1.1";
            public static readonly string tcpConnLocalAddress = "1.3.6.1.2.1.6.13.1.2";
            public static readonly string tcpConnLocalPort    = "1.3.6.1.2.1.6.13.1.3";
            public static readonly string tcpConnRemAddress   = "1.3.6.1.2.1.6.13.1.4";
            public static readonly string tcpConnRemPort      = "1.3.6.1.2.1.6.13.1.5";
        }

        public struct Udp
        {
            public static readonly string udpTable        = "1.3.6.1.2.1.7.5";
            public static readonly string udpEntry        = "1.3.6.1.2.1.7.5.1";
            public static readonly string udpLocalAddress = "1.3.6.1.2.1.7.5.1.1";
            public static readonly string udpLocalPort    = "1.3.6.1.2.1.7.5.1.2";
        }

        public struct Ip
        {
            public static readonly string ipAddrTable         = "1.3.6.1.2.1.4.20";
            public static readonly string ipAddrEntry         = "1.3.6.1.2.1.4.20.1";
            public static readonly string ipAdEntAddr         = "1.3.6.1.2.1.4.20.1.1";
            public static readonly string ipAdEntIfIndex      = "1.3.6.1.2.1.4.20.1.2";
            public static readonly string ipAdEntNetMask      = "1.3.6.1.2.1.4.20.1.3";
            public static readonly string ipAdEntBcastAddr    = "1.3.6.1.2.1.4.20.1.4";
            public static readonly string ipAdEntReasmMaxSize = "1.3.6.1.2.1.4.20.1.5";

            public static readonly string ipRouteTable   = "1.3.6.1.2.1.4.21";
            public static readonly string ipRouteEntry   = "1.3.6.1.2.1.4.21.1";
            public static readonly string ipRouteDest    = "1.3.6.1.2.1.4.21.1.1";
            public static readonly string ipRouteIfIndex = "1.3.6.1.2.1.4.21.1.2";
            public static readonly string ipRouteMetric1 = "1.3.6.1.2.1.4.21.1.3";
            public static readonly string ipRouteMetric2 = "1.3.6.1.2.1.4.21.1.4";
            public static readonly string ipRouteMetric3 = "1.3.6.1.2.1.4.21.1.5";
            public static readonly string ipRouteMetric4 = "1.3.6.1.2.1.4.21.1.6";
            public static readonly string ipRouteNextHop = "1.3.6.1.2.1.4.21.1.7";
            public static readonly string ipRouteType    = "1.3.6.1.2.1.4.21.1.8";
            public static readonly string ipRouteProto   = "1.3.6.1.2.1.4.21.1.9";
            public static readonly string ipRouteAge     = "1.3.6.1.2.1.4.21.1.10";
            public static readonly string ipRouteMask    = "1.3.6.1.2.1.4.21.1.11";
            public static readonly string ipRouteMetric5 = "1.3.6.1.2.1.4.21.1.12";
            public static readonly string ipRouteInfo    = "1.3.6.1.2.1.4.21.1.13";
        }

        public struct IfMIB
        {
            public static readonly string ifXTable                   = "1.3.6.1.2.1.31.1.1";
            public static readonly string ifXEntry                   = "1.3.6.1.2.1.31.1.1.1";
            public static readonly string ifName                     = "1.3.6.1.2.1.31.1.1.1.1";
            public static readonly string ifInMulticastPkts          = "1.3.6.1.2.1.31.1.1.1.2";
            public static readonly string ifInBroadcastPkts          = "1.3.6.1.2.1.31.1.1.1.3";
            public static readonly string ifOutMulticastPkts         = "1.3.6.1.2.1.31.1.1.1.4";
            public static readonly string ifOutBroadcastPkts         = "1.3.6.1.2.1.31.1.1.1.5";
            public static readonly string ifHCInOctets               = "1.3.6.1.2.1.31.1.1.1.6";
            public static readonly string ifHCInUcastPkts            = "1.3.6.1.2.1.31.1.1.1.7";
            public static readonly string ifHCInMulticastPkts        = "1.3.6.1.2.1.31.1.1.1.8";
            public static readonly string ifHCInBroadcastPkts        = "1.3.6.1.2.1.31.1.1.1.9";
            public static readonly string ifHCOutOctets              = "1.3.6.1.2.1.31.1.1.1.10";
            public static readonly string ifHCOutUcastPkts           = "1.3.6.1.2.1.31.1.1.1.11";
            public static readonly string ifHCOutMulticastPkts       = "1.3.6.1.2.1.31.1.1.1.12";
            public static readonly string ifHCOutBroadcastPkts       = "1.3.6.1.2.1.31.1.1.1.13";
            public static readonly string ifLinkUpDownTrapEnable     = "1.3.6.1.2.1.31.1.1.1.14";
            public static readonly string ifHighSpeed                = "1.3.6.1.2.1.31.1.1.1.15";
            public static readonly string ifPromiscuousMode          = "1.3.6.1.2.1.31.1.1.1.16";
            public static readonly string ifConnectorPresent         = "1.3.6.1.2.1.31.1.1.1.17";
            public static readonly string ifAlias                    = "1.3.6.1.2.1.31.1.1.1.18";
            public static readonly string ifCounterDiscontinuityTime = "1.3.6.1.2.1.31.1.1.1.19";
        }

        public struct Vlans
        {
            public static readonly string dot1qVlanStaticTable =     "1.3.6.1.2.1.17.7.1.4.3";
            public static readonly string dot1qVlanStaticEntry =     "1.3.6.1.2.1.17.7.1.4.3.1";
            public static readonly string dot1qVlanStaticName  =     "1.3.6.1.2.1.17.7.1.4.3.1.1";
            public static readonly string dot1qVlanStaticEgressPorts = "1.3.6.1.2.1.17.7.1.4.3.1.2";
            public static readonly string dot1qVlanStaticUntaggedPorts = "1.3.6.1.2.1.17.7.1.4.3.1.4";
            public static readonly string dot1qVlanStaticRowStatus = "1.3.6.1.2.1.17.7.1.4.3.1.5";

            public static readonly string dot1dBasePortIfIndex = "1.3.6.1.2.1.17.1.4.1.2";

            public static readonly string dot1qPvid = "1.3.6.1.2.1.17.7.1.4.5.1.1";
        }
    }
}


