using System;
using System.Net;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// The socket receive event args.
    /// </summary>
    public sealed class SocketReceiveEventArgs : EventArgs
    {
        /// <summary>
        /// The data.
        /// </summary>
        private readonly byte[] data;

        /// <summary>
        /// The length.
        /// </summary>
        private readonly int length;

        /// <summary>
        /// The offset.
        /// </summary>
        private readonly int offset;

        /// <summary>
        /// The remote end point.
        /// </summary>
        private readonly EndPoint remoteEndPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Net.Sockets.SocketReceiveEventArgs"/> class.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public SocketReceiveEventArgs(EndPoint remoteEndPoint, byte[] buffer, int offset, int length)
        {
            this.remoteEndPoint = remoteEndPoint;
            this.data = buffer;
            this.offset = offset;
            this.length = length;
        }

        /// <summary>
        /// Gets the data buffer containing the received data.
        /// </summary>
        /// <value>Buffer with the receuved data.</value>
        public byte[] Buffer
        {
            get
            {
                return this.data;
            }
        }

        /// <summary>
        /// Gets the number of bytes received in the socket operation.
        /// </summary>
        /// <value>The number of bytes received in the socket operation.</value>
        public int BytesReceived
        {
            get
            {
                return this.length;
            }
        }

        /// <summary>
        /// Gets the offset, in bytes, of data in the <see 
        /// cref="P:ExitGames.Net.Sockets.SocketReceiveEventArgs.Buffer"/> property.
        /// </summary>
        /// <value>The offset in <see cref="P:ExitGames.Net.Sockets.SocketReceiveEventArgs.Buffer"/> where the received data begins.</value>
        public int Offset
        {
            get
            {
                return this.offset;
            }
        }

        /// <summary>
        /// Gets the remote endpoint from which data was received.
        /// </summary>
        /// <value>The remote endpoint from which data was received.</value>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.remoteEndPoint;
            }
        }
    }
}
