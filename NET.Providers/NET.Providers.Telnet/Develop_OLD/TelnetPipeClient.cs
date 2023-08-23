using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.IO.Pipelines;

namespace NET.Tools.Telnet_DevelopOld
{
	public class TelnetPipeClient : TcpPipeChannel, ITelnetClient
	{
		private TelnetProtocol protocolHandler = null;
		
		private string[] waitForList = null;
		private bool ignoreCase = false;
		private StringBuilder stringBuilder = new StringBuilder();
		private ManualResetEvent waitForFoundResetEvent = new ManualResetEvent(false);

		public TelnetPipeClient() : this(ChannelOptions.Default) { }

		public TelnetPipeClient(ChannelOptions options)
		{
			this.Options = options;
			this.protocolHandler = new TelnetProtocol(telnetClient: this);
		}

		public event TextEventHandler TextReceived;

		public override async ValueTask ConnectAsync(string remoteHost, int remotePort = 23, CancellationToken cancellationToken = default)
		{
			await base.ConnectAsync(remoteHost, remotePort, cancellationToken);
		}

		public override async ValueTask ConnectAsync(IPAddress remoteIpAddress, int remotePort = 23, CancellationToken cancellationToken = default)
		{
			await base.ConnectAsync(remoteIpAddress, remotePort, cancellationToken);
		}

		public async ValueTask<string> SendLineAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor)
		{
			return await this.SendAsync(text + "\r\n", waitForIgnoreCase, cancellationToken, waitFor);
		}

		public async ValueTask<string> SendAsync(string text, bool waitForIgnoreCase = false, CancellationToken cancellationToken = default, params string[] waitFor)
		{
			if (cancellationToken == default || cancellationToken == CancellationToken.None)
			{
				var tokenSource = new CancellationTokenSource();
				int timeout = (waitFor != null) ? this.Options.SendTimeout + this.Options.ReceiveTimeout : this.Options.SendTimeout;

				tokenSource.CancelAfter(timeout);
				cancellationToken = tokenSource.Token;
			}

			await this.SendAsync(this.Options.Encoding.GetBytes(text), cancellationToken);

			if (waitFor.Length > 0)
				return await this.WaitFor(waitForIgnoreCase, cancellationToken, waitFor);
			else
				return null; // It was nothing to wait for, thus return result is null
		}

		public override async ValueTask SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var dataToSend = this.protocolHandler.Transpose(buffer.ToArray());

			await base.SendAsync(dataToSend, cancellationToken);
		}

		//public async ValueTask SendAsync(string text, CancellationToken cancellationToken = default)
		//{
		//	if (!this.Connected)
		//		throw new Exception("The client is not connected.");

		//	var data = this.protocolHandler.Transpose(this.Options.Encoding.GetBytes(text));

		//	await this.SendAsync(data, cancellationToken);
		//}

		// TODO: Consider using two CancelationToken sources: Timeout for no endpoint response and click on from button disconnect, for example!!!
		// But it depense of user implementation of input cancellationToken, and maybe here we do not need to wory about additional timeout cancelation tokens.
		//
		// var parentCts = new CancellationTokenSource();
		// var childCts = CancellationTokenSource.CreateLinkedTokenSource(parentCts.Token);
		// childCts.CancelAfter(1000); // Child is for timeout
		// parentCts.Cancel(); // parent is for form button cancel, for example

		public async ValueTask<string> WaitFor(bool ignoreCase = false, CancellationToken cancellationToken = default, params string[] waitForText)
		{
			if (waitForText.Length == 0) // No nothing to wait for
				return null;

			// WaitFor makes no sense to call from multiple threads simultaneously.
			// So the parameters here is passed as local class variables when processing in OnDataReceive method
			this.ignoreCase = ignoreCase;
			this.waitForList = waitForText;

			string receivedText = String.Empty;

			if (cancellationToken == default || cancellationToken == CancellationToken.None)
			{
				var tokenSource = new CancellationTokenSource();

				tokenSource.CancelAfter(this.Options.ReceiveTimeout);
				cancellationToken = tokenSource.Token;
			}

			if (this.IsWaitForReceived())
			{
				receivedText = this.stringBuilder.ToString();
			}
			else if (await this.waitForFoundResetEvent.WaitOneAsync(cancellationToken)) // WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle, this.waitForFoundResetEvent });
			{
				receivedText = this.stringBuilder.ToString();
			}

			this.waitForList = null;
			this.stringBuilder.Clear();

			return receivedText;
		}

		protected override void OnDataReceive(ref SequenceReader<byte> sequenceReader)
		{
			byte[] buffer = sequenceReader.Sequence.ToArray();

			this.protocolHandler.InputFeed(buffer, buffer.Length);
			
			int bytesOfText = this.protocolHandler.Negotiate(buffer);
			
			if (bytesOfText > 0)
			{
				string receivedText = this.Options.Encoding.GetString(buffer, 0, bytesOfText);

				this.TextReceived?.Invoke(receivedText).ConfigureAwait(false);

				if (this.IsWaitForReceived())
				{
					this.waitForFoundResetEvent.Set(); // Unblock curent WaitFor
					this.waitForFoundResetEvent.Reset(); // Sets the state of the event to nonsignaled, causing threads to block for another WaitFor calls
				}
			}

			sequenceReader.Advance(buffer.LongLength); // We consume all given data
		}

		protected override void OnConnect()
		{
			base.OnConnect();
			
			this.stringBuilder.Clear();
			this.protocolHandler.Reset();
			this.waitForFoundResetEvent.Close();
		}

		protected override void OnClosed() => base.OnClosed();

		// TODO: Check is this telnet EOF
		protected override void WriteEOF() => this.SendAsync(TelnetProtocol.IACBRK).ConfigureAwait(false);

		private bool IsWaitForReceived()
		{
			if (this.waitForList == null)
				return false;

			string receivedText = this.stringBuilder.ToString();

			return this.waitForList.Any((waitFor) => receivedText.Contains(waitFor, this.ignoreCase));
		}
	}
}
