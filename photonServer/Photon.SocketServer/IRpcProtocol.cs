using System;
using System.IO;
using Photon.SocketServer.Rpc;
using Photon.SocketServer.Rpc.Protocols;
using Photon.SocketServer.Rpc.Reflection;
using Photon.SocketServer.Security;

namespace Photon.SocketServer
{
    /// <summary>
    /// The implementation class supports a specific real time server protocol. 
    /// </summary>
    public interface IRpcProtocol
    {
        /// <summary>
        /// Serialze an object to a stream 
        /// </summary>
        /// <param name="stream">The stream. </param>
        /// <param name="obj">The object to serialize. </param>
        void Serialize(Stream stream, object obj);

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.EventData"/>.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <returns>The serialized event.</returns>
        byte[] SerializeEventData(EventData eventData);

        /// <summary>
        /// Encrypts an <see cref="T:Photon.SocketServer.EventData"/>.
        /// </summary>
        /// <param name="eventData"> The event data.</param>
        /// <param name="cryptoProvider"> The crypto provider.</param>
        /// <returns> The serialized event.</returns>
        byte[] SerializeEventDataEncrypted(IEventData eventData, ICryptoProvider cryptoProvider);

        /// <summary>
        /// Serialize an init request.
        /// </summary>
        /// <param name="appName">The app Name.</param>
        /// <param name="version">The version.</param>
        /// <returns> The serialized init response.</returns>
        byte[] SerializeInitRequest(string appName, Version version);

        /// <summary>
        ///  Serialize an init response.
        /// </summary>
        /// <returns>The serialized init response.</returns>
        byte[] SerializeInitResponse();

        /// <summary>
        /// Serializes an internal <see cref="T:Photon.SocketServer.OperationRequest"/>.
        /// </summary>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns> The serialized operation request.</returns>
        byte[] SerializeInternalOperationRequest(OperationRequest operationRequest);

        /// <summary>
        ///  Serialize an <see cref="T:Photon.SocketServer.OperationResponse"/> for system operations.
        /// </summary>
        /// <param name="operationResponse">The operation response.</param>
        /// <returns> The serialized operation response.</returns>
        byte[] SerializeInternalOperationResponse(OperationResponse operationResponse);

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationRequest"/>
        /// </summary>
        /// <param name="operationRequest">The operation request.</param>
        /// <returns> A byte array containing the serialized operation request.</returns>
        byte[] SerializeOperationRequest(OperationRequest operationRequest);

        /// <summary>
        ///  Serializes an <see cref="T:Photon.SocketServer.OperationRequest"/>
        /// The operation request data will be encrypted using the specified <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/>.
        /// </summary>
        /// <param name="operationRequest">The operation request.</param>
        /// <param name="cryptoProvider">An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to encrypt the operation request.</param>
        /// <returns> A byte array containing the serialized operation request.</returns>
        byte[] SerializeOperationRequestEncrypted(OperationRequest operationRequest, ICryptoProvider cryptoProvider);

        /// <summary>
        ///  Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// </summary>
        /// <param name="operationResponse">The response.</param>
        /// <returns>The serialized operation response.</returns>
        byte[] SerializeOperationResponse(OperationResponse operationResponse);

        /// <summary>
        /// Serializes an <see cref="T:Photon.SocketServer.OperationResponse"/>.
        /// The operation response data will be encrypted using the specified <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/>.
        /// </summary>
        /// <param name="operationResponse">The response.</param>
        /// <param name="cryptoProvider"> An <see cref="T:Photon.SocketServer.Security.ICryptoProvider"/> instance used to encrypt the operation response.</param>
        /// <returns>The serialized operation response.</returns>
        byte[] SerializeOperationResponseEncrypted(OperationResponse operationResponse, ICryptoProvider cryptoProvider);

