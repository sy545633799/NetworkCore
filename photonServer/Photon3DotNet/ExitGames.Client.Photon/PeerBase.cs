using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Photon.SocketServer.Security;

namespace ExitGames.Client.Photon
{
    internal abstract class PeerBase
    {
        internal readonly Queue<MyAction> ActionQueue = new Queue<MyAction>();
        internal bool ApplicationIsInitialized;
        public int ByteCountCurrentDispatch;
        public int ByteCountLastOperation;
        internal long bytesIn;
        internal long bytesOut;
        internal byte ChannelCount = 2;
        internal int commandBufferSize = 100;
        internal bool crcEnabled = false;
        public DiffieHellmanCryptoProvider CryptoProvider;
        internal DebugLevel debugOut = DebugLevel.ERROR;
        internal int DisconnectTimeout = 0x2710;
        internal const int ENET_PEER_DEFAULT_ROUND_TRIP_TIME = 300;
        internal const int ENET_PEER_PACKET_LOSS_SCALE = 0x10000;
        internal const int ENET_PEER_PACKET_THROTTLE_INTERVAL = 0x1388;
        internal int highestRoundTripTimeVariance;
        internal byte[] INIT_BYTES = new byte[0x29];
        internal bool isEncryptionAvailable;
        private readonly Random lagRandomizer = new Random();
        internal int lastRoundTripTime;
        internal int lastRoundTripTimeVariance;
        internal int limitOfUnreliableCommands = 0;
        internal int lowestRoundTripTime;

        /// <summary>
        ///  Maximum Transfer Unit to be used for UDP+TCP
        /// </summary>
        internal int mtu = 0x4b0;
        internal readonly LinkedList<SimulationItem> NetSimListIncoming = new LinkedList<SimulationItem>();
        internal readonly LinkedList<SimulationItem> NetSimListOutgoing = new LinkedList<SimulationItem>();
        private readonly NetworkSimulationSet networkSimulationSettings = new NetworkSimulationSet();
        internal int outgoingCommandsInStream = 0;
        internal static int outgoingStreamBufferSize = 0x4b0;
        internal int packetLossByCrc;
        internal int packetThrottleInterval;

        //<summary>
        // This is the (low level) connection state of the peer. It's internal and based on eNet's states.
        //</summary>
        //<remarks>Applications can read the "high level" state as PhotonPeer.PeerState, which uses a different enum.</remarks>
        internal ConnectionStateValue peerConnectionState;
        internal static short peerCount;

        /// <summary>
        /// This ID is assigned by the Realtime Server upon connection.
        /// The application does not have to care about this, but it is useful in debugging.
        /// </summary>
        internal short peerID = -1;
        internal int roundTripTime;
        internal int roundTripTimeVariance;
        internal int sentCountAllowance = 5;
        protected MemoryStream SerializeMemStream = new MemoryStream();

        /// <summary>
        ///  The serverTimeOffset is serverTimestamp - localTime. Used to approximate the serverTimestamp with help of localTime
        /// </summary>
        internal int serverTimeOffset;
        internal bool serverTimeOffsetIsAvailable;
        internal int timeBase;
        internal int timeInt;
        internal int timeLastAckReceive;
        internal int timeoutInt;
        internal int timePingInterval = 0x3e8;
        internal int timestampOfLastReceive;
        internal int TrafficPackageHeaderSize;
        private bool trafficStatsEnabled = false;
        public TrafficStatsGameLevel TrafficStatsGameLevel;
        public TrafficStats TrafficStatsIncoming;
        public TrafficStats TrafficStatsOutgoing;
        private Stopwatch trafficStatsStopwatch;
        internal ConnectionProtocol usedProtocol;
        internal int warningSize = 100;

        protected PeerBase()
        {
        }

        /// <summary>
        /// nodeId can be ignored by implementations. TCP uses this to control the tcp-proxy
        /// </summary>
        /// <param name="serverAddress"></param>
        /// <param name="appID"></param>
        /// <returns></returns>
        internal abstract bool Connect(string serverAddress, string appID);

