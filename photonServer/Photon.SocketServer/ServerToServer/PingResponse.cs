using System;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// The ping response.
    /// </summary>
    public class PingResponse
    {
        /// <summary>
        /// The size in bytes.
        /// </summary>
        public static readonly int SizeInBytes = 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.PingResponse"/> class.
        /// </summary>
        /// <param name="buffer"> The buffer.</param>
        public PingResponse(byte[] buffer)
        {
            this.ServerTime = BitConverter.ToInt32(buffer, 0);
            this.ClientTime = BitConverter.ToInt32(buffer, 4);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.PingResponse"/> class.
        /// </summary>
        /// <param name="clientTime">The client time.</param>
        /// <param name="serverTime">The server time.</param>
        public PingResponse(int clientTime, int serverTime)
        {
            this.ClientTime = clientTime;
            this.ServerTime = serverTime;
        }

        /// <summary>
        /// Gets ClientTime.
        /// </summary>
        public int ClientTime { get; private set; }

        /// <summary>
        ///  Gets ServerTime.
        /// </summary>
        public int ServerTime { get; private set; }
    }
}
