
using System;

namespace Thought.Net.Telnet
{

    /// <summary>
    ///     The signature of a handler of a telnet command event.
    /// </summary>
    /// <param name="sender">
    ///     The sender of the event.
    /// </param>
    /// <param name="e">
    ///     Additional data about the event.
    /// </param>
    public delegate void TelnetCommandEventHandler(
        object sender,
        TelnetCommandEventArgs e);
}
