using System;
using System.Net;
using System.Net.Sockets;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// The socket disconnect event args.
    /// Used by the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/>.
    /// </summary>
    public sealed class SocketDisconnectEventArgs : EventArgs
    {
        /// <summary>
        /// The remote end point.
        /// </summary>
        private readonly EndPoint remoteEndpoint;

        /// <summary>
        /// The socket error.
        /// </summary>
        private readonly SocketError socketError;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.SocketDisconnectEventArgs"/> class.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="socketError">The socket error.</param>
        public SocketDisconnectEventArgs(EndPoint endPoint, SocketError socketError)
        {
            this.remoteEndpoint = endPoint;
            this.socketError = socketError;
        }

        /// <summary>
        /// Gets the remote endpoint.
        /// </summary>
        /// <value>The <see cref="T:System.Net.EndPoint"/> from which the Socket disconnected.</value>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.remoteEndpoint;
            }
        }

        /// <summary>
        /// Gets the socket error.
        /// </summary>
        /// <value>The error that caused the disconnect.</value>
        public SocketError SocketError
        {
            get
            {
                return this.socketError;
            }
        }
    }
}
