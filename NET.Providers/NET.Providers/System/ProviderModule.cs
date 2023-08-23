using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NET.Tools.Providers
{
	public abstract class ProviderModule : IDisposable
	{
		public Provider Provider { get; internal set; }
		public int ModuleType { get; internal set; }

		void IDisposable.Dispose()
		{
			this.Provider = null;
		}
	}
}
