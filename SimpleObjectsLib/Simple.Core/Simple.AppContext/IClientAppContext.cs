using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.AppContext
{
	public interface IClientAppContext : IAppContext
	{
		string SystemAdminUsername { get; set; }
		UserSettings UserSettings { get; }

	}
}
