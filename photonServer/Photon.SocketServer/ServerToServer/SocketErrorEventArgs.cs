using System;
using System.Net.Sockets;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// Provides data for the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.ConnectError"/> event.
    /// </summary>
    public class SocketErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.SocketErrorEventArgs"/> class.
        /// </summary>
        /// <param name="socketError">The <see cref="P:Photon.SocketServer.ServerToServer.SocketErrorEventArgs.SocketError"/> occured during connection attemp.</param>
        public SocketErrorEventArgs(SocketError socketError)
        {
            this.SocketError = socketError;
        }

        /// <summary>
        /// Gets the socket error code occured during the connection attemp.
        /// </summary>
        public SocketError SocketError { get; private set; }
    }
}
