using System;
using System.Collections.Generic;
using System.Reflection;

namespace Photon.SocketServer.Rpc.Reflection
{
    /// <summary>
    /// This class contains functions to dispatch operations.
      ///         The target function depends on the incoming operation code.
    ///        All registered methods require the <see cref="T:Photon.SocketServer.Rpc.Reflection.OperationAttribute"/> and the signature <c>public OperationResponse MyMethod(PeerBase peer, OperationRequest request, SendParameters sendParameters);</c>.
    /// </summary>
    public sealed class OperationDispatcher
    {
        /// <summary>
        /// The operations.
        /// </summary>
        private readonly Dictionary<byte, Func<PeerBase, OperationRequest, SendParameters, OperationResponse>> operations;

        /// <summary>
        ///   Initializes a new instance of the <see cref="T:Photon.SocketServer.Rpc.Reflection.OperationDispatcher"/> class.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <param name="operationHandler">The operation Handler.</param>
        public OperationDispatcher(OperationMethodInfoCache operations, IOperationHandler operationHandler)
        {
            this.operations = new Dictionary<byte, Func<PeerBase, OperationRequest, SendParameters, OperationResponse>>(operations.OperationFunctions);
            this.CreateDelegates(operations, operationHandler);
        }

        /// <summary>
        /// The create delegates.
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="handler">The handler.</param>
        private void CreateDelegates(OperationMethodInfoCache cache, IOperationHandler handler)
        {
            Dictionary<byte, MethodInfo> operationMethodInfos = cache.OperationMethodInfos;
            Type type = handler.GetType();
            foreach (KeyValuePair<byte, MethodInfo> pair in operationMethodInfos)
            {
                MethodInfo method = pair.Value;
                if (method.ReflectedType != type)
                {
                    throw new ArgumentException(string.Format("Type {0} does not support method {1}.{2}", type.Name, method.ReflectedType, method.Name));
                }
                Func<PeerBase, OperationRequest, SendParameters, OperationResponse> func = (Func<PeerBase, OperationRequest, SendParameters, OperationResponse>)Delegate.CreateDelegate(OperationMethodInfoCache.OperationDelegateType, handler, method);
                this.operations.Add(pair.Key, func);
            }
        }

        /// <summary>
        ///  The dispatch operation request.
        /// </summary>
        /// <param name="peer"> The peer.</param>
        /// <param name="operationRequest"> The operation Request.</param>
        /// <param name="sendParameters"> The send parameters.</param>
        /// <param name="returnValue"> The return Value.</param>
        /// <returns> ok or error.</returns>
        public bool DispatchOperationRequest(PeerBase peer, OperationRequest operationRequest, SendParameters sendParameters, out OperationResponse returnValue)
        {
            Func<PeerBase, OperationRequest, SendParameters, OperationResponse> func;
            if (this.operations.TryGetValue(operationRequest.OperationCode, out func))
            {
                returnValue = func(peer, operationRequest, sendParameters);
                return true;
            }
            returnValue = null;
            return false;
        }
    }
}
