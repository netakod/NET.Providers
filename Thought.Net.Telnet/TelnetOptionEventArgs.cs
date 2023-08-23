
using System;

namespace Thought.Net.Telnet
{
    /// <summary>
    ///     Base class for events related to telnet option negotiations.
    /// </summary>
    public class TelnetOptionEventArgs : EventArgs
    {

        private bool agreed;
        private TelnetCommand command;
        private TelnetOption option;

        /// <summary>
        ///     Creates a new instance of the event arguments structure.
        /// </summary>
        /// <param name="option">
        ///     The <see cref="TelnetOption"/> related to the event.
        /// </param>
        public TelnetOptionEventArgs(TelnetCommand command, TelnetOption option)
            : base()
        {
            this.command = command;
            this.option = option;
        }


        /// <summary>
        ///     Indicates the client agrees to implement a protocol option.
        /// </summary>
        public bool Agreed
        {
            get
            {
                return this.agreed;
            }
            set
            {
                this.agreed = value;
            }
        }


        /// <summary>
        ///     The command (e.g. WILL, DONT, etc) for the option.
        /// </summary>
        public TelnetCommand Command
        {
            get
            {
                return this.command;
            }
            set
            {
                this.command = value;
            }
        }


        /// <summary>
        ///     The <see cref="TelnetOption"/> related to the event..
        /// </summary>
        /// <seealso cref="TelnetOption"/>
        public TelnetOption Option
        {
            get
            {
                return this.option;
            }
        }

    }
}
