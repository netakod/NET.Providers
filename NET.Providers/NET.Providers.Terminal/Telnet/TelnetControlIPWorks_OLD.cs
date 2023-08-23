//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using nsoftware.IPWorks;

//namespace NET.Tools.Terminal
//{
//    public class TelnetControlIPWorks : ITerminalControl, IDisposable
//    {
//        #region |   Private Members   |

//        private Telnet telnet = new Telnet();

//        #endregion |   Private Members   |

//        #region |   Constructors and Initialization   |

//        public TelnetControlIPWorks()
//        {
//            this.telnet.OnDataIn += new Telnet.OnDataInHandler(telnet_OnDataIn);
//            this.telnet.OnConnected += new Telnet.OnConnectedHandler(telnet_OnConnected);
//            this.telnet.OnDisconnected += new Telnet.OnDisconnectedHandler(telnet_OnDisconnected);
//            this.telnet.OnWill += new Telnet.OnWillHandler(telnet_OnWill);
//            this.telnet.OnDo += new Telnet.OnDoHandler(telnet_OnDo);
//        }

//        #endregion |   Constructors and Initialization   |

//        #region |   Events   |

//        public event TerminalTextEventHandler TextReceived;

//        #endregion

//        #region |   Public Properties   |

//        public string RemoteHost
//        {
//            get { return this.telnet.RemoteHost; ; }
//            set { this.telnet.RemoteHost = value; }
//        }

//        public int RemotePort
//        {
//            get { return this.telnet.RemotePort; }
//            set { this.telnet.RemotePort = value; }
//        }

//        /// <summary>
//        /// Timeout in seconds.
//        /// </summary>
//        public int Timeout
//        {
//            get { return this.telnet.Timeout; }
//            set { this.telnet.Timeout = value; }
//        }

//        public bool IsConnected
//        {
//            get { return this.telnet.Connected; }
//        }

//        public bool AcceptData
//        {
//            get { return this.telnet.AcceptData; }
//            set { this.telnet.AcceptData = value; }
//        }

//        #endregion |   Public Properties   |

//        #region |   Public Methods   |

//        public void Connect()
//        {
//            if (this.telnet.Connected)
//            {
//                this.Disconnect();
//            }

//            this.telnet.Connected = true;
//        }

//        public void Disconnect()
//        {
//            this.telnet.Linger = true;
//            this.telnet.Connected = false;
//        }

//        public void Send(string command)
//        {
//            this.telnet.Send(Encoding.ASCII.GetBytes(command));
//        }

//        public void Dispose()
//        {
//            this.telnet.Dispose();
//        }

//        #endregion |   Public Methods   |

//        #region |   Private Methods   |

//        private void telnet_OnDo(object sender, TelnetDoEventArgs e)
//        {
//            try
//            {
//                this.telnet.WontOption = e.OptionCode;
//            }
//            catch (IPWorksException)
//            {
//            }
//        }

//        private void telnet_OnWill(object sender, TelnetWillEventArgs e)
//        {
//            try
//            {
//                if (e.OptionCode == 38)
//                {
//                    this.telnet.DontOption = 38;
//                }
//            }
//            catch (IPWorksException)
//            {
//            }
//        }

//        private void telnet_OnDisconnected(object sender, TelnetDisconnectedEventArgs e)
//        {
//        }

//        private void telnet_OnConnected(object sender, TelnetConnectedEventArgs e)
//        {
//            this.telnet.AcceptData = true;
//        }

//        private void telnet_OnDataIn(object sender, TelnetDataInEventArgs e)
//        {
//            this.RaiseTextReceived(e.Text);
//        }

//        #endregion |   Private Methods   |

//        #region |   Private Raise Events Methods   |

//        private void RaiseTextReceived(string text)
//        {
//            if (this.TextReceived != null)
//            {
//                this.TextReceived(this, new TerminalTextEventArgs(text));
//            }
//        }

//        #endregion |   Private Raise Events Methods   |
//    }
//}
