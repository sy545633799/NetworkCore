using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Protocols;
using Photon.SocketServer.Rpc.Protocols.GpBinaryByte;
using Photon.SocketServer.Security;

namespace Photon.SocketServer.ServerToServer
{
    /// <summary>
    ///  Base class for Tcp client implementations.
    /// </summary>
    public abstract class TcpClientBase : IDisposable
    {
        /// <summary>
        /// The loger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log operations to the logging framework.
        /// </summary>
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");

        /// <summary>
        /// The default protocol to use for operation and event serialization.
        /// </summary>
        private static readonly IRpcProtocol defaultProtocol = GpBinaryByteProtocolV16.HeaderV2Instance;

        /// <summary>
        /// The <see cref="T:Photon.SocketServer.IRpcProtocol"/> protocl used by this instance.
        /// </summary>
        private readonly IRpcProtocol protocol;

        /// <summary>
        ///  The binary reader.
        /// </summary>
        private readonly TcpBinaryReader binaryReader;

        /// <summary>
        ///  The client version.
        /// </summary>
        private readonly Version clientVersion;

        /// <summary>
        ///  A sync root for connect/disconnect.
        /// </summary>
        private readonly object socketSynRoot;

        /// <summary>
        /// Backing field for <see cref="P:Photon.SocketServer.ServerToServer.TcpClientBase.ApplicationId"/>.
        /// </summary>
        private string application;

        /// <summary>
        /// Backing field of <see cref="P:Photon.SocketServer.ServerToServer.TcpClientBase.Connected"/>.
        /// </summary>
        private byte connected;

        /// <summary>
        /// the key exchange
        /// </summary>
        private DiffieHellmanKeyExchange keyExchange;

        /// <summary>
        /// Backing field for <see cref="P:Photon.SocketServer.ServerToServer.TcpClientBase.LocalEndPoint"/>.
        /// </summary>
        private IPEndPoint localEnd;

        /// <summary>
        /// Backing field for <see cref="P:Photon.SocketServer.ServerToServer.TcpClientBase.RemoteEndPoint"/>.
        /// </summary>
        private IPEndPoint remoteEnd;

        /// <summary>
        ///      the used socket
        /// </summary>
        private Socket socketConnection;

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClientBase"/> class.
        /// </summary>
        protected TcpClientBase()
            : this(defaultProtocol, null)
        {
        }

        /// <summary>
        ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClientBase"/> class.
        /// </summary>
        /// <param name="protocol"> The <see cref="T:Photon.SocketServer.IRpcProtocol"/> to use for operation and event serialization.</param>
        protected TcpClientBase(IRpcProtocol protocol)
            : this(protocol, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClientBase"/> class.
        /// </summary>
        /// <param name="clientVersion"> The client version.</param>
        protected TcpClientBase(Version clientVersion)
            : this(defaultProtocol, clientVersion)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TcpClientBase"/> class.
        /// </summary>
        /// <param name="protocol">The <see cref="T:Photon.SocketServer.IRpcProtocol"/> to use for operation and event serialization.</param>
        /// <param name="clientVersion"> The client version.</param>
        protected TcpClientBase(IRpcProtocol protocol, Version clientVersion)
        {
            this.socketSynRoot = new object();
            this.protocol = protocol;
            this.clientVersion = clientVersion ?? Versions.TcpClientVersion;
            this.binaryReader = new TcpBinaryReader();
            this.binaryReader.OnDataReceived += new Action<byte[], SendParameters>(this.OnDataReceived);
            this.binaryReader.OnPingResponse += new Action<PingResponse>(this.OnPingResponse);
        }

        /// <summary>
        /// Establishes a connection to the remote host.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point to connect to.</param>
        /// <param name="applicationId">The photon application to connect to.</param>
        public void Connect(IPEndPoint remoteEndPoint, string applicationId)
        {
            this.Connect(remoteEndPoint, applicationId, null);
        }

        /// <summary>
        ///  Establishes a connection to the remote host.
        /// </summary>
        /// <param name="remoteEndPoint"> The remote end point to connect to.</param>
        /// <param name="applicationId"> The photon application to connect to.</param>
        /// <param name="localEndPoint"> The local end point to bind the socket to. Use null to avoid binding.</param>
        public void Connect(IPEndPoint remoteEndPoint, string applicationId, IPEndPoint localEndPoint)
        {
            if (this.Connected)
            {
                throw new InvalidOperationException("already connected");
            }
            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }
            if (applicationId == null)
            {
                throw new ArgumentNullException("applicationId");
            }
            this.ApplicationId = applicationId;
            this.RemoteEndPoint = remoteEndPoint;
            Socket sender = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            if (localEndPoint != null)
            {
                sender.Bind(localEndPoint);
            }
            SocketAsyncEventArgs e = new SocketAsyncEventArgs
            {
                RemoteEndPoint = remoteEndPoint
            };
            e.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnConnectAsyncCompleted);
            lock (this.socketSynRoot)
            {
                this.socketConnection = sender;
            }
            if (!sender.ConnectAsync(e))
            {
                this.OnConnectAsyncCompleted(sender, e);
            }
        }

