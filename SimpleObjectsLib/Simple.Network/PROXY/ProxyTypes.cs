﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Network
{
	/// <summary>
	/// Specifies the type of proxy client will use to connect to server.
	/// </summary>
	public enum ProxyTypes
	{
		/// <summary>No proxy server.</summary>
		None,
		/// <summary>A SOCKS4 proxy server.</summary>
		Socks4,
		/// <summary>A SOCKS5 proxy server.</summary>
		Socks5,
		/// <summary>A HTTP proxy server.</summary>
		Http,
	}
}
