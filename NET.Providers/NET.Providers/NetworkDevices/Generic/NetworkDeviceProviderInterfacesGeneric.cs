using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple;
using Simple.Network;
using NET.Tools.Snmp;

namespace NET.Tools.Providers
{
    [NetworkDeviceProviderType(DeviceProviderType.Generic)]
    public class NetworkDeviceProviderInterfacesGeneric : NetworkDeviceProviderInterfaces, INetworkDeviceProviderInterfaces
    {
        #region |   Private Members   |

        private Dictionary<string, int> interfaceIndexesByInterfaceName = null;
        private Dictionary<int, List<NetworkInfo>> ipAddressListsByInterfaceIndex = null;
        private Dictionary<int, int> dot1dBasePortsByInterfaceIndex = null;

        #endregion |   Private Members   |

        #region |   Public Methods   |

        public async ValueTask<Dictionary<string, int>> GetInterfaceIndexesByInterfaceName()
        {
            if (this.interfaceIndexesByInterfaceName == null)
                await this.GenerateInterfaceDictionary();

            return this.interfaceIndexesByInterfaceName;
        }

        protected void SetInterfaceIndexesByInterfaceName(Dictionary<string, int> value) => this.interfaceIndexesByInterfaceName = value;

        public async ValueTask<Dictionary<int, List<NetworkInfo>>> GetIpAddressListsByInterfaceIndex()
        {
            if (this.ipAddressListsByInterfaceIndex == null)
                await this.GenerateIpAddressDictionary();

            return this.ipAddressListsByInterfaceIndex;
        }

        public async ValueTask<Dictionary<int, int>> GetDot1dBasePortsByInterfaceIndex()
        {
            if (this.dot1dBasePortsByInterfaceIndex == null)
                await this.GenerateDot1dBasePortDictionary();

            return this.dot1dBasePortsByInterfaceIndex;
        }

        #endregion |   Public Methods   |

        #region |   Interface Data   |

        public override async ValueTask<IEnumerable<string>> GetInterfaceNames()
        {
            var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();

            return interfaceIndexesByInterfaceName.Keys;
        }

        //public override bool Contains(string interfaceName)
        //{
        //    return this.GetInterfaceIndexesByInterfaceName().ContainsKey(interfaceName);
        //}

        public override async ValueTask<string> GetName(int interfaceIndex)
        {
            SnmpData snmpData = await this.Provider.Snmp.GetAsync(String.Format("{0}.{1}", SnmpOIDs.Interfaces.ifDescr, interfaceIndex));
            string interfaceName = this.GetStandardizedName(snmpData.Value);

            return interfaceName;
        }

        public override string GetStandardizedName(string interfaceName) => ProviderHelper.GetStandardizedInterfaceName(interfaceName);

        public override async ValueTask<int> GetIndex(string interfaceName)
        {
            string standardizedInterfaceName = this.GetStandardizedName(interfaceName);
            var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();
            int interfaceIndex = -1;

            if (!interfaceIndexesByInterfaceName.ContainsKey(standardizedInterfaceName))
                await this.GenerateInterfaceDictionary();

            if (interfaceIndexesByInterfaceName.ContainsKey(standardizedInterfaceName))
                interfaceIndex = interfaceIndexesByInterfaceName[standardizedInterfaceName];

            return interfaceIndex;
        }