        /// <summary>
        /// Since C# supports many more types than the used protocol some parameters need to be converted.
        /// This method tries to convert an operation request parameter into to a type that works for a target field or property.
        /// </summary>
        /// <param name="parameterInfo"> The parameter info.</param>
        /// <param name="value"> The value.</param>
        /// <returns>True if value has a valid type.</returns>
        bool TryConvertParameter(ObjectMemberInfo<DataMemberAttribute> parameterInfo, ref object value);

        /// <summary>
        ///  Try to parse an object from a stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="obj"> The result object.</param>
        /// <returns> True on success.</returns>
        bool TryParse(Stream stream, out object obj);

        /// <summary>
        /// Tries to convert a byte array into an <see cref="T:Photon.SocketServer.EventData"/> instance.
        /// The <paramref name="data"/> was serialized with <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeEventData(Photon.SocketServer.EventData)"/>.
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="eventData">The event data.</param>
        /// <returns> True on success.</returns>
        bool TryParseEventData(byte[] data, out EventData eventData);

        /// <summary>
        /// Tries to convert a byte array into an <see cref="T:Photon.SocketServer.EventData"/> instance.
        /// The <paramref name="data"/> was serialized with <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeEventDataEncrypted(Photon.SocketServer.IEventData,Photon.SocketServer.Security.ICryptoProvider)"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cryptoProvider">The crypto provider.</param>
        /// <param name="eventData"> The event data.</param>
        /// <returns>True on success.</returns>
        bool TryParseEventDataEncrypted(byte[] data, ICryptoProvider cryptoProvider, out EventData eventData);

        /// <summary>
        ///Tries to parse the header. 
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="header">The header.</param>
        /// <returns>True on success.</returns>
        bool TryParseMessageHeader(byte[] data, out RtsMessageHeader header);

        /// <summary>
        /// Tries to convert a byte array into an <see cref="T:Photon.SocketServer.OperationRequest"/> instance.
        /// The <paramref name="data"/> was serialized with <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeOperationRequest(Photon.SocketServer.OperationRequest)"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="operationRequest">The operation Request.</param>
        /// <returns>True on success.</returns>
        bool TryParseOperationRequest(byte[] data, out OperationRequest operationRequest);

        /// <summary>
        ///  Tries to convert a byte array into an <see cref="T:Photon.SocketServer.OperationRequest"/> instance.
        /// The <paramref name="data"/> was serialized with <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeOperationRequestEncrypted(Photon.SocketServer.OperationRequest,Photon.SocketServer.Security.ICryptoProvider)"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cryptoProvider"> The crypto Provider.</param>
        /// <param name="operationRequest">The operation Request.</param>
        /// <returns></returns>
        bool TryParseOperationRequestEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationRequest operationRequest);

        /// <summary>
        /// Tries to convert a byte array into an <see cref="T:Photon.SocketServer.OperationResponse"/> instance.
        /// The <paramref name="data"/> was serialized with <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeOperationResponse(Photon.SocketServer.OperationResponse)"/>. 
        /// </summary>
        /// <param name="data"> The data.</param>
        /// <param name="operationResponse">The operation Response.</param>
        /// <returns>True on success.</returns>
        bool TryParseOperationResponse(byte[] data, out OperationResponse operationResponse);

        /// <summary>
        ///  Tries to convert a byte array into an <see cref="T:Photon.SocketServer.OperationResponse"/> instance.
        /// The <paramref name="data"/> was serialized with <see cref="M:Photon.SocketServer.IRpcProtocol.SerializeOperationResponseEncrypted(Photon.SocketServer.OperationResponse,Photon.SocketServer.Security.ICryptoProvider)"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="cryptoProvider">The crypto Provider.</param>
        /// <param name="operationResponse"> The operation Response.</param>
        /// <returns>True on success.</returns>
        bool TryParseOperationResponseEncrypted(byte[] data, ICryptoProvider cryptoProvider, out OperationResponse operationResponse);

        /// <summary>
        ///  Gets the type of the protocol.
        /// </summary>
        /// <value>The type of the protocol.</value>
        ProtocolType ProtocolType { get; }
    }
}
