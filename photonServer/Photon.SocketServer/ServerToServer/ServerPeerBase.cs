using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Protocols;
using Photon.SocketServer.Security;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    /// The server peer base. 
    /// </summary>
    public abstract class ServerPeerBase : PeerBase
    {
        // Fields
        private DiffieHellmanKeyExchange keyExchange;

        /// <summary>
        /// The logger. 
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The operation data logger. 
        /// </summary>
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.ServerPeerBase"/> class.
        /// </summary>
        /// <param name="protocol">The rpc protocol.</param>
        /// <param name="unmanagedPeer">The unmanaged peer.</param>
        [CLSCompliant(false)]
        protected ServerPeerBase(IRpcProtocol protocol, IPhotonPeer unmanagedPeer)
            : base(protocol, unmanagedPeer)
        {
        }

        /// <summary>
        /// Initializes the peer to receive and send encrypted operations.
        /// </summary>
        /// <returns>
        /// Returns <see cref="F:Photon.SocketServer.SendResult.Ok"/> if the event was successfully sent; 
        /// otherwise an error value. See <see cref="T:Photon.SocketServer.SendResult"/> for more information. 
        /// </returns>
        public SendResult InitializeEncryption()
        {
            this.keyExchange = new DiffieHellmanKeyExchange();
            OperationRequest request2 = new OperationRequest
            {
                OperationCode = 0
            };
            Dictionary<byte, object> dictionary = new Dictionary<byte, object>(2);
            dictionary.Add(1, this.keyExchange.PublicKey);
            dictionary.Add(2, (byte)0);
            request2.Parameters = dictionary;
            OperationRequest request = request2;
            return this.SendInternalOperationRequest(request);
        }

        /// <summary>
        /// Logs the operation request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="data">The data.</param>
        /// <param name="sendResult"> The send result.</param>
        /// <param name="sendParameters">The send Options.</param>
        private void LogOperationRequest(OperationRequest request, byte[] data, SendResult sendResult, SendParameters sendParameters)
        {
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("SentOpRequest: ConnID={0}, opCode={1}, ChannelId={2} result={3}, data=({4} bytes) {5}", new object[] { base.UnmanagedPeer.GetConnectionID(), request.OperationCode, sendParameters.ChannelId, sendResult, data.Length, BitConverter.ToString(data) });
            }
            else if (log.IsDebugEnabled)
            {
                log.DebugFormat("SentOpRequest: ConnID={0}, opCode={1}, ChannelId={2} result={3} size={4} bytes", new object[] { base.UnmanagedPeer.GetConnectionID(), request.OperationCode, sendParameters.ChannelId, sendResult, data.Length });
            }
        }

        /// <summary>
        /// Called when an <see cref="T:Photon.SocketServer.EventData"/> was received.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        /// <param name="sendParameters">The send Parameters.</param>
        protected internal abstract void OnEvent(IEventData eventData, SendParameters sendParameters);

        /// <summary>
        /// Invoked if an initialize encryption request was completed.
        /// </summary>
        /// <param name="resultCode">The result code.</param>
        /// <param name="debugMessage">The debuf message.</param>
        protected virtual void OnInitializeEcryptionCompleted(short resultCode, string debugMessage)
        {
        }

        private void OnInternalOperationResponse(OperationResponse response)
        {
            if (response.OperationCode != 0)
            {
                if (log.IsWarnEnabled)
                {
                    log.WarnFormat("Received unknown internal operation code {0}", new object[] { response.OperationCode });
                }
            }
            else
            {
                if (response.ReturnCode == 0)
                {
                    object obj2;
                    byte[] buffer2;
                    if (!response.Parameters.TryGetValue(1, out obj2))
                    {
                        log.Error("Parameter server key missing in initialize encryption response;");
                        return;
                    }
                    byte[] otherPartyPublicKey = obj2 as byte[];
                    if (otherPartyPublicKey == null)
                    {
                        log.ErrorFormat("Parameter server key has wrong type {0} in initialize encryption response;", new object[] { obj2.GetType() });
                    }
                    this.keyExchange.DeriveSharedKey(otherPartyPublicKey);
                    using (SHA256 sha = SHA256.Create())
                    {
                        buffer2 = sha.ComputeHash(this.keyExchange.SharedKey);
                    }
                    base.CryptoProvider = new RijndaelCryptoProvider(buffer2, PaddingMode.PKCS7);
                }
                this.OnInitializeEcryptionCompleted(response.ReturnCode, response.DebugMessage);
            }
        }

        /// <summary>
        /// Called when an <see cref="T:Photon.SocketServer.OperationResponse"/> was received.
        /// </summary>
        /// <param name="operationResponse">The operation response.</param>
        /// <param name="sendParameters"> The send Parameters.</param>
        protected internal abstract void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters);

        /// <summary>
        /// Enables the server peer to receive events and operation response.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="sendParameters">The send Options.</param>
        /// <param name="rtt">The round trip time.</param>
        /// <param name="rttVariance">The round trip time variance.</param>
        /// <param name="numFailures">The number of failures.</param>
        internal override void OnReceiveInternal(byte[] data, SendParameters sendParameters, int rtt, int rttVariance, int numFailures)
        {
            RtsMessageHeader header;
            base.RoundTripTime = rtt;
            base.RoundTripTimeVariance = rttVariance;
            base.NumFailures = numFailures;
            if (!base.Protocol.TryParseMessageHeader(data, out header))
            {
                this.OnUnexpectedDataReceived(data, sendParameters);
            }
            else
            {
                if (header.IsEncrypted)
                {
                    sendParameters.Encrypted = true;
                }
                switch (header.MessageType)
                {
                    case RtsMessageType.Operation:
                        OperationRequest request;
                        if (sendParameters.Encrypted ? base.Protocol.TryParseOperationRequestEncrypted(data, base.CryptoProvider, out request) : base.Protocol.TryParseOperationRequest(data, out request))
                        {
                            this.OnOperationRequest(request, sendParameters);
                            return;
                        }
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Failed to parse operation request for peer with connection id {0}.", new object[] { base.UnmanagedPeer.GetConnectionID() });
                        }
                        return;

                    case RtsMessageType.OperationResponse:
                        OperationResponse response;
                        if (sendParameters.Encrypted ? base.Protocol.TryParseOperationResponseEncrypted(data, base.CryptoProvider, out response) : base.Protocol.TryParseOperationResponse(data, out response))
                        {
                            this.OnOperationResponse(response, sendParameters);
                            return;
                        }
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Failed to parse operation response for peer with connection id {0}.", new object[] { base.UnmanagedPeer.GetConnectionID() });
                        }
                        return;

                    case RtsMessageType.Event:
                        EventData data2;
                        if (sendParameters.Encrypted ? base.Protocol.TryParseEventDataEncrypted(data, base.CryptoProvider, out data2) : base.Protocol.TryParseEventData(data, out data2))
                        {
                            this.OnEvent(data2, sendParameters);
                            return;
                        }
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Failed to parse event for peer with connection id {0}.", new object[] { base.UnmanagedPeer.GetConnectionID() });
                        }
                        return;

                    case RtsMessageType.InternalOperationRequest:
                        base.OnInternalOperationRequest(data, sendParameters);
                        return;

                    case RtsMessageType.InternalOperationResponse:
                        OperationResponse response2;
                        if (sendParameters.Encrypted ? base.Protocol.TryParseOperationResponseEncrypted(data, base.CryptoProvider, out response2) : base.Protocol.TryParseOperationResponse(data, out response2))
                        {
                            this.OnInternalOperationResponse(response2);
                            return;
                        }
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Failed to parse internal operation response for peer with connection id {0}.", new object[] { base.UnmanagedPeer.GetConnectionID() });
                        }
                        return;
                }
                log.WarnFormat("Received invalid message type: {0}", new object[] { header.MessageType });
            }
        }

        /// <summary>
        /// Sends an internal operaton request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>the send result</returns>
        private SendResult SendInternalOperationRequest(OperationRequest request)
        {
            byte[] data = base.Protocol.SerializeInternalOperationRequest(request);
            return base.SendData(data, new SendParameters());
        }

        /// <summary>
        /// Sends an operation request.
        /// </summary>
        /// <param name="operationRequest"> The operation request.</param>
        /// <param name="sendParameters"> The send Options.</param>
        /// <returns>
        /// <see cref="F:Photon.SocketServer.SendResult.EncryptionNotSupported"/>: Encryption not initialized.
        ///  <see cref="F:Photon.SocketServer.SendResult.Disconnected"/>: Not connected anymore.
        ///  <see cref="F:Photon.SocketServer.SendResult.SendBufferFull"/>: The send buffer was full.
        ///  <see cref="F:Photon.SocketServer.SendResult.Ok"/>: Success.
        /// </returns>
        public SendResult SendOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (operationRequest == null)
            {
                throw new ArgumentNullException("operationRequest");
            }
            if (sendParameters.Encrypted && ((base.CryptoProvider == null) || !base.CryptoProvider.IsInitialized))
            {
                if (log.IsWarnEnabled)
                {
                    log.WarnFormat("SendEvent - Cryptography has not been initialized.", new object[0]);
                }
                this.OnSendFailed(SendResult.EncryptionNotSupported, sendParameters, 0);
                return SendResult.EncryptionNotSupported;
            }
            byte[] data = sendParameters.Encrypted ? base.Protocol.SerializeOperationRequestEncrypted(operationRequest, base.CryptoProvider) : base.Protocol.SerializeOperationRequest(operationRequest);
            return this.SendOperationRequestInternal(data, operationRequest, sendParameters);
        }

        /// <summary>
        /// Used by <see 
        /// cref="M:Photon.SocketServer.ServerToServer.ServerPeerBase.SendOperationRequest(Photon.SocketServer.OperationRequest,Photon.SocketServer.SendParameters)"/>.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="operationRequest">The operation request.</param>
        /// <param name="sendParameters">The send parameters.</param>
        /// <returns>
        /// <see cref="F:Photon.SocketServer.SendResult.Disconnected"/>: Not connected anymore.
        ///<see cref="F:Photon.SocketServer.SendResult.SendBufferFull"/>: The send buffer was full.
        ///<see cref="F:Photon.SocketServer.SendResult.Ok"/>: Success.
        /// </returns>
        private SendResult SendOperationRequestInternal(byte[] data, OperationRequest operationRequest, SendParameters sendParameters)
        {
            SendResult sendResult = base.SendData(data, sendParameters);
            if (log.IsDebugEnabled || operationDataLogger.IsDebugEnabled)
            {
                this.LogOperationRequest(operationRequest, data, sendResult, sendParameters);
            }
            return sendResult;
        }
    }
}
