namespace Photon.SocketServer
{
    /// <summary>
    /// Return value of <see 
    /// cref="M:Photon.SocketServer.PeerBase.SendEvent(Photon.SocketServer.IEventData,Photon.SocketServer.SendParameters)"/>, <see 
    /// cref="M:Photon.SocketServer.PeerBase.SendOperationResponse(Photon.SocketServer.OperationResponse,Photon.SocketServer.SendParameters)"/> and <see 
    /// cref="M:Photon.SocketServer.ServerToServer.ServerPeerBase.SendOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>.
    /// </summary>
    public enum SendResult
    {
        /// <summary>
        /// Encrypted sending failed; peer does not support encryption.
        /// </summary>
        EncryptionNotSupported = -1,

        /// <summary>
        /// Successfully enqueued for sending.
        /// </summary>
        Ok = 0,
                  
        /// <summary>
        /// The peer's send buffer is full; data sending was refused.
        /// </summary>
        SendBufferFull = 1,

        /// <summary>
        ///  Peer is disconnected; data sending was refused.
        /// </summary>
        Disconnected = 2,

        /// <summary>
        /// Sending failed because the message size exceeded the MaxMessageSize that was configured for the receiver.
        /// </summary>
        MessageToBig = 3,

        /// <summary>
        /// Send Failed due an unexpected error.
        /// </summary>
        Failed = 4,
        /// <summary>
        /// Send failed because the specified channel is not supported by the peer.
        /// </summary>
        InvalidChannel = 5,

        /// <summary>
        /// Send failed because the specified content type is not supported by the peer.
        /// </summary>
        InvalidContentType = 6,
    }
}
