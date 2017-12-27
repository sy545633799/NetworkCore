using System;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// Provides data for the <see cref="E:Photon.SocketServer.ServerToServer.TcpClient.PingResponse"/> event.
    /// </summary>
    public class PingResponseEventArgs : EventArgs
    {
        /// <summary>
        /// ping response recived from the server.
        /// </summary>
        private readonly PingResponse pingResponse;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.PingResponseEventArgs"/> class.
        /// </summary>
        /// <param name="pingResponse">The ping response received from the server.</param>
        public PingResponseEventArgs(PingResponse pingResponse)
        {
            this.pingResponse = pingResponse;
        }

        /// <summary>
        /// Gets the ping response recived from the server.
        /// </summary>
        public PingResponse PingResponse
        {
            get
            {
                return this.pingResponse;
            }
        }
    }
}
