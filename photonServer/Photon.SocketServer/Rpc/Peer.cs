using System;
using ExitGames.Logging;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.Rpc
{
    /// <summary>
    /// Inheritance class of <see cref="T:Photon.SocketServer.PeerBase"/>. 
    ///This class uses an <see cref="T:Photon.SocketServer.Rpc.IOperationHandler"/> that can be set with <see cref="M:Photon.SocketServer.Rpc.Peer.SetCurrentOperationHandler(Photon.SocketServer.Rpc.IOperationHandler)"/>.
    ///This is useful if operations should have a different behavior when the state of the peer changes, e.g. after authentication.
    /// </summary>
    public class Peer : PeerBase
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Peer"/> class.
        /// </summary>
        /// <param name="rpcProtocol"> The rpc Protocol.</param>
        /// <param name="photonPeer">The photon Peer.</param>
        [CLSCompliant(false)]
        protected Peer(IRpcProtocol rpcProtocol, IPhotonPeer photonPeer)
            : base(rpcProtocol, photonPeer)
        {
            this.SetCurrentOperationHandler(null);
        }

        /// <summary>
        /// Enqueues <see cref="M:Photon.SocketServer.Rpc.Peer.OnDisconnectByOtherPeer(Photon.SocketServer.PeerBase,Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/> to the <see cref="P:Photon.SocketServer.PeerBase.RequestFiber"/>.
        /// This method is intended to be used to disconnect a user's peer if he connects with multiple clients while the application logic wants to allow just one.
        /// </summary>
        /// <param name="otherPeer">The other peer.</param>
        /// <param name="otherRequest">The other request.</param>
        /// <param name="sendParameters"> The send Parameters.</param>
        public void DisconnectByOtherPeer(PeerBase otherPeer, OperationRequest otherRequest, SendParameters sendParameters)
        {
            base.RequestFiber.Enqueue(() => this.OnDisconnectByOtherPeer(otherPeer, otherRequest, sendParameters));
        }

        /// <summary>
        /// Executed when a peer disconnects.
        /// This method is being enqueued to the fiber.
        /// </summary>
        /// <param name="reasonCode"></param>
        /// <param name="reasonDetail"></param>
        [CLSCompliant(false)]
        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
            this.CurrentOperationHandler.OnDisconnect(this);
        }

        /// <summary>
        ///  Called by <see cref="M:Photon.SocketServer.Rpc.Peer.DisconnectByOtherPeer(Photon.SocketServer.PeerBase,Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)">DisconnectByOtherPeer</see> after being enqueued to the <see cref="P:Photon.SocketServer.PeerBase.RequestFiber"/>.
        /// It calls <see cref="M:Photon.SocketServer.Rpc.IOperationHandler.OnDisconnectByOtherPeer(Photon.SocketServer.PeerBase)">CurrentOperationHandler.OnDisconnectByOtherPeer</see> and 
        /// then continues the <paramref name="otherRequest">original request</paramref> by calling the <paramref name="otherPeer">original peer's</paramref> <see cref="M:Photon.SocketServer.PeerBase.OnOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)">OnOperationRequest</see> method.
        /// </summary>
        /// <param name="otherPeer">The original peer.</param>
        /// <param name="otherRequest">The original request.</param>
        /// <param name="sendParameters">The send Parameters.</param>
        protected virtual void OnDisconnectByOtherPeer(PeerBase otherPeer, OperationRequest otherRequest, SendParameters sendParameters)
        {
            this.CurrentOperationHandler.OnDisconnectByOtherPeer(this);
            PeerHelper.InvokeOnOperationRequest(otherPeer, otherRequest, sendParameters);
        }

        /// <summary>
        /// Incoming <see cref="T:Photon.SocketServer.OperationRequest"/>s are handled here.
        /// </summary>
        /// <param name="operationRequest">The operation Request.</param>
        /// <param name="sendParameters"> The send Parameters.</param>
        protected internal override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            OperationResponse operationResponse = this.CurrentOperationHandler.OnOperationRequest(this, operationRequest, sendParameters);
            if (operationResponse != null)
            {
                base.SendOperationResponse(operationResponse, sendParameters);
            }
        }

        /// <summary>
        ///  Sets <see cref="P:Photon.SocketServer.Rpc.Peer.CurrentOperationHandler"/>.
        /// </summary>
        /// <param name="operationHandler"> The new operation handler.</param>
        public void SetCurrentOperationHandler(IOperationHandler operationHandler)
        {
            if (operationHandler == null)
            {
                operationHandler = OperationHandlerDisabled.Instance;
            }
            if (log.IsDebugEnabled)
            {
                string str = (this.CurrentOperationHandler == null) ? "{null}" : this.CurrentOperationHandler.GetType().ToString();
                string str2 = (operationHandler == null) ? "{null}" : operationHandler.GetType().ToString();
                log.DebugFormat("set operation handler to {0}, was {1} - peer id {2}", new object[] { str2, str, base.ConnectionId });
            }
            this.CurrentOperationHandler = operationHandler;
        }

        /// <summary>
        /// Gets the current <see cref="T:Photon.SocketServer.Rpc.IOperationHandler"/>.
        ///<see cref="M:Photon.SocketServer.Rpc.Peer.OnOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/> and <see cref="M:Photon.SocketServer.Rpc.Peer.OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason,System.String)"/> calls the <see cref="P:Photon.SocketServer.Rpc.Peer.CurrentOperationHandler"/>.
        ///<para>
        /// The operation handler can be changed with <see cref="M:Photon.SocketServer.Rpc.Peer.SetCurrentOperationHandler(Photon.SocketServer.Rpc.IOperationHandler)">SetCurrentOperationHandler</see>.
        ///This allows a different behavior when a peer's state changes: For instance, after a login operation many operations will no longer have to return an error and can actually do something.
        ///  Using different <see cref="T:Photon.SocketServer.Rpc.IOperationHandler">IOperationHandlers</see> is much more elegant than checking a "isLoggedIn" variable with every request.
        ///  </para>
        /// </summary>
        public IOperationHandler CurrentOperationHandler { get; private set; }
    }
}
