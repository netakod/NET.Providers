using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using Simple.Threading;
using NET.Tools.Web;

namespace NET.Tools.Providers
{
	[NetworkDeviceProviderType(DeviceProviderType.Linksys)]
    public class WebClientLinksys : NET.Tools.Web.WebClient
    {
        public WebClientLinksys()
        {
            this.connectAction = "config/log_off_page.htm";
            this.usernamePostDataPrefix = "userName%24query=";
            this.passwordPostDataPrefix = "&password%24query=";
            this.logOffAction = "config/authentication_page.htm?logOff";
            this.logOffRefererAction = "home.htm";
        }

		protected override ICredentials CreateRequestCredentials()
		{
			return null; // Network credentials will be passed using post data on connect
		}
	}
}
