using System;
using System.Collections.Generic;
using System.Text;

namespace Thought.Net.Telnet
{

	/// <summary>
	///     Event arguments for a telnet command event.
	/// </summary>
	public class TelnetCommandEventArgs : EventArgs
	{

		private TelnetCommand command;


		/// <summary>
		///     Initializes the structure with the specified command.
		/// </summary>
		/// <param name="command">
		///     The telnet command specific to the event.
		/// </param>
		public TelnetCommandEventArgs(TelnetCommand command)
			: base()
		{
			this.command = command;
		}


		/// <summary>
		///     The command that caused the event.
		/// </summary>
		public TelnetCommand Command
		{
			get
			{
				return this.command;
			}
		}

	}
}
