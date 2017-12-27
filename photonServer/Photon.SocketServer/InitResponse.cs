using System;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer
{
    /// <summary>
    /// Provides initialization request parameter. 
    /// </summary>
    public sealed class InitResponse
    {
        /// <summary>
        /// The application id. 
        /// </summary>
        private readonly string applicationId;

        /// <summary>
        /// The rpc protocol. 
        /// </summary>
        private readonly IRpcProtocol protocol;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.InitResponse"/> class.
        /// </summary>
        /// <param name="applicationId">The application id.</param>
        /// <param name="protocol">The rpc Protocol.</param>
        internal InitResponse(string applicationId, IRpcProtocol protocol)
        {
            this.applicationId = applicationId;
            this.protocol = protocol;
        }

        /// <summary>
        /// Gets the application id.
        /// </summary>
        /// <value>The application id.</value>
        public string ApplicationId
        {
            get
            {
                return this.applicationId;
            }
        }

        /// <summary>
        ///  Gets the connection id.
        /// </summary>
        /// <remarks>
        /// Peers connected to different ports may have similar connection ids.
        /// </remarks>
        public int ConnectionId
        {
            get
            {
                return this.PhotonPeer.GetConnectionID();
            }
        }

        /// <summary>
        /// Gets the local IP the client connected to.
        /// </summary>
        public string LocalIP
        {
            get
            {
                return this.PhotonPeer.GetLocalIP();
            }
        }

        /// <summary>
        /// Gets the port the client connects to.
        /// </summary>
        public int LocalPort
        {
            get
            {
                return this.PhotonPeer.GetLocalPort();
            }
        }

        /// <summary>
        /// Gets the native peer.
        /// </summary>
        [CLSCompliant(false)]
        public IPhotonPeer PhotonPeer { get; internal set; }

        /// <summary>
        /// Gets the used rpc protocol.
        /// </summary>
        public IRpcProtocol Protocol
        {
            get
            {
                return this.protocol;
            }
        }

        /// <summary>
        /// Gets the client's IP address.
        /// </summary>
        public string RemoteIP
        {
            get
            {
                return this.PhotonPeer.GetRemoteIP();
            }
        }

        /// <summary>
        ///  Gets the port the client connects from.
        /// </summary>
        public int RemotePort
        {
            get
            {
                return this.PhotonPeer.GetRemotePort();
            }
        }
    }

}
