namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    ///<summary>
    /// Instances of the PhotonPeer class are used to connect to a Photon server and communicate with it.
    ///</summary>
    ///<remarks>
    /// A PhotonPeer instance allows communication with the Photon Server, which in turn distributes messages
    /// to other PhotonPeer clients.<para></para>
    /// An application can use more than one PhotonPeer instance, which are treated as separate users on the
    /// server. Each should have its own listener instance, to separate the operations, callbacks and events.
    ///</remarks>
    public class PhotonPeer
    {
        private readonly object DispatchLockObject;

        private readonly object EnqueueLock;

        /// <summary>
        /// Implements the message-protocol, based on the underlying network protocol (udp, tcp, http).
        /// </summary>
        internal PeerBase peerBase;

        private readonly object SendOutgoingLockObject;

        protected internal PhotonPeer(ConnectionProtocol protocolType)
        {
            this.SendOutgoingLockObject = new object();
            this.DispatchLockObject = new object();
            this.EnqueueLock = new object();
            if (protocolType == ConnectionProtocol.Tcp)
            {
                this.peerBase = new TPeer();
                this.peerBase.usedProtocol = protocolType;
            }
            else if (protocolType == ConnectionProtocol.Udp)
            {
                this.peerBase = new EnetPeer();
                this.peerBase.usedProtocol = protocolType;
            }
            else if (protocolType == ConnectionProtocol.RHttp)
            {
                this.peerBase = new HttpBase3();
                this.peerBase.usedProtocol = protocolType;
            }
        }

        ///<summary>
        /// Creates a new PhotonPeer instance to communicate with Photon.<para></para>
        /// Connection is UDP based, except for Silverlight.
        ///</summary>
        ///<param name="listener">a IPhotonPeerListener implementation</param>
        [Obsolete("Use the constructor with ConnectionProtocol instead.")]
        public PhotonPeer(IPhotonPeerListener listener)
            : this(listener, ConnectionProtocol.Udp)
        {
        }

        ///<summary>
        /// Creates a new PhotonPeer instance to communicate with Photon and selects either UDP or TCP as
        /// protocol. We recommend UDP.
        ///</summary>
        ///<param name="listener">a IPhotonPeerListener implementation</param>
        ///<param name="protocolType">Protocol to use to connect to Photon.</param>
        public PhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
            : this(protocolType)
        {
            if (listener == null)
            {
                throw new Exception("listener cannot be null");
            }
            this.Listener = listener;
        }

        /// <summary>
        /// Deprecated. Please use: PhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType).
        /// </summary>
        /// <param name="protocolType"></param>
        [Obsolete("Use the constructor with ConnectionProtocol instead.")]
        public PhotonPeer(IPhotonPeerListener listener, bool useTcp)
            : this(listener, useTcp ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp)
        {
        }

        ///<summary>
        /// This method does a DNS lookup (if necessary) and connects to the given serverAddress.

        /// The return value gives you feedback if the address has the correct format. If so, this
        /// starts the process to establish the connection itself, which might take a few seconds.

        /// When the connection is established, a callback to IPhotonPeerListener.OnStatusChanged
        /// will be done. If the connection can't be established, despite having a valid address,
        /// the OnStatusChanged is called with an error-value.

        /// The applicationName defines the application logic to use server-side and it should match the name of
        /// one of the apps in your server's config.

        /// By default, the applicationName is "Lite" but other samples use "LiteLobby" and "MmoDemo" in
        /// Connect(). You can setup your own application and name it any way you like.
        ///</summary>
        ///<param name="serverAddress">
        /// Address of the Photon server. Format: ip:port (e.g. 127.0.0.1:5055) or hostname:port (e.g. localhost:5055)
        ///</param>
        ///<param name="applicationName">
        /// The name of the application to use within Photon or the appId of PhotonCloud.
        /// Should match a "Name" for an application, as setup in your PhotonServer.config.
        ///</param>
        ///<returns>
        /// true if IP is available (DNS name is resolved) and server is being connected. false on error.
        ///</returns>
        public virtual bool Connect(string serverAddress, string applicationName)
        {
            lock (this.DispatchLockObject)
            {
                lock (this.SendOutgoingLockObject)
                {
                    return this.peerBase.Connect(serverAddress, applicationName);
                }
            }
        }

        ///<summary>
        /// This method initiates a mutual disconnect between this client and the server.
        /// </summary>
        /// <remarks>
        /// Calling this method does not immediately close a connection. Disconnect lets the server
        /// know that this client is no longer listening. For the server, this is a much faster way
        /// to detect that the client is gone but it requires the client to send a few final messages.

        /// On completition, OnStatusChanged is called with the StatusCode.Disconnect.

        /// If the client is disconnected already or the connection thread is stopped, then there is no callback.

        /// Lite: The default server logic will leave any joined game and trigger the respective event
        /// (<see cref="F:ExitGames.Client.Photon.Lite.LiteEventCode.Leave" text="LiteEventCode.Leave"/>) for the remaining players.
        /// </remarks>
        public virtual void Disconnect()
        {
            lock (this.DispatchLockObject)
            {
                lock (this.SendOutgoingLockObject)
                {
                    this.peerBase.Disconnect();
                }
            }
        }

        ///<summary>
        /// This method directly causes the callbacks for events, responses and state changes
        /// within a IPhotonPeerListener. DispatchIncomingCommands only executes a single received
        /// command per call. If a command was dispatched, the return value is true and the method
        /// should be called again.
        /// This method is called by Service() until currently available commands are dispatched.
        /// </summary>
        /// <remarks>
        /// In general, this method should be called until it returns false. In a few cases, it might
        /// make sense to pause dispatching (if a certain state is reached and the app needs to load
        /// data, before it should handle new events).

        /// The callbacks to the peer's IPhotonPeerListener are executed in the same thread that is
        /// calling DispatchIncomingCommands. This makes things easier in a game loop: Event execution
        /// won't clash with painting objects or the game logic.
        /// </remarks>
        public virtual bool DispatchIncomingCommands()
        {
            this.peerBase.ByteCountCurrentDispatch = 0;
            if (this.TrafficStatsEnabled)
            {
                this.TrafficStatsGameLevel.DispatchIncomingCommandsCalled();
            }
            lock (this.DispatchLockObject)
            {
                return this.peerBase.DispatchIncomingCommands();
            }
        }

        ///<summary>
        /// This method creates a public key for this client and exchanges it with the server.
        /// </summary>
        /// <remarks>
        /// Encryption is not instantly available but calls OnStatusChanged when it finishes.
        /// Check for StatusCode EncryptionEstablished and EncryptionFailedToEstablish.

        /// Calling this method sets IsEncryptionAvailable to false.
        /// This method must be called before the "encrypt" parameter of OpCustom can be used.
        /// </remarks>
        /// <returns>If operation could be enqueued for sending</returns>
        public bool EstablishEncryption()
        {
            return this.peerBase.ExchangeKeysForEncryption();
        }

        ///<summary>
        /// This will fetch the server's timestamp and update the approximation for property ServerTimeInMilliseconds.

        /// The server time approximation will NOT become more accurate by repeated calls. Accuracy currently depends
        /// on a single roundtrip which is done as fast as possible.

        /// The command used for this is immediately acknowledged by the server. This makes sure the roundtrip time is
        /// low and the timestamp + rountriptime / 2 is close to the original value.
        /// </summary>
        public virtual void FetchServerTimestamp()
        {
            this.peerBase.FetchServerTimestamp();
        }

        ///<summary>
        ///Channel-less wrapper for OpCustom().
        ///</summary>
        ///<param name="customOpCode">Operations are handled by their byte\-typed code. The codes of the
        ///                           "Lite" application are in the struct <see cref="T:ExitGames.Client.Photon.Lite.LiteOpCode"/>.</param>
        ///<param name="customOpParameters">Containing parameters as key\-value pair. The key is byte\-typed, while the value is any serializable datatype.</param>
        ///<param name="sendReliable">Selects if the operation must be acknowledged or not. If false, the
        ///                           operation is not guaranteed to reach the server.</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable)
        {
            return this.OpCustom(customOpCode, customOpParameters, sendReliable, 0);
        }

        ///<summary>
        /// Allows the client to send any operation to the Photon Server by setting any opCode and the operation's parameters.
        /// </summary>
        /// <summary>
        /// Variant with encryption parameter.

        /// Use this only after encryption was established by EstablishEncryption and waiting for the OnStateChanged callback.
        /// </summary>
        /// <param name="customOpCode">Operations are handled by their byte\-typed code. The codes of the
        ///                            "Lite" application are in the struct <see cref="T:ExitGames.Client.Photon.Lite.LiteOpCode"/>.</param>
        /// <param name="customOpParameters">Containing parameters as key\-value pair. The key is byte\-typed, while the value is any serializable datatype.</param>
        /// <param name="sendReliable">Selects if the operation must be acknowledged or not. If false, the
        ///                            operation is not guaranteed to reach the server.</param>
        /// <param name="channelId">The channel in which this operation should be sent.</param>
        /// <param name="encrypt">Can only be true, while IsEncryptionAvailable is true, too.</param>
        /// <returns>If operation could be enqueued for sending</returns>
        public virtual bool OpCustom(OperationRequest operationRequest, bool sendReliable, byte channelId, bool encrypt)
        {
            if (!(!encrypt || this.IsEncryptionAvailable))
            {
                throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
            }
            lock (this.EnqueueLock)
            {
                return this.peerBase.EnqueueOperation(operationRequest.Parameters, operationRequest.OperationCode, sendReliable, channelId, encrypt);
            }
        }

        ///<summary>
        /// Allows the client to send any operation to the Photon Server by setting any opCode and the operation's parameters.
        ///</summary>
        ///<remarks></remarks>
        /// Photon can be extended with new operations which are identified by a single
        /// byte, defined server side and known as operation code (opCode). Similarly, the operation's parameters
        /// are defined server side as byte keys of values, which a client sends as customOpParameters
        /// accordingly.<para></para>
        /// This is explained in more detail as "<see cref="!:Operations" text="Custom Operations"/>".
        ///<param name="customOpCode">Operations are handled by their byte\-typed code. The codes of the
        ///                           "Lite" application are in the struct <see cref="T:ExitGames.Client.Photon.Lite.LiteOpCode"/>.</param>
        ///<param name="customOpParameters">Containing parameters as key\-value pair. The key is byte\-typed, while the value is any serializable datatype.</param>
        ///<param name="sendReliable">Selects if the operation must be acknowledged or not. If false, the
        ///                           operation is not guaranteed to reach the server.</param>
        ///<param name="channelId">The channel in which this operation should be sent.</param>
        ///<returns>If operation could be enqueued for sending</returns>
        public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId)
        {
            lock (this.EnqueueLock)
            {
                return this.peerBase.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, false);
            }
        }

        ///<summary>
        /// Allows the client to send any operation to the Photon Server by setting any opCode and the operation's parameters.
        /// </summary>
        /// <remarks>
        /// Variant with an OperationRequest object.

        /// This variant offers an alternative way to describe a operation request. Operation code and it's parameters
        /// are wrapped up in a object. Still, the parameters are a Dictionary.
        /// </remarks>
        /// <param name="operationRequest">The operation to call on Photon.</param>
        /// <param name="sendReliable">Use unreliable (false) if the call might get lost (when it's content is soon outdated).</param>
        /// <param name="channelId">Defines the sequence of requests this operation belongs to.</param>
        /// <param name="encrypt">Encrypt request before sending. Depends on IsEncryptionAvailable.</param>
        /// <returns>If operation could be enqueued for sending</returns>
        public virtual bool OpCustom(byte customOpCode, Dictionary<byte, object> customOpParameters, bool sendReliable, byte channelId, bool encrypt)
        {
            if (!(!encrypt || this.IsEncryptionAvailable))
            {
                throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
            }
            lock (this.EnqueueLock)
            {
                return this.peerBase.EnqueueOperation(customOpParameters, customOpCode, sendReliable, channelId, encrypt);
            }
        }

        ///<summary>
        /// Registers new types/classes for de/serialization and the fitting methods to call for this type.
        /// </summary>
        /// <remarks>
        /// SerializeMethod and DeserializeMethod are complementary: Feed the product of serializeMethod to
        /// the constructor, to get a comparable instance of the object.

        /// After registering a Type, it can be used in events and operations and will be serialized like
        /// built-in types.
        /// </remarks>
        /// <param name="customType">Type (class) to register.</param>
        /// <param name="code">A byte-code used as shortcut during transfer of this Type.</param>
        /// <param name="serializeMethod">Method delegate to create a byte[] from a customType instance.</param>
        /// <param name="constructor">Method delegate to create instances of customType's from byte[].</param>
        /// <returns>If the Type was registered successfully.</returns>
        public static bool RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)
        {
            return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
        }

        public virtual bool SendAcksOnly()
        {
            if (this.TrafficStatsEnabled)
            {
                this.TrafficStatsGameLevel.SendOutgoingCommandsCalled();
            }
            lock (this.SendOutgoingLockObject)
            {
                return this.peerBase.SendAcksOnly();
            }
        }

        ///<summary>
        /// This method creates a UDP/TCP package for outgoing commands (operations and acknowledgements)
        /// and sends them to the server.
        /// This method is also called by Service().
        /// </summary>
        /// <remarks>
        /// As the Photon library does not create any UDP/TCP packages by itself. Instead, the application
        /// fully controls how many packages are sent and when. A tradeoff, an application will
        /// lose connection, if it is no longer calling SendOutgoingCommands or Service.

        /// If multiple operations and ACKs are waiting to be sent, they will be aggregated into one
        /// package. The package fills in this order:
        ///   ACKs for received commands
        ///   A "Ping" - only if no reliable data was sent for a while
        ///   Starting with the lowest Channel-Nr:
        ///     Reliable Commands in channel
        ///     Unreliable Commands in channel

        /// This gives a higher priority to lower channels.

        /// A longer interval between sends will lower the overhead per sent operation but
        /// increase the internal delay (which adds "lag").

        /// Call this 2..20 times per second (depending on your target platform).
        /// </remarks>
        /// <returns>The if commands are not yet sent. Udp limits it's package size, Tcp doesnt.</returns>
        public virtual bool SendOutgoingCommands()
        {
            if (this.TrafficStatsEnabled)
            {
                this.TrafficStatsGameLevel.SendOutgoingCommandsCalled();
            }
            lock (this.SendOutgoingLockObject)
            {
                return this.peerBase.SendOutgoingCommands();
            }
        }

        ///<summary>
        /// This method excutes DispatchIncomingCommands and SendOutgoingCommands in your application Thread-context.
        /// </summary>
        /// <remarks>
        /// The Photon client libraries are designed to fit easily into a game or application. The application
        /// is in control of the context (thread) in which incoming events and responses are executed and has
        /// full control of the creation of UDP/TCP packages.

        /// Sending packages and dispatching received messages are two separate tasks. Service combines them
        /// into one method at the cost of control. It calls DispatchIncomingCommands and SendOutgoingCommands.

        /// Call this method regularly (2..20 times a second).

        /// This will Dispatch ANY remaining buffered responses and events AND will send queued outgoing commands.
        /// Fewer calls might be more effective if a device cannot send many packets per second, as multiple
        /// operations might be combined into one package.
        /// </remarks>
        /// <example>
        /// You could replace Service by:

        ///   while (DispatchIncomingCommands()); //Dispatch until everything is Dispatched...
        ///   SendOutgoingCommands(); //Send a UDP/TCP package with outgoing messages
        /// </example>
        /// <seealso cref="M:ExitGames.Client.Photon.PhotonPeer.DispatchIncomingCommands"/>
        /// <seealso cref="M:ExitGames.Client.Photon.PhotonPeer.SendOutgoingCommands"/>
        public virtual void Service()
        {
            while (this.DispatchIncomingCommands())
            {
            }
            while (this.SendOutgoingCommands())
            {
            }
        }

        ///<summary>
        /// This method immediately closes a connection (pure client side) and ends related listening Threads.
        ///</summary>
        ///<remarks>
        /// Unlike Disconnect, this method will simply stop to listen to the server. Udp connections will timeout.
        /// If the connections was open, this will trigger a callback to OnStatusChanged with code StatusCode.Disconnect.
        ///</remarks>
        public virtual void StopThread()
        {
            lock (this.DispatchLockObject)
            {
                lock (this.SendOutgoingLockObject)
                {
                    this.peerBase.StopConnection();
                }
            }
        }

        /// <summary>
        /// Creates new instances of TrafficStats and starts a new timer for those.
        /// </summary>
        public void TrafficStatsReset()
        {
            this.peerBase.InitializeTrafficStats();
            this.peerBase.TrafficStatsEnabled = true;
        }

        ///<summary>
        /// Returns a string of the most interesting connection statistics.
        /// When you have issues on the client side, these might contain hints about the issue's cause.
        ///</summary>
        ///<param name="all">If true, Incoming and Outgoing low-level stats are included in the string.</param>
        ///<returns>Stats as string.</returns>
        public string VitalStatsToString(bool all)
        {
            if (this.TrafficStatsGameLevel == null)
            {
                return "Stats not available. Use PhotonPeer.TrafficStatsEnabled.";
            }
            if (!all)
            {
                return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {4}sec.\n{3}", new object[] { this.RoundTripTime, this.RoundTripTimeVariance, SupportClass.GetTickCount() - this.TimestampOfLastSocketReceive, this.TrafficStatsGameLevel.ToStringVitalStats(), this.TrafficStatsElapsedMs / 0x3e8L });
            }
            return string.Format("Rtt(variance): {0}({1}). Ms since last receive: {2}. Stats elapsed: {6}sec.\n{3}\n{4}\n{5}", new object[] { this.RoundTripTime, this.RoundTripTimeVariance, SupportClass.GetTickCount() - this.TimestampOfLastSocketReceive, this.TrafficStatsGameLevel.ToStringVitalStats(), this.TrafficStatsIncoming.ToString(), this.TrafficStatsOutgoing.ToString(), this.TrafficStatsElapsedMs / 0x3e8L });
        }

        ///<summary>
        /// Gets the size of the dispatched event or operation-result in bytes.
        /// This value is set before OnEvent() or OnOperationResponse() is called (within DispatchIncomingCommands()).
        ///</summary>
        ///<remarks>
        /// Get this value directly in OnEvent() or OnOperationResponse(). Example:
        /// void OnEvent(...) {
        ///  int eventSizeInBytes = this.peer.ByteCountCurrentDispatch;
        ///  //...

        /// void OnOperationResponse(...) {
        ///  int resultSizeInBytes = this.peer.ByteCountCurrentDispatch;
        ///  //...
        ///</remarks>
        public int ByteCountCurrentDispatch
        {
            get
            {
                return this.peerBase.ByteCountCurrentDispatch;
            }
        }

        ///<summary>
        /// Gets the size of the last serialized operation call in bytes.
        /// The value includes all headers for this single operation but excludes those of UDP, Enet Package Headers and TCP.
        ///</summary>
        ///<remarks>
        /// Get this value immediately after calling an operation. Example:
        /// this.litepeer.OpJoin("myroom");
        /// int opjoinByteCount = this.peer.ByteCountLastOperation;
        ///</remarks>
        public int ByteCountLastOperation
        {
            get
            {
                return this.peerBase.ByteCountLastOperation;
            }
        }

        /// <summary>
        /// Gets count of all bytes coming in (including headers, excluding UDP/TCP overhead)
        /// </summary>
        public long BytesIn
        {
            get
            {
                return this.peerBase.BytesIn;
            }
        }

        /// <summary>
        /// Gets count of all bytes going out (including headers, excluding UDP/TCP overhead)
        /// </summary>
        public long BytesOut
        {
            get
            {
                return this.peerBase.BytesOut;
            }
        }

        ///<summary>
        /// Gets / sets the number of channels available in UDP connections with Photon.
        /// Photon Channels are only supported for UDP.
        /// The default ChannelCount is 2. Channel IDs start with 0 and 255 is a internal channel.
        ///</summary>
        public byte ChannelCount
        {
            get
            {
                return this.peerBase.ChannelCount;
            }
            set
            {
                if ((value == 0) || (this.PeerState != PeerStateValue.Disconnected))
                {
                    throw new Exception("ChannelCount can only be set while disconnected and must be > 0.");
                }
                this.peerBase.ChannelCount = value;
            }
        }

        ///<summary>
        /// Initial size internal lists for incoming/outgoing commands (reliable and unreliable).
        /// </summary>
        /// <remarks>
        /// This sets only the initial size. All lists simply grow in size as needed. This means that
        /// incoming or outgoing commands can pile up and consume heap size if Service is not called
        /// often enough to handle the messages in either direction.

        /// Configure the WarningSize, to get callbacks when the lists reach a certain size.

        /// UDP: Incoming and outgoing commands each have separate buffers for reliable and unreliable sending.
        /// There are additional buffers for &quot;sent commands&quot; and &quot;ACKs&quot;.
        /// TCP: Only two buffers exist: incoming and outgoing commands.
        /// </remarks>
        public int CommandBufferSize
        {
            get
            {
                return this.peerBase.commandBufferSize;
            }
        }

        ///<summary>
        /// While not connected, this controls if the next connection(s) should use a per-package CRC checksum.
        /// </summary>
        /// <remarks>
        /// While turned on, the client and server will add a CRC checksum to every sent package.
        /// The checksum enables both sides to detect and ignore packages that were corrupted during transfer.
        /// Corrupted packages have the same impact as lost packages: They require a re-send, adding a delay
        /// and could lead to timeouts.

        /// Building the checksum has a low processing overhead but increases integrity of sent and received data.
        /// Packages discarded due to failed CRC cecks are counted in PhotonPeer.PacketLossByCrc.
        /// </remarks>
        public bool CrcEnabled
        {
            get
            {
                return this.peerBase.crcEnabled;
            }
            set
            {
                if (this.peerBase.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
                {
                    throw new Exception("CrcEnabled can only be set while disconnected.");
                }
                this.peerBase.crcEnabled = value;
            }
        }

        ///<summary>
        /// Sets the level (and amount) of debug output provided by the library.
        ///</summary>
        ///<remarks>
        /// This affects the callbacks to IPhotonPeerListener.DebugReturn.
        /// Default Level: Error.
        ///</remarks>
        public DebugLevel DebugOut
        {
            get
            {
                return this.peerBase.debugOut;
            }
            set
            {
                this.peerBase.debugOut = value;
            }
        }

        ///<summary>
        /// Milliseconds after which a reliable UDP command triggers a timeout disconnect, unless acknowledged by server.
        /// This value currently only affects UDP connections.
        /// DisconnectTimeout is not an exact value for a timeout. The exact timing of the timeout depends on the frequency
        /// of Service() calls and commands that are sent with long roundtrip-times and variance are checked less often for
        /// re-sending!

        /// DisconnectTimeout and SentCountAllowance are competing settings: either might trigger a disconnect on the
        /// client first, depending on the values and Rountrip Time.
        /// Default: 10000 ms.
        /// </summary>
        public int DisconnectTimeout
        {
            get
            {
                return this.peerBase.DisconnectTimeout;
            }
            set
            {
                this.peerBase.DisconnectTimeout = value;
            }
        }

        public string HttpUrlParameters
        {
            get
            {
                if ((this.UsedProtocol == ConnectionProtocol.Http) || (this.UsedProtocol == ConnectionProtocol.RHttp))
                {
                    return this.peerBase.HttpUrlParameters;
                }
                return string.Empty;
            }
            set
            {
                if ((this.UsedProtocol == ConnectionProtocol.Http) || (this.UsedProtocol == ConnectionProtocol.RHttp))
                {
                    this.peerBase.HttpUrlParameters = value;
                }
                else if (this.DebugOut >= DebugLevel.ERROR)
                {
                    this.Listener.DebugReturn(DebugLevel.ERROR, "Failed to set HttpUrlParameters. Can be set only when using HTTP.");
                }
            }
        }

        /// <summary>
        /// This property is set internally, when OpExchangeKeysForEncryption successfully finished.
        /// While it's true, encryption can be used for operations.
        /// </summary>
        public bool IsEncryptionAvailable
        {
            get
            {
                return this.peerBase.isEncryptionAvailable;
            }
        }

        /// <summary>
        /// TODO: Comment this!
        /// </summary>
        public bool IsSendingOnlyAcks
        {
            get
            {
                return this.peerBase.IsSendingOnlyAcks;
            }
            set
            {
                lock (this.SendOutgoingLockObject)
                {
                    this.peerBase.IsSendingOnlyAcks = value;
                }
            }
        }

        ///<summary>
        /// Gets or sets the network simulation "enabled" setting.
        /// Changing this value also locks this peer's sending and when setting false,
        /// the internally used queues are executed (so setting to false can take some cycles).
        ///</summary>
        public virtual bool IsSimulationEnabled
        {
            get
            {
                return this.NetworkSimulationSettings.IsSimulationEnabled;
            }
            set
            {
                if (value != this.NetworkSimulationSettings.IsSimulationEnabled)
                {
                    lock (this.SendOutgoingLockObject)
                    {
                        this.NetworkSimulationSettings.IsSimulationEnabled = value;
                    }
                }
            }
        }

        ///<summary>
        /// Limits the queue of received unreliable commands within DispatchIncomingCommands before dispatching them.
        /// This works only in UDP.
        /// This limit is applied when you call DispatchIncomingCommands. If this client (already) received more than
        /// LimitOfUnreliableCommands, it will throw away the older ones instead of dispatching them. This can produce
        /// bigger gaps for unreliable commands but your client catches up faster.
        ///</summary>
        ///<remarks>
        /// This can be useful when the client couldn't dispatch anything for some time (cause it was in a room but
        /// loading a level).
        /// If set to 20, the incoming unreliable queues are truncated to 20.
        /// If 0, all received unreliable commands will be dispatched.
        /// This is a "per channel" value, so each channel can hold up to LimitOfUnreliableCommands commands.
        /// This value interacts with DispatchIncomingCommands: If that is called less often, more commands get skipped.
        ///</remarks>
        public int LimitOfUnreliableCommands
        {
            get
            {
                return this.peerBase.limitOfUnreliableCommands;
            }
            set
            {
                this.peerBase.limitOfUnreliableCommands = value;
            }
        }

        ///<summary>
        /// Gets the IPhotonPeerListener of this instance (set in constructor).
        /// Can be used in derived classes for Listener.DebugReturn().
        /// </summary>
        public IPhotonPeerListener Listener
        {
            get
            {
                return this.peerBase.Listener;
            }
            protected set
            {
                this.peerBase.Listener = value;
            }
        }

        ///<summary>
        /// This setter for the (local-) timestamp delegate replaces the default Environment.TickCount with any equal function.
        ///</summary>
        ///<remarks>
        /// About Environment.TickCount:
        /// The value of this property is derived from the system timer and is stored as a 32-bit signed integer.
        /// Consequently, if the system runs continuously, TickCount will increment from zero to Int32..::.MaxValue
        /// for approximately 24.9 days, then jump to Int32..::.MinValue, which is a negative number, then increment
        /// back to zero during the next 24.9 days.
        ///</remarks>
        ///<exception cref="T:System.Exception">Exception is thrown peer.PeerState is not PS_DISCONNECTED.</exception>
        public SupportClass.IntegerMillisecondsDelegate LocalMsTimestampDelegate
        {
            set
            {
                if (this.PeerState != PeerStateValue.Disconnected)
                {
                    throw new Exception("LocalMsTimestampDelegate only settable while disconnected. State: " + this.PeerState);
                }
                SupportClass.IntegerMilliseconds = value;
            }
        }

        ///<summary>
        /// Gets a local timestamp in milliseconds by calling SupportClass.GetTickCount().
        /// See LocalMsTimestampDelegate.
        ///</summary>
        [Obsolete("Should be replaced by: SupportClass.GetTickCount(). Internally this is used, too.")]
        public int LocalTimeInMilliSeconds
        {
            get
            {
                return SupportClass.GetTickCount();
            }
        }

        ///<summary>
        /// The Maximum Trasfer Unit (MTU) defines the (network-level) packet-content size that is
        /// guaranteed to arrive at the server in one piece. The Photon Protocol uses this
        /// size to split larger data into packets and for receive-buffers of packets.
        ///</summary>
        ///<remarks>
        /// This value affects the Packet-content. The resulting UDP packages will have additional
        /// headers that also count against the package size (so it's bigger than this limit in the end)
        /// Setting this value while being connected is not allowed and will throw an Exception.
        /// Minimum is 520. Huge values won't speed up connections in most cases!
        ///</remarks>
        public int MaximumTransferUnit
        {
            get
            {
                return this.peerBase.mtu;
            }
            set
            {
                if (this.PeerState != PeerStateValue.Disconnected)
                {
                    throw new Exception("MaximumTransferUnit is only settable while disconnected. State: " + this.PeerState);
                }
                if (value < 520)
                {
                    value = 520;
                }
                this.peerBase.mtu = value;
            }
        }

        ///<summary>
        /// Gets the settings for built-in Network Simulation for this peer instance
        /// while IsSimulationEnabled will enable or disable them.
        /// Once obtained, the settings can be modified by changing the properties.
        ///</summary>
        public NetworkSimulationSet NetworkSimulationSettings
        {
            get
            {
                return this.peerBase.NetworkSimulationSettings;
            }
        }

        ///<summary>
        /// Defines the initial size of an internally used MemoryStream for Tcp.
        /// The MemoryStream is used to aggregate operation into (less) send calls,
        /// which uses less resoures.
        ///</summary>
        ///<remarks>
        /// The size is not restricing the buffer and does not affect when poutgoing data is actually sent.
        ///</remarks>
        public static int OutgoingStreamBufferSize
        {
            get
            {
                return PeerBase.outgoingStreamBufferSize;
            }
            set
            {
                PeerBase.outgoingStreamBufferSize = value;
            }
        }

        /// <summary>
        /// Count of packages dropped due to failed CRC checks for this connection.
        /// </summary>
        /// <see cref="P:ExitGames.Client.Photon.PhotonPeer.CrcEnabled"/>
        public int PacketLossByCrc
        {
            get
            {
                return this.peerBase.packetLossByCrc;
            }
        }

        /// <summary>
        /// This peer's ID as assigned by the server or 0 if not using UDP. Will be 0xFFFF before the client connects.
        /// </summary>
        public string PeerID
        {
            get
            {
                return this.peerBase.PeerID;
            }
        }

        ///<summary>
        /// This is the (low level) state of the connection to the server of a PhotonPeer.
        /// It is managed internally and read-only.
        ///</summary>
        ///<remarks>
        /// Don't mix this up with the StatusCode provided in IPhotonListener.OnStatusChanged().
        /// Applications should use the StatusCode of OnStatusChanged() to track their state, as
        /// it also covers the higher level initialization between a client and Photon.
        ///</remarks>
        public PeerStateValue PeerState
        {
            get
            {
                if (!((this.peerBase.peerConnectionState != PeerBase.ConnectionStateValue.Connected) || this.peerBase.ApplicationIsInitialized))
                {
                    return PeerStateValue.InitializingApplication;
                }
                return (PeerStateValue)this.peerBase.peerConnectionState;
            }
        }

        /// <summary>
        ///  Count of all currently received but not-yet-Dispatched reliable commands
        ///  (events and operation results) from all channels.
        /// </summary>
        public int QueuedIncomingCommands
        {
            get
            {
                return this.peerBase.QueuedIncomingCommandsCount;
            }
        }

        /// <summary>
        /// Count of all commands currently queued as outgoing, including all channels and reliable, unreliable.
        /// </summary>
        public int QueuedOutgoingCommands
        {
            get
            {
                return this.peerBase.QueuedOutgoingCommandsCount;
            }
        }

        public int ResentReliableCommands
        {
            get
            {
                return ((this.UsedProtocol != ConnectionProtocol.Udp) ? 0 : ((EnetPeer)this.peerBase).reliableCommandsRepeated);
            }
        }

        ///<summary>
        /// Time until a reliable command is acknowledged by the server.

        /// The value measures network latency and for UDP it includes the server's ACK-delay (setting in config).
        /// In TCP, there is no ACK-delay, so the value is slightly lower (if you use default settings for Photon).

        /// RoundTripTime is updated constantly. Every reliable command will contribute a fraction to this value.

        /// This is also the approximate time until a raised event reaches another client or until an operation
        /// result is available.
        ///</summary>
        public int RoundTripTime
        {
            get
            {
                return this.peerBase.roundTripTime;
            }
        }

        ///<summary>
        /// Changes of the roundtriptime as variance value. Gives a hint about how much the time is changing.
        ///</summary>
        public int RoundTripTimeVariance
        {
            get
            {
                return this.peerBase.roundTripTimeVariance;
            }
        }

        ///<summary>
        /// Number of send retries before a peer is considered lost/disconnected. Default: 5.
        /// The initial timeout countdown of a command is calculated by the current roundTripTime + 4 * roundTripTimeVariance.
        /// Please note that the timeout span until a command will be resent is not constant, but based on
        /// the roundtrip time at the initial sending, which will be doubled with every failed retry.

        /// DisconnectTimeout and SentCountAllowance are competing settings: either might trigger a disconnect on the
        /// client first, depending on the values and Rountrip Time.
        /// </summary>
        public int SentCountAllowance
        {
            get
            {
                return this.peerBase.sentCountAllowance;
            }
            set
            {
                this.peerBase.sentCountAllowance = value;
            }
        }

        ///<summary>
        /// The server address which was used in PhotonPeer.Connect() or null (before Connect() was called).
        ///</summary>
        ///<remarks>
        /// The ServerAddress can only be changed for HTTP connections (to replace one that goes through a Loadbalancer with a direct URL).
        ///</remarks>
        public string ServerAddress
        {
            get
            {
                return this.peerBase.ServerAddress;
            }
            set
            {
                if ((this.UsedProtocol == ConnectionProtocol.Http) || (this.UsedProtocol == ConnectionProtocol.RHttp))
                {
                    this.peerBase.ServerAddress = value;
                }
                else if (this.DebugOut >= DebugLevel.ERROR)
                {
                    this.Listener.DebugReturn(DebugLevel.ERROR, "Failed to set ServerAddress. Can be set only when using HTTP.");
                }
            }
        }

        ///<summary>
        /// Approximated Environment.TickCount value of server (while connected).
        /// </summary>
        /// <remarks>
        /// UDP: The server's timestamp is automatically fetched after connecting (once). This is done
        /// internally by a command which is acknowledged immediately by the server.
        /// TCP: The server's timestamp fetched with each ping but set only after connecting (once).

        /// The approximation will be off by +/- 10ms in most cases. Per peer/client and connection, the
        /// offset will be constant (unless FetchServerTimestamp() is used). A constant offset should be
        /// better to adjust for. Unfortunately there is no way to find out how much the local value
        /// differs from the original.

        /// The approximation adds RoundtripTime / 2 and uses this.LocalTimeInMilliSeconds to calculate
        /// in-between values (this property returns a new value per tick).

        /// The value sent by Photon equals Environment.TickCount in the logic layer (e.g. Lite).
        /// </remarks>
        /// <value>
        /// 0 until connected.
        /// While connected, the value is an approximation of the server's current timestamp.
        /// </value>
        public int ServerTimeInMilliSeconds
        {
            get
            {
                return (this.peerBase.serverTimeOffsetIsAvailable ? (this.peerBase.serverTimeOffset + SupportClass.GetTickCount()) : 0);
            }
        }

        ///<summary>
        /// Sets the milliseconds without reliable command before a ping command (reliable) will be sent (Default: 1000ms).
        /// The ping command is used to keep track of the connection in case the client does not send reliable commands
        /// by itself.
        /// A ping (or reliable commands) will update the RoundTripTime calculation.
        ///</summary>
        public int TimePingInterval
        {
            get
            {
                return this.peerBase.timePingInterval;
            }
            set
            {
                this.peerBase.timePingInterval = value;
            }
        }

        ///<summary>
        /// Stores timestamp of the last time anything (!) was received from the server (including
        /// low level Ping and ACKs but also events and operation-returns). This is not the time when
        /// something was dispatched.
        /// If you enable NetworkSimulation, this value is affected as well.
        ///</summary>
        public int TimestampOfLastSocketReceive
        {
            get
            {
                return this.peerBase.timestampOfLastReceive;
            }
        }

        /// <summary>
        /// Returns the count of milliseconds the stats are enabled for tracking.
        /// </summary>
        public long TrafficStatsElapsedMs
        {
            get
            {
                return this.peerBase.TrafficStatsEnabledTime;
            }
        }

        ///<summary>
        /// Gets the size of the last serialized operation call in bytes.
        /// The value includes all headers for this single operation but excludes those of UDP, Enet Package Headers and TCP.
        ///</summary>
        ///<remarks>
        /// Get this value immediately after calling an operation. Example:
        /// this.litepeer.OpJoin("myroom");
        /// int opjoinByteCount = this.peer.ByteCountLastOperation;
        ///</remarks>
        public bool TrafficStatsEnabled
        {
            get
            {
                return this.peerBase.TrafficStatsEnabled;
            }
            set
            {
                this.peerBase.TrafficStatsEnabled = value;
            }
        }

        ///<summary>
        /// Gets a statistic of incoming and outgoing traffic, split by operation, operation-result and event.
        /// Operations are outgoing traffic, results and events are incoming.
        /// Includes the per-command header sizes (Udp: Enet Command Header or Tcp: Message Header).
        ///</summary>
        public TrafficStatsGameLevel TrafficStatsGameLevel
        {
            get
            {
                return this.peerBase.TrafficStatsGameLevel;
            }
        }

        ///<summary>
        /// Gets the byte-count of incoming "low level" messages, which are either Enet Commands or Tcp Messages.
        /// These include all headers, except those of the underlying internet protocol Udp or Tcp.
        ///</summary>
        public TrafficStats TrafficStatsIncoming
        {
            get
            {
                return this.peerBase.TrafficStatsIncoming;
            }
        }

        ///<summary>
        /// Gets the byte-count of outgoing "low level" messages, which are either Enet Commands or Tcp Messages.
        /// These include all headers, except those of the underlying internet protocol Udp or Tcp.
        ///</summary>
        public TrafficStats TrafficStatsOutgoing
        {
            get
            {
                return this.peerBase.TrafficStatsOutgoing;
            }
        }

        /// <summary>
        /// The protocol this Peer uses to connect to Photon.
        /// </summary>
        public ConnectionProtocol UsedProtocol
        {
            get
            {
                return this.peerBase.usedProtocol;
            }
        }

        ///<summary>
        /// The WarningSize is used test all message queues for congestion (in and out, reliable and unreliable).
        /// OnStatusChanged will be called with a warning if a queue holds WarningSize commands or a multiple
        /// of it.
        /// Default: 100.
        /// Example: If command is received, OnStatusChanged will be called when the respective command queue
        /// has 100, 200, 300 ... items.
        ///</summary>
        public int WarningSize
        {
            get
            {
                return this.peerBase.warningSize;
            }
            set
            {
                this.peerBase.warningSize = value;
            }
        }
    }
}
