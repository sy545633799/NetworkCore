using System;
using System.Collections.Generic;
using System.Net;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// Interface for a class that sends data to a socket. 
    /// </summary>
    public interface ISocketSender : IDisposable
    {
        /// <summary>
        /// Sends a byte array to the socket. 
        /// </summary>
        /// <param name="data"></param>
        void Send(byte[] data);

        /// <summary>
        /// Sends a list of <see cref="T:System.ArraySegment`1"/> of type byte to the socket.
        /// </summary>
        /// <param name="data">The data.</param>
        void Send(IList<ArraySegment<byte>> data);

        /// <summary>
        /// Sends a byte array to the socket. 
        /// </summary>
        /// <param name="data">The data. </param>
        /// <param name="offset">The offset. </param>
        /// <param name="length">The length. </param>
        void Send(byte[] data, int offset, int length);

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:ExitGames.Net.Sockets.ISocketSender"/> is connected  to a remote host as of the last Send operation.
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        /// <value>Returns the underlying <see cref="P:ExitGames.Net.Sockets.ISocketSender.EndPoint"/>.</value>
        IPEndPoint EndPoint { get; }

        /// <summary>
        /// Gets the total number of bytes sent.
        /// </summary>
        /// <value>Returns the underlying <see cref="P:ExitGames.Net.Sockets.ISocketSender.TotalBytesSent"/>.</value>
        long TotalBytesSent { get; }
    }
}
