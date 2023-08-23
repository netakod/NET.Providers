using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers;
using Simple.Network;

namespace NET.Tools.Telnet
{
	public class TelnetTcpPipeChannel : TcpPipeChannel
	{
		private OnDataReceivedAction onDataReceivedAction;
		private OnClosedAction onClosedAction;

		public TelnetTcpPipeChannel(OnDataReceivedAction onDataReceivedAction, OnClosedAction onClosedAction)
		{
			this.onDataReceivedAction = onDataReceivedAction;
			this.onClosedAction = onClosedAction;
		}

		protected override void OnDataReceive(ref SequenceReader<byte> receivedDataSequence) => this.onDataReceivedAction(ref receivedDataSequence);

		protected override void OnClosed()
		{
			base.OnClosed();

			var closeReason = this.CloseReason; //.HasValue ? this.CloseReason.Value : Telnet.CloseReason.Unknown;
			this.onClosedAction(closeReason);

		}
		// TODO: Check is this telnet EOF
		//protected override void WriteEOF() => this.SendAsync(TelnetProtocol.IACBRK).ConfigureAwait(false);
		protected override void WriteEOF() => this.SendAsync(new byte[] { (byte)TelnetCommand.IAC, (byte)TelnetCommand.BREAK }).ConfigureAwait(false);
	}

	public delegate void OnDataReceivedAction(ref SequenceReader<byte> receivedDataSequence);
	public delegate void OnClosedAction(CloseReason closeReason);
}
