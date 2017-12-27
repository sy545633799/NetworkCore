using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ExitGames.Logging;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// The PGM listener receives PGM messages from one or more network interfaces.
    ///   This class is used internally by the <see
    ///   cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/>.
    /// </summary>
    public sealed class PgmListener : IDisposable
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The local end point.
        /// </summary>
        private readonly IPEndPoint localEndPoint;

        /// <summary>
        /// A lock sync root.
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// The receive interfaces.
        /// </summary>
        private readonly IPAddressCollection receiveInterfaces = new IPAddressCollection();

        /// <summary>
        /// The receive buffer size.
        /// </summary>
        private int receiveBufferSize = 0x20000;

        /// <summary>
        /// Indicates whether to reuse the local address.
        /// </summary>
        private bool reuseAddress = true;

        /// <summary>
        /// Indicates wheter the instance is listening.
        /// </summary>
        private bool running;

        /// <summary>
        /// The used socket.
        /// </summary>
        private PgmSocket socket;

        /// <summary>
        /// The used socket async event args.
        /// </summary>
        private SocketAsyncEventArgs socketAsyncEventArgs;

        /// <summary>
        /// Indicates whether to use high speed intranet.
        /// </summary>
        private bool useHighSpeedIntranet = true;

        /// <summary>
        /// The accept socket event. 
        /// </summary>
        public event EventHandler<SocketAsyncEventArgs> AcceptSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/> class.
        /// </summary>
        /// <param name="endPoint">The multicast end point.</param>
        public PgmListener(IPEndPoint endPoint)
        {
            localEndPoint = endPoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/> class.
        /// </summary>
        /// <param name="address">The multicast address.</param>
        /// <param name="port">The port.</param>
        public PgmListener(IPAddress address, int port) : this(new IPEndPoint(address, port)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/> class.
        /// </summary>
        /// <param name="address">The multicast address.</param>
        /// <param name="port">The port.</param>
        public PgmListener(string address, int port)
        {
            IPAddress address2 = IPAddress.Parse(address);
            if (address2 == null)
            {
                throw new ArgumentOutOfRangeException("address", string.Format("Invalid IpAdress '{0}'.", address));
            }
            localEndPoint = new IPEndPoint(address2, port);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/> class.
        /// </summary>
        ~PgmListener()
        {
            Dispose(false);
        }

        /// <summary>
        /// Starts the listening.
        /// </summary>
        ///             <exception cref="T:System.InvalidOperationException">
        ///        Already running.
        ///     </exception>
        ///     <exception cref="T:System.Net.Sockets.SocketException">
        ///      Exception from the underlying <see cref="T:System.Net.Sockets.Socket"/>.
        ///     </exception>
        public void Start()
        {
            try
            {
                lock (_lock)
                {
                    if (running)
                    {
                        throw new InvalidOperationException("Listener is allready started.");
                    }
                    socket = new PgmSocket() { ReceiveBufferSize = receiveBufferSize };
                    socket.SetReuseAddress(ReuseAddress);
                    socket.Bind(LocalEndPoint);
                    socket.AddReceiveInterfaces(receiveInterfaces);
                    if (UseHighSpeedIntranet)
                    {
                        socket.SetHighSpeedIntranetOption(UseHighSpeedIntranet);
                    }
                    socket.Listen(5);
                    socketAsyncEventArgs = new SocketAsyncEventArgs();
                    socketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptAsyncCompleted);
                    running = true;
                }
                BeginAccept();
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Listening on {0}", LocalEndPoint);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (SocketException exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error(SocketHelper.FormatSocketException(exception), exception);
                }
                throw;
            }
        }

        /// <summary>
        /// Stops the listening and closes the socket.
        /// </summary>
        public void Stop()
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Stopp listening on {0}", LocalEndPoint);
            }
            lock (_lock)
            {
                running = false;
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
            }
        }

        /// <summary>
        /// Disposes the listener.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

        }

        /// <summary>
        /// Disposes the listener.
        /// </summary>
        /// <param name="flag1"> The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    running = false;
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Disposing PgmListener for endpoint {0}.", LocalEndPoint);
                    }
                    if (socket != null)
                    {
                        socket.Close();
                        socket = null;
                    }
                    if (socketAsyncEventArgs != null)
                    {
                        socketAsyncEventArgs.Dispose();
                        socketAsyncEventArgs = null;
                    }
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous operation to accept an incoming connection attempt.
        /// </summary>
        private void BeginAccept()
        {
            bool willRaiseEvent;

            lock (_lock)
            {
                if (!running)
                {
                    return;
                }
                // socket must be cleared since the context object is being reused
                socketAsyncEventArgs.AcceptSocket = null;
                // Socket.AcceptAsync begins asynchronous operation to accept the connection.
                // Note the listening socket will pass info to the SocketAsyncEventArgs
                // object that has the Socket that does the accept operation.
                // If you do not create a Socket object and put it in the SAEA object
                // before calling AcceptAsync and use the AcceptSocket property to get it,
                // then a new Socket object will be created for you by .NET.
                willRaiseEvent = socket.AcceptAsync(socketAsyncEventArgs);
            }
            if (!willRaiseEvent)
            {
                OnAcceptAsyncCompleted(null, socketAsyncEventArgs);
            }
        }

        /// <summary>
        /// Accept async completed event callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnAcceptAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            // This is when there was an error with the accept op. That should NOT
            // be happening often. It could indicate that there is a problem with
            // that socket. If there is a problem, then we would have an infinite
            // loop here, if we tried to reuse that same socket.
            if (OnAcceptCompleted(e))
            {
                BeginAccept();
            }
        }

        /// <summary>
        /// Accept completed event callback.
        /// </summary>
        /// <param name="e">The event args.</param>
        /// <returns>True if socket is still open.</returns>
        private bool OnAcceptCompleted(SocketAsyncEventArgs e)
        {
            try
            {
                SocketError error = e.SocketError;
                if (error != SocketError.Success)
                {
                    if (error == SocketError.OperationAborted)
                    {
                        return false;
                    }
                    if (error != SocketError.ConnectionReset)
                    {
                        throw new SocketException((int)e.SocketError);
                    }
                }
                else
                {
                    OnAcceptSocket(e);
                    return true;
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Connection was reset by remote peer {0} at end point {1}", e.AcceptSocket, LocalEndPoint);
                }
                return true;
            }
            catch (SocketException exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Accept failed, " + SocketHelper.FormatSocketException(exception), exception);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception2)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Accept failed, EndPoint=" + localEndPoint, exception2);
                }
            }
            return false;
        }

        /// <summary>
        /// Accept socket event callback.
        /// </summary>
        /// <param name="e">The event args.</param>
        private void OnAcceptSocket(SocketAsyncEventArgs e)
        {
            EventHandler<SocketAsyncEventArgs> handler = AcceptSocket;
            if (handler != null)
            {
                handler(this, e);
            }
        }



        /// <summary>
        /// Gets a value indicating whether the listener is running.
        /// </summary>
        /// <value>True if listening, otherwise false.</value>
        public bool IsRunning
        {
            get
            {
                return running;
            }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Net.IPEndPoint"/> that contains the local IP address 
        ///  and port number on which the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/> will listen 
        /// for new connections.
        /// </summary>
        /// <value>The local <see cref="T:System.Net.IPEndPoint"/>.</value>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return localEndPoint;
            }
        }

        /// <summary>
        /// Gets or sets the size in bytes of the receive buffer of the <see cref="T:System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <value>The default value is 128 KB.</value>
        public int ReceiveBufferSize
        {
            get
            {
                return receiveBufferSize;
            }
            set
            {
                receiveBufferSize = value;
            }
        }

        /// <summary>
        /// Gets the list of interfaces that the instance receives messages from.
        /// If no interface is specified the first local interface enumerated is used.
        /// </summary>
        /// <value>An <see cref="T:ExitGames.Net.IPAddressCollection"/> that contains the IPs of all listening interfaces.</value>
        public IPAddressCollection ReceiveInterfaces
        {
            get
            {
                return receiveInterfaces;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the receiver socket is allowed to be bound to an address that is already in use.
        /// </summary>
        /// <value>Default is true.</value>
        public bool ReuseAddress
        {
            get
            {
                return reuseAddress;
            }
            set
            {
                reuseAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a high bandwidth LAN (100Mbps+) connection is used.
        /// </summary>
        /// <value>Default is true.</value>
        public bool UseHighSpeedIntranet
        {
            get
            {
                return useHighSpeedIntranet;
            }
            set
            {
                useHighSpeedIntranet = value;
            }
        }
    }
}
