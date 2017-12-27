using System.Collections.Generic;
using Photon.SocketServer.Rpc;
using Photon.SocketServer.Rpc.Reflection;

namespace Photon.SocketServer
{
    /// <summary>
    /// Incoming <see cref="T:Photon.SocketServer.OperationRequest">OperationRequests</see> are often answered with a response (represented by this class).
    /// It can be sent to the client by using the <see cref="M:Photon.SocketServer.PeerBase.SendOperationResponse(Photon.SocketServer.OperationResponse,Photon.SocketServer.SendParameters)">PeerBase.SendOperationResponse</see> method.
    /// The <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeOperationResponse(Photon.SocketServer.OperationResponse)">IRpcProtocol.SerializeOperationResponse</see> method serializes OperationResponse instances.
    ///<see cref="M:Photon.SocketServer.OperationResponse.SetParameters(System.Object)"/> converts properties that are flagged with <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/> into the <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/> dictionary.
    /// </summary>
    public sealed class OperationResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationResponse"/> class.
        /// </summary>
        public OperationResponse()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationResponse"/> class.
        /// This constructor sets the <see cref="P:Photon.SocketServer.OperationResponse.OperationCode"/>.
        /// </summary>
        /// <param name="operationCode">Determines the <see cref="P:Photon.SocketServer.OperationResponse.OperationCode"/>.</param>
        public OperationResponse(byte operationCode)
        {
            this.OperationCode = operationCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationResponse"/> class.
        /// This constructor calls <see cref="M:Photon.SocketServer.OperationResponse.SetParameters(System.Object)"/> and sets the <see cref="P:Photon.SocketServer.OperationResponse.OperationCode"/>.
        /// </summary>
        /// <param name="operationCode">Determines the <see cref="P:Photon.SocketServer.OperationResponse.OperationCode"/>.</param>
        /// <param name="parameters">All properties of the <paramref name="dataContract"/> with the <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/> are copied to the <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/> dictionary.</param>
        public OperationResponse(byte operationCode, Dictionary<byte, object> parameters)
        {
            this.OperationCode = operationCode;
            this.Parameters = parameters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.OperationResponse"/> class.
        /// This constructor sets the <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/> and the <see cref="P:Photon.SocketServer.OperationResponse.OperationCode"/>.
        /// </summary>
        /// <param name="operationCode">Determines the <see cref="P:Photon.SocketServer.OperationResponse.OperationCode"/>.</param>
        /// <param name="dataContract">Determines the <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/>.</param>
        public OperationResponse(byte operationCode, object dataContract)
        {
            this.OperationCode = operationCode;
            this.SetParameters(dataContract);
        }

        /// <summary>
        /// Converts properties of an object to response <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/>.
        /// Included properties require the <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/>.
        /// </summary>
        /// <param name="dataContract">Properties of this object with the the <see cref="T:Photon.SocketServer.Rpc.DataMemberAttribute"/> converted to <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/>.</param>
        public void SetParameters(object dataContract)
        {
            this.Parameters = (dataContract == null) ? null : ObjectDataMemberMapper.GetValues<DataMemberAttribute>(dataContract);
        }

        /// <summary>
        ///  Replaces the <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/> with <paramref name="parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters to set.</param>
        public void SetParameters(Dictionary<byte, object> parameters)
        {
            this.Parameters = parameters;
        }

        /// <summary>
        /// Gets or sets the debug message. Error code 0 returns typically debug message "Ok".
        /// </summary>
        public string DebugMessage { get; set; }

        /// <summary>
        /// Gets or sets the paramter associated with the specified key.
        /// </summary>
        /// <param name="parameterKey">The key of the parameter to get or set.</param>
        /// <returns>The parameter associated with the specified key. 
        /// If the specified key is not found, a get operation throws a KeyNotFoundException, 
        /// and a set operation creates a new paramter with the specified key.</returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <see cref="P:Photon.SocketServer.OperationResponse.Parameters"/> property has not been initialized.
        ///</exception>
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
        /// Gets or sets the operation code. It allows the client to idenitfy which operation was answered.
        /// </summary>
        public byte OperationCode { get; set; }

        /// <summary>
        /// Gets or sets the response parameters.
        /// </summary>
        public Dictionary<byte, object> Parameters { get; set; }

        /// <summary>
        ///  Gets or sets the error code. Code 0 means OK.
        /// </summary>
        public short ReturnCode { get; set; }
    }
}
