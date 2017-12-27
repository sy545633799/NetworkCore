namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;

    //<summary>
    // Class to handle Http based connections to a Photon server.
    // Requests are done asynchronous and not queued at all.

    // All responses are put into the game's thread-context and 
    // all results and state changes are done within calls of
    // Service() or DispatchIncomingCommands().
    // </summary>
    internal class HttpBase3 : PeerBase
    {
        private int _challengId;
        private List<byte> _connectionLimitCache;
        private MemoryStream _connectionLimitStream;
        private int _grossErrorCount;
        private InvocationCache _invocationCache;
        private int _lastSendAck;
        private int _maxConnectionsCount;
        private int _minConnectionsCount;
        private Dictionary<int, AsyncRequestState> _requestCache;
        private static int _rerequestCountBeforeDiconnect = 30;
        private static Random _rnd = new Random();
        private bool _sendAck;
        private List<byte[]> _sendCache;
        private int _stateIdBase;

        internal GetLocalMsTimestampDelegate GetLocalMsTimestamp;
        private string HttpPeerID;
        private List<byte[]> incomingList;
        private long lastPingTimeStamp;
        private const int MAX_GROSS_ERRORS = 3;
        internal static readonly byte[] messageHeader = new byte[] { 0xfb, 0, 0, 0, 0, 0, 0, 0xf3, 2 };
        private MemoryStream outgoingStream;
        private static readonly byte[] pingData = null;
        internal const int TCP_HEADER_LEN = 7;
        private string UrlParameters;

        internal HttpBase3()
        {
            this._challengId = 0;
            this.incomingList = new List<byte[]>();
            this._lastSendAck = 0;
            this._sendAck = false;
            this._invocationCache = new InvocationCache();
            this._minConnectionsCount = 2;
            this._maxConnectionsCount = 4;
            this._requestCache = new Dictionary<int, AsyncRequestState>();
            this._stateIdBase = -1;
            this._sendCache = new List<byte[]>();
            this.outgoingStream = null;
            this._connectionLimitCache = new List<byte>();
            this._connectionLimitStream = new MemoryStream(0x4b0);
            this._grossErrorCount = 0;
            this.GetLocalMsTimestamp = delegate
            {
                return Environment.TickCount;
            };
            PeerBase.peerCount = (short)(PeerBase.peerCount + 1);
            this._challengId = _rnd.Next();
            base.InitOnce();
        }

        internal HttpBase3(IPhotonPeerListener listener)
            : this()
        {
            base.Listener = listener;
        }

        private void _addAckId(ref string urlParamter)
        {
            if (this._sendAck)
            {
                this._sendAck = false;
                urlParamter = urlParamter + "&ack=" + this._lastSendAck;
                if (base.debugOut >= DebugLevel.ALL)
                {
                    base.Listener.DebugReturn(DebugLevel.ALL, string.Format("ack sent for id {0}, pid={1}, cid={2}", this._lastSendAck, this.HttpPeerID, this._challengId));
                }
            }
        }

        private void _checkAckCondition()
        {
            if ((this._requestCache.Keys.Count != 0) && (((this._stateIdBase - this._maxConnectionsCount) - this._lastSendAck) > 5))
            {
                int minRequestId = ((IEnumerable<int>)this._requestCache.Keys).Min();
                if ((minRequestId != 0x7fffffff) && ((minRequestId - this._lastSendAck) > 5))
                {
                    this._lastSendAck = minRequestId - 2;
                    this._sendAck = true;
                }
            }
        }

        private static int _getStatusCodeFromResponse(HttpWebResponse response, HttpBase3 peer)
        {
            int statusCode = 0;
            if (response.ContentLength >= 4L)
            {
                try
                {
                    BinaryReader binReader = new BinaryReader(response.GetResponseStream());
                    statusCode |= binReader.ReadByte() << 0x18;
                    statusCode |= binReader.ReadByte() << 0x10;
                    statusCode |= binReader.ReadByte() << 8;
                    statusCode |= binReader.ReadByte();
                }
                catch (Exception e)
                {
                    peer.Listener.DebugReturn(DebugLevel.ERROR, string.Format("Exception '{0}' happened during response status reading", e.Message));
                }
            }
            return statusCode;
        }

        private static bool _gotIgnoreStatus(HttpStatusCode code)
        {
            return (code == HttpStatusCode.NotFound);
        }

        private static void _handleError(AsyncRequestState state, HttpWebRequest request, WebException webEx)
        {
            if (_isExceptionGross(webEx.Status))
            {
                Interlocked.Increment(ref state.Base._grossErrorCount);
            }
            if (state.Base._grossErrorCount >= 3)
            {
                if (state.Base.debugOut >= DebugLevel.ALL)
                {
                    state.Base.Listener.DebugReturn(DebugLevel.ALL, "limit of gross errors reached. Connection closed");
                }
                state.Base.EnqueueErrorDisconnect(StatusCode.InternalReceiveException);
            }
            else
            {
                _rerequestState(state, state.Base.UrlParameters, request);
            }
        }

        private static bool _isExceptionGross(WebExceptionStatus webExceptionStatus)
        {
            return (webExceptionStatus == WebExceptionStatus.ConnectFailure);
        }

        private void _parseMessage(byte[] inBuff, BinaryReader br)
        {
            int len = inBuff.Length;
            using (Stream baseStream = br.BaseStream)
            {
                using (br)
                {
                    while (baseStream.Position != baseStream.Length)
                    {
                        int dataLen = _readMessageHeader(br) - 7;
                        if (dataLen == -1)
                        {
                            if (base.debugOut >= DebugLevel.ERROR)
                            {
                                base.Listener.DebugReturn(DebugLevel.ERROR, string.Format("Invalid message header for pid={0} cid={1} and message {2}", this.HttpPeerID, this._challengId, ByteArrayToString(inBuff)));
                            }
                            br.Close();
                            baseStream.Close();
                            return;
                        }
                        Debug.Assert(dataLen >= 2);
                        byte[] msgBuf = br.ReadBytes(dataLen);
                        if (dataLen < 2)
                        {
                            base.Listener.DebugReturn(DebugLevel.WARNING, string.Format("data len is to small. data {0}", ByteArrayToString(inBuff)));
                        }
                        lock (this.incomingList)
                        {
                            this.incomingList.Add(msgBuf);
                            if ((this.incomingList.Count % base.warningSize) == 0)
                            {
                                base.EnqueueStatusCallback(StatusCode.QueueIncomingReliableWarning);
                            }
                        }
                    }
                    br.Close();
                    baseStream.Close();
                }
            }
        }

        private static int _readMessageHeader(BinaryReader br)
        {
            if (br.ReadByte() != 0xfb)
            {
                return -1;
            }
            int msgLen = (((br.ReadByte() << 0x18) | (br.ReadByte() << 0x10)) | (br.ReadByte() << 8)) | br.ReadByte();
            byte reliable = br.ReadByte();
            byte channel = br.ReadByte();
            return msgLen;
        }

        private static void _rerequestState(AsyncRequestState state, string url, HttpWebRequest request)
        {
            if (state.rerequested < _rerequestCountBeforeDiconnect)
            {
                state.rerequested++;
                state.restarting = false;
                if (state.Base.debugOut >= DebugLevel.ALL)
                {
                    state.Base.Listener.DebugReturn(DebugLevel.ALL, string.Format("rerequest for state {0} from exception handler", state.id));
                }
                state.Base.Request(state, url);
            }
            else
            {
                if (state.Base.debugOut >= DebugLevel.ERROR)
                {
                    state.Base.EnqueueDebugReturn(DebugLevel.ERROR, "Exception Url: " + request.RequestUri);
                }
                state.Base.EnqueueErrorDisconnect(StatusCode.Exception);
            }
        }

        private void _sendPing()
        {
            int count = this._minConnectionsCount - this._requestCache.Count;
            if (count > 0)
            {
                this.lastPingTimeStamp = this.GetLocalMsTimestamp();
                for (int i = 0; i < count; i++)
                {
                    this.Request(pingData, this.UrlParameters);
                }
            }
            else if ((this.GetLocalMsTimestamp() - this.lastPingTimeStamp) > base.timePingInterval)
            {
                this.lastPingTimeStamp = this.GetLocalMsTimestamp();
                this.Request(pingData, this.UrlParameters);
            }
        }

        private static void _webExceptionHandler(AsyncRequestState state, HttpWebRequest request, WebException webEx)
        {
            HttpWebResponse response = (HttpWebResponse)webEx.Response;
            if ((((state.Base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting) || (state.Base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)) && !state.restarting) && (state.Base.debugOut >= DebugLevel.ERROR))
            {
                if (response != null)
                {
                    int subCode = _getStatusCodeFromResponse(response, state.Base);
                    state.Base.Listener.DebugReturn(DebugLevel.ERROR, string.Format("Request {0} for pid={1} cid={2} failed with exception {3}, msg: {4}, status code: {5}, reason: {6}", state.id, state.Base.HttpPeerID, state.Base._challengId, webEx.Status, webEx.Message, subCode, response.StatusDescription));
                }
                else
                {
                    state.Base.Listener.DebugReturn(DebugLevel.ERROR, string.Format("Request {0} for pid={1} cid={2} failed with exception {3}, msg: {4}", state.id, state.Base.HttpPeerID, state.Base._challengId, webEx.Status, webEx.Message));
                }
            }
            if (state.Base.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
            {
                state.Base.EnqueueErrorDisconnect(StatusCode.ExceptionOnConnect);
            }
            else if (state.IsDisconnect || (state.Base.peerConnectionState != PeerBase.ConnectionStateValue.Connected))
            {
                state.Base.Listener.DebugReturn(DebugLevel.ERROR, string.Format("pid={0} cid={1} is already disconnected", state.Base.HttpPeerID, state.Base._challengId));
            }
            else if (state.Aborted)
            {
                state.Base.EnqueueErrorDisconnect(StatusCode.TimeoutDisconnect);
            }
            else if (response != null)
            {
                if (_gotIgnoreStatus(response.StatusCode))
                {
                    if (state.Base.debugOut >= DebugLevel.ALL)
                    {
                        state.Base.Listener.DebugReturn(DebugLevel.ALL, "got statues which we ignore");
                    }
                }
                else
                {
                    state.Base.EnqueueErrorDisconnect(StatusCode.DisconnectByServer);
                }
            }
            else
            {
                _handleError(state, request, webEx);
            }
        }

        public static string ByteArrayToString(byte[] ba)
        {
            if (ba == null)
            {
                return "null";
            }
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
            {
                hex.AppendFormat("{0:x2} ", b);
            }
            return hex.ToString();
        }

        internal override bool Connect(string serverAddress, string appID)
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
            {
                if (base.debugOut >= DebugLevel.WARNING)
                {
                    base.Listener.DebugReturn(DebugLevel.WARNING, "Connect() called while peerConnectionState != Disconnected. Nothing done.");
                }
                return false;
            }
            this.outgoingStream = new MemoryStream(PeerBase.outgoingStreamBufferSize);
            base.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
            base.ServerAddress = serverAddress;
            this.HttpPeerID = "";
            this.UrlParameters = "?init&cid=";
            this.UrlParameters = this.UrlParameters + this._challengId;
            if (appID == null)
            {
                appID = "NUnit";
            }
            this.UrlParameters = this.UrlParameters + "&app=" + appID;
            this.lastPingTimeStamp = this.GetLocalMsTimestamp();
            for (int i = 0; i < 0x20; i++)
            {
                base.INIT_BYTES[i + 9] = (i < appID.Length) ? ((byte)appID[i]) : ((byte)0);
            }
            new Thread(() =>
            {
                this.Request(base.INIT_BYTES, this.UrlParameters, MessageType.CONNECT);
            }).Start();
            return true;
        }

        internal override void Disconnect()
        {
            if ((base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected) && (base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting))
            {
                this.Request(null, this.UrlParameters, MessageType.DISCONNECT);
                base.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
            }
        }

        internal void Disconnected()
        {
            this.InitPeerBase();
            base.Listener.OnStatusChanged(StatusCode.Disconnect);
        }

        internal override bool DispatchIncomingCommands()
        {
            byte[] payload;
            lock (base.ActionQueue)
            {
                while (base.ActionQueue.Count > 0)
                {
                    base.ActionQueue.Dequeue()();
                }
            }
            lock (this.incomingList)
            {
                if (this.incomingList.Count <= 0)
                {
                    return false;
                }
                payload = this.incomingList[0];
                this.incomingList.RemoveAt(0);
            }
            base.ByteCountCurrentDispatch = payload.Length + 3;
            if (payload.Length < 2)
            {
                base.Listener.DebugReturn(DebugLevel.WARNING, string.Format("message has length less then 2. data {0}", ByteArrayToString(payload)));
            }
            return this.DeserializeMessageAndCallback(payload);
        }

        /// <summary>
        /// Called internally when some error (or timeout) causes a disconnect. this takes care state is set and callbacks are done (once)
        /// </summary>
        /// <param name="statusCode"></param>
        private void EnqueueErrorDisconnect(StatusCode statusCode)
        {
            lock (this)
            {
                if ((base.peerConnectionState != PeerBase.ConnectionStateValue.Connected) && (base.peerConnectionState != PeerBase.ConnectionStateValue.Connecting))
                {
                    return;
                }
                base.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
            }
            if (base.debugOut >= DebugLevel.INFO)
            {
                base.Listener.DebugReturn(DebugLevel.INFO, string.Format("pid={0} cid={1} is disconnected", this.HttpPeerID, this._challengId));
            }
            base.EnqueueStatusCallback(statusCode);
            base.EnqueueActionForDispatch(delegate
            {
                this.Disconnected();
            });
        }

        internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted, PeerBase.EgMessageType messageType)
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Not connected. PeerState: " + base.peerConnectionState);
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
                return false;
            }
            byte[] fullMessageBytes = this.SerializeOperationToMessage(opCode, parameters, messageType, encrypted);
            if (fullMessageBytes == null)
            {
                return false;
            }
            this._sendCache.Add(fullMessageBytes);
            return true;
        }

        internal override void FetchServerTimestamp()
        {
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            AsyncRequestState state = (AsyncRequestState)asynchronousResult.AsyncState;
            state.cancelTimeout();
            HttpWebRequest request = state.Request;
            try
            {
                Stream postStream = request.EndGetRequestStream(asynchronousResult);
                if (state.OutgoingData != null)
                {
                    if (state.type == MessageType.CONNECT)
                    {
                        byte[] tcpheader = new byte[7] { 0xfb, 0, 0, 0, 0, 0, 0x1 };

                        writeLenght(tcpheader, state.OutgoingData.Length + tcpheader.Length, 1);
                        postStream.Write(tcpheader, 0, tcpheader.Length);
                    }
                    postStream.Write(state.OutgoingData, 0, state.OutgoingData.Length);
                }
                postStream.Close();
                IAsyncResult result = request.BeginGetResponse(new AsyncCallback(this.GetResponseCallback), state);
                state.asyncResult = result;
            }
            catch (WebException webEx)
            {
                _webExceptionHandler(state, request, webEx);
            }
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            AsyncRequestState state = (AsyncRequestState)asynchronousResult.AsyncState;
            HttpWebRequest request = state.Request;
            state.cancelTimeout();
            byte[] responseData = null;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult))
                {
                    if (state.IsDisconnect)
                    {
                        state.Base.EnqueueActionForDispatch(delegate
                        {
                            state.Base.Disconnected();
                        });
                        response.Close();
                        return;
                    }
                    using (Stream streamResponse = response.GetResponseStream())
                    {
                        using (BinaryReader binReader = new BinaryReader(streamResponse))
                        {
                            responseData = binReader.ReadBytes((int)response.ContentLength);
                            streamResponse.Close();
                            binReader.Close();
                        }
                    }
                    response.Close();
                }
                if (responseData.Length > 0)
                {
                    state.Base.ReceiveIncomingCommands(responseData, responseData.Length);
                }
                Interlocked.Exchange(ref state.Base._grossErrorCount, 0);
            }
            catch (WebException webEx)
            {
                _webExceptionHandler(state, request, webEx);
                return;
            }
            lock (state.Base._requestCache)
            {
                state.Base._checkAckCondition();
                state.Base._requestCache.Remove(state.id);
                state.Request = null;
                state.OutgoingData = null;
            }
        }

        internal override void InitPeerBase()
        {
            base.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
        }

        internal override void ReceiveIncomingCommands(byte[] inBuff, int dataLen)
        {
            if (base.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
            {
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(inBuff, 4, 4);
                }
                this.HttpPeerID = BitConverter.ToInt32(inBuff, 4).ToString();
                this.UrlParameters = string.Concat(new object[] { "?pid=", this.HttpPeerID, "&cid=", this._challengId });
                base.peerConnectionState = PeerBase.ConnectionStateValue.Connected;
                base.EnqueueActionForDispatch(new PeerBase.MyAction(this.InitCallback));
            }
            else
            {
                base.timestampOfLastReceive = this.GetLocalMsTimestamp();
                base.bytesIn += inBuff.Length + 7;
                Array.Reverse(inBuff, 0, 4);
                using (BinaryReader br = new BinaryReader(new MemoryStream(inBuff)))
                {
                    int responseId = br.ReadInt32();
                    this._invocationCache.Invoke(responseId, delegate
                    {
                        this._parseMessage(inBuff, br);
                    });
                }
            }
        }

        //<summary>
        // The initial request uses "?init" as UrlParameters and the binary data is the app-id (binary in up to 32 bytes).
        // Init returns a GUID for use in following requests as UrlParameters "?pid=GUID".
        //</summary>
        //<param name="data"></param>
        //<param name="urlParamter">The url paramters to append to the server adress uri.</param>
        internal void Request(byte[] data, string urlParamter)
        {
            this.Request(data, urlParamter, MessageType.NORMAL);
        }

        private void Request(AsyncRequestState state, string urlParamter)
        {
            if (this.UseGet)
            {
                urlParamter = urlParamter + string.Format("&seq={0}&data={1}", state.id, Convert.ToBase64String(state.OutgoingData, Base64FormattingOptions.None));
            }
            else
            {
                urlParamter = urlParamter + string.Format("&seq={0}", state.id);
            }
            if (state.type == MessageType.DISCONNECT)
            {
                urlParamter = urlParamter + "&abrt";
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(base.ServerAddress + urlParamter + base.HttpUrlParameters);
            if (this.UseGet)
            {
                request.Method = "GET";
                request.Pipelined = true;
            }
            else
            {
                request.Method = "POST";
            }
            request.Proxy = null;
            state.Request = request;
            request.KeepAlive = true;
            this.lastPingTimeStamp = this.GetLocalMsTimestamp();
            if (this.UseGet)
            {
                IAsyncResult result = request.BeginGetResponse(new AsyncCallback(this.GetResponseCallback), state);
                state.asyncResult = result;
            }
            else
            {
                request.BeginGetRequestStream(new AsyncCallback(this.GetRequestStreamCallback), state);
            }
        }

        internal void Request(byte[] data, string urlParameter, MessageType type)
        {
            int id = Interlocked.Increment(ref this._stateIdBase);
            this._addAckId(ref urlParameter);
            AsyncRequestState state = new AsyncRequestState()
            {
                Base = this,
                OutgoingData = data,
                type = type,
                id = id
            };

            lock (this._requestCache)
            {
                this._requestCache.Add(id, state);
            }
            this.Request(state, urlParameter);
        }

        internal override bool SendOutgoingCommands()
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
            {
                return false;
            }
            if (this._maxConnectionsCount < this._requestCache.Count)
            {
                if (this._sendCache.Count > 0)
                {
                    this._connectionLimitStream.Write(this._sendCache[0], 0, this._sendCache[0].Length);
                    this._sendCache.RemoveAt(0);
                    if (base.debugOut >= DebugLevel.ALL)
                    {
                        base.Listener.DebugReturn(DebugLevel.ALL, string.Format("Connection limit reached. Data pushed to cache. cache size {0}, pid={1} cid={2}", this._connectionLimitCache.Count, this.HttpPeerID, this._challengId));
                    }
                }
                return false;
            }
            bool result = false;
            if (this._connectionLimitStream.Length > 0L)
            {
                if (base.debugOut >= DebugLevel.ALL)
                {
                    base.Listener.DebugReturn(DebugLevel.ALL, string.Format("Connection limit data cache are sent. data size {0}, pid={1}, cid={2}", this._connectionLimitCache.Count, this.HttpPeerID, this._challengId));
                }
                this.Request(this._connectionLimitStream.ToArray(), this.UrlParameters);
                this._connectionLimitStream.SetLength(0L);
                this._connectionLimitStream.Position = 0L;
            }
            else if (this._sendCache.Count > 0)
            {
                if (this._sendCache.Count == 1)
                {
                    this.Request(this._sendCache[0], this.UrlParameters);
                    this._sendCache.RemoveAt(0);
                }
                else
                {
                    for (int i = 0; i < this._sendCache.Count; i++)
                    {
                        this.outgoingStream.Write(this._sendCache[i], 0, this._sendCache[i].Length);
                    }
                    this._sendCache.Clear();
                    this.Request(this.outgoingStream.ToArray(), this.UrlParameters);
                    this.outgoingStream.SetLength(0L);
                    this.outgoingStream.Position = 0L;
                }
                result = this._sendCache.Count > 0;
            }
            this._sendPing();
            return result;
        }

        internal override byte[] SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt)
        {
            byte[] fullMessageBytes;
            lock (base.SerializeMemStream)
            {
                base.SerializeMemStream.Position = 0L;
                base.SerializeMemStream.SetLength(0L);
                if (!encrypt)
                {
                    base.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
                }
                Protocol.SerializeOperationRequest(base.SerializeMemStream, opCode, parameters, false);
                if (encrypt)
                {
                    byte[] opBytes = base.SerializeMemStream.ToArray();
                    opBytes = base.CryptoProvider.Encrypt(opBytes);
                    base.SerializeMemStream.Position = 0L;
                    base.SerializeMemStream.SetLength(0L);
                    base.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
                    base.SerializeMemStream.Write(opBytes, 0, opBytes.Length);
                }
                fullMessageBytes = base.SerializeMemStream.ToArray();
            }
            if (messageType != PeerBase.EgMessageType.Operation)
            {
                fullMessageBytes[messageHeader.Length - 1] = (byte)messageType;
            }
            if (encrypt)
            {
                fullMessageBytes[messageHeader.Length - 1] = (byte)(fullMessageBytes[messageHeader.Length - 1] | 0x80);
            }
            int offsetForLength = 1;
            Protocol.Serialize(fullMessageBytes.Length, fullMessageBytes, ref offsetForLength);
            return fullMessageBytes;
        }

        internal override void StopConnection()
        {
            throw new NotImplementedException();
        }

        private static void TimeoutCallback(object stateObject, bool timedOut)
        {
            AsyncRequestState state = stateObject as AsyncRequestState;
            if ((timedOut && (state != null)) && (state.Request != null))
            {
                if (state.Base.debugOut >= DebugLevel.WARNING)
                {
                    state.Base.Listener.DebugReturn(DebugLevel.WARNING, string.Format("Request {0} for pid={1} cid={2} aborted by timeout", state.id, state.Base.HttpPeerID, state.Base._challengId));
                }
                state.Aborted = true;
                state.Request.Abort();
            }
        }

        private static void writeLenght(byte[] target, int value, int targetOffset)
        {
            target[targetOffset++] = (byte)(value >> 0x18);
            target[targetOffset++] = (byte)(value >> 0x10);
            target[targetOffset++] = (byte)(value >> 8);
            target[targetOffset++] = (byte)value;
        }

        public int currentRequestCount
        {
            get
            {
                return this._requestCache.Count;
            }
        }

        /// <summary>
        /// The *pid* for this peer, which is assigned by the server on connect (init).
        /// Initially this is Guid.Empty.
        /// </summary>
        public override string PeerID
        {
            get
            {
                return this.HttpPeerID;
            }
        }

        internal override int QueuedIncomingCommandsCount
        {
            get
            {
                return 0;
            }
        }

        internal override int QueuedOutgoingCommandsCount
        {
            get
            {
                return 0;
            }
        }

        public int totalRequestCount
        {
            get
            {
                return this._stateIdBase;
            }
        }

        public bool UseGet { get; set; }

        private class AsyncRequestState
        {
            public IAsyncResult asyncResult = null;
            public int id = 0;
            public RegisteredWaitHandle regWaitHandle = null;
            public int rerequested = 0;
            public bool restarting = false;
            public HttpBase3.MessageType type;
            private Stopwatch Watch = new Stopwatch();

            public AsyncRequestState()
            {
                this.Watch.Start();
            }

            public void cancelTimeout()
            {
                if (this.regWaitHandle != null)
                {
                    this.regWaitHandle.Unregister(this.asyncResult.AsyncWaitHandle);
                    this.regWaitHandle = null;
                }
                this.asyncResult = null;
            }

            public bool Aborted { get; set; }

            public HttpBase3 Base { get; set; }

            public int ElapsedTime
            {
                get
                {
                    return (int)this.Watch.ElapsedMilliseconds;
                }
            }

            public bool IsDisconnect
            {
                get
                {
                    return (this.type == HttpBase3.MessageType.DISCONNECT);
                }
            }

            public byte[] OutgoingData { get; set; }

            public HttpWebRequest Request { get; set; }
        }

        public delegate int GetLocalMsTimestampDelegate();

        internal enum MessageType
        {
            CONNECT,
            DISCONNECT,
            NORMAL
        }
    }
}
