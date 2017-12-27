using System;
using System.Net;

namespace ExitGames.Net.Sockets
{
    /// <summary>
    /// Interface for a class that receives data from a socket. 
    /// </summary>
    public interface ISocketReceiver : IDisposable
    {
        /// <summary>
        /// Event that is invoked when new data is received. 
        /// </summary>
        event EventHandler<SocketReceiveEventArgs> Receive;

        /// <summary>
        /// Gets the remote end point. 
        /// </summary>
        /// <value>The <see cref="P:ExitGames.Net.Sockets.ISocketReceiver.EndPoint"/> from which the implementation class receives the data.</value>
        IPEndPoint EndPoint { get; }

        /// <summary>
        /// Gets the total number of resecived bytes. 
        /// </summary>
        /// <value>The total number of received bytes. </value>
        long TotalBytesReceived { get; }
    }
}