        internal void DeriveSharedKey(OperationResponse operationResponse)
        {
            if (operationResponse.ReturnCode != 0)
            {
                this.EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. " + operationResponse.ToStringFull());
                this.EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
            }
            else
            {
                byte[] serverPublicKey = (byte[])operationResponse[PhotonCodes.ServerKey];
                if ((serverPublicKey == null) || (serverPublicKey.Length == 0))
                {
                    this.EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. Server's public key is null or empty. " + operationResponse.ToStringFull());
                    this.EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
                }
                else
                {
                    this.CryptoProvider.DeriveSharedKey(serverPublicKey);
                    this.isEncryptionAvailable = true;
                    this.EnqueueStatusCallback(StatusCode.EncryptionEstablished);
                }
            }
        }

        internal virtual bool DeserializeMessageAndCallback(byte[] inBuff)
        {
            OperationResponse opRes;
            if (inBuff.Length < 2)
            {
                if (this.debugOut >= DebugLevel.ERROR)
                {
                    this.Listener.DebugReturn(DebugLevel.ERROR, "Incoming UDP data too short! " + inBuff.Length);
                }
                return false;
            }
            if ((inBuff[0] != 0xf3) && (inBuff[0] != 0xfd))
            {
                if (this.debugOut >= DebugLevel.ERROR)
                {
                    this.Listener.DebugReturn(DebugLevel.ALL, "No regular operation UDP message: " + inBuff[0]);
                }
                return false;
            }
            byte msgType = (byte)(inBuff[1] & 0x7f);
            bool isEncrypted = (inBuff[1] & 0x80) > 0;
            MemoryStream stream = null;
            if (msgType != 1)
            {
                try
                {
                    if (isEncrypted)
                    {
                        inBuff = this.CryptoProvider.Decrypt(inBuff, 2, inBuff.Length - 2);
                        stream = new MemoryStream(inBuff);
                    }
                    else
                    {
                        stream = new MemoryStream(inBuff);
                        stream.Seek(2L, SeekOrigin.Begin);
                    }
                }
                catch (Exception ex)
                {
                    if (this.debugOut >= DebugLevel.ERROR)
                    {
                        this.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
                    }
                    SupportClass.WriteStackTrace(ex, null);
                    return false;
                }
            }
            int timeBeforeCallback = 0;
            switch (msgType)
            {
                case 1:
                    this.InitCallback();
                    break;
                case 3:
                    opRes = Protocol.DeserializeOperationResponse(stream);
                    if (this.TrafficStatsEnabled)
                    {
                        this.TrafficStatsGameLevel.CountResult(this.ByteCountCurrentDispatch);
                        timeBeforeCallback = SupportClass.GetTickCount();
                    }
                    this.Listener.OnOperationResponse(opRes);
                    if (this.TrafficStatsEnabled)
                    {
                        this.TrafficStatsGameLevel.TimeForResponseCallback(opRes.OperationCode, SupportClass.GetTickCount() - timeBeforeCallback);
                    }
                    break;
                case 4:
                    {
                        EventData ev = Protocol.DeserializeEventData(stream);
                        if (this.TrafficStatsEnabled)
                        {
                            this.TrafficStatsGameLevel.CountEvent(this.ByteCountCurrentDispatch);
                            timeBeforeCallback = SupportClass.GetTickCount();
                        }
                        this.Listener.OnEvent(ev);
                        if (this.TrafficStatsEnabled)
                        {
                            this.TrafficStatsGameLevel.TimeForEventCallback(ev.Code, SupportClass.GetTickCount() - timeBeforeCallback);
                        }
                        break;
                    }
                case 7:
                    opRes = Protocol.DeserializeOperationResponse(stream);
                    if (this.TrafficStatsEnabled)
                    {
                        this.TrafficStatsGameLevel.CountResult(this.ByteCountCurrentDispatch);
                        timeBeforeCallback = SupportClass.GetTickCount();
                    }
                    if (opRes.OperationCode == PhotonCodes.InitEncryption)
                    {
                        this.DeriveSharedKey(opRes);
                    }
                    else
                    {
                        this.EnqueueDebugReturn(DebugLevel.ERROR, "Received unknown internal operation. " + opRes.ToStringFull());
                    }
                    if (this.TrafficStatsEnabled)
                    {
                        this.TrafficStatsGameLevel.TimeForResponseCallback(opRes.OperationCode, SupportClass.GetTickCount() - timeBeforeCallback);
                    }
                    break;

                default:
                    this.EnqueueDebugReturn(DebugLevel.ERROR, "unexpected msgType " + msgType);
                    break;
            }
            return true;
        }

