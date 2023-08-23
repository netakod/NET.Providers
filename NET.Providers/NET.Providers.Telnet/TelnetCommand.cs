using System;
using System.Collections.Generic;
using System.Text;

namespace NET.Tools.Telnet
{

	/// <summary>
	///     A telnet command.
	/// </summary>
	/// <remarks>
	///     A telnet command is embedded in the stream of data being transferred
	///     from one side of the connection to the other.  A command begins
	///     with an IAC character (ASCII 255) followed by a byte that identifies
	///     the command.
	/// </remarks>
	public enum TelnetCommand : byte
	{

		/// <summary>
		///     End Subnegotiation (SE)
		/// </summary>
		/// <remarks>
		///     This command indicates the end of subnegotiation data.
		/// </remarks>
		/// <seealso cref="Subnegotiation"/>
		EndSubnegotiation = 240,


		/// <summary>
		///     No operation (NOP)
		/// </summary>
		NoOperation = 241,


		/// <summary>
		///     Data Mark
		/// </summary>
		DataMark = 242,


		/// <summary>
		///     Break (BRK)
		/// </summary>
		BREAK = 243,


		/// <summary>
		///     Interrupt Process (IP)
		/// </summary>
		InterruptProcess = 244,


		/// <summary>
		///     Abort output (AO)
		/// </summary>
		AbortOutput = 245,


		/// <summary>
		///     Are You There (AYT)
		/// </summary>
		AreYouThere = 246,


		/// <summary>
		///     Erase Character (EC)
		/// </summary>
		/// <seealso cref="EraseLine"/>
		EraseCharacter = 247,


		/// <summary>
		///     Erase Line (EL)
		/// </summary>
		/// <seealso cref="EraseCharacter"/>
		EraseLine = 248,


		/// <summary>
		///     Go Ahead (GA)
		/// </summary>
		GoAhead = 249,


		/// <summary>
		///     Subnegotiation (SB)
		/// </summary>
		/// <seealso cref="EndSubnegotiation"/>
		Subnegotiation = 250,


		/// <summary>
		///     Will do option (WILL)
		/// </summary>
		/// <seealso cref="DO"/>
		/// <seealso cref="DONT"/>
		/// <seealso cref="WONT"/>
		WILL = 251,


		/// <summary>
		///     Will not do option (WONT)
		/// </summary>
		/// <seealso cref="DO"/>
		/// <seealso cref="DONT"/>
		/// <seealso cref="WILL"/>
		WONT = 252,


		/// <summary>
		///     Do option (DO)
		/// </summary>
		/// <seealso cref="DONT"/>
		/// <seealso cref="WILL"/>
		/// <seealso cref="WONT"/>
		DO = 253,


		/// <summary>
		///     Don't do option (DONT)
		/// </summary>
		/// <seealso cref="DO"/>
		/// <seealso cref="WILL"/>
		/// <seealso cref="WONT"/>
		DONT = 254,


		/// <summary>
		///     IAC
		/// </summary>
		IAC = 255
	}
}
