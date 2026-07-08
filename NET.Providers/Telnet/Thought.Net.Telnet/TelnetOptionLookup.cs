using System;
using System.Collections;

namespace Thought.Net.Telnet
{


	/// <summary>
	///     A utility class for translating RFC telnet option names into the
	///     corresponding byte or <see cref="TelnetOption"/> values.
	/// </summary>
	public static class TelnetOptionLookup
	{

		private static Hashtable keys;

		static TelnetOptionLookup()
		{

			keys = new Hashtable(50);

			keys.Add(TelnetOption.TransmitBinary, "TRANSMIT-BINARY");         /* 00 */
			keys.Add(TelnetOption.Echo, "ECHO");                              /* 01 */
			keys.Add(TelnetOption.SuppressGoAhead, "SUPPRESS-GO-AHEAD");      /* 03 */
			keys.Add(TelnetOption.Status, "STATUS");                          /* 05 */
			keys.Add(TelnetOption.TimingMark, "TIMING-MARK");                 /* 06 */
			keys.Add(TelnetOption.OutputCarriageReturnDisposition, "NAOCRD"); /* 10 */
			keys.Add(TelnetOption.OutputHorizontalTabStops, "NAOHTS");        /* 11 */
			keys.Add(TelnetOption.OutputHorizontalTabDisposition, "NAOHTD");  /* 12 */
			keys.Add(TelnetOption.OutputFormfeedDisposition, "NAOFFD");       /* 13 */
			keys.Add(TelnetOption.OutputVerticalTabStops, "NAOVTS");          /* 14 */
			keys.Add(TelnetOption.OutputVerticalTabDisposition, "NAOVTD");    /* 15 */
			keys.Add(TelnetOption.OutputLinefeedDisposition, "NAOLFD");       /* 16 */
			keys.Add(TelnetOption.ExtendedAscii, "EXTEND-ASCII");             /* 17 */
			keys.Add(TelnetOption.Logout, "LOGOUT");                          /* 18 */
			keys.Add(TelnetOption.ByteMacro, "BM");                           /* 19 */
			keys.Add(TelnetOption.DataEntryTerminal, "DET");                  /* 20 */
			keys.Add(TelnetOption.SendLocation, "SEND-LOCATION");             /* 23 */
			keys.Add(TelnetOption.TerminalType, "TERMINAL-TYPE");             /* 24 */
			keys.Add(TelnetOption.EndOfRecord, "END-OF-RECORD");              /* 25 */
			keys.Add(TelnetOption.TacacsUserId, "TUID");                      /* 26 */
			keys.Add(TelnetOption.OutputMarking, "OUTMRK");                   /* 27 */
			keys.Add(TelnetOption.TerminalLocation, "TTYLOC");                /* 28 */
			keys.Add(TelnetOption.Ibm3270Regime, "3270-REGIME");              /* 29 */
			keys.Add(TelnetOption.X3Pad, "X.3-PAD");                          /* 30 */
			keys.Add(TelnetOption.WindowSize, "NAWS");                        /* 31 */
			keys.Add(TelnetOption.TerminalSpeed, "TERMINAL-SPEED");           /* 32 */
			keys.Add(TelnetOption.ToggleFlowControl, "TOGGLE-FLOW-CONTROL");  /* 33 */
			keys.Add(TelnetOption.LineMode, "LINEMODE");                      /* 34 */
			keys.Add(TelnetOption.XDisplayLocation, "X-DISPLAY-LOCATION");    /* 35 */
			keys.Add(TelnetOption.Environment, "ENVIRON");                    /* 36 */
			keys.Add(TelnetOption.Authentication, "AUTHENTICATION");          /* 37 */
			keys.Add(TelnetOption.Encrypt, "ENCRYPT");                        /* 38 */
			keys.Add(TelnetOption.NewEnvironment, "NEW-ENVIRON");             /* 39 */
			keys.Add(TelnetOption.TN3270E, "TN3260E");                        /* 40 */
			keys.Add(TelnetOption.CharacterSet, "CHARSET");                   /* 42 */
			keys.Add(TelnetOption.ComPort, "COM-PORT-OPTION");                /* 44 */
			keys.Add(TelnetOption.Kermit, "KERMIT");                          /* 47 */

		}


		/// <summary>
		///     Returns the text name of the option as documented in the RFC.
		/// </summary>
		public static string GetName(TelnetOption option)
		{
			return (string)keys[option];
		}


		public static TelnetOption GetOption(string name)
		{

			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");


			switch (name.ToUpperInvariant())
			{
				case "ECHO":
					return TelnetOption.Echo;
			}

			throw new NotSupportedException();

		}

	}
}
