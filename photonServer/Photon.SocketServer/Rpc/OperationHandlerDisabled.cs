namespace Photon.SocketServer.Rpc
{
    /// <summary>
    ///  This class is the operation handler for peers that are disconnected.
    /// It logs warnings for any operation requests or disconnect calls since they are unexpected.
    /// </summary>
    internal sealed class OperationHandlerDisabled : IOperationHandler
    {
        /// <summary>
        /// The singletone instance.
        /// </summary>
        public static readonly OperationHandlerDisabled Instance = new OperationHandlerDisabled();

        /// <summary>
        /// <see cref="T:Photon.SocketServer.Rpc.IOperationHandler"/> implementation. Does nothing.
        /// </summary>
        /// <param name="peer"> The calling peer.</param>
        void IOperationHandler.OnDisconnect(PeerBase peer)
        {
        }

        /// <summary>
        /// <see cref="T:Photon.SocketServer.Rpc.IOperationHandler"/> implementation. Does nothing.
        /// </summary>
        /// <param name="peer"> The calling peer.</param>
        void IOperationHandler.OnDisconnectByOtherPeer(PeerBase peer)
        {
        }

        /// <summary>
        /// <see cref="T:Photon.SocketServer.Rpc.IOperationHandler"/> implementation. Does nothing.
        /// </summary>
        /// <param name="peer"> The calling peer.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <param name="sendParameters">The send parameters.</param>
        /// <returns>  The operation response.</returns>
        OperationResponse IOperationHandler.OnOperationRequest(PeerBase peer, OperationRequest operationRequest, SendParameters sendParameters)
        {
            return null;
        }
    }
}
