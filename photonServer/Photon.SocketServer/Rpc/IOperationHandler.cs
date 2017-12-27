namespace Photon.SocketServer.Rpc
{
    /// <summary>
    ///  <see cref="P:Photon.SocketServer.Rpc.Peer.CurrentOperationHandler">Peer.CurrentOperationHandler</see> is an <see cref="T:Photon.SocketServer.Rpc.IOperationHandler"/>.
    /// The implementor handles incoming <see cref="T:Photon.SocketServer.OperationRequest">OperationRequests</see> (<see cref="M:Photon.SocketServer.Rpc.Peer.OnOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>, 
    /// peer disconnects (<see cref="M:Photon.SocketServer.Rpc.Peer.OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason,System.String)"/>) and disconnects that are invoked from other peers (<see cref="M:Photon.SocketServer.Rpc.Peer.OnDisconnectByOtherPeer(Photon.SocketServer.PeerBase,Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>).
    /// </summary>
    public interface IOperationHandler
    {
        /// <summary>
        /// Called by <see cref="M:Photon.SocketServer.Rpc.Peer.OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason,System.String)"/>.
        /// </summary>
        /// <param name="peer">The calling peer.</param>
        void OnDisconnect(PeerBase peer);

        /// <summary>
        ///  Called by the <see cref="M:Photon.SocketServer.Rpc.Peer.OnDisconnectByOtherPeer(Photon.SocketServer.PeerBase,Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>.
        /// </summary>
        /// <param name="peer">The calling peer.</param>
        void OnDisconnectByOtherPeer(PeerBase peer);

        /// <summary>
        ///  Called by <see cref="M:Photon.SocketServer.Rpc.Peer.OnOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>.
        /// </summary>
        /// <param name="peer"> The calling peer.</param>
        /// <param name="operationRequest"> The operation request.</param>
        /// <param name="sendParameters"> The send Parameters.</param>
        /// <returns> The operation response.</returns>
        OperationResponse OnOperationRequest(PeerBase peer, OperationRequest operationRequest, SendParameters sendParameters);
    }
}