        /// <summary>
        /// Closes the connection to the remote host.
        /// </summary>
        public void Disconnect()
        {
            lock (this.socketSynRoot)
            {
                if (this.Connected && (this.socketConnection != null))
                {
                    if (this.socketConnection.Connected)
                    {
                        this.socketConnection.Disconnect(false);
                    }
                    this.socketConnection.Close();
                    this.socketConnection = null;
                }
            }
        }

        /// <summary>
        ///  Initializes the peer to receive and send encrypted operations.
        /// </summary>
        /// <returns>
        ///  Returns <see cref="F:Photon.SocketServer.SendResult.Ok"/> if the event was successfully sent; 
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
        /// Sends an operation request to the server.
        /// </summary>
        /// <param name="operationRequest"></param>
        /// <param name="sendParameters"></param>
        /// <returns></returns>
        public SendResult SendOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            byte[] buffer;
            if (sendParameters.Encrypted)
            {
                if (this.CryptoProvider == null)
                {
                    return SendResult.EncryptionNotSupported;
                }
                buffer = this.Protocol.SerializeOperationRequestEncrypted(operationRequest, this.CryptoProvider);
            }
            else
            {
                buffer = this.Protocol.SerializeOperationRequest(operationRequest);
            }
            return this.Send(buffer, sendParameters);
        }

        /// <summary>
        /// Sends a ping request to the server.
        /// The ping request will be send with <see cref="P:System.Environment.TickCount"/> 
        /// as the tme stamp.
        /// </summary>
        /// <returns>Returns OK or disconnected.</returns>
        public SendResult SendPing()
        {
            return this.SendPing(Environment.TickCount);
        }

        /// <summary>
        /// Sends a ping request to the server.
        /// </summary>
        /// <param name="timeStamp"> A user definined time stamp. 
        ///    The time stamp will be send back by the server with the ping response
        ///   ans can be used to mesure the duration of the request.</param>
        /// <returns>Returns OK or disconnected.</returns>
        public SendResult SendPing(int timeStamp)
        {
            Socket socketConnection = this.socketConnection;
            if ((socketConnection == null) || !socketConnection.Connected)
            {
                return SendResult.Disconnected;
            }
            List<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>(2) {
            new ArraySegment<byte>(new byte[] { 240 }),
            new ArraySegment<byte>(BitConverter.GetBytes(timeStamp))
        };
            try
            {
                socketConnection.Send(buffers);
            }
            catch (SocketException exception)
            {
                SendResult result;
                if (!this.HandleSocketException(exception, out result))
                {
                    throw;
                }
                return result;
            }
            return SendResult.Ok;
        }

        /// <summary>
        /// Releases all resources used this instance.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Disconnect();
                this.binaryReader.OnDataReceived -= new Action<byte[], SendParameters>(this.OnDataReceived);
                this.binaryReader.OnPingResponse -= new Action<PingResponse>(this.OnPingResponse);
            }
        }

        /// <summary>
        /// Invoked when data was received.
        /// </summary>
        /// <param name="data">The received data.</param>
        /// <param name="sendParameters"> The send parameters the data was received with.</param>
        protected virtual void OnReceived(byte[] data, SendParameters sendParameters)
        {
            if (data.Length < 2)
            {
                log.WarnFormat("Received data to short: length={0}", new object[] { data.Length });
            }
            else
            {
                RtsMessageType messageType = (RtsMessageType)((byte)(data[1] & 0x7f));
                sendParameters.Encrypted = (data[1] & 0x80) == 0x80;
                switch (messageType)
                {
                    case RtsMessageType.OperationResponse:
                    case RtsMessageType.InternalOperationResponse:
                        this.ParseOperationResponse(data, sendParameters, messageType);
                        return;

                    case RtsMessageType.Event:
                        this.ParseEventData(data, sendParameters);
                        return;
                }
                log.Warn("Unexpected message type " + messageType);
            }
        }

        /// <summary>
        ///   A asynchronous operation completed with a <see cref="T:System.Net.Sockets.SocketError"/>.
        /// </summary>
        /// <param name="socketError">The socket error.</param>
        protected abstract void OnAsyncSocketError(SocketError socketError);

        /// <summary>
        /// On connect completed...
        /// </summary>
        protected abstract void OnConnectCompleted();

        /// <summary>
        /// On connect error...
        /// </summary>
        /// <param name="socketError"> The error.</param>
        protected abstract void OnConnectError(SocketError socketError);

        /// <summary>
        /// On disconnect ...
        /// </summary>
        /// <param name="socketError"> The error.</param>
        protected abstract void OnDisconnect(SocketError socketError);

        /// <summary>
        /// The on event.
        /// </summary>
        /// <param name="eventData"> The event data.</param>
        /// <param name="sendParameters"> The send parameters the event was received with.</param>
        protected abstract void OnEvent(IEventData eventData, SendParameters sendParameters);

        /// <summary>
        /// Invoked if an initialize encryption request was completed.
        /// </summary>
        /// <param name="resultCode">The result code.</param>
        /// <param name="debugMessage">The debuf message.</param>
        protected virtual void OnInitializeEcryptionCompleted(short resultCode, string debugMessage)
        {
        }

        /// <summary>
        /// The on operation response.
        /// </summary>
        /// <param name="operationResponse"> The operation response.</param>
        /// <param name="sendParameters"> The send parameters the event was received with.</param>
        protected abstract void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters);

        /// <summary>
        ///  The on ping response.
        /// </summary>
        /// <param name="pingResponse"> The ping response.</param>
        protected abstract void OnPingResponse(PingResponse pingResponse);

        /// <summary>
        ///  Begin async receive ...
        /// </summary>
        /// <param name="socket"> The socket.</param>
        /// <param name="e">The event args.</param>
        private void BeginReceive(Socket socket, SocketAsyncEventArgs e)
        {
            if (!socket.Connected)
            {
                if (this.Connected)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("BeginReceive 1 Disconnect: {0}", new object[] { e.SocketError });
                    }
                    e.Dispose();
                    this.Connected = false;
                    this.OnDisconnect(SocketError.Success);
                }
            }
            else if (!socket.ReceiveAsync(e))
            {
                this.OnReceiveAsyncCompleted(socket, e);
            }
        }

        /// <summary>
        /// On async connect completed ...
        /// </summary>
        /// <param name="sender"> The sender.</param>
        /// <param name="e">The e.</param>
        private void OnConnectAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("OnConnectError: " + e.SocketError);
                    }
                    this.OnConnectError(e.SocketError);
                }
                else
                {
                    Socket socket = (Socket)sender;
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs
                    {
                        RemoteEndPoint = e.RemoteEndPoint
                    };
                    args.SetBuffer(new byte[0x800], 0, 0x800);
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnReceiveAsyncCompleted);
                    this.LocalEndPoint = (IPEndPoint)socket.LocalEndPoint;
                    this.BeginReceive(socket, args);
                    this.SendInitRequest();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException exception)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("OnAsyncSocketError: " + exception.SocketErrorCode);
                }
                this.OnAsyncSocketError(exception.SocketErrorCode);
            }
            finally
            {
                e.Dispose();
            }
        }

        /// <summary>
        /// Handles the InitEncryption response.
        /// </summary>
        /// <param name="response">The response.</param>
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
                        return;
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
        /// On async receive completed ...
        /// </summary>
        /// <param name="sender">  The sender.</param>
        /// <param name="e">The e.</param>
        private void OnReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if ((e.LastOperation == SocketAsyncOperation.Disconnect) || (e.BytesTransferred == 0))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OnReceiveAsyncCompleted 1 Disconnect: {0}", new object[] { e.SocketError });
                    }
                    if (this.Connected)
                    {
                        this.Connected = false;
                        e.Dispose();
                        this.OnDisconnect(e.SocketError);
                    }
                }
                else if (e.SocketError != SocketError.Success)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OnReceiveAsyncCompleted 2 Disconnect: {0}", new object[] { e.SocketError });
                    }
                    if (this.Connected)
                    {
                        this.Connected = false;
                        e.Dispose();
                        this.OnDisconnect(e.SocketError);
                    }
                }
                else
                {
                    if (operationDataLogger.IsDebugEnabled)
                    {
                        operationDataLogger.DebugFormat("OnReceiveAsyncCompleted - data=({0} bytes) {1}", new object[] { e.BytesTransferred, BitConverter.ToString(e.Buffer, 0, e.BytesTransferred) });
                    }
                    this.binaryReader.Parse(e.Buffer, e.BytesTransferred);
                    this.BeginReceive((Socket)sender, e);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException exception)
            {
                this.OnAsyncSocketError(exception.SocketErrorCode);
            }
            catch (Exception exception2)
            {
                log.Error(exception2);
            }
        }

        /// <summary>
        ///  Sends bytes ...
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="sendParameters">The send parameters are received on the server and can be used for further dispatching.</param>
        /// <returns>Ok or disconnected</returns>
        public SendResult Send(byte[] data, SendParameters sendParameters)
        {
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("Send - data=({0} bytes) {1}", new object[] { data.Length, BitConverter.ToString(data) });
            }
            int num = data.Length + 7;
            byte channelId = sendParameters.ChannelId;
            byte num3 = sendParameters.Unreliable ? ((byte)0) : ((byte)1);
            byte[] array = new byte[] { 0xfb, (byte)((num >> 0x18) & 0xff), (byte)((num >> 0x10) & 0xff), (byte)((num >> 8) & 0xff), (byte)(num & 0xff), channelId, num3 };
            List<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>(2) {
            new ArraySegment<byte>(array),
            new ArraySegment<byte>(data)
        };
            Socket socketConnection = this.socketConnection;
            if ((socketConnection == null) || !socketConnection.Connected)
            {
                return SendResult.Disconnected;
            }
            try
            {
                socketConnection.Send(buffers);
            }
            catch (SocketException exception)
            {
                SendResult result;
                if (!this.HandleSocketException(exception, out result))
                {
                    throw;
                }
                return result;
            }
            return SendResult.Ok;
        }

        /// <summary>
        /// Sends an init request
        /// </summary>
        private void SendInitRequest()
        {
            byte[] data = this.Protocol.SerializeInitRequest(this.ApplicationId, this.ClientVersion);
            this.Send(data, new SendParameters());
        }

        /// <summary>
        /// Sends an internal operaton request.
        /// </summary>
        /// <param name="request">  The request.</param>
        /// <returns> the send result</returns>
        private SendResult SendInternalOperationRequest(OperationRequest request)
        {
            byte[] data = this.Protocol.SerializeInternalOperationRequest(request);
            return this.Send(data, new SendParameters());
        }

        private bool HandleSocketException(SocketException exception, out SendResult sendResult)
        {
            switch (exception.SocketErrorCode)
            {
                case SocketError.ConnectionReset:
                    this.Disconnect();
                    this.OnDisconnect(exception.SocketErrorCode);
                    sendResult = SendResult.Disconnected;
                    return true;

                case SocketError.NoBufferSpaceAvailable:
                    sendResult = SendResult.SendBufferFull;
                    return true;
            }
            this.Disconnect();
            this.OnDisconnect(exception.SocketErrorCode);
            sendResult = SendResult.Disconnected;
            return false;
        }

        private void OnDataReceived(byte[] data, SendParameters sendParameters)
        {
            if (this.Connected)
            {
                this.OnReceived(data, sendParameters);
            }
            else if (data.Length < 2)
            {
                log.WarnFormat("Received data to short: length={0}", new object[] { data.Length });
            }
            else
            {
                RtsMessageType type = (RtsMessageType)((byte)(data[1] & 0x7f));
                sendParameters.Encrypted = (data[1] & 0x80) == 0x80;
                if (type != RtsMessageType.InitResponse)
                {
                    log.Warn("Unexpected message type " + type);
                }
                else
                {
                    this.Connected = true;
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("OnConnectCompleted");
                    }
                    this.OnConnectCompleted();
                }
            }
        }

        private void ParseEventData(byte[] data, SendParameters sendParameters)
        {
            bool flag;
            EventData data2;
            if (!sendParameters.Encrypted)
            {
                flag = this.Protocol.TryParseEventData(data, out data2);
            }
            else
            {
                if ((this.CryptoProvider == null) && log.IsWarnEnabled)
                {
                    log.WarnFormat("Received encrypted event data with uninitialized crypto provider.", new object[0]);
                    return;
                }
                flag = this.Protocol.TryParseEventDataEncrypted(data, this.CryptoProvider, out data2);
            }
            if (!flag)
            {
                log.Warn("Error in encrypted event");
            }
            else
            {
                this.OnEvent(data2, sendParameters);
            }
        }

        private void ParseOperationResponse(byte[] data, SendParameters sendParameters, RtsMessageType messageType)
        {
            bool flag;
            OperationResponse response;
            if (sendParameters.Encrypted)
            {
                if ((this.CryptoProvider == null) && log.IsWarnEnabled)
                {
                    log.WarnFormat("Received encrypted operation response with uninitialized crypto provider.", new object[0]);
                    return;
                }
                flag = this.Protocol.TryParseOperationResponseEncrypted(data, this.CryptoProvider, out response);
            }
            else
            {
                flag = this.Protocol.TryParseOperationResponse(data, out response);
            }
            if (!flag)
            {
                log.Warn("Error in operation response");
            }
            else if (messageType == RtsMessageType.OperationResponse)
            {
                this.OnOperationResponse(response, sendParameters);
            }
            else
            {
                this.OnInternalOperationResponse(response);
            }
        }

        /// <summary>
        /// Gets the application id.
        /// </summary>
        public string ApplicationId
        {
            get
            {
                return this.application;
            }
            private set
            {
                Interlocked.Exchange<string>(ref this.application, value);
            }
        }

        /// <summary>
        ///  Gets the client version.
        /// </summary>
        public Version ClientVersion
        {
            get
            {
                return this.clientVersion;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is connected to a remote host.
        /// </summary>
        public bool Connected
        {
            get
            {
                return Convert.ToBoolean(this.connected);
            }
            private set
            {
                Thread.VolatileWrite(ref this.connected, Convert.ToByte(value));
            }
        }

        /// <summary>
        ///  Gets or sets the CryptoProvider.
        /// </summary>
        public ICryptoProvider CryptoProvider
        {
            get
            {
                return this.binaryReader.CryptoProvider;
            }
            set
            {
                this.binaryReader.CryptoProvider = value;
            }
        }

        /// <summary>
        ///  Gets the local endpoint.
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return this.localEnd;
            }
            private set
            {
                Interlocked.Exchange<IPEndPoint>(ref this.localEnd, value);
            }
        }

        /// <summary>
        /// Gets the used rpc protocol.
        /// </summary>
        public IRpcProtocol Protocol
        {
            get
            {
                return this.protocol;
            }
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return this.remoteEnd;
            }
            private set
            {
                Interlocked.Exchange<IPEndPoint>(ref this.remoteEnd, value);
            }
        }
    }
}
