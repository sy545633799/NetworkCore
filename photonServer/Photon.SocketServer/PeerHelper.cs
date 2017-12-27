using System;
using Photon.SocketServer.ServerToServer;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer
{
    /// <summary>
    /// This class provides access to some internal members of <see cref="T:Photon.SocketServer.PeerBase"/> and <see cref="T:Photon.SocketServer.ServerToServer.ServerPeerBase"/> 
    /// In general these methods are indended for testing.
    /// </summary>
    public static class PeerHelper
    {
        /// <summary>
        /// Invokes <see cref="M:Photon.SocketServer.ServerToServer.ServerPeerBase.OnEvent(Photon.SocketServer.IEventData,Photon.SocketServer.SendParameters)"/>.
        /// This method is useful for testing.
        /// </summary>
        /// <param name="peer">The server peer.</param>
        /// <param name="eventData">The event Data.</param>
        /// <param name="sendParameters">The send Parameters.</param>
        public static void InvokeOnEvent(ServerPeerBase peer, IEventData eventData, SendParameters sendParameters)
        {
            peer.RequestFiber.Enqueue(() => peer.OnEvent(eventData, sendParameters));
        }

        /// <summary>
        /// Invokes <see cref="M:Photon.SocketServer.PeerBase.OnOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>.
        /// This method is useful for testing.
        /// </summary>
        /// <param name="peer"> The peer.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <param name="sendParameters">The send Parameters.</param>
        public static void InvokeOnOperationRequest(PeerBase peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            peer.RequestFiber.Enqueue(() => peer.OnOperationRequest(operationRequest, sendParameters));
        }

        /// <summary>
        /// Invokes <see cref="M:Photon.SocketServer.ServerToServer.ServerPeerBase.OnOperationResponse(Photon.SocketServer.OperationResponse,Photon.SocketServer.SendParameters)"/>.
        /// This method is useful for testing.
        /// </summary>
        /// <param name="peer">The server peer.</param>
        /// <param name="operationResponse">The operation response.</param>
        /// <param name="sendParameters"> The send Parameters.</param>
        public static void InvokeOnOperationResponse(ServerPeerBase peer, OperationResponse operationResponse, SendParameters sendParameters)
        {
            peer.RequestFiber.Enqueue(() => peer.OnOperationResponse(operationResponse, sendParameters));
        }

        /// <summary>
        ///  This method simlates a disconnect.
        /// This method is useful for testing.
        /// </summary>
        /// <param name="peer"> The disconnected peer.</param>
        public static void SimulateDisconnect(PeerBase peer)
        {
            ((IManagedPeer)peer).Application_OnDisconnect(DisconnectReason.ClientDisconnect, string.Empty, 0, 0, 0);
        }

        /// <summary>
        /// This method simulates that data was received from a client.
        /// </summary>
        /// <param name="peer">The peer.</param>
        /// <param name="data"> The received data.</param>
        /// <param name="sendParameters"> The send Options.</param>
        [CLSCompliant(false)]
        public static void SimulateReceive(PeerBase peer, byte[] data, SendParameters sendParameters)
        {
            ((IManagedPeer)peer).Application_OnReceive(data, sendParameters, 0, 0, 0);
        }
    }
}