        public override async ValueTask<string> GetShortName(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifName)).ToString();
        }

        public override async ValueTask<string> GetDescription(string interfaceName)
        {
            string result = String.Empty;

            try
            {
                SnmpData snmpData = await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifAlias);
                result = snmpData.ToString();

                if (result != null)
                    result = result.Trim();
                //SnmpData snmpData = this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifAlias);
                //return snmpData.IsNullOrNoSuchObject() ? null : snmpData.Value;
            }
            catch
            {
            }

            return result;
        }

        public override async ValueTask SetDescription(string interfaceName, string description)
        {
            string valueToSet = description.IsNullOrEmpty() ? " " : description.Trim();

            try
            {
                await this.SetInterfaceSnmpDataValue(interfaceName, SnmpOIDs.IfMIB.ifAlias, valueToSet);
            }
            catch (Exception ex)
            {
                throw new ProviderInfoException(ex.Message);
            }
        }

        public override async ValueTask<InterfaceSnmpType> GetInterfaceType(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifType)).ToCustom<InterfaceSnmpType>();
        }

        public override async ValueTask<uint> GetMtu(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifMtu)).ToUInt32();
        }

        public override async ValueTask<uint> GetSpeed(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifSpeed)).ToUInt32();
        }

        public override async ValueTask<string> GetPhysicalAddress(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifPhysAddress)).ToString();
        }

        public override async ValueTask<InterfaceAdminStatus> GetAdminStatus(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifAdminStatus)).ToCustom<InterfaceAdminStatus>();
        }

        public override async ValueTask SetAdminStatus(string interfaceName, InterfaceAdminStatus adminStatus)
        {
            await this.SetInterfaceSnmpDataValue(interfaceName, SnmpOIDs.Interfaces.ifAdminStatus, (int)adminStatus);
        }

        public override async ValueTask<InterfaceOperationalStatus> GetOperationalStatus(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifOperStatus)).ToCustom<InterfaceOperationalStatus>();
        }

        public override async ValueTask<Dictionary<string, InterfaceOperationalStatus>> GetBulkOperationalStatus()
        {
            Dictionary<string, InterfaceOperationalStatus> result = new Dictionary<string, InterfaceOperationalStatus>();
            IList<SnmpData> snmpDataList = await this.Provider.Snmp.WalkAsync(SnmpOIDs.Interfaces.ifOperStatus);

            foreach (SnmpData snmpData in snmpDataList)
            {
                int interfaceIndex = Conversion.TryChangeType<int>(snmpData.OID.Substring(SnmpOIDs.Interfaces.ifOperStatus.Length + 1));
                InterfaceOperationalStatus interfaceOperationalStatus = Conversion.TryChangeType<InterfaceOperationalStatus>(snmpData.Value, InterfaceOperationalStatus.Unknown);
                var interfaceIndexesByInterfaceName = await this.GetInterfaceIndexesByInterfaceName();
                string interfaceName = null;

                if (!interfaceIndexesByInterfaceName.ContainsValue(interfaceIndex))
                    await this.GenerateInterfaceDictionary();

                var interfaceItem = this.interfaceIndexesByInterfaceName.SingleOrDefault(i => i.Value == interfaceIndex);

                if (interfaceItem.Key != null)
                    interfaceName = interfaceItem.Key;

                if (interfaceName != null)
                    result.Add(interfaceName, interfaceOperationalStatus);
            }

            return result;
        }

        public override async ValueTask<TimeSpan> GetLastChange(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifLastChange)).ToTimeSpan();
        }

        public override async ValueTask<uint> GetInOctets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifInOctets)).ToUInt32();
        }

        public override async ValueTask<uint> GetInUnicastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifInUcastPkts)).ToUInt32();
        }

        public override async ValueTask<uint> GetInNUnicastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifInNUcastPkts)).ToUInt32();
        }

        public override async ValueTask<uint> GetInDiscards(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifInDiscards)).ToUInt32();
        }

        public override async ValueTask<uint> GetInErrors(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifInErrors)).ToUInt32();
        }

        public override async ValueTask<uint> GetInUnknownProtocols(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifInUnknownProtos)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutOctets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifOutOctets)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutUnicastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifOutUcastPkts)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutNUnicastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifOutNUcastPkts)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutDiscards(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifOutDiscards)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutErrors(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifOutErrors)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutQLen(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifOutQLen)).ToUInt32();
        }

        public override async ValueTask<uint> GetSpecific(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.Interfaces.ifSpecific)).ToUInt32();
        }

        public override async ValueTask<uint> GetInMulticastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifInMulticastPkts)).ToUInt32();
        }

        public override async ValueTask<uint> GetInBrotcastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifInBroadcastPkts)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutMulticastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifOutMulticastPkts)).ToUInt32();
        }

        public override async ValueTask<uint> GetOutBrotcastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifOutBroadcastPkts)).ToUInt32();
        }

        public override async ValueTask<ulong> GetHCInOctets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCInOctets)).ToUInt64();
        }

        public override async ValueTask<ulong> GetHCInUnicastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCInUcastPkts)).ToUInt64();
        }

        public override async ValueTask<ulong> GetHCInMulticastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCInMulticastPkts)).ToUInt64();
        }

        public override async ValueTask<ulong> GetHCInBrotcastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCInBroadcastPkts)).ToUInt64();
        }

        public override async ValueTask<ulong> GetHCOutOctets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCOutOctets)).ToUInt64();
        }

        public override async ValueTask<ulong> GetHCOutUnicastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCOutUcastPkts)).ToUInt64();
        }

        public override async ValueTask<ulong> GetHCOutMulticastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCOutMulticastPkts)).ToUInt64();
        }

        public override async ValueTask<ulong> GetHCOutBrotcastPackets(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHCOutBroadcastPkts)).ToUInt64();
        }

        public override async ValueTask<InterfaceSnmpTrapUpDownEnable> GetLinkUpDownTrapEnable(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifLinkUpDownTrapEnable)).ToCustom<InterfaceSnmpTrapUpDownEnable>();
        }

        public override async ValueTask SetLinkUpDownTrapEnable(string interfaceName, InterfaceSnmpTrapUpDownEnable linkUpDownTrapEnable)
        {
            await this.SetInterfaceSnmpDataValue(interfaceName, SnmpOIDs.IfMIB.ifLinkUpDownTrapEnable, (int)linkUpDownTrapEnable);
        }

        public override async ValueTask<uint> GetHighSpeed(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifHighSpeed)).ToUInt32();
        }

        public override async ValueTask<InterfacePromiscuousMode> GetPromiscuousMode(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifPromiscuousMode)).ToCustom<InterfacePromiscuousMode>();
        }

        public override async ValueTask SetPromiscuousMode(string interfaceName, InterfacePromiscuousMode promiscuousMode)
        {
            await this.SetInterfaceSnmpDataValue(interfaceName, SnmpOIDs.IfMIB.ifPromiscuousMode, (int)promiscuousMode);
        }

        public override async ValueTask<InterfaceConnectionPresent> GetConnectorPresent(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifConnectorPresent)).ToCustom<InterfaceConnectionPresent>();
        }

        public override async ValueTask<TimeSpan> GetCounterDiscontinuityTime(string interfaceName)
        {
            return (await this.GetInterfaceSnmpData(interfaceName, SnmpOIDs.IfMIB.ifCounterDiscontinuityTime)).ToTimeSpan();
        }

        public override bool IsAddRemoveSupported() => false;

        public override ValueTask Add(string interfaceName)
        {
            throw new ProviderInfoException("Add Interface is not supported using generic SNMP.");
        }

        public override ValueTask Remove(string interfaceName)
        {
            throw new ProviderInfoException("Remove Interface is not supported using generic SNMP.");
        }

        #endregion |   Interface Data   |

        #region |   Interface IP Addresses   |

        public override async ValueTask<bool> IsIpAddressSupported(string interfaceName)
        {
            NetworkInfo ipAddressInfo = await this.GetIpAddress(interfaceName);

            return ipAddressInfo.IpAddressText.Length > 0;
        }

        public override ValueTask<bool> IsWriteIpAddressSupported(string interfaceName)
        {
            return new ValueTask<bool>(false);   // Seting ip address via SNMP is not possible.
        }

        public override async ValueTask<NetworkInfo?> GetIpAddress(string interfaceName)
        {
            List<NetworkInfo> ipAddressInfoList = await this.GetIpAddresses(interfaceName);

            return ipAddressInfoList.Count > 0 ? ipAddressInfoList[0] : null;
        }

        public virtual async ValueTask<List<NetworkInfo>> GetIpAddresses(string interfaceName)
        {
            int interfaceIndex = await this.GetIndex(interfaceName);
            List<NetworkInfo> ipAddressInfoList;
            List<NetworkInfo> resultList = new List<NetworkInfo>();
            var ipAddressListsByInterfaceIndex = await this.GetIpAddressListsByInterfaceIndex();

            if (ipAddressListsByInterfaceIndex.TryGetValue(interfaceIndex, out ipAddressInfoList))
                resultList = ipAddressInfoList.ToList();

            return resultList;
        }

        public override ValueTask SetIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            throw new ProviderInfoException("Sets an IP Address is not suported.");
        }

        public override ValueTask<bool> IsSecondaryIpAddressSupported(string interfaceName) => new ValueTask<bool>(true);

        public override async ValueTask<IEnumerable<NetworkInfo>> GetSecondaryIpAddresses(string interfaceName)
        {
            List<NetworkInfo> ipAddressInfoList = await this.GetIpAddresses(interfaceName);

            if (ipAddressInfoList.Count > 0)
                ipAddressInfoList.RemoveAt(0);

            return ipAddressInfoList;
        }

        public override ValueTask<bool> IsAddRemoveSecondaryIpAddressSupported(string interfaceName)
        {
            return new ValueTask<bool>(false);   // We dont know now is secondary interface ip address capable using SNMP.
        }

        public override ValueTask AddSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            throw new ProviderInfoException("Add Secondary IP Address is not suported.");
        }

        public override ValueTask RemoveSecondaryIpAddress(string interfaceName, IpAddress ipAddress, int subnetMaskPrefix)
        {
            throw new ProviderInfoException("Remove Secondary IP Address is not suported.");
        }

        public override async ValueTask GenerateIpAddressDictionary()
        {
            this.ipAddressListsByInterfaceIndex = new Dictionary<int, List<NetworkInfo>>();

            SnmpData[,] ipAddrTable = await this.Provider.Snmp.GetTableAsync(SnmpOIDs.Ip.ipAddrTable);

            for (int i = 0; i < ipAddrTable.GetLength(0); i++)
            {
                int interfaceIndex = ipAddrTable[i, 1].ToInt32();
                string ipAddress = ipAddrTable[i, 0].Value;
                string ipSubnetMask = ipAddrTable[i, 2].Value;

                List<NetworkInfo> ipAddressInfoList = null;

                if (!this.ipAddressListsByInterfaceIndex.TryGetValue(interfaceIndex, out ipAddressInfoList))
                {
                    ipAddressInfoList = new List<NetworkInfo>();

                    this.ipAddressListsByInterfaceIndex.Add(interfaceIndex, ipAddressInfoList);
                }

                NetworkInfo ipAddressInfo = new NetworkInfo(ipAddress, ipSubnetMask);

                if (ipAddressInfo.Validate())
                    ipAddressInfoList.Add(ipAddressInfo);
            }
        }

        #endregion |   Interface IP Addresses   |

        #region |   Interface Vlans   |

        //      public override bool IsVlanSupported(string interfaceName)
        //      {
        //	SnmpData snmpData;

        //	try
        //	{
        //		int interfaceIndex = this.GetIndex(interfaceName);
        //		int dot1dBasePortIndex = this.GetDot1dBasePortIndex(interfaceIndex);

        //		snmpData = this.Provider.Snmp.Get(SnmpOIDs.Vlans.dot1qPvid + "." + dot1dBasePortIndex);
        //	}
        //	catch
        //	{
        //		return this.GetSwitchportInfo(interfaceName).SwitchportMode != InterfaceSwitchportMode.VlanIsNotSupported;
        //	}

        //	return snmpData != null;
        //}

        public override async ValueTask<SwitchportInfo> GetSwitchportInfo(string interfaceName)
        {
            InterfaceSwitchportMode switchportMode = InterfaceSwitchportMode.VlanIsNotSupported;
            int vlanId = await this.GetVlanId(interfaceName);

            return new SwitchportInfo(switchportMode, vlanId);
        }

        public override ValueTask SetSwitchport(string interfaceName, InterfaceSwitchportMode switchportMode, int vlanId)
        {
            throw new ProviderInfoException("Set switchport mode is not supported for Generic network device.");
        }

        public override ValueTask<bool> IsL3InterfaceBasedOnVlan(string interfaceName)
        {
            string strVlan = "vlan";
            bool result = false;
            string ifName = interfaceName.Trim().ToLower();

            if (ifName.StartsWith(strVlan))
            {
                string strVlanId = ifName.Replace(strVlan, "").Trim();
                int vlanId = Conversion.TryChangeType<int>(strVlanId, 0);

                if (vlanId > 0 && vlanId <= 4096)
                    return new ValueTask<bool>(true);
            }

            return new ValueTask<bool>(result);
        }

        public override ValueTask<int> GetL3InterfaceBasedVlanId(string interfaceName)
        {
            string strVlan = "vlan";
            int vlanId = 0;
            bool isL3InterfaceBasedOnVlan = this.IsL3InterfaceBasedOnVlan(interfaceName).GetAwaiter().GetResult();

            if (isL3InterfaceBasedOnVlan)
            {
                string strVlanId = interfaceName.Trim().ToLower().Replace(strVlan, "").Trim();
                vlanId = Conversion.TryChangeType<int>(strVlanId, 0);
            }

            return new ValueTask<int>(vlanId);
        }

        #endregion |   Interface Vlans   |

        #region |   Interface ACL   |

        //[ProviderAction("Interfaces.IsInAclSupported")]
        //public virtual bool IsInAclSupported(string interfaceName)
        //{
        //    // TODO:
        //    return false;
        //}

        //[ProviderAction("Interfaces.IsOutAclSupported")]
        //public virtual bool IsOutAclSupported(string interfaceName)
        //{
        //    // TODO:
        //    return false;
        //}


        //[ProviderAction("Interfaces.GetAclName")]
        //private virtual void GetAclName(string interfaceName, InterfaceAclDirection aclDirection)
        //{
        //    // TODO:
        //    string strDirection = aclDirection == InterfaceAclDirection.In ? "Inbound" : "Outbound";
        //}

        public override async ValueTask AttachAcl(string interfaceName, string aclName, AclDirection aclDirection)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("ip access-list extended " + aclName);
            await this.Provider.Terminal.SendAsync("exit");     // exit access-list
            await this.Provider.Terminal.SendAsync("interface " + interfaceName);
            await this.Provider.Terminal.SendAsync("ip access-group " + aclName + " " + aclDirection.ToString().ToLower());
            await this.Provider.Terminal.SendAsync("exit");     // exit interface
        }

        public override async ValueTask DetachAcl(string interfaceName, string aclName, AclDirection aclDirection)
        {
            await this.Provider.Terminal.EnterConfigModeAsync();
            await this.Provider.Terminal.SendAsync("interface " + interfaceName);
            await this.Provider.Terminal.SendAsync("no ip access-group " + aclName + " " + aclDirection.ToString().ToLower());
            await this.Provider.Terminal.SendAsync("exit");     // exit interface

            //response = this.Provider.Terminal.Send("no ip access-list extended " + aclName);
        }

        #endregion |   Interface ACL   |

        #region |   Protected Methods   |

        protected virtual async ValueTask GenerateInterfaceDictionary()
        {
            // InterfacesByInterfaceIndex
            if (this.interfaceIndexesByInterfaceName == null)
                this.interfaceIndexesByInterfaceName = new Dictionary<string, int>();
            else
                this.interfaceIndexesByInterfaceName.Clear();

            IList<SnmpData> ifDescrList = await this.Provider.Snmp.WalkAsync(SnmpOIDs.Interfaces.ifDescr);

            foreach (SnmpData snmpData in ifDescrList)
            {
                int interfaceIndex = Conversion.TryChangeType<int>(snmpData.OID.Substring(SnmpOIDs.Interfaces.ifDescr.Length + 1));
                string interfaceName = this.GetStandardizedName(snmpData.Value);

                if (this.interfaceIndexesByInterfaceName.Keys.Contains(interfaceName))
                {
                    int sameInterfaceNameCounter = 1;

                    while (this.interfaceIndexesByInterfaceName.Keys.Contains(interfaceName + sameInterfaceNameCounter))
                        sameInterfaceNameCounter++;

                    interfaceName += sameInterfaceNameCounter;
                }

                this.OnInterfaceIndexesByInterfaceNameAdd(interfaceName, interfaceIndex);
            }
        }

        protected virtual async ValueTask GenerateDot1dBasePortDictionary()
        {
			if (this.dot1dBasePortsByInterfaceIndex == null)
                this.dot1dBasePortsByInterfaceIndex = new Dictionary<int, int>();
            else
                this.dot1dBasePortsByInterfaceIndex.Clear();

            IList<SnmpData> dot1dBasePortIfIndexList = await this.Provider.Snmp.WalkAsync(SnmpOIDs.Vlans.dot1dBasePortIfIndex);

            foreach (SnmpData snmpData in dot1dBasePortIfIndexList)
            {
                int dot1dBasePort = Conversion.TryChangeType<int>(snmpData.OID.Substring(SnmpOIDs.Vlans.dot1dBasePortIfIndex.Length + 1));
                int interfaceIndex = snmpData.ToInt32();

                this.dot1dBasePortsByInterfaceIndex.Add(interfaceIndex, dot1dBasePort);
            }
        }

		protected virtual async ValueTask<int> GetVlanId(string interfaceName)
		{
			int interfaceIndex = await this.GetIndex(interfaceName);
			int dot1dBasePortIndex = await this.GetDot1dBasePortIndex(interfaceIndex);

            if (dot1dBasePortIndex < 0)
                return 1;

            SnmpData snmpData = await this.Provider.Snmp.GetAsync(SnmpOIDs.Vlans.dot1qPvid + "." + dot1dBasePortIndex);
            int vlanId = snmpData.ToInt32();

			if (vlanId <= 0 || vlanId > 4096)
				vlanId = 1;

			return vlanId;
		}

		protected virtual void OnInterfaceIndexesByInterfaceNameAdd(string interfaceName, int interfaceIndex)
		{
			this.interfaceIndexesByInterfaceName.Add(interfaceName, interfaceIndex);
		}

		//protected virtual void GenerateInterfaceDictionaryOld()
		//{
		//	// InterfacesByInterfaceIndex
		//	if (this.interfaceIndexesByInterfaceName == null)
		//	{
		//		this.interfaceIndexesByInterfaceName = new Dictionary<string, int>();
		//	}

		//	this.interfaceIndexesByInterfaceName.Clear();

		//	string interfaceOid = SnmpOIDs.Interfaces.ifDescr;
		//	SnmpData snmpData = this.Provider.DeviceConnection.Snmp.GetNext(interfaceOid);

		//	while (snmpData.OID.Contains(interfaceOid))
		//	{
		//		int interfaceIndex = Conversion.TryChangeType<int>(snmpData.OID.Substring(interfaceOid.Length + 1));
		//		//uint interfaceSpeed = this.Provider.Connection.Snmp.Get(String.Format("{0}.{1}", SnmpOIDs.Interfaces.ifSpeed, interfaceIndex)).ToUInt32();

		//		//if (interfaceSpeed <= 0)
		//		//{
		//		//    snmpData = this.Provider.Connection.Snmp.GetNext(snmpData.OID);
		//		//    continue;
		//		//}
                
		//		string interfaceName = this.GetStandardizedInterfaceName(snmpData.Value);

		//		if (this.interfaceIndexesByInterfaceName.Keys.Contains(interfaceName))
		//		{
		//			int sameInterfaceNameCounter = 1;

		//			while (this.interfaceIndexesByInterfaceName.Keys.Contains(interfaceName + sameInterfaceNameCounter))
		//			{
		//				sameInterfaceNameCounter++;
		//			}

		//			interfaceName = interfaceName + sameInterfaceNameCounter;
		//		}

		//		this.interfaceIndexesByInterfaceName.Add(interfaceName, interfaceIndex);
		//		snmpData = this.Provider.DeviceConnection.Snmp.GetNext(snmpData.OID);
		//	}

		//	// Dot1dBasePortDictionary
		//	if (this.dot1dBasePortsByInterfaceIndex == null)
		//	{
		//		this.dot1dBasePortsByInterfaceIndex = new Dictionary<int, int>();
		//	}

		//	this.dot1dBasePortsByInterfaceIndex.Clear();

		//	string dot1dBasePortIfIndexOid = SnmpOIDs.Vlans.dot1dBasePortIfIndex;
		//	snmpData = this.Provider.DeviceConnection.Snmp.GetNext(dot1dBasePortIfIndexOid);

		//	while (snmpData.OID.Contains(dot1dBasePortIfIndexOid) && snmpData.SnmpObjectValueType != SnmpObjectValueType.EndOfMibView) // && snmpData.OID != dot1dBasePortIfIndexOid)
		//	{
		//		int dot1dBasePort = Conversion.TryChangeType<int>(snmpData.OID.Substring(dot1dBasePortIfIndexOid.Length + 1));
		//		int interfaceIndex = snmpData.ToInt32();

		//		this.dot1dBasePortsByInterfaceIndex.Add(interfaceIndex, dot1dBasePort);
		//		snmpData = this.Provider.DeviceConnection.Snmp.GetNext(snmpData.OID);
		//	}
		//}


        protected virtual async ValueTask<int> GetDot1dBasePortIndex(int interfaceIndex)
        {
            int result = -1;
            var dot1dBasePortsByInterfaceIndex = await this.GetDot1dBasePortsByInterfaceIndex();

            if (dot1dBasePortsByInterfaceIndex.ContainsKey(interfaceIndex))
                result = dot1dBasePortsByInterfaceIndex[interfaceIndex];

            return result;
        }

        protected virtual string GetAclProtocolString(byte protocol)
        {
            string result;

            switch (protocol)
            {
                case 4:

					result = "ip";
                    
                    break;

				case 6:

					result = "tcp";
                    
                    break;

				case 17:

					result = "udp";
                    
                    break;

                default:
                    
                    result = protocol.ToString();
                    
                    break;
            }

            return result;
        }

        protected virtual string GetAclIpAddressString(string ipAddress, byte ipSubnetMaskNumOfBits)
        {
            string result = "";
            
            if (ipSubnetMaskNumOfBits == 0)
            {
                result = "any";
            }
            else if (ipSubnetMaskNumOfBits == 32)
            {
                result = ipAddress + " host ";
            }
            else
            {
                result = ipAddress + " " + IpHelper.GetSubnetMaskWildCard(ipSubnetMaskNumOfBits);
            }

            return result;
        }

		protected virtual string GetAclPortString(AclPortOperator portCriteria, ushort? port, ushort? port2)
        {
            string result = string.Empty;

            switch (portCriteria)
            {
				case AclPortOperator.Equal:
                    
                    result = "eq";
                    
                    break;

				case AclPortOperator.NotEqual:
                    
                    result = "neq";
                    
                    break;

				case AclPortOperator.GreaterThan:
                    
                    result = "gt";
                    
                    break;

				case AclPortOperator.LessThan:
                    
                    result = "lt";
                    
                    break;

                case AclPortOperator.Range:
                    
                    result = "range";
                    
                    break;
            }

            if (result != string.Empty)
            {
                result += " " + port;

                if (portCriteria != AclPortOperator.Range)
                    result += " " + port2;
            }

            return result;
        }

        protected async ValueTask<SnmpData> GetInterfaceSnmpData(string interfaceName, string snmpOID)
        {
            int interfaceIndex = await this.GetIndex(interfaceName);
			SnmpData result = await this.Provider.Snmp.GetAsync(String.Format("{0}.{1}", snmpOID, interfaceIndex));

			return result;
        }

        protected async ValueTask SetInterfaceSnmpDataValue(string interfaceName, string snmpOID, object snmpDataValue)
        {
            int interfaceIndex = await this.GetIndex(interfaceName);

            if (interfaceIndex == -1)
                throw new ArgumentOutOfRangeException("No interface index for interface name: " + interfaceName);

            await this.Provider.Snmp.SetAsync(String.Format("{0}.{1}", snmpOID, interfaceIndex), snmpDataValue);
		}

		#endregion |   Protected Methods   |
	}
}
