using System;
using System.Collections.Generic;
using System.Text;

namespace Thought.Net.Telnet
{

	/// <summary>
	///     Defines the signature of an event handler for the
	///     option-related events of the <see cref="TelnetClient"/> class.
	/// </summary>
	public delegate void TelnetOptionEventHandler(
		object sender,
		TelnetOptionEventArgs e);

}