        internal abstract void Disconnect();

        //<summary>
        // Checks the incoming queue and Dispatches received data if possible.
        //</summary>
        //<returns>If a Dispatch happened or not, which shows if more Dispatches might be needed.</returns>
        internal abstract bool DispatchIncomingCommands();

        internal void EnqueueActionForDispatch(MyAction action)
        {
            lock (this.ActionQueue)
            {
                this.ActionQueue.Enqueue(action);
            }
        }

        internal void EnqueueDebugReturn(DebugLevel level, string debugReturn)
        {
            lock (this.ActionQueue)
            {
                this.ActionQueue.Enqueue(delegate
                {
                    this.Listener.DebugReturn(level, debugReturn);
                });
            }
        }

        internal bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted)
        {
            return this.EnqueueOperation(parameters, opCode, sendReliable, channelId, encrypted, EgMessageType.Operation);
        }

        internal abstract bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted, EgMessageType messageType);

        internal void EnqueueStatusCallback(StatusCode statusValue)
        {
            lock (this.ActionQueue)
            {
                this.ActionQueue.Enqueue(delegate
                {
                    this.Listener.OnStatusChanged(statusValue);
                });
            }
        }

        /// <summary>
        /// Internally uses an operation to exchange encryption keys with the server.
        /// </summary>
        /// <returns></returns>
        internal bool ExchangeKeysForEncryption()
        {
            this.isEncryptionAvailable = false;
            this.CryptoProvider = new DiffieHellmanCryptoProvider();
            Dictionary<byte, object> parameters = new Dictionary<byte, object>(1);
            parameters[PhotonCodes.ClientKey] = this.CryptoProvider.PublicKey;
            return this.EnqueueOperation(parameters, PhotonCodes.InitEncryption, true, 0, false, EgMessageType.InternalOperationRequest);
        }

        internal abstract void FetchServerTimestamp();

        internal static EndPoint GetEndpoint(string addressAndPort)
        {
            if (!string.IsNullOrEmpty(addressAndPort))
            {
                short serverPort;
                string[] addressParts = addressAndPort.Split(new char[] { ':' });
                if (addressParts.Length != 2)
                {
                    return null;
                }
                string serverIp = addressParts[0];
                if (!short.TryParse(addressParts[1], out serverPort))
                {
                    return null;
                }
                IPAddress[] addresses = Dns.GetHostAddresses(serverIp);
                foreach (IPAddress ipA in addresses)
                {
                    if (ipA.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return new IPEndPoint(ipA, serverPort);
                    }
                }
            }
            return null;
        }

        internal void InitCallback()
        {
            if (this.peerConnectionState == ConnectionStateValue.Connecting)
            {
                this.peerConnectionState = ConnectionStateValue.Connected;
            }
            this.ApplicationIsInitialized = true;
            this.FetchServerTimestamp();
            this.Listener.OnStatusChanged(StatusCode.Connect);
        }

        internal void InitializeTrafficStats()
        {
            this.TrafficStatsIncoming = new TrafficStats(this.TrafficPackageHeaderSize);
            this.TrafficStatsOutgoing = new TrafficStats(this.TrafficPackageHeaderSize);
            this.TrafficStatsGameLevel = new TrafficStatsGameLevel();
            this.trafficStatsStopwatch = new Stopwatch();
        }

        internal void InitOnce()
        {
            this.networkSimulationSettings.peerBase = this;
            this.INIT_BYTES[0] = 0xf3;
            this.INIT_BYTES[1] = 0;
            this.INIT_BYTES[2] = 1;
            this.INIT_BYTES[3] = 6;
            this.INIT_BYTES[4] = 1;
            this.INIT_BYTES[5] = 3;
            this.INIT_BYTES[6] = 0;
            this.INIT_BYTES[7] = 1;
            this.INIT_BYTES[8] = 7;
        }

        internal virtual void InitPeerBase()
        {
            this.TrafficStatsIncoming = new TrafficStats(this.TrafficPackageHeaderSize);
            this.TrafficStatsOutgoing = new TrafficStats(this.TrafficPackageHeaderSize);
            this.TrafficStatsGameLevel = new TrafficStatsGameLevel();
            this.ByteCountLastOperation = 0;
            this.ByteCountCurrentDispatch = 0;
            this.bytesIn = 0L;
            this.bytesOut = 0L;
            this.packetLossByCrc = 0;
            this.networkSimulationSettings.LostPackagesIn = 0;
            this.networkSimulationSettings.LostPackagesOut = 0;
            lock (this.NetSimListOutgoing)
            {
                this.NetSimListOutgoing.Clear();
            }
            lock (this.NetSimListIncoming)
            {
                this.NetSimListIncoming.Clear();
            }
            this.peerConnectionState = ConnectionStateValue.Disconnected;
            this.timeBase = SupportClass.GetTickCount();
            this.isEncryptionAvailable = false;
            this.ApplicationIsInitialized = false;
            this.roundTripTime = 300;
            this.roundTripTimeVariance = 0;
            this.packetThrottleInterval = 0x1388;
            this.serverTimeOffsetIsAvailable = false;
            this.serverTimeOffset = 0;
        }

        /// <summary>
        /// Core of the Network Simulation, which is available in Debug builds.
        /// Called by a timer in intervals.
        /// </summary>
        protected internal void NetworkSimRun()
        {
            SimulationItem item;

            bool enabled = false;

            do
            {
                lock (this.networkSimulationSettings.NetSimManualResetEvent)
                {
                    enabled = this.networkSimulationSettings.IsSimulationEnabled;
                }
                if (!enabled)
                {
                    this.networkSimulationSettings.NetSimManualResetEvent.WaitOne();
                }
            }
            while (!enabled);

            lock (this.NetSimListIncoming)
            {
                item = null;
                while (this.NetSimListIncoming.First != null)
                {
                    item = this.NetSimListIncoming.First.Value;
                    if (item.stopw.ElapsedMilliseconds < item.Delay)
                    {
                        break;
                    }
                    item.ActionToExecute();
                    this.NetSimListIncoming.RemoveFirst();
                }
            }
            Monitor.Enter(this.NetSimListOutgoing);
            try
            {
                item = null;
                while (this.NetSimListOutgoing.First != null)
                {
                    item = this.NetSimListOutgoing.First.Value;
                    if (item.stopw.ElapsedMilliseconds < item.Delay)
                    {
                        break;
                    }
                    item.ActionToExecute();
                    this.NetSimListOutgoing.RemoveFirst();
                }
            }
            finally
            {
                Monitor.Exit(this.NetSimListOutgoing);
            }
            Thread.Sleep(0);
        }

        internal abstract void ReceiveIncomingCommands(byte[] inBuff, int dataLength);

        internal void ReceiveNetworkSimulated(MyAction receiveAction)
        {
            if (!this.networkSimulationSettings.IsSimulationEnabled)
            {
                receiveAction();
            }
            else if (((this.usedProtocol == ConnectionProtocol.Udp) && (this.networkSimulationSettings.IncomingLossPercentage > 0)) && (this.lagRandomizer.Next(0x65) < this.networkSimulationSettings.IncomingLossPercentage))
            {
                this.networkSimulationSettings.LostPackagesIn++;
            }
            else
            {
                int jitter = (this.networkSimulationSettings.IncomingJitter <= 0) ? 0 : (this.lagRandomizer.Next(this.networkSimulationSettings.IncomingJitter * 2) - this.networkSimulationSettings.IncomingJitter);
                int delay = this.networkSimulationSettings.IncomingLag + jitter;
                int timeToExecute = SupportClass.GetTickCount() + delay;
                SimulationItem simItem = new SimulationItem()
                {
                    ActionToExecute = receiveAction,
                    TimeToExecute = timeToExecute,
                    Delay = delay
                };
                lock (this.NetSimListIncoming)
                {
                    if ((this.NetSimListIncoming.Count == 0) || (this.usedProtocol == ConnectionProtocol.Tcp))
                    {
                        this.NetSimListIncoming.AddLast(simItem);
                    }
                    else
                    {
                        LinkedListNode<SimulationItem> node = this.NetSimListIncoming.First;
                        while ((node != null) && (node.Value.TimeToExecute < timeToExecute))
                        {
                            node = node.Next;
                        }
                        if (node == null)
                        {
                            this.NetSimListIncoming.AddLast(simItem);
                        }
                        else
                        {
                            this.NetSimListIncoming.AddBefore(node, simItem);
                        }
                    }
                }
            }
        }

        internal virtual bool SendAcksOnly()
        {
            return false;
        }

        internal void SendNetworkSimulated(MyAction sendAction)
        {
            if (!this.NetworkSimulationSettings.IsSimulationEnabled)
            {
                sendAction();
            }
            else if (((this.usedProtocol == ConnectionProtocol.Udp) && (this.NetworkSimulationSettings.OutgoingLossPercentage > 0)) && (this.lagRandomizer.Next(0x65) < this.NetworkSimulationSettings.OutgoingLossPercentage))
            {
                this.networkSimulationSettings.LostPackagesOut++;
            }
            else
            {
                int jitter = (this.networkSimulationSettings.OutgoingJitter <= 0) ? 0 : (this.lagRandomizer.Next(this.networkSimulationSettings.OutgoingJitter * 2) - this.networkSimulationSettings.OutgoingJitter);
                int delay = this.networkSimulationSettings.OutgoingLag + jitter;
                int timeToExecute = SupportClass.GetTickCount() + delay;
                SimulationItem simItem = new SimulationItem()
                {
                    ActionToExecute = sendAction,
                    TimeToExecute = timeToExecute,
                    Delay = delay
                };
                lock (this.NetSimListOutgoing)
                {
                    if ((this.NetSimListOutgoing.Count == 0) || (this.usedProtocol == ConnectionProtocol.Tcp))
                    {
                        this.NetSimListOutgoing.AddLast(simItem);
                    }
                    else
                    {
                        LinkedListNode<SimulationItem> node = this.NetSimListOutgoing.First;
                        while ((node != null) && (node.Value.TimeToExecute < timeToExecute))
                        {
                            node = node.Next;
                        }
                        if (node == null)
                        {
                            this.NetSimListOutgoing.AddLast(simItem);
                        }
                        else
                        {
                            this.NetSimListOutgoing.AddBefore(node, simItem);
                        }
                    }
                }
            }
        }

        //<summary>
        // Checks outgoing queues for commands to send and puts them on their way.
        // This creates one package per go in UDP.
        //</summary>
        //<returns>If commands are not sent, cause they didn't fit into the package that's sent.</returns>
        internal abstract bool SendOutgoingCommands();

        internal abstract byte[] SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt);

        internal abstract void StopConnection();

        internal void UpdateRoundTripTimeAndVariance(int lastRoundtripTime)
        {
            if (lastRoundtripTime >= 0)
            {
                this.roundTripTimeVariance -= this.roundTripTimeVariance / 4;
                if (lastRoundtripTime >= this.roundTripTime)
                {
                    this.roundTripTime += (lastRoundtripTime - this.roundTripTime) / 8;
                    this.roundTripTimeVariance += (lastRoundtripTime - this.roundTripTime) / 4;
                }
                else
                {
                    this.roundTripTime += (lastRoundtripTime - this.roundTripTime) / 8;
                    this.roundTripTimeVariance -= (lastRoundtripTime - this.roundTripTime) / 4;
                }
                if (this.roundTripTime < this.lowestRoundTripTime)
                {
                    this.lowestRoundTripTime = this.roundTripTime;
                }
                if (this.roundTripTimeVariance > this.highestRoundTripTimeVariance)
                {
                    this.highestRoundTripTimeVariance = this.roundTripTimeVariance;
                }
            }
        }

        /// <summary>
        /// Count of all bytes coming in (including headers)
        /// </summary>
        internal long BytesIn
        {
            get
            {
                return this.bytesIn;
            }
        }

        /// <summary>
        /// Count of all bytes going out (including headers)
        /// </summary>
        internal long BytesOut
        {
            get
            {
                return this.bytesOut;
            }
        }

        internal string HttpUrlParameters
        {
            get;
            set;
        }

        internal bool IsSendingOnlyAcks
        {
            get;
            set;
        }

        internal IPhotonPeerListener Listener
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the currently used settings for the built-in network simulation.
        /// Please check the description of NetworkSimulationSet for more details.
        /// </summary>
        public NetworkSimulationSet NetworkSimulationSettings
        {
            get
            {
                return this.networkSimulationSettings;
            }
        }

        public virtual string PeerID
        {
            get
            {
                return this.peerID.ToString();
            }
        }

        internal abstract int QueuedIncomingCommandsCount { get; }

        internal abstract int QueuedOutgoingCommandsCount { get; }

        internal string ServerAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Enables or disables collection of statistics.
        /// Setting this to true, also starts the stopwatch to measure the timespan the stats are collected.
        /// </summary>
        public bool TrafficStatsEnabled
        {
            get
            {
                return this.trafficStatsEnabled;
            }
            set
            {
                this.trafficStatsEnabled = value;
                if (value)
                {
                    if (this.trafficStatsStopwatch == null)
                    {
                        this.InitializeTrafficStats();
                    }
                    this.trafficStatsStopwatch.Start();
                }
                else
                {
                    this.trafficStatsStopwatch.Stop();
                }
            }
        }

        public long TrafficStatsEnabledTime
        {
            get
            {
                return this.trafficStatsStopwatch != null ? this.trafficStatsStopwatch.ElapsedMilliseconds : 0L;
            }
        }

        /// <summary>
        /// This is the replacement for the const values used in eNet like: PS_DISCONNECTED, PS_CONNECTED, etc.
        /// </summary>
        public enum ConnectionStateValue : byte
        {
            /// <summary>
            /// No connection is available. Use connect.
            /// </summary>
            Disconnected = 0,

            /// <summary>
            /// Establishing a connection already. The app should wait for a status callback.
            /// </summary>
            Connecting = 1,

            ///<summary>
            /// The low level connection with Photon is established. On connect, the library will automatically
            /// send an Init package to select the application it connects to (see also PhotonPeer.Connect()).
            /// When the Init is done, IPhotonPeerListener.OnStatusChanged() is called with connect.
            ///</summary>
            ///<remarks>Please note that calling operations is only possible after the OnStatusChanged() with StatusCode.Connect.</remarks>
            Connected = 3,

            /// <summary>
            /// Connection going to be ended. Wait for status callback.
            /// </summary>
            Disconnecting = 4,

            /// <summary>
            /// Acknowledging a disconnect from Photon. Wait for status callback.
            /// </summary>
            AcknowledgingDisconnect = 5,

            /// <summary>
            /// Connection not properly disconnected.
            /// </summary>
            Zombie = 6
        }

        internal enum EgMessageType : byte
        {
            Init = 0,
            InitResponse = 1,
            Operation = 2,
            OperationResponse = 3,
            Event = 4,
            InternalOperationRequest = 6,
            InternalOperationResponse = 7,
        }

        internal delegate void MyAction();
    }
}
