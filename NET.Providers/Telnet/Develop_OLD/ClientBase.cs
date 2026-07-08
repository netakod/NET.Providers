using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NET.Providers.Telnet_DevelopOld
{
	internal abstract class ClientBase
	{
		public abstract IAsyncResult BeginRead(byte[] buffer, int offset, int size, AsyncCallback callback, object state);
	}
}
