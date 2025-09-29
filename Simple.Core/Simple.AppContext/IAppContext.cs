using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.AppContext
{
	public interface IAppContext
	{
		string AppName { get; set; }
		string Copyright { get; set; }
		Version Version { get; set; }
	}
}
