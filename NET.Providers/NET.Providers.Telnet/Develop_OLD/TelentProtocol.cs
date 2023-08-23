using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace NET.Tools.Telnet_DevelopOld
{
	public class TelnetProtocol
	{
		#region |   Private Memebers   |

		/// <summary>
		/// The telnet client used to send commands
		/// </summary>
		private ITelnetClient telnetClient = null;
		
		/// <summary>
		/// temporary buffer for data-telnetstuff-data transformation
		/// </summary>
		private byte[] tempBuffer = new byte[0];

		/// <summary>
		/// the data sent on pressing [RETURN] \n
		/// </summary>
		private byte[] crlf = new byte[2];

		/// <summary>
		/// the data sent on pressing [LineFeed] \r
		/// </summary>
		private byte[] cr = new byte[2];


		/// <summary>
		///  The default buffer size
		/// </summary>
		private const int BufferSize = 256;

		/// <summary>
		/// state variable for telnet negotiation reader
		/// </summary>
		private byte negotiationState = 0;

		/// <summary>
		/// What IAC SB we are handling right now
		/// </summary>
		private byte currentSB;

		/// <summary>
		/// What IAC DO(NT) request do we have received already ?
		/// </summary>
		private byte[] receivedDX;
		
		/// <summary>
		/// What IAC WILL/WONT request do we have received already ?
		/// </summary>
		private byte[] receivedWX;
		
		/// <summary>
		/// What IAC DO/DONT request do we have sent already ?
		/// </summary>
		private byte[] sentDX;
		
		/// <summary>
		/// What IAC WILL/WONT request do we have sent already ?
		/// </summary>
		private byte[] sentWX;

		#endregion |   Private Memebers   |

		#region |   Constructor & Initialization    |

		/// <summary>
		/// Create a new telnet protocol handler.
		/// </summary>
		public TelnetProtocol(ITelnetClient telnetClient)
		{
			this.telnetClient = telnetClient;

			this.Reset();

			this.crlf[0] = 13;
			this.crlf[1] = 10;
			this.cr[0] = 13;
			this.cr[1] = 0;
		}

		#endregion |    Constructor & Initialization    |

		#region |   Public Methods   |

		/// <summary>
		/// Adds bytes to the input buffer we'll parse for codes.
		/// </summary>
		/// <param name="b">Bytes array from which to add.</param>
		/// <param name="len">Number of bytes to add.</param>
		public void InputFeed(byte[] b, int len)
		{
			// TODO: this.tempBuffer change to Memory<byte> ??? and input to ReadOnlySequence<byte>

			byte[] bytesTmp = new byte[this.tempBuffer.Length + len];

			Array.Copy(this.tempBuffer, 0, bytesTmp, 0, this.tempBuffer.Length);
			Array.Copy(b, 0, bytesTmp, this.tempBuffer.Length, len);

			this.tempBuffer = bytesTmp;
		}

		/// <summary>
		/// Reset the protocol handler. This may be necessary after the
		/// connection was closed or some other problem occured.
		/// </summary>
		public void Reset()
		{
			this.negotiationState = 0;
			this.receivedDX = new byte[BufferSize];
			this.sentDX = new byte[BufferSize];
			this.receivedWX = new byte[BufferSize];
			this.sentWX = new byte[BufferSize];
		}

		#endregion |   Public Methods   |

		#region |   Public Properties    |

		/// <summary>
		/// Gets or sets the data sent on pressing [RETURN] \n
		/// </summary>
		public string CRLF { get => Encoding.ASCII.GetString(this.crlf); set => this.crlf = Encoding.ASCII.GetBytes(value); }

		/// <summary>
		/// Gets or sets the data sent on pressing [LineFeed] \r
		/// </summary>
		public string CR { get => Encoding.ASCII.GetString(this.cr); set => this.cr = Encoding.ASCII.GetBytes(value); }

		#endregion |  Public Properties   |

		#region |   Protected Properties    |

		/// <summary>
		/// The current terminal type for TTYPE telnet option.
		/// </summary>
		protected string terminalType = "dumb";

		/// <summary>
		/// The window size of the terminal for the NAWS telnet option.
		/// </summary>
		protected Size windowSize = new Size(80, 25); // Size.Empty;

		/// <summary>
		/// Set the local echo option of telnet.
		/// </summary>
		/// <param name="echo">true for local echo, false for no local echo</param>
		protected virtual void SetLocalEcho(bool echo) { }

		/// <summary>
		/// Generate an EOR (end of record) request. For use by prompt displaying.
		/// </summary>
		protected virtual void NotifyEndOfRecord() { }

		#endregion |   Protected Properties    |

		#region |   Telnet Protocol Codes   |

		// constants for the negotiation state
		public const byte STATE_DATA = 0;
		public const byte STATE_IAC = 1;
		public const byte STATE_IACSB = 2;
		public const byte STATE_IACWILL = 3;
		public const byte STATE_IACDO = 4;
		public const byte STATE_IACWONT = 5;
		public const byte STATE_IACDONT = 6;
		public const byte STATE_IACSBIAC = 7;
		public const byte STATE_IACSBDATA = 8;
		public const byte STATE_IACSBDATAIAC = 9;

		/// <summary>
		/// IAC - init sequence for telnet negotiation.
		/// </summary>
		public const byte IAC = 255;

		/// <summary>
		/// [IAC] DONT
		/// </summary>
		public const byte DONT = 254;

		/// <summary>
		/// [IAC] DO
		/// </summary>
		public const byte DO = 253;

		/// <summary>
		/// [IAC] WONT
		/// </summary>
		public const byte WONT = 252;

		/// <summary>
		/// [IAC] WILL
		/// </summary>
		public const byte WILL = 251;

		/// <summary>
		/// [IAC] Sub Begin
		/// </summary>
		public const byte SB = 250;

		/// <summary>
		/// [IAC] Go Ahead (GA)
		/// </summary>
		public const byte GoAhead = 249;

		/// <summary>
		/// [IAC] Erase Line (EL)
		/// </summary>
		/// <seealso cref="EraseCharacter"/>
		public const byte EL = 248;

		/// <summary>
		/// [IAC] Erase Character (EC)
		/// </summary>
		/// <seealso cref="EraseLine"/>
		public const byte EC = 247;

		/// <summary>
		/// [IAC] Are You There (AYT)
		/// </summary>
		public const byte AYT = 246;

		/// <summary>
		/// [IAC] Abort output (AO)
		/// </summary>
		public const byte AO = 245;

		/// <summary>
		/// [IAC] Interrupt Process (IP)
		/// </summary>
		public const byte IP = 244;

		/// <summary>
		/// [IAC] Break (BRK)
		/// </summary>
		public const byte BRK = 243;

		/// <summary>
		/// [IAC] Data Mark
		/// </summary>
		public const byte DM = 242;

		/// <summary>
		/// [IAC] No operation (NOP)
		/// </summary>
		public const byte NOP = 241;

		/// <summary>
		/// [IAC] Sub End
		/// </summary>
		public const byte SE = 240;

		/// <summary>
		/// [IAC] End Of Record
		/// </summary>
		public const byte EOR = 239;

		/// <summary>
		/// Telnet option: binary mode
		/// </summary>
		public const byte TELOPT_BINARY = 0;  /* binary mode */
		/// <summary>
		/// Telnet option: echo text
		/// </summary>
		public const byte TELOPT_ECHO = 1;  /* echo on/off */
		/// <summary>
		/// Telnet option: sga
		/// </summary>
		public const byte TELOPT_SGA = 3;  /* supress go ahead */
		/// <summary>
		/// Telnet option: End Of Record
		/// </summary>
		public const byte TELOPT_EOR = 25; /* end of record */
		/// <summary>
		/// Telnet option: Negotiate About Window Size
		/// </summary>
		public const byte TELOPT_NAWS = 31; /* NA-WindowSize*/
		/// <summary>
		/// Telnet option: Terminal Type
		/// </summary>
		public const byte TELOPT_TTYPE = 24;  /* terminal type */

		public static byte[] IACWILL = { IAC, WILL };
		public static byte[] IACWONT = { IAC, WONT };
		public static byte[] IACDO   = { IAC, DO };
		public static byte[] IACDONT = { IAC, DONT };
		public static byte[] IACSB   = { IAC, SB };
		public static byte[] IACSE   = { IAC, SE };
		public static byte[] IACBRK  = { IAC, BRK };

		/// <summary>
		/// Telnet option qualifier 'IS'
		/// </summary>
		public static byte TELQUAL_IS = (byte)0;
		/// <summary>
		/// Telnet option qualifier 'SEND'
		/// </summary>
		public static byte TELQUAL_SEND = (byte)1;

		#endregion |   Telnet Protocol Codes   |

		#region |   The actual negotiation handling for the telnet protocol   |

		/// <summary>
		/// Send a Telnet Escape character
		/// </summary>
		/// <param name="code">IAC code</param>
		protected void SendTelnetControl(byte code)
		{
			byte[] b = new byte[2];

			b[0] = IAC;
			b[1] = code;
			
			this.Send(b);
		}

		/// <summary>
		/// Transpose special telnet codes like 0xff or newlines to values
		/// that are compliant to the protocol. This method will also send
		/// the buffer immediately after transposing the data.
		/// </summary>
		/// <param name="buf">the data buffer to be sent</param>
		public ReadOnlyMemory<byte> Transpose(byte[] buf)
		{
			int i;

			byte[] nbuf, xbuf;
			int nbufptr = 0;
			nbuf = new byte[buf.Length * 2];

			for (i = 0; i < buf.Length; i++)
			{
				switch (buf[i])
				{
					// Escape IAC twice in stream ... to be telnet protocol compliant
					// this is there in binary and non-binary mode.
					case IAC:
						
						nbuf[nbufptr++] = IAC;
						nbuf[nbufptr++] = IAC;
						
						break;
					
					// We need to heed RFC 854. LF (\n) is 10, CR (\r) is 13
					// we assume that the Terminal sends \n for lf+cr and \r for just cr
					// linefeed+carriage return is CR LF  
					case 10:    // \n
						
						if (this.receivedDX[TELOPT_BINARY + 128] != DO)
						{
							while (nbuf.Length - nbufptr < this.crlf.Length)
							{
								xbuf = new byte[nbuf.Length * 2];
								Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
								nbuf = xbuf;
							}
							
							for (int j = 0; j < this.crlf.Length; j++)
								nbuf[nbufptr++] = this.crlf[j];
							
							break;
						}
						else
						{
							// copy verbatim in binary mode.
							nbuf[nbufptr++] = buf[i];
						}
						
						break;
					
					// carriage return is CR NUL */ 
					case 13:    // \r
						
						if (this.receivedDX[TELOPT_BINARY + 128] != DO)
						{
							while (nbuf.Length - nbufptr < this.cr.Length)
							{
								xbuf = new byte[nbuf.Length * 2];
								Array.Copy(nbuf, 0, xbuf, 0, nbufptr);
								nbuf = xbuf;
							}
							for (int j = 0; j < this.cr.Length; j++)
								nbuf[nbufptr++] = this.cr[j];
						}
						else
						{
							// copy verbatim in binary mode.
							nbuf[nbufptr++] = buf[i];
						}
						
						break;
					
					// all other characters are just copied
					default:
						
						nbuf[nbufptr++] = buf[i];
						
						break;
				}
			}
			
			//xbuf = new byte[nbufptr];
			//Array.Copy(nbuf, 0, xbuf, 0, nbufptr);

			//this.Send(xbuf);

			return new ReadOnlyMemory<byte>(nbuf, 0, nbufptr); 
		}

		/// <summary>
		/// Handle telnet protocol negotiation. The buffer will be parsed
		/// and necessary actions are taken according to the telnet protocol.
		/// <see cref="RFC-Telnet"/>
		/// </summary>
		/// <param name="nbuf">the byte buffer used for negotiation</param>
		/// <returns>a new buffer after negotiation</returns>
		public int Negotiate(byte[] nbuf)
		{
			int count = this.tempBuffer.Length;
			
			if (count == 0)     // buffer is empty.
				return -1;

			byte[] sendbuf = new byte[3];
			byte[] sbbuf = new byte[this.tempBuffer.Length];
			byte[] buf = this.tempBuffer;

			byte b;
			byte reply;

			int sbcount = 0;
			int boffset = 0;
			int noffset = 0;

			bool done = false;
			bool foundSE = false;

			//bool dobreak = false;

			while (!done && (boffset < count && noffset < nbuf.Length))
			{
				b = buf[boffset++];

				// of course, byte is a signed entity (-128 -> 127)
				// but apparently the SGI Netscape 3.0 doesn't seem
				// to care and provides happily values up to 255
				if (b >= 128)
					b = (byte)((int)b - 256);

				switch (this.negotiationState)
				{
					case STATE_DATA:
						
						if (b == IAC)
						{
							this.negotiationState = STATE_IAC;
							//dobreak = true; // leave the loop so we can sync.
						}
						else
						{
							nbuf[noffset++] = b;
						}

						break;

					case STATE_IAC:
						
						switch (b)
						{
							case IAC:
								
								this.negotiationState = STATE_DATA;
								nbuf[noffset++] = IAC; // got IAC, IAC: set option to IAC
								
								break;

							case WILL:
								
								this.negotiationState = STATE_IACWILL;
								
								break;

							case WONT:
								
								this.negotiationState = STATE_IACWONT;
								
								break;

							case DONT:
								
								this.negotiationState = STATE_IACDONT;
								
								break;

							case DO:
								
								this.negotiationState = STATE_IACDO;
								
								break;

							case EOR:
								
								this.NotifyEndOfRecord();
								//dobreak = true; // leave the loop so we can sync.
								this.negotiationState = STATE_DATA;
								
								break;

							case SB:
								
								this.negotiationState = STATE_IACSB;
								sbcount = 0;
								
								break;

							default:
								
								this.negotiationState = STATE_DATA;
								
								break;
						}

						break;

					case STATE_IACWILL:
						
						switch (b)
						{
							case TELOPT_ECHO:
								
								reply = DO;
								this.SetLocalEcho(false);
								
								break;

							case TELOPT_SGA:
								
								reply = DO;
								
								break;

							case TELOPT_EOR:
								
								reply = DO;
								
								break;

							case TELOPT_BINARY:
								
								reply = DO;
								
								break;

							default:
								
								reply = DONT;
								
								break;
						}

						if (reply != this.sentDX[b + 128] || WILL != this.receivedWX[b + 128])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							this.Send(sendbuf);

							this.sentDX[b + 128] = reply;
							this.receivedWX[b + 128] = WILL;
						}

						this.negotiationState = STATE_DATA;
						
						break;

					case STATE_IACWONT:

						switch (b)
						{
							case TELOPT_ECHO:
								
								this.SetLocalEcho(true);
								reply = DONT;
								
								break;

							case TELOPT_SGA:
								
								reply = DONT;
								
								break;

							case TELOPT_EOR:
								
								reply = DONT;
								
								break;

							case TELOPT_BINARY:
								
								reply = DONT;
								
								break;

							default:
								
								reply = DONT;
								
								break;
						}

						if (reply != sentDX[b + 128] || WONT != receivedWX[b + 128])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							this.Send(sendbuf);

							this.sentDX[b + 128] = reply;
							this.receivedWX[b + 128] = WILL;
						}

						this.negotiationState = STATE_DATA;
						
						break;

					case STATE_IACDO:
						
						switch (b)
						{
							case TELOPT_ECHO:
								
								reply = WILL;
								this.SetLocalEcho(true);
								
								break;

							case TELOPT_SGA:
								
								reply = WILL;
								
								break;

							case TELOPT_TTYPE:
								
								reply = WILL;
								
								break;

							case TELOPT_BINARY:
								
								reply = WILL;
								
								break;

							case TELOPT_NAWS:
								
								Size size = this.windowSize;
								
								this.receivedDX[b] = DO;

								//if (size.GetType() != typeof(Size))
								if (size.GetType() != typeof(Size))
								{
									// this shouldn't happen
									this.Send(IAC);
									this.Send(WONT);
									this.Send(TELOPT_NAWS);
									reply = WONT;
									this.sentWX[b] = WONT;
									
									break;
								}

								reply = WILL;
								this.sentWX[b] = WILL;
								sendbuf[0] = IAC;
								sendbuf[1] = WILL;
								sendbuf[2] = TELOPT_NAWS;

								this.Send(sendbuf);
								//this.Send(IAC);
								//this.Send(SB);
								//this.Send(TELOPT_NAWS);
								//this.Send((byte)(size.Width >> 8));
								//this.Send((byte)(size.Width & 0xff));
								//this.Send((byte)(size.Height >> 8));
								//this.Send((byte)(size.Height & 0xff));
								//this.Send(IAC); 
								//this.Send(SE);
								
								break;

							default:
								
								reply = WONT;
								
								break;
						}

						if (reply != sentWX[128 + b] || DO != receivedDX[128 + b])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							this.Send(sendbuf);

							this.sentWX[b + 128] = reply;
							this.receivedDX[b + 128] = DO;
						}

						this.negotiationState = STATE_DATA;
						
						break;

					case STATE_IACDONT:
						
						switch (b)
						{
							case TELOPT_ECHO:
								
								reply = WONT;
								this.SetLocalEcho(false);
								
								break;

							case TELOPT_SGA:
								
								reply = WONT;
								
								break;

							case TELOPT_NAWS:
								
								reply = WONT;
								
								break;

							case TELOPT_BINARY:
								
								reply = WONT;
								
								break;

							default:
								
								reply = WONT;
								
								break;
						}

						if (reply != this.sentWX[b + 128] || DONT != this.receivedDX[b + 128])
						{
							sendbuf[0] = IAC;
							sendbuf[1] = reply;
							sendbuf[2] = b;
							
							this.Send(sendbuf);

							this.sentWX[b + 128] = reply;
							this.receivedDX[b + 128] = DONT;
						}

						this.negotiationState = STATE_DATA;
						
						break;

					case STATE_IACSBIAC:

						// If SE not found in this buffer, move on until we get it
						for (int i = boffset; i < this.tempBuffer.Length; i++)
							if (this.tempBuffer[i] == SE)
								foundSE = true;

						if (!foundSE)
						{
							boffset--;
							done = true;
							
							break;
						}

						foundSE = false;

						if (b == IAC)
						{
							sbcount = 0;
							this.currentSB = b;
							this.negotiationState = STATE_IACSBDATA;
						}
						else
						{
							this.negotiationState = STATE_DATA;
						}
						
						break;

					case STATE_IACSB:

						// If SE not found in this buffer, move on until we get it
						for (int i = boffset; i < this.tempBuffer.Length; i++)
							if (this.tempBuffer[i] == SE)
								foundSE = true;

						if (!foundSE)
						{
							boffset--;
							done = true;
							
							break;
						}

						foundSE = false;

						switch (b)
						{
							case IAC:
								
								this.negotiationState = STATE_IACSBIAC;
								
								break;

							default:
								
								this.currentSB = b;
								sbcount = 0;
								this.negotiationState = STATE_IACSBDATA;
								
								break;
						}
						break;

					case STATE_IACSBDATA:

						// If SE not found in this buffer, move on until we get it
						for (int i = boffset; i < this.tempBuffer.Length; i++)
							if (this.tempBuffer[i] == SE)
								foundSE = true;

						if (!foundSE)
						{
							boffset--;
							done = true;
							
							break;
						}

						foundSE = false;

						switch (b)
						{
							case IAC:
								
								this.negotiationState = STATE_IACSBDATAIAC;
								
								break;
							
							default:
								
								sbbuf[sbcount++] = b;
								
								break;
						}
						break;

					case STATE_IACSBDATAIAC:
						
						switch (b)
						{
							case IAC:
								
								this.negotiationState = STATE_IACSBDATA;
								sbbuf[sbcount++] = IAC;
								
								break;
							
							case SE:
								
								this.HandleSB(currentSB, sbbuf, sbcount);
								this.currentSB = 0;
								this.negotiationState = STATE_DATA;
								
								break;
							
							case SB:
								
								this.HandleSB(currentSB, sbbuf, sbcount);
								this.negotiationState = STATE_IACSB;
								
								break;
							
							default:
								
								this.negotiationState = STATE_DATA;
								
								break;
						}
						break;

					default:
						
						this.negotiationState = STATE_DATA;
						
						break;
				}
			}

			// shrink tempbuf to new processed size.
			byte[] xb = new byte[count - boffset];
			Array.Copy(this.tempBuffer, boffset, xb, 0, count - boffset);
			this.tempBuffer = xb;

			return noffset;
		}

		#endregion |   The actual negotiation handling for the telnet protocol   |

		#region |  Private Methods   |

		/// <summary>
		/// Send data to the remote host.
		/// </summary>
		/// <param name="b">array of bytes to send</param>
		private void Send(byte[] b) => this.telnetClient.SendAsync(b, cancellationToken: default);

		/// <summary>
		/// Send one byte to the remote host.
		/// </summary>
		/// <param name="b">the byte to be sent</param>
		private void Send(byte b) => this.Send(new byte[] { b });

		/// <summary>
		/// Handle an incoming IAC SB type bytes IAC SE
		/// </summary>
		/// <param name="type">type of SB</param>
		/// <param name="sbdata">byte array as bytes</param>
		/// <param name="sbcount">nr of bytes. may be 0 too</param>
		private void HandleSB(byte type, byte[] sbdata, int sbcount)
		{
			switch (type)
			{
				case TELOPT_TTYPE:

					if (sbcount > 0 && sbdata[0] == TELQUAL_SEND)
					{
						this.Send(IACSB);
						this.Send(TELOPT_TTYPE);
						this.Send(TELQUAL_IS);
						/* FIXME: need more logic here if we use 
						* more than one terminal type
						*/
						this.Send(Encoding.ASCII.GetBytes(terminalType));
						this.Send(IACSE);
					}

					break;
			}
		}

		#endregion |  Private Methods   |
	}
}
