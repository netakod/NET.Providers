using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.AppContext
{
    public class SystemSettings : AppSettings
    {
		public const string SettingLicenseName = "LicenseName";
        public const string SettingLicenseCompany = "LicenseCompany";
        public const string SettingLicenseKey = "LicenseKey";
        public const string SettingSerialNumber = "LicenseSerialNumber";

        public SystemSettings(string filePath)
            : base(filePath)
        {
        }

		public string LicenseName
        {
            get { return this.GetValue<string>(SettingLicenseName); }
            set { this.SetValue(SettingLicenseName, value); }
        }

        public string LicenseCompany
        {
            get { return this.GetValue<string>(SettingLicenseCompany); }
            set { this.SetValue(SettingLicenseCompany, value); }
        }

        public string LicenseKey
        {
            get { return this.GetValue<string>(SettingLicenseKey); }
            set { this.SetValue(SettingLicenseKey, value); }
        }

        public string SerialNumber
        {
            get { return this.GetValue<string>(SettingSerialNumber); }
            set { this.SetValue(SettingSerialNumber, value); }
        }
    }
}
