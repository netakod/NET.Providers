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
	[NetworkDeviceProviderType(DeviceProviderType.ZyXELWebManaged)]
    public class WebClientZyXELWebManaged : NET.Tools.Web.WebClient
    {
        public WebClientZyXELWebManaged()
        {
			this.ConnectionMethod = WebRequestMethod.POST;

			this.connectAction = "login.cgi";
            this.usernamePostDataPrefix = "username=";
            this.passwordPostDataPrefix = "&password=";
            this.logOffAction = "logout.cgi";
            this.logOffRefererAction = "left2.htm";
        }

		protected override ICredentials CreateRequestCredentials()
		{
			return null; // Network credentials will be passed using POST data on connect
		}
	}
}
