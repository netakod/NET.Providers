

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Thought.Net.Telnet
{

    /// <summary>
    ///     Listens for connections from Telnet clients.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The TelnetListener class provides simple methods for accepting incoming
    ///         Telnet connections.  It is modeled after the standard TcpListener class
    ///         and operates in much the same manner.
    ///     </para>
    /// </remarks>
    public class TelnetListener
    {

        private bool isRunning;   // Listener running?
        private TcpListener tcpListener; // Listener object

        // Events:

        private TelnetListenerEventHandler started;
        private TelnetListenerEventHandler starting;
        private TelnetListenerEventHandler stopped;
        private TelnetListenerEventHandler stopping;

        // Event object locks:

        private object startedLock = new object();
        private object startingLock = new object();
        private object stoppedLock = new object();
        private object stoppingLock = new object();


        /// <summary>
        ///     Initializes a new instance of the TcpListener class
        ///     with the specified local endpoint.
        /// </summary>
        /// <param name="localEndPoint">
        ///     An IPEndPoint that represents the local endpoint to which
        ///     to bind the listener.
        /// </param>
        public TelnetListener(IPEndPoint localEndPoint)
        {

            if (localEndPoint == null)
                throw new ArgumentNullException("localEndPoint");

            this.tcpListener = new TcpListener(localEndPoint);

        }


        /// <summary>
        ///     Initalizes a new instance of the TelnetListener class
        ///     that listens for incoming network connection attempts
        ///     on the specified IP address and port number.
        /// </summary>
        /// <param name="localAddress">
        ///     A local IP address.
        /// </param>
        /// <param name="port">
        ///     The port on which to listen for incoming connection attempts.
        /// </param>
        public TelnetListener(IPAddress localAddress, int port)
        {

            if (localAddress == null)
                throw new ArgumentNullException("localAddress");

            if ((port < IPEndPoint.MinPort) || (port > IPEndPoint.MaxPort))
                throw new ArgumentOutOfRangeException("port");

            this.tcpListener = new TcpListener(localAddress, port);

        }


        /// <summary>
        ///     Accepts an incoming Telnet connection and returns a corresponding
        ///     stream object.
        /// </summary>
        public TelnetClient Accept()
        {

            // This method can only be called when the listener
            // is running.  Raise an exception if stopped.

            if (!this.isRunning)
                throw new InvalidOperationException();

            // Wait for the next socket connection.  The AcceptSocket
            // method is blocking call (it does not return until a socket
            // is available, or an error occurs).
            //
            // Since this call is running on a listener thread, it may
            // be aborted at any time (due to the listener being stopped).

            Socket socket = tcpListener.AcceptSocket();

            // Create a new connection driver that provides
            // a low-level I/O interface to the connection.

            return new TelnetClient(socket);

        }


        /// <summary>
        ///     Gets the underlying EndPoint of the telnet listener.
        /// </summary>
        public EndPoint LocalEndPoint
        {
            get
            {
                return this.tcpListener.LocalEndpoint;
            }
        }


        /// <summary>
        ///     Raises the <see cref="Started"/> event.
        /// </summary>
        protected void OnStarted()
        {
            TelnetListenerEventHandler handler = this.started;
            if (started != null)
            {
                started(this, new EventArgs());
            }
        }


        /// <summary>
        ///     Raises the <see cref="Starting"/> event.
        /// </summary>
        protected void OnStarting()
        {
            TelnetListenerEventHandler handler = this.starting;
            if (starting != null)
            {
                starting(this, new EventArgs());
            }
        }


        /// <summary>
        ///     Raises the <see cref="Stopped"/> event.
        /// </summary>
        protected void OnStopped()
        {
            TelnetListenerEventHandler handler = this.stopped;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }


        /// <summary>
        ///     Raises the <see cref="Stopping"/> event.
        /// </summary>
        protected void OnStopping()
        {
            TelnetListenerEventHandler handler = this.stopping;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        ///     Determines if there are pending connection requests.
        /// </summary>
        public bool Pending
        {
            get
            {
                return this.tcpListener.Pending();
            }
        }


        /// <summary>
        ///     Starts listening for incoming connection requests.
        /// </summary>
        public void Start()
        {

            if (this.isRunning)
            {
                // The caller attempt to call the .Start method
                // when the listener was already started.  Raise
                // an exception.

                throw new InvalidOperationException();
            }

            OnStarting();
            this.tcpListener.Start();

            // The code reached this point because all other
            // initializations have been successful.  Mark the
            // listener as running.

            this.isRunning = true;

            OnStarted();

        }


        /// <summary>
        ///     Occurs when the listener is started.
        /// </summary>
        public event TelnetListenerEventHandler Started
        {
            add
            {
                lock (this.startedLock)
                {
                    this.started += value;
                }
            }
            remove
            {
                lock (this.startedLock)
                {
                    this.started -= value;
                }
            }
        }


        /// <summary>
        ///     Occurs when the listener is starting.
        /// </summary>
        public event TelnetListenerEventHandler Starting
        {
            add
            {
                lock (this.startingLock)
                {
                    this.starting += value;
                }
            }
            remove
            {
                lock (this.startingLock)
                {
                    this.starting -= value;
                }
            }
        }


        /// <summary>
        ///     Stops the telnet listener.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Existing connections already established by the
        ///         listener are not disconnected.
        ///     </para>
        /// </remarks>
        public void Stop()
        {

            if (this.isRunning)
            {

                OnStopping();

                if (tcpListener != null)
                {
                    tcpListener.Stop();
                }

                // The shutdown code was successful.  Mark
                // the listener as not running.

                this.isRunning = false;

                OnStopped();

            }

        }


        /// <summary>
        ///     Occurs when the listener has stopped.
        /// </summary>
        public event TelnetListenerEventHandler Stopped
        {
            add
            {
                lock (this.stoppedLock)
                {
                    this.stopped += value;
                }
            }
            remove
            {
                lock (this.stoppedLock)
                {
                    this.stopped -= value;
                }
            }
        }


        /// <summary>
        ///     Occurs when the listener is stopping.
        /// </summary>
        public event TelnetListenerEventHandler Stopping
        {
            add
            {
                lock (this.stoppingLock)
                {
                    this.stopping += value;
                }
            }
            remove
            {
                lock (this.stoppingLock)
                {
                    this.stopping -= value;
                }
            }
        }

    }
}
