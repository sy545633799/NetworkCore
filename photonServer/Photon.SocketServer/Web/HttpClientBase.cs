using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Protocols;
using Photon.SocketServer.Security;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.Web
{
    /// <summary>
    /// Photon http application client implementation.
    /// </summary>     
    /// <remarks>
    /// The
    /// </remarks>
    public abstract class HttpClientBase
    {
        // Fields
        private DiffieHellmanKeyExchange keyExchange;
        private long lastRequestTimeStamp;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly int pingInterval;
        private Timer pingTimer;
        private static readonly IRpcProtocol protocol = Protocol.GpBinaryV162;
        private readonly object syscRoot;
        private readonly int timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Web.HttpClientBase"/> class.
        /// </summary>
        /// <param name="url">
        /// The url of the photon http application.
        /// </param>
        /// <param name="timeout">
        /// The length of time, in milliseconds, before http request are timing out.
        /// </param>
        /// <param name="pingInterval">
        /// The time in milliseconds when an automatic ping will be sent after the last operation 
        ///  request to fetch new messages from the server.
        ///  </param>
        protected HttpClientBase(string url, int timeout, int pingInterval)
        {
            this.pingInterval = 100;
            this.syscRoot = new object();
            this.Address = url;
            this.timeout = timeout;
            this.pingInterval = Math.Max(pingInterval, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Web.HttpClientBase"/> class.
        /// </summary>
        /// <param name="url">The url of the photon http application.</param>
        /// <param name="timeout">The length of time, in milliseconds, before the operation request are timing out.</param>
        /// <param name="pingInterval">The time in milliseconds when an automatic ping will be sent after the last operation 
        /// request to fetch new messages from the server.</param>
        /// <param name="connectionId">The connection id.</param>
        protected HttpClientBase(string url, int timeout, int pingInterval, string connectionId)
            : this(url, timeout, pingInterval)
        {
            this.ConnectionId = connectionId;
        }

        /// <summary>
        /// Connects this instance to a photon http application.
        /// </summary>
        /// <param name="applicationId">True if successfully connected to the photon application; otherwise false.</param>
        /// <returns></returns>
        public bool Connect(string applicationId)
        {
            lock (this.syscRoot)
            {
                if (this.IsConnected)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.WarnFormat("Http client is allready connected: pid={0}", new object[] { this.ConnectionId });
                    }
                    return false;
                }
                WebRequest request = WebRequest.Create(this.Address + "?init");
                request.Proxy = null;
                request.Method = "POST";
                request.Timeout = this.timeout;
                Version httpClientVersion = Versions.HttpClientVersion;
                byte[] buffer = protocol.SerializeInitRequest(applicationId, httpClientVersion);
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(buffer, 0, buffer.Length);
                requestStream.Close();
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    return false;
                }
                byte[] buffer2 = new byte[response.ContentLength];
                responseStream.Read(buffer2, 0, (int)response.ContentLength);
                responseStream.Close();
                if (buffer2.Length < 0x10)
                {
                    return false;
                }
                byte[] dst = new byte[0x10];
                Buffer.BlockCopy(buffer2, 0, dst, 0, 0x10);
                this.ConnectionId = new Guid(dst).ToString();
                this.IsConnected = true;
                this.SetLastTimeStamp();
                this.pingTimer = new Timer(new TimerCallback(this.OnPingTimer), null, this.pingInterval, -1);
                return true;
            }
        }

        /// <summary>
        /// Disconnects the http client.
        /// </summary>
        public void Disconnect()
        {
            this.OnDisconnectInternal(DisconnectReason.ClientDisconnect, string.Empty);
        }

        private long GetTimeSinceLastSend()
        {
            long timestamp = Stopwatch.GetTimestamp();
            long num2 = Interlocked.Read(ref this.lastRequestTimeStamp);
            return (((timestamp - num2) * 0x3e8L) / Stopwatch.Frequency);
        }

        /// <summary>
        /// Initializes the peer to receive and send encrypted operations.
        /// </summary>
        /// <returns></returns>
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
        /// The connection was closed.
        /// </summary>
        /// <param name="reasonCode"></param>
        /// <param name="reasonDetail"></param>
        [CLSCompliant(false)]
        protected abstract void OnDisconnect(DisconnectReason reasonCode, string reasonDetail);
        private void OnDisconnectInternal(DisconnectReason reason, string reasonDetails)
        {
            lock (this.syscRoot)
            {
                if (this.IsConnected)
                {
                    this.IsConnected = false;
                    if (this.pingTimer != null)
                    {
                        this.pingTimer.Dispose();
                    }
                    if (reason == DisconnectReason.ClientDisconnect)
                    {
                        this.SendData(new byte[] { 1 });
                    }
                    this.OnDisconnect(reason, reasonDetails);
                }
            }
        }

        /// <summary>
        ///  Called when an <see cref="T:Photon.SocketServer.EventData"/> was received.
        /// </summary>
        /// <param name="eventData"> The event data.</param>
        /// <param name="sendParameters"> The send Parameters.</param>
        protected internal abstract void OnEvent(IEventData eventData, SendParameters sendParameters);
        private void OnEventInternal(RtsMessageHeader header, byte[] data)
        {
            EventData data2;
            if (!(header.IsEncrypted ? protocol.TryParseEventDataEncrypted(data, this.CryptoProvider, out data2) : protocol.TryParseEventData(data, out data2)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Failed to parse event for peer with connection id {0}.", new object[] { this.ConnectionId });
                }
            }
            else
            {
                SendParameters sendParameters = new SendParameters
                {
                    Encrypted = header.IsEncrypted
                };
                this.OnEvent(data2, sendParameters);
            }
        }

        /// <summary>
        /// Invoked if an initialize encryption request was completed.
        /// </summary>
        /// <param name="resultCode"> The result code.</param>
        /// <param name="debugMessage"> The debuf message.</param>
        protected virtual void OnInitializeEcryptionCompleted(short resultCode, string debugMessage)
        {
        }

        private void OnInternalOperationResponse(RtsMessageHeader header, byte[] data)
        {
            OperationResponse response;
            if (!(header.IsEncrypted ? protocol.TryParseOperationResponseEncrypted(data, this.CryptoProvider, out response) : protocol.TryParseOperationResponse(data, out response)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Failed to parse internal operation response for peer with connection id {0}.", new object[] { this.ConnectionId });
                }
            }
            else if (response.OperationCode != 0)
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
                    if (obj2 == null)
                    {
                        log.ErrorFormat("Parameter server key was not set in initialize encryption response;", new object[0]);
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
                    this.CryptoProvider = new RijndaelCryptoProvider(buffer2, PaddingMode.PKCS7);
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
        private void OnOperationResponseInternal(RtsMessageHeader header, byte[] data)
        {
            OperationResponse response;
            if (!(header.IsEncrypted ? protocol.TryParseOperationResponseEncrypted(data, this.CryptoProvider, out response) : protocol.TryParseOperationResponse(data, out response)))
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Failed to parse operation response for peer with connection id {0}.", new object[] { this.ConnectionId });
                }
            }
            else
            {
                SendParameters sendParameters = new SendParameters
                {
                    Encrypted = header.IsEncrypted
                };
                this.OnOperationResponse(response, sendParameters);
            }
        }

        private void OnPingTimer(object state)
        {
            long timeSinceLastSend = this.GetTimeSinceLastSend();
            if (timeSinceLastSend >= this.pingInterval)
            {
                this.SendPing();
                timeSinceLastSend = this.GetTimeSinceLastSend();
            }
            long dueTime = this.pingInterval - timeSinceLastSend;
            if (dueTime < 0L)
            {
                dueTime = 0L;
            }
            lock (this.syscRoot)
            {
                if (this.IsConnected)
                {
                    this.pingTimer = new Timer(new TimerCallback(this.OnPingTimer), null, dueTime, -1L);
                }
            }
        }

        private void OnReceive(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            using (BinaryReader reader = new BinaryReader(input))
            {
                int num = reader.ReadInt16();
                for (int i = 0; i < num; i++)
                {
                    RtsMessageHeader header;
                    int count = reader.ReadInt32();
                    byte[] buffer = reader.ReadBytes(count);
                    if (!protocol.TryParseMessageHeader(buffer, out header))
                    {
                        this.OnUnexpectedDataReceived(buffer, new SendParameters());
                        return;
                    }
                    switch (header.MessageType)
                    {
                        case RtsMessageType.OperationResponse:
                            {
                                this.OnOperationResponseInternal(header, buffer);
                                continue;
                            }
                        case RtsMessageType.Event:
                            {
                                this.OnEventInternal(header, buffer);
                                continue;
                            }
                        case RtsMessageType.InternalOperationResponse:
                            {
                                this.OnInternalOperationResponse(header, buffer);
                                continue;
                            }
                    }
                    log.WarnFormat("Received invalid message type: {0}", new object[] { header.MessageType });
                }
            }
        }

        /// <summary>
        /// This method is called if incoming data has an unexpected format.
        ///  Per default this method disconnects the client. 
        ///  Override to change this behavior.
        /// </summary>
        /// <param name="data"> The received data.</param>
        /// <param name="sendParameters"> The send options.</param>
        protected virtual void OnUnexpectedDataReceived(byte[] data, SendParameters sendParameters)
        {
            log.WarnFormat("Disconnecting peer {0}: Unexpected data received", new object[] { this.ConnectionId });
            this.Disconnect();
        }

        private SendResult SendData(byte[] data)
        {
            if (!this.IsConnected)
            {
                return SendResult.Disconnected;
            }
            try
            {
                WebRequest request = WebRequest.Create(this.Address + "?pid=" + this.ConnectionId);
                request.Proxy = null;
                request.Method = "POST";
                request.Timeout = this.timeout;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                if (responseStream != null)
                {
                    byte[] buffer = new byte[response.ContentLength];
                    responseStream.Read(buffer, 0, (int)response.ContentLength);
                    responseStream.Dispose();
                    this.SetLastTimeStamp();
                    if (buffer.Length > 0)
                    {
                        this.OnReceive(buffer);
                    }
                }
                return SendResult.Ok;
            }
            catch (WebException exception)
            {
                if (exception.Status == WebExceptionStatus.Timeout)
                {
                    this.OnDisconnectInternal(DisconnectReason.TimeoutDisconnect, exception.Message);
                }
                else if (exception.Response != null)
                {
                    HttpWebResponse response2 = (HttpWebResponse)exception.Response;
                    if (response2.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        this.OnDisconnectInternal(DisconnectReason.ManagedDisconnect, string.Empty);
                    }
                    else
                    {
                        this.OnDisconnectInternal(DisconnectReason.ServerDisconnect, exception.Message);
                    }
                }
                else
                {
                    this.OnDisconnectInternal(DisconnectReason.ServerDisconnect, exception.Message);
                }
                return SendResult.Disconnected;
            }
        }

        private SendResult SendInternalOperationRequest(OperationRequest request)
        {
            byte[] data = protocol.SerializeInternalOperationRequest(request);
            return this.SendData(data);
        }

        /// <summary>
        ///  Sends an operation request to a photon http application.
        /// </summary>
        /// <param name="request">
        /// The operation request.
        /// </param>
        /// <returns>
        /// The operation response received from the photon application or null if no
        ///   resonse was sent.
        ///   </returns>
        public void SendOperationRequest(OperationRequest request)
        {
            this.SendOperationRequest(request, false);
        }

        /// <summary>
        ///  Sends an operation request to a photn http application.
        /// </summary>
        /// <param name="request">The operation request.</param>
        /// <param name="encrypt">Specifies if the request should be encrypted.</param>
        /// <returns>The operation response received from the photon application or null if no
        /// resonse was sent.</returns>
        public SendResult SendOperationRequest(OperationRequest request, bool encrypt)
        {
            byte[] buffer;
            if (encrypt && (this.CryptoProvider == null))
            {
                throw new InvalidOperationException("Encryption must be initialized before sending encrypted requests.");
            }
            if (encrypt)
            {
                buffer = protocol.SerializeOperationRequestEncrypted(request, this.CryptoProvider);
            }
            else
            {
                buffer = protocol.SerializeOperationRequest(request);
            }
            return this.SendData(buffer);
        }

        /// <summary>
        ///  Send a ping request to a photon http application.
        /// </summary>
        /// <returns></returns>
        public SendResult SendPing()
        {
            return this.SendData(new byte[0]);
        }

        private void SetLastTimeStamp()
        {
            long timestamp = Stopwatch.GetTimestamp();
            Interlocked.Exchange(ref this.lastRequestTimeStamp, timestamp);
        }

        /// <summary>
        ///   Gets the address.
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        ///  Gets the connection id.
        /// </summary>
        public string ConnectionId { get; private set; }

        /// <summary>
        ///  Gets or sets the CryptoProvider.
        /// </summary>
        public ICryptoProvider CryptoProvider { get; set; }

        /// <summary>
        ///  Gets a value indicating if the client is connected.
        /// </summary>
        public bool IsConnected { get; private set; }
    }
}
