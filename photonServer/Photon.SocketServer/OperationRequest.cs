using System.Collections.Generic;
using Photon.SocketServer.Rpc;
using Photon.SocketServer.Rpc.Reflection;

namespace Photon.SocketServer
{
    /// <summary>
    /// Each incoming operation is translated to an <see cref="T:Photon.SocketServer.OperationRequest"/>.
    /// It is then dispatched with <see 
    /// cref="M:Photon.SocketServer.PeerBase.OnOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>.
    /// </summary>
    public sealed class OperationRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationRequest"/> class.
        /// </summary>
        public OperationRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationRequest"/> class.
        /// </summary>
        /// <param name="operationCode">The operation Code.</param>
        public OperationRequest(byte operationCode)
        {
            this.OperationCode = operationCode;
        }

        /// <summary>
        ///    Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationRequest"/> class.
        /// </summary>
        /// <param name="operationCode">
        /// The operation Code.
        /// </param>
        /// <param name="dataContract">
        /// All properties of <paramref name="dataContract"/> with the <see
        /// cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/> are mapped to the <see 
        /// cref="P:Photon.SocketServer.OperationRequest.Parameters"/> dictionary.
        /// </param>
        public OperationRequest(byte operationCode, object dataContract)
        {
            this.OperationCode = operationCode;
            this.SetParameters(dataContract);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationRequest"/> class.
        /// This constructor sets the <see cref="P:Photon.SocketServer.OperationRequest.Parameters"/> and the <see cref="P:Photon.SocketServer.OperationRequest.OperationCode"/>.
        /// </summary>
        /// <param name="operationCode">
        /// Determines the <see cref="P:Photon.SocketServer.OperationRequest.OperationCode"/>.
        /// </param>
        /// <param name="parameters">
        /// Determines the <see cref="P:Photon.SocketServer.OperationRequest.Parameters"/>.
        /// </param>
        public OperationRequest(byte operationCode, Dictionary<byte, object> parameters)
        {
            this.OperationCode = operationCode;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Converts properties of an object to <see cref="P:Photon.SocketServer.OperationRequest.Parameters"/>.
        ///  Included properties require the <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/>.
        /// </summary>
        /// <param name="dataContract">
        /// The properties of this object are mapped to <see 
        /// cref="P:Photon.SocketServer.OperationRequest.Parameters"/>.
        /// </param>
        public void SetParameters(object dataContract)
        {
            this.Parameters = (dataContract == null) ? null : ObjectDataMemberMapper.GetValues<DataMemberAttribute>(dataContract);
        }

        /// <summary>
        /// Replaces the <see cref="P:Photon.SocketServer.OperationRequest.Parameters"/> with <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">
        /// The parameters to set.
        /// </param>
        public void SetParameters(Dictionary<byte, object> parameters)
        {
            this.Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets the paramter associated with the specified key.
        /// </summary>
        /// <param name="parameterKey">
        /// The key of the parameter to get or set.
        /// </param>
        /// <returns>
        /// The parameter associated with the specified key. 
        /// If the specified key is not found, a get operation throws a KeyNotFoundException, 
        /// and a set operation creates a new paramter with the specified key.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        ///  The <see cref="P:Photon.SocketServer.OperationRequest.Parameters"/> property has not been initialized.
        ///  </exception>
        public object this[byte parameterKey]
        {
            get
            {
                return this.Parameters[parameterKey];
            }
            set
            {
                this.Parameters[parameterKey] = value;
            }
        }

        /// <summary>
        /// Gets or sets the operation code. It determines how the server responds.
        /// </summary>
        public byte OperationCode { get; set; }

        /// <summary>
        /// Gets or sets the request parameters.
        /// </summary>
        public Dictionary<byte, object> Parameters { get; set; }
    }
}
