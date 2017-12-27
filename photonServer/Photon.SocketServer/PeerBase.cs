using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using Photon.SocketServer.Diagnostics;
using Photon.SocketServer.Operations;
using Photon.SocketServer.PeerConnectionStateMachine;
using Photon.SocketServer.Rpc;
using Photon.SocketServer.Rpc.Protocols;
using Photon.SocketServer.Rpc.Protocols.Json;
using Photon.SocketServer.Security;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer
{
    public abstract class PeerBase : IDisposable, IManagedPeer
    {
        // Fields
        private IConnectionState connectionState;
        private ICryptoProvider cryptoProvider;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private int onDisconnectCallCount;
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");
        private readonly IRpcProtocol protocol;
        private readonly PoolFiber requestFiber;
        private IPhotonPeer unmanagedPeer;

        // Methods
        protected PeerBase(InitRequest initRequest)
            : this(initRequest.Protocol, initRequest.PhotonPeer)
        {
        }

        [CLSCompliant(false)]
        protected PeerBase(IRpcProtocol protocol, IPhotonPeer unmanagedPeer)
        {
            this.unmanagedPeer = unmanagedPeer;
            this.connectionState = Photon.SocketServer.PeerConnectionStateMachine.Connected.Instance;
            this.protocol = protocol;
            this.requestFiber = new PoolFiber();
            this.requestFiber.Start();
        }

        public void AbortConnection()
        {
            this.UnmanagedPeer.AbortClient();
            if (log.IsWarnEnabled)
            {
                log.Warn("Abort client called");
            }
        }

        public void Disconnect()
        {
            if (this.connectionState.TransitDisconnect(this))
            {
                this.UnmanagedPeer.DisconnectClient();
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Disconnect client called", new object[0]);
                }
            }
        }

        public void Dispose()
        {
            if (this.connectionState.TransitDisposeConnected(this))
            {
                this.unmanagedPeer.DisconnectClient();
            }
            if (this.connectionState.TransitDisposeDisconnected(this))
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.requestFiber.Dispose();
            }
        }

        private static void ExchangeKeys(object state)
        {
            byte num = 0;
            KeyExchangeParameter parameter = (KeyExchangeParameter)state;
            DiffieHellmanKeyExchange exchange = null;
            try
            {
                exchange = new DiffieHellmanKeyExchange();
                num = 1;
                exchange.DeriveSharedKey(parameter.ClientPublicKey);
                parameter.PublicKey = exchange.PublicKey;
                parameter.SharedKey = exchange.SharedKey;
            }
            catch (ThreadAbortException)
            {
                if (log.IsWarnEnabled)
                {
                    string message = string.Format("Key exchange was aborted\r\nstep={0}\r\nClientKey={1}", num, (parameter.ClientPublicKey == null) ? "{null}" : BitConverter.ToString(parameter.ClientPublicKey));
                    if (exchange != null)
                    {
                        message = message + string.Format("\r\nSecret={0}\r\nPrime={1}\r\nPublicKey={2}\r\nSharedKey={3}", new object[] { (exchange.Secret == null) ? "{null}" : exchange.Secret.ToString(), (exchange.Prime == null) ? "{null}" : exchange.Prime.ToString(), (exchange.PublicKey == null) ? "{null}" : BitConverter.ToString(exchange.PublicKey), (exchange.SharedKey == null) ? "{null}" : BitConverter.ToString(exchange.SharedKey) });
                    }
                    log.Warn(message);
                }
            }
        }

        ~PeerBase()
        {
            this.Dispose(false);
        }

        public void Flush()
        {
            this.unmanagedPeer.Flush();
        }

        public void GetStats(out int roundTripTime, out int roundTripTimeVariance, out int numFailures)
        {
            this.unmanagedPeer.GetStats(out roundTripTime, out roundTripTimeVariance, out numFailures);
        }

        [Browsable(false)]
        public byte[] InitializeEncryption(byte[] otherPartyPublicKey)
        {
            return this.InitializeEncryption(otherPartyPublicKey, EncryptionMethod.Sha256Pkcs7);
        }

        [Browsable(false)]
        public byte[] InitializeEncryption(byte[] otherPartyPublicKey, EncryptionMethod mode)
        {
            KeyExchangeParameter parameter = new KeyExchangeParameter
            {
                ClientPublicKey = otherPartyPublicKey
            };
            Thread thread = new Thread(new ParameterizedThreadStart(PeerBase.ExchangeKeys));
            thread.Start(parameter);
            if (!thread.Join(0x7d0))
            {
                thread.Abort();
                return null;
            }
            switch (mode)
            {
                case EncryptionMethod.Sha256Pkcs7:
                    byte[] buffer;
                    using (SHA256 sha = SHA256.Create())
                    {
                        buffer = sha.ComputeHash(parameter.SharedKey);
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("InitializeEncryption: conId={0}, HashMode=SHA256, Paddin=PKCS7", new object[] { this.UnmanagedPeer.GetConnectionID() });
                    }
                    this.CryptoProvider = new RijndaelCryptoProvider(buffer, PaddingMode.PKCS7);
                    break;

                case EncryptionMethod.Md5Iso10126:
                    byte[] buffer2;
                    using (MD5 md = MD5.Create())
                    {
                        buffer2 = md.ComputeHash(parameter.SharedKey);
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("InitializeEncryption: conId={0}, HashMode=MD5, Paddin=ISO10126", new object[] { this.UnmanagedPeer.GetConnectionID() });
                    }
                    this.CryptoProvider = new RijndaelCryptoProvider(buffer2, PaddingMode.ISO10126);
                    break;

                default:
                    throw new ArgumentOutOfRangeException("mode", "Invalid mode specified. Mode must be between 0 and 1");
            }
            return parameter.PublicKey;
        }

        private void LogEvent(IEventData eventData, int channelId, byte[] data, SendResult sendResult)
        {
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("SentEvent: ConnID={0}, evCode={5}, ChannelId={1}, result={2}, data=({3} bytes) {4}", new object[] { this.UnmanagedPeer.GetConnectionID(), channelId, sendResult, data.Length, BitConverter.ToString(data), eventData.Code });
            }
            else if (log.IsDebugEnabled)
            {
                log.DebugFormat("SentEvent: ConnID={0}, evCode={4}, ChannelId={1}, result={2} size={3} bytes", new object[] { this.UnmanagedPeer.GetConnectionID(), channelId, sendResult, data.Length, eventData.Code });
            }
        }

        private void LogOperationResponse(OperationResponse response, byte[] data, SendResult sendResult, SendParameters sendParameters)
        {
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("SentOpResponse: ConnID={0}, opCode={1}, return={6}{7}, ChannelId={2}, result={3}, data=({4} bytes) {5}", new object[] { this.UnmanagedPeer.GetConnectionID(), response.OperationCode, sendParameters.ChannelId, sendResult, data.Length, BitConverter.ToString(data), response.ReturnCode, (response.DebugMessage == null) ? null : ("(" + response.DebugMessage + ")") });
            }
            else if (log.IsDebugEnabled)
            {
                log.DebugFormat("SentOpResponse: ConnID={0}, opCode={1}, return={5}{6}, ChannelId={2} result={3} size={4} bytes", new object[] { this.UnmanagedPeer.GetConnectionID(), response.OperationCode, sendParameters.ChannelId, sendResult, data.Length, response.ReturnCode, (response.DebugMessage == null) ? null : ("(" + response.DebugMessage + ")") });
            }
        }

        [CLSCompliant(false)]
        protected abstract void OnDisconnect(DisconnectReason reasonCode, string reasonDetail);
        private void OnInitEncryptionRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            InitEncryptionRequest operation = new InitEncryptionRequest(this.Protocol, operationRequest);
            if (this.ValidateInternalOperation(operation, sendParameters))
            {
                byte[] buffer = this.InitializeEncryption(operation.ClientKey, (EncryptionMethod)operation.Mode);
                InitEncryptionResponse dataContract = new InitEncryptionResponse
                {
                    ServerKey = buffer
                };
                OperationResponse operationResponse = new OperationResponse(operation.OperationRequest.OperationCode, dataContract);
                if (buffer == null)
                {
                    operationResponse.ReturnCode = -1;
                }
                this.SendInternalOperationResponse(operationResponse, sendParameters);
            }
        }

        internal void OnInternalOperationRequest(byte[] data, SendParameters sendParameters)
        {
            bool flag;
            OperationRequest request;
            if (this.protocol.ProtocolType != ProtocolType.Json)
            {
                flag = sendParameters.Encrypted ? this.Protocol.TryParseOperationRequestEncrypted(data, this.CryptoProvider, out request) : this.Protocol.TryParseOperationRequest(data, out request);
            }
            else
            {
                flag = ((JsonProtocol)this.Protocol).TryParseInternalOperationRequest(data, out request);
            }
            if (!flag)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Failed to parse internal operation request for peer with connection id {0}.", new object[] { this.ConnectionId });
                }
                this.OnUnexpectedDataReceived(data, sendParameters);
            }
            else
            {
                switch (request.OperationCode)
                {
                    case 0:
                        this.OnInitEncryptionRequest(request, sendParameters);
                        return;

                    case 1:
                        this.OnPingRequest(request, sendParameters);
                        return;
                }
                OperationResponse operationResponse = new OperationResponse
                {
                    OperationCode = request.OperationCode,
                    ReturnCode = -1,
                    DebugMessage = "Unknown operation"
                };
                this.SendInternalOperationResponse(operationResponse, sendParameters);
            }
        }

        protected internal abstract void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters);
        private void OnPingRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            PingRequest operation = new PingRequest(this.Protocol, operationRequest);
            if (this.ValidateInternalOperation(operation, sendParameters))
            {
                int tickCount = Environment.TickCount;
                PingResponse dataContract = new PingResponse(operation.ClientTimeStamp, tickCount);
                OperationResponse operationResponse = new OperationResponse(operation.OperationRequest.OperationCode, dataContract);
                this.SendInternalOperationResponse(operationResponse, sendParameters);
            }
        }

        protected virtual void OnReceive(byte[] data, SendParameters sendParameters)
        {
            RtsMessageHeader header;
            if (!this.protocol.TryParseMessageHeader(data, out header))
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
                        if (sendParameters.Encrypted ? this.Protocol.TryParseOperationRequestEncrypted(data, this.CryptoProvider, out request) : this.Protocol.TryParseOperationRequest(data, out request))
                        {
                            this.OnOperationRequest(request, sendParameters);
                            return;
                        }
                        if (log.IsDebugEnabled)
                        {
                            log.DebugFormat("Failed to parse operation request for peer with connection id {0}.", new object[] { this.UnmanagedPeer.GetConnectionID() });
                        }
                        this.OnUnexpectedDataReceived(data, sendParameters);
                        return;

                    case RtsMessageType.InternalOperationRequest:
                        this.OnInternalOperationRequest(data, sendParameters);
                        return;
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received invalid message type: {0}", new object[] { header.MessageType });
                }
                this.OnUnexpectedDataReceived(data, sendParameters);
            }
        }

        internal virtual void OnReceiveInternal(byte[] data, SendParameters sendParameters, int rtt, int rttVariance, int numFailures)
        {
            try
            {
                this.RoundTripTime = rtt;
                this.RoundTripTimeVariance = rttVariance;
                this.NumFailures = numFailures;
                this.OnReceive(data, sendParameters);
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        protected internal virtual void OnSend(int bytesSend)
        {
        }

        protected virtual void OnSendBufferEmpty()
        {
        }

        protected internal virtual void OnSendBufferFull()
        {
            log.WarnFormat("Disconnecting peer {0}: Send buffer full", new object[] { this.ConnectionId });
            this.Disconnect();
        }

        protected internal virtual void OnSendFailed(SendResult sendResult, SendParameters sendParameters, int messageSize)
        {
            if ((sendResult != SendResult.Disconnected) && log.IsWarnEnabled)
            {
                log.WarnFormat("Send failed: conId={0}, reason={1}, channelId={2}, msgSize{3}", new object[] { this.ConnectionId, sendResult, sendParameters.ChannelId, messageSize });
            }
        }

        protected virtual void OnUnexpectedDataReceived(byte[] data, SendParameters sendParameters)
        {
            log.WarnFormat("Disconnecting peer {0}: Unexpected data received", new object[] { this.ConnectionId });
            this.Disconnect();
        }

        bool IManagedPeer.Application_OnDisconnect(DisconnectReason reasonCode, string reasonDetail, int rtt, int rttVariance, int numFailures)
        {
            Action action = null;
            if (Interlocked.Increment(ref this.onDisconnectCallCount) == 1)
            {
                if (action == null)
                {
                    action = delegate
                    {
                        this.RoundTripTime = rtt;
                        this.RoundTripTimeVariance = rttVariance;
                        this.NumFailures = numFailures;
                        if (this.connectionState.TransitOnDisconnect(this))
                        {
                            this.OnDisconnect(reasonCode, reasonDetail);
                            this.Dispose();
                        }
                    };
                }
                this.requestFiber.Enqueue(action);
                return true;
            }
            log.Warn("duplicate ondisconnect call: " + new StackTrace(true));
            return false;
        }

        void IManagedPeer.Application_OnReceive(byte[] data, SendParameters sendParameters, int rtt, int rttVariance, int numFailures)
        {
            this.requestFiber.Enqueue(() => this.OnReceiveInternal(data, sendParameters, rtt, rttVariance, numFailures));
        }

        void IManagedPeer.Application_OnSendBufferEmpty()
        {
            this.requestFiber.Enqueue(new Action(this.OnSendBufferEmpty));
        }

        protected SendResult SendData(byte[] data, SendParameters sendParameters)
        {
            if (this.connectionState.Value != ConnectionState.Connected)
            {
                this.OnSendFailed(SendResult.Disconnected, sendParameters, data.Length);
                return SendResult.Disconnected;
            }
            MessageReliablity reliability = sendParameters.Unreliable ? MessageReliablity.UnReliable : MessageReliablity.Reliable;
            if (sendParameters.Flush)
            {
                reliability |= MessageReliablity.Flush;
            }
            SendResult sendResult = (SendResult)this.UnmanagedPeer.Send(data, reliability, sendParameters.ChannelId, this.MessageContentType);
            switch (sendResult)
            {
                case SendResult.EncryptionNotSupported:
                case SendResult.Disconnected:
                case SendResult.MessageToBig:
                case SendResult.Failed:
                case SendResult.InvalidChannel:
                    this.OnSendFailed(sendResult, sendParameters, data.Length);
                    return sendResult;

                case SendResult.Ok:
                    this.OnSend(data.Length);
                    return sendResult;

                case SendResult.SendBufferFull:
                    this.OnSendBufferFull();
                    return sendResult;
            }
            return sendResult;
        }

        public SendResult SendEvent(IEventData eventData, SendParameters sendParameters)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException("eventData");
            }
            if (sendParameters.Encrypted && ((this.CryptoProvider == null) || !this.CryptoProvider.IsInitialized))
            {
                if (log.IsWarnEnabled)
                {
                    log.WarnFormat("SendEvent - Cryptography has not been initialized.", new object[0]);
                }
                this.OnSendFailed(SendResult.EncryptionNotSupported, sendParameters, 0);
                return SendResult.EncryptionNotSupported;
            }
            byte[] data = sendParameters.Encrypted ? this.Protocol.SerializeEventDataEncrypted(eventData, this.cryptoProvider) : eventData.Serialize(this.Protocol);
            if (data == null)
            {
                return SendResult.Failed;
            }
            SendResult sendResult = this.SendData(data, sendParameters);
            PhotonCounter.EventSentCount.Increment();
            PhotonCounter.EventSentPerSec.Increment();
            if (log.IsDebugEnabled || operationDataLogger.IsDebugEnabled)
            {
                this.LogEvent(eventData, sendParameters.ChannelId, data, sendResult);
            }
            return sendResult;
        }

        private void SendInternalOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            if (operationResponse == null)
            {
                throw new ArgumentNullException("operationResponse");
            }
            byte[] data = this.Protocol.SerializeInternalOperationResponse(operationResponse);
            SendResult sendResult = this.SendData(data, sendParameters);
            if (log.IsDebugEnabled || operationDataLogger.IsDebugEnabled)
            {
                this.LogOperationResponse(operationResponse, data, sendResult, sendParameters);
            }
        }

        public SendResult SendOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
        {
            if (operationResponse == null)
            {
                throw new ArgumentNullException("operationResponse");
            }
            if (sendParameters.Encrypted && ((this.CryptoProvider == null) || !this.CryptoProvider.IsInitialized))
            {
                if (log.IsWarnEnabled)
                {
                    log.WarnFormat("SendOperationResponse - Cryptography has not been initialized.", new object[0]);
                }
                this.OnSendFailed(SendResult.EncryptionNotSupported, sendParameters, 0);
                return SendResult.EncryptionNotSupported;
            }
            byte[] data = !sendParameters.Encrypted ? this.Protocol.SerializeOperationResponse(operationResponse) : this.Protocol.SerializeOperationResponseEncrypted(operationResponse, this.CryptoProvider);
            if (data == null)
            {
                return SendResult.Failed;
            }
            SendResult sendResult = this.SendData(data, sendParameters);
            if (log.IsDebugEnabled || operationDataLogger.IsDebugEnabled)
            {
                this.LogOperationResponse(operationResponse, data, sendResult, sendParameters);
            }
            return sendResult;
        }

        public void SetDebugString(string message)
        {
            this.unmanagedPeer.SetDebugString(message);
        }

        internal bool TransitConnectionState(IConnectionState newState, IConnectionState oldState)
        {
            if (Interlocked.CompareExchange<IConnectionState>(ref this.connectionState, newState, oldState) == oldState)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Peer {0} changed state from {1} to {2}", new object[] { this.ConnectionId, oldState.GetType().Name, newState.GetType().Name });
                }
                return true;
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Peer {0} did not change state from {1} to {2}", new object[] { this.ConnectionId, oldState.GetType().Name, newState.GetType().Name });
            }
            return false;
        }

        private bool ValidateInternalOperation(Operation operation, SendParameters sendParameters)
        {
            if (operation.IsValid)
            {
                return true;
            }
            string errorMessage = operation.GetErrorMessage();
            OperationResponse operationResponse = new OperationResponse
            {
                ReturnCode = -1,
                DebugMessage = errorMessage,
                OperationCode = operation.OperationRequest.OperationCode
            };
            this.SendInternalOperationResponse(operationResponse, sendParameters);
            return false;
        }

        // Properties
        public bool Connected
        {
            get
            {
                return (this.connectionState.Value == ConnectionState.Connected);
            }
        }

        public int ConnectionId
        {
            get
            {
                return this.UnmanagedPeer.GetConnectionID();
            }
        }

        internal IConnectionState ConnectionStateImpl
        {
            get
            {
                return this.connectionState;
            }
        }

        protected internal ICryptoProvider CryptoProvider
        {
            get
            {
                return this.cryptoProvider;
            }
            set
            {
                Interlocked.Exchange<ICryptoProvider>(ref this.cryptoProvider, value);
            }
        }

        public bool Disposed
        {
            get
            {
                return (this.connectionState.Value == ConnectionState.Disposed);
            }
        }

        public string LocalIP
        {
            get
            {
                return this.UnmanagedPeer.GetLocalIP();
            }
        }

        public int LocalPort
        {
            get
            {
                return this.UnmanagedPeer.GetLocalPort();
            }
        }

        internal MessageContentType MessageContentType
        {
            get
            {
                if (this.protocol is JsonProtocol)
                {
                    return MessageContentType.Text;
                }
                return MessageContentType.Binary;
            }
        }

        public NetworkProtocolType NetworkProtocol
        {
            get
            {
                PeerType peerType = this.unmanagedPeer.GetPeerType();
                switch (peerType)
                {
                    case PeerType.ENetPeer:
                    case PeerType.UDPChunkPeer:
                    case PeerType.ENetOutboundPeer:
                        return NetworkProtocolType.Udp;

                    case PeerType.TCPPeer:
                    case PeerType.XMLPeer:
                    case PeerType.TCPChunkPeer:
                    case PeerType.S2SPeer:
                        return NetworkProtocolType.Tcp;

                    case PeerType.WebSocketPeer:
                    case PeerType.WebSocketOutboundPeer:
                        return NetworkProtocolType.WebSocket;

                    case PeerType.HTTPPeer:
                        return NetworkProtocolType.Http;
                }
                log.WarnFormat("Unknown peer type: {0}", new object[] { peerType });
                return NetworkProtocolType.Unknown;
            }
        }

        public int NumFailures { get; protected set; }

        public IRpcProtocol Protocol
        {
            get
            {
                return this.protocol;
            }
        }

        public string RemoteIP
        {
            get
            {
                return this.UnmanagedPeer.GetRemoteIP();
            }
        }

        public int RemotePort
        {
            get
            {
                return this.UnmanagedPeer.GetRemotePort();
            }
        }

        public IFiber RequestFiber
        {
            get
            {
                return this.requestFiber;
            }
        }

        public int RoundTripTime { get; protected set; }

        public int RoundTripTimeVariance { get; protected set; }

        internal IPhotonPeer UnmanagedPeer
        {
            get
            {
                return this.unmanagedPeer;
            }
            set
            {
                Interlocked.Exchange<IPhotonPeer>(ref this.unmanagedPeer, value);
            }
        }

        // Nested Types
        private class KeyExchangeParameter
        {
            // Properties
            public byte[] ClientPublicKey { get; set; }

            public byte[] PublicKey { get; set; }

            public byte[] SharedKey { get; set; }
        }
    }
}
