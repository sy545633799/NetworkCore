using System;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// Defines current version for peer implementations. 
    /// </summary>
    internal class Versions
    {
        /// <summary>
        /// The version for websocket peers. 
        /// </summary>
        public static readonly Version DefaultWebsocketPeerVersion = new Version(3, 0, 5);

        /// <summary>
        /// The tcp client version. 
        /// </summary>
        public static readonly Version TcpClientVersion = new Version(3, 0, 5);

        /// <summary>
        /// The version for outgoing tcp peers. 
        /// </summary>
        public static readonly Version TcpOutboundPeerVersion = new Version(3, 0, 5);
    }
}
