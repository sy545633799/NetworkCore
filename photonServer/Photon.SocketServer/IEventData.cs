using System.Collections.Generic;

namespace Photon.SocketServer
{
    /// <summary>
    /// The implementation class contains data that is sent to clients with <see cref="M:Photon.SocketServer.PeerBase.SendEvent(Photon.SocketServer.IEventData,Photon.SocketServer.SendParameters)"/>.
    /// Implementors are <see cref="T:Photon.SocketServer.EventData"/> and <see cref="T:Photon.SocketServer.SerializedEventData"/>.
    /// </summary>
    public interface IEventData
    {
        /// <summary>
        /// Serializes the event data.
        /// </summary>
        /// <param name="protocol">The protocol used to serialize the event data.</param>
        /// <returns>A byte array that contains the serialized event data parameters and the event code.</returns>
        byte[] Serialize(IRpcProtocol protocol);

        /// <summary>
        /// Gets Code.
        /// </summary>
        byte Code { get; }

        /// <summary>
        /// Gets or sets the paramter associated with the specified key.
        /// </summary>
        /// <param name="parameterKey">The key of the parameter to get or set.</param>
        /// <returns>The parameter associated with the specified key. 
        ///  If the specified key is not found, a get operation throws a KeyNotFoundException, 
        /// and a set operation creates a new paramter with the specified key.</returns>
        /// <exception cref="T:System.NullReferenceException">
        ///  The <see cref="P:Photon.SocketServer.IEventData.Parameters"/> property has not been initialized.
        /// </exception>
        object this[byte parameterKey] { get; set; }

        /// <summary>
        /// Gets the event parameters that will be sent to the client.
        /// </summary>
        Dictionary<byte, object> Parameters { get; }
    }
}
