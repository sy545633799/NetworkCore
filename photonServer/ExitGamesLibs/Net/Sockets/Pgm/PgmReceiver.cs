using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ExitGames.Logging;
using ExitGames.Net.Sockets;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// This PGM receiver is a <see cref="T:ExitGames.Net.Sockets.ISocketReceiver"/> implementation that wraps a <see
    /// cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/>.
    /// </summary>
    public sealed class PgmReceiver : IDisposable, ISocketReceiver
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The sync root for dispose.
        /// </summary>
        private readonly object disposeLock = new object();

        /// <summary>
        /// A dictionary for <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver.AsyncPgmSocketReceiver"/> instances.
        /// </summary>
        private readonly Dictionary<EndPoint, AsyncPgmSocketReceiver> readerDictionary = new Dictionary<EndPoint, AsyncPgmSocketReceiver>();

        /// <summary>
        /// The number of bytes received.
        /// </summary>
        private long bytesReceived;

        /// <summary>
        /// The <see cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/>.
        /// </summary>
        private PgmListener listener;

        /// <summary>
        /// The read buffer size.
        /// </summary>
        private int readBufferSize = 0x10000;

        /// <summary>
        /// The receive buffer size.
        /// </summary>
        private int receiveBufferSize = 0x100000;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/> class.
        /// </summary>
        /// <param name="endPoint">The multicast end point.</param>
        public PgmReceiver(IPEndPoint endPoint)
        {
            listener = new PgmListener(endPoint);
            listener.AcceptSocket += new EventHandler<SocketAsyncEventArgs>(PgmListener_OnAcceptSocket);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/> class.
        /// </summary>
        /// <param name="address">The multicast address.</param>
        /// <param name="port">The port.</param>
        public PgmReceiver(IPAddress address, int port) : this(new IPEndPoint(address, port)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/> class.
        /// </summary>
        /// <param name="address">The multicast address.</param>
        /// <param name="port">The port.</param>
        public PgmReceiver(string address, int port) : this(IPAddress.Parse(address), port) { }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/> class.
        /// </summary>
        ~PgmReceiver()
        {
            Dispose(false);
        }

        /// <summary>
        /// Starts the underlying <see cref="T:ExitGames.Net.Sockets.Pgm.PgmListener"/>.
        /// </summary>         
        public void Start()
        {
            listener.ReceiveBufferSize = receiveBufferSize;
            listener.Start();
        }

        /// <summary>
        /// Disposes the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (disposeLock)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Disposing Receiver for endpoint {0}.", listener.LocalEndPoint);
                    }

                    if (listener != null)
                    {
                        listener.AcceptSocket -= new EventHandler<SocketAsyncEventArgs>(PgmListener_OnAcceptSocket);
                        listener.Dispose();
                        listener = null;
                    }

                    lock (readerDictionary)
                    {
                        foreach (AsyncPgmSocketReceiver receiveBufferSize in readerDictionary.Values)
                        {
                            receiveBufferSize.Dispose();
                        }
                        readerDictionary.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Invokes the <see cref="E:ExitGames.Net.Sockets.Pgm.PgmReceiver.Accept"/> event.
        /// </summary>
        /// <param name="remoteEndpoint">The remote end point.</param>
        /// <param name="cancel">Default return value if no event handler is assigned to <see 
        /// cref="E:ExitGames.Net.Sockets.Pgm.PgmReceiver.Accept"/>.
        /// </param>
        /// <returns>True if canceled.</returns>
        private bool OnAccept(EndPoint remoteEndpoint, bool cancel)
        {
            EventHandler<SocketAcceptEventArgs> handler = Accept;
            if (handler != null)
            {
                SocketAcceptEventArgs e = new SocketAcceptEventArgs(remoteEndpoint, cancel);
                handler(this, e);
                return e.Cancel;
            }
            return cancel;
        }

        /// <summary>
        /// Invokes the <see cref="E:ExitGames.Net.Sockets.Pgm.PgmReceiver.Disconnected"/> event.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        /// <param name="error1">The socket error.</param>    
        private void OnDisconnected(EndPoint remoteEndpoint, SocketError socketError)
        {
            EventHandler<SocketDisconnectEventArgs> handler = Disconnected;
            if (handler != null)
            {
                handler(this, new SocketDisconnectEventArgs(remoteEndpoint, socketError));
            }
        }

        /// <summary>
        ///  Called when an <see cref="T:ExitGames.Net.Sockets.AsyncSocketReceiver"/> receives a message.
        /// </summary>
        /// <param name="sender">The <see cref="T:ExitGames.Net.Sockets.AsyncSocketReceiver"/>.</param>
        /// <param name="e">The event args.</param>
        private void OnReaderReceive(AsyncSocketReceiver sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    Interlocked.Exchange(ref bytesReceived, e.BytesTransferred);
                    OnReceive(sender.RemoteEndpoint, e.Buffer, e.Offset, e.BytesTransferred);
                    sender.BeginReceive();
                    return;
                }
                SocketError error = e.SocketError;
                if (error != SocketError.OperationAborted)
                {
                    if (error == SocketError.ConnectionReset)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Connection was reset by remote peer {0}.", sender.RemoteEndpoint);
                        }
                    }
                    else if (error == SocketError.Disconnecting)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Disconnecting on remote endpoint {0}.", sender.RemoteEndpoint);
                        }
                    }
                    else
                    {
                        if (log.IsErrorEnabled)
                        {
                            log.ErrorFormat("Receive failed on {0}: SocketError={1}", sender.RemoteEndpoint, e.SocketError);
                        }
                    }
                }
                else
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OperationAborted for remote endpoint {0}.", sender.RemoteEndpoint);
                    }
                }
                RemoveSocketReceiver(sender);
                OnDisconnected(sender.RemoteEndpoint, e.SocketError);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        /// <summary>
        /// The invoke receive.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="bytesTransferred">The bytes transferred.</param>
        private void OnReceive(EndPoint remoteEndpoint, byte[] buffer, int offset, int bytesTransferred)
        {
            EventHandler<SocketReceiveEventArgs> handler = Receive;
            if (handler != null)
            {
                handler(this, new SocketReceiveEventArgs(remoteEndpoint, buffer, offset, bytesTransferred));
            }
        }

        /// <summary>
        /// The <see cref="E:ExitGames.Net.Sockets.Pgm.PgmListener.AcceptSocket"/> event listener.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void PgmListener_OnAcceptSocket(object sender, SocketAsyncEventArgs e)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Accept connection from {0} on {1}.", e.RemoteEndPoint, listener.LocalEndPoint);
            }
            if (!OnAccept(e.RemoteEndPoint, false))
            {
                AsyncPgmSocketReceiver receiver = new AsyncPgmSocketReceiver(this, e.AcceptSocket, readBufferSize);
                lock (readerDictionary)
                {
                    AsyncPgmSocketReceiver oldReceiver;
                    if (readerDictionary.TryGetValue(receiver.RemoteEndpoint, out oldReceiver))
                    {
                        readerDictionary.Remove(receiver.RemoteEndpoint);
                        oldReceiver.Dispose();
                    }
                    readerDictionary.Add(e.RemoteEndPoint, receiver);
                }
                receiver.BeginReceive();
            }
        }

        /// <summary>
        /// Removes an <see cref="T:ExitGames.Net.Sockets.AsyncSocketReceiver"/>.
        /// </summary>
        /// <param name="socketReceiver">The <see cref="T:ExitGames.Net.Sockets.AsyncSocketReceiver"/>.</param>
        private void RemoveSocketReceiver(AsyncSocketReceiver socketReceiver)
        {
            lock (readerDictionary)
            {
                readerDictionary.Remove(socketReceiver.RemoteEndpoint);
                socketReceiver.Dispose();
            }
        }

        /// <summary>
        /// Event for new PGM connections.
        /// </summary>
        public event EventHandler<SocketAcceptEventArgs> Accept;

        /// <summary>
        /// Event for disconnected PGM connections.
        /// </summary>
        public event EventHandler<SocketDisconnectEventArgs> Disconnected;

        /// <summary>
        /// Event for received PGM messages.
        /// </summary>
        public event EventHandler<SocketReceiveEventArgs> Receive;


        /// <summary>
        ///  Gets the multicast end point.
        /// </summary>
        /// <value>The end point.</value>
        public IPEndPoint EndPoint
        {
            get
            {
                return listener.LocalEndPoint;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the receiver is running.
        /// </summary>
        /// <value>True if listening.</value>
        public bool IsRunning
        {
            get
            {
                return listener.IsRunning;
            }
        }

        /// <summary>
        /// Gets or sets the read buffer size.
        /// </summary>
        /// <value>The buffer size for incoming messages.</value>
        public int ReadBufferSize
        {
            get
            {
                return readBufferSize;
            }
            set
            {
                readBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the size in bytes of the receive buffer of the <see cref="T:System.Net.Sockets.Socket"/>.
        /// </summary>
        /// <value>The receive buffer size of the <see cref="T:System.Net.Sockets.Socket"/>. </value>
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
        ///          If no interface is specified the first local interface enumerated is used.
        /// </summary>
        /// <value>An <see cref="T:ExitGames.Net.IPAddressCollection"/> that contains the IPs of all listening interfaces.</value>
        public IPAddressCollection ReceiveInterfaces
        {
            get
            {
                return listener.ReceiveInterfaces;
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
                return listener.ReuseAddress;
            }
            set
            {
                listener.ReuseAddress = value;
            }
        }

        /// <summary>
        /// Gets or sets the total number of received bytes.
        /// </summary>
        /// <value>The total number of received bytes.</value>
        public long TotalBytesReceived
        {
            get
            {
                return Interlocked.Read(ref bytesReceived);
            }
            set
            {
                Interlocked.Exchange(ref bytesReceived, value);
            }
        }

        /// <summary>
        ///  Gets or sets a value indicating whether a high bandwidth LAN (100Mbps+) connection is used.
        /// </summary>
        /// <value>Default is true.</value>
        public bool UseHighSpeedIntranet
        {
            get
            {
                return listener.UseHighSpeedIntranet;
            }
            set
            {
                listener.UseHighSpeedIntranet = value;
            }
        }

        /// <summary>
        /// The async pgm socket receiver receives messages from another host.
        /// </summary>
        private class AsyncPgmSocketReceiver : AsyncSocketReceiver
        {
            /// <summary>
            /// The parent.
            /// </summary>
            private readonly PgmReceiver pgmReceiver;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver.AsyncPgmSocketReceiver"/> class.
            /// </summary>
            /// <param name="pgmReceiver">The parent pgm receiver.</param>
            /// <param name="socket">The socket.</param>
            /// <param name="bufferSize">The buffer size.</param>
            internal AsyncPgmSocketReceiver(PgmReceiver pgmReceiver, Socket socket, int bufferSize)
                : base(socket, bufferSize)
            {
                this.pgmReceiver = pgmReceiver;
            }

            /// <summary>
            /// Disposes the receiver.
            /// </summary>
            /// <param name="disposing">The disposing.</param>
            protected override void Dispose(bool disposing)
            {
                if (PgmReceiver.log.IsDebugEnabled)
                {
                    PgmReceiver.log.DebugFormat("Disposing PgmSocketReceiver for remote endpoint {0}", RemoteEndpoint);
                }
                try
                {
                    base.Dispose(disposing);
                }
                catch (ThreadAbortException)
                {
                    throw;
                }
                catch (OutOfMemoryException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    if (PgmReceiver.log.IsErrorEnabled)
                    {
                        PgmReceiver.log.Error(exception);
                    }
                }
            }

            /// <summary>
            /// The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed"/> event handler.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="e">The event args.</param>
            protected override void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
            {
                pgmReceiver.OnReaderReceive(this, e);
            }
        }
    }
}
