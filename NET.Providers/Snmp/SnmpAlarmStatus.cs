using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Tools.Snmp
{
    
    /// <summary>
    /// SNMP event severity level 
    /// </summary>
    public enum SnmpAlarmStatus
    {
        Unknown = 0,
        Normal = 1,
        Warning = 2,
        Minor = 3,
        Major = 4,
        Critical = 5
    }
}
