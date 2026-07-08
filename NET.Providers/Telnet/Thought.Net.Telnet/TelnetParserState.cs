namespace Thought.Net.Telnet
{

	/// <summary>
	///     The internal state of the telnet protocol parser.
	/// </summary>
	public enum TelnetParserState
	{

		/// <summary>
		///     The client is reading or waiting for data.
		/// </summary>
		Data,

		/// <summary>
		///     The client received a carriage-return (ASCII 13) and
		///     is attempting to determine its purpose.
		/// </summary>
		ReceivedCarriageReturn,

		/// <summary>
		///     The client has received an IAC character.
		/// </summary>
		ReceivedIAC,

		/// <summary>
		///     The client has received a DO code.
		/// </summary>
		ReceivedDo,

		/// <summary>
		///     The client has recieved a DONT code.
		/// </summary>
		ReceivedDont,

		/// <summary>
		///     The client has received a WILL code.
		/// </summary>
		ReceivedWill,

		/// <summary>
		///     The client has received a WONT code.
		/// </summary>
		ReceivedWont
	}

}

