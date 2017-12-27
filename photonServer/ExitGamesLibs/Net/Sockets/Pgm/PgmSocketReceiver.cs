using System;
using System.Net;

namespace ExitGames.Net.Sockets.Pgm
{
    /// <summary>
    /// This <see cref="T:ExitGames.Net.Sockets.ISocketReceiver"/> wraps a <see 
    /// cref="T:ExitGames.Net.Sockets.PooledReceiver"/> and uses a <
    /// see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/>.
    /// </summary>
    public sealed class PgmSocketReceiver : IDisposable, ISocketReceiver
    {
        /// <summary>
        /// The used <see cref="T:ExitGames.Net.Sockets.PooledReceiver"/>.
        /// </summary>
        public readonly PooledReceiver PooledReceiver;

        /// <summary>
        /// The used <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/>.
        /// </summary>
        public readonly PgmReceiver SocketReceiver;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSocketReceiver"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="receiveInterfaces">The receive Interfaces.</param>
        public PgmSocketReceiver(string ip, int port, params string[] receiveInterfaces)
            : this(ip, port, true, receiveInterfaces)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSocketReceiver"/> class.
        /// </summary>
        /// <param name="ip">The ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="batchedSender">The batched sender.</param>
        /// <param name="receiveInterfaces">The receive Interfaces.</param>
        public PgmSocketReceiver(string ip, int port, bool batchedSender, params string[] receiveInterfaces)
        {
            SocketReceiver = new PgmReceiver(ip, port);
            PooledReceiver = new PooledReceiver(SocketReceiver, batchedSender);
            PooledReceiver.Receive += new EventHandler<SocketReceiveEventArgs>(SocketReceiver_OnReceive);
            if (receiveInterfaces != null)
            {
                foreach (string str in receiveInterfaces)
                {
                    if (!string.IsNullOrEmpty(str))
                    {
                        IPAddress address;
                        if (IPAddress.TryParse(str, out address))
                        {
                            SocketReceiver.ReceiveInterfaces.Add(address);
                        }
                        else
                        {
                            IPHostEntry entry = Dns.GetHostEntry(str);
                            foreach (IPAddress address2 in entry.AddressList)
                            {
                                SocketReceiver.ReceiveInterfaces.Add(address2);
                            }
                        }
                    }
                }
            }
            SocketReceiver.Start();
        }

        /// <summary>
        ///  Finalizes an instance of the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmSocketReceiver"/> class.
        /// </summary>
        ~PgmSocketReceiver()
        {
            Dispose(false);
        }

        /// <summary>
        ///Disposes this instance. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">The disposing.</param>
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                PooledReceiver.Dispose();
                SocketReceiver.Dispose();
            }
        }

        /// <summary>
        /// The invoke receive.
        /// </summary>
        /// <param name="e">The e.</param>
        private void OnReceive(SocketReceiveEventArgs e)
        {
            if (Receive != null)
            {
                Receive(this, e);
            }
        }

        /// <summary>
        /// The socket receiver_ receive.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void SocketReceiver_OnReceive(object sender, SocketReceiveEventArgs e)
        {
            OnReceive(e);
        }

        /// <summary>
        /// The receive event.
        /// </summary>
        public event EventHandler<SocketReceiveEventArgs> Receive;

        /// <summary>
        /// Gets the multicast end point.
        /// </summary>
        /// <value>The end point.</value>
        public IPEndPoint EndPoint
        {
            get
            {
                return SocketReceiver.EndPoint;
            }
        }

        /// <summary>
        /// Gets or sets the totanl bytes received.
        /// </summary>
        /// <value>The total bytes received.</value>
        public long TotalBytesReceived
        {
            get
            {
                return SocketReceiver.TotalBytesReceived;
            }
            set
            {
                SocketReceiver.TotalBytesReceived = value;
            }
        }
    }
}
