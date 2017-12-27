using System.ComponentModel;
using System.Net;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// The socket accept event args.
    /// Used by the <see cref="T:ExitGames.Net.Sockets.Pgm.PgmReceiver"/>.
    /// </summary>
    public sealed class SocketAcceptEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The remote endpoint.
        /// </summary>
        private readonly EndPoint remoteEndpoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.SocketAcceptEventArgs"/> class.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        public SocketAcceptEventArgs(EndPoint remoteEndpoint)
        {
            this.remoteEndpoint = remoteEndpoint;
        }

        /// <summary>
        /// Initializes a new instance of the <see 
        /// cref="T:ExitGames.Net.Sockets.SocketAcceptEventArgs"/> class with the <see 
        /// cref="P:System.ComponentModel.CancelEventArgs.Cancel"/> property set to the given value.
        /// </summary>
        /// <param name="remoteEndpoint">The remote endpoint.</param>
        /// <param name="cancel">True to cancel the event; otherwise, false.</param>
        public SocketAcceptEventArgs(EndPoint remoteEndpoint, bool cancel)
            : base(cancel)
        {
            this.remoteEndpoint = remoteEndpoint;
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>The <see cref="T:System.Net.EndPoint"/> with which the Socket is communicating.</value>
        public EndPoint RemoteEndpoint
        {
            get
            {
                return this.remoteEndpoint;
            }
        }
    }
}
