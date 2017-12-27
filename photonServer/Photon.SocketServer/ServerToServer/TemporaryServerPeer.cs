using System;
using System.IO;
using System.Net;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;
using Photon.SocketServer.Rpc.Protocols;
using Photon.SocketServer.Rpc.Protocols.GpBinaryByte;
using Photon.SocketServer.ServerToServer;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.ServerToServer
{
    internal class TemporaryServerPeer : IManagedPeer
    {
        // Fields
        private bool aborted;
        private readonly ApplicationBase application;
        private readonly string appName;
        public static readonly IRpcProtocol DefaultProtocol = GpBinaryByteProtocolV16.HeaderV2Instance;
        private readonly PoolFiber executionFiber;
        private readonly bool isENet;
        private readonly bool isHixie76;
        private readonly bool isWebSocket;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly short? mtu;
        private readonly byte numChannels;

        /// <summary>
        /// The operation data logger.
        /// </summary>
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");
        private readonly string origin;
        private IPhotonPeer photonPeer;
        private readonly IRpcProtocol protocol;
        private readonly IPEndPoint remoteEndPoint;
        private ServerPeerBase serverPeer;
        private readonly object state;
        private readonly object syncRoot;
        private readonly bool useMux;
        private WebSocketVersion? webSocketVersion;

        // Methods
        private TemporaryServerPeer()
        {
            this.protocol = DefaultProtocol;
            this.syncRoot = new object();
            this.executionFiber = new PoolFiber();
            this.executionFiber.Start();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TemporaryServerPeer"/> class for TCP connections. 
        /// </summary>
        /// <param name="application">The application that requested to establish the connection.</param>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="appName">The application name to connect to.</param>
        /// <param name="useMux">Share a single physical connection between multiple logical connections.</param>
        /// <param name="state">A state object that is passed to the callback </param>
        public TemporaryServerPeer(ApplicationBase application, IPEndPoint remoteEndPoint, string appName, bool useMux, object state)
            : this()
        {
            this.application = application;
            this.appName = appName;
            this.state = state;
            this.remoteEndPoint = remoteEndPoint;
            this.useMux = useMux;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TemporaryServerPeer"/> class for Hixie76 WebSocket connections.
        /// </summary>
        /// <param name="application">The application that requested to establish the connection.</param>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="appName"> The application name to connect to.</param>
        /// <param name="state"> A state object that is passed to the callback </param>
        /// <param name="origin">The origin of the connection</param>
        public TemporaryServerPeer(ApplicationBase application, IPEndPoint remoteEndPoint, string appName, object state, string origin)
            : this()
        {
            this.application = application;
            this.appName = appName;
            this.state = state;
            this.remoteEndPoint = remoteEndPoint;
            this.isWebSocket = true;
            this.isHixie76 = true;
            this.origin = origin;
            this.protocol = Protocol.Json;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TemporaryServerPeer"/> class for WebSocket connections other than Hixie76 (currenctly supported: HiBy10, RFC6455). 
        /// </summary>
        /// <param name="application">The application that requested to establish the connection.</param>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="appName">The application name to connect to.</param>
        /// <param name="state">A state object that is passed to the callback </param>
        /// <param name="webSocketVersion"> The WebSocket protocol version.</param>
        /// <param name="protocol"> The protocol used to serialize the message data.</param>
        public TemporaryServerPeer(ApplicationBase application, IPEndPoint remoteEndPoint, string appName, object state, WebSocketVersion webSocketVersion, IRpcProtocol protocol)
            : this()
        {
            this.application = application;
            this.appName = appName;
            this.state = state;
            this.remoteEndPoint = remoteEndPoint;
            this.isWebSocket = true;
            this.isHixie76 = false;
            this.webSocketVersion = new WebSocketVersion?(webSocketVersion);
            if (protocol != null)
            {
                this.protocol = protocol;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TemporaryServerPeer"/> class for TCP connections. 
        /// </summary>
        /// <param name="application">The application that requested to establish the connection.</param>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="appName">The application name to connect to.</param>
        /// <param name="state"> A state object that is passed to the callback </param>
        /// <param name="useMux">Share a single physical connection between multiple logical connections.</param>
        /// <param name="protocol">The protocol used to serialize the message data.</param>
        public TemporaryServerPeer(ApplicationBase application, IPEndPoint remoteEndPoint, string appName, object state, bool useMux, IRpcProtocol protocol)
            : this()
        {
            this.application = application;
            this.remoteEndPoint = remoteEndPoint;
            this.appName = appName;
            this.state = state;
            this.useMux = useMux;
            if (protocol != null)
            {
                this.protocol = protocol;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ServerToServer.TemporaryServerPeer"/> class for reliable UDP connections.
        /// </summary>
        /// <param name="application">The application that requested to establish the connection.</param>
        /// <param name="remoteEndPoint">The remote endpoint to connect to.</param>
        /// <param name="appName">The application name to connect to.</param>
        /// <param name="state">A state object that is passed to the callback </param>
        /// <param name="numChannels">The number of channels that are used by the UDP connection.</param>
        /// <param name="mtu">The maximum size for each package.</param>
        public TemporaryServerPeer(ApplicationBase application, IPEndPoint remoteEndPoint, string appName, object state, byte numChannels, short? mtu)
            : this()
        {
            this.application = application;
            this.appName = appName;
            this.state = state;
            this.remoteEndPoint = remoteEndPoint;
            this.isENet = true;
            this.numChannels = numChannels;
            this.mtu = mtu;
        }

        public void Application_OnOutboundConnectionEstablished(IPhotonPeer peer)
        {
            lock (this.syncRoot)
            {
                this.photonPeer = peer;
            }
            if (peer.GetListenerType() != ListenerType.WebSocketListener)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("OnOutboundConnectionEstablished: sending init request");
                }
                byte[] data = this.protocol.SerializeInitRequest(this.appName, Versions.TcpOutboundPeerVersion);
                SendResults results = peer.Send(data, MessageReliablity.Reliable, 0, MessageContentType.Binary);
                LogInitRequest(peer, 0, data, (SendResult)results);
            }
        }

        public void Connect()
        {
            lock (this.syncRoot)
            {
                if (this.isENet)
                {
                    this.photonPeer = this.application.ApplicationSink.ConnectENet(this.remoteEndPoint.Address.ToString(), (ushort)this.remoteEndPoint.Port, this.numChannels, this, this.mtu);
                }
                else if (this.useMux)
                {
                    this.photonPeer = this.application.ApplicationSink.ConnectMux(this.remoteEndPoint.Address.ToString(), (ushort)this.remoteEndPoint.Port, this);
                }
                else if (this.isWebSocket)
                {
                    if (this.isHixie76)
                    {
                        this.photonPeer = this.application.ApplicationSink.ConnectHixie76WebSocket(this.remoteEndPoint.Address.ToString(), (ushort)this.remoteEndPoint.Port, this.appName, this.origin, this);
                    }
                    else
                    {
                        this.photonPeer = this.application.ApplicationSink.ConnectWebSocket(this.remoteEndPoint.Address.ToString(), (ushort)this.remoteEndPoint.Port, this.webSocketVersion.GetValueOrDefault(WebSocketVersion.HyBi13), this.appName, Enum.GetName(typeof(ProtocolType), this.protocol.ProtocolType), this);
                    }
                }
                else
                {
                    this.photonPeer = this.application.ApplicationSink.Connect(this.remoteEndPoint.Address.ToString(), (ushort)this.remoteEndPoint.Port, this);
                }
            }
        }

        private void CreateServerPeer()
        {
            InitResponse initResponse = new InitResponse(this.appName, this.protocol)
            {
                PhotonPeer = this.photonPeer
            };
            lock (this.syncRoot)
            {
                this.serverPeer = this.application.CreateServerPeer(initResponse, this.state);
                if (this.serverPeer != null)
                {
                    this.photonPeer.SetUserData(this.serverPeer);
                    this.application.IncrementPeerCounter();
                    return;
                }
            }
            log.Warn("CreateServerPeer returned null, disconnecting s2s connection");
            this.Disconnect();
        }

        private void Disconnect()
        {
            IPhotonPeer photonPeer;
            lock (this.syncRoot)
            {
                photonPeer = this.photonPeer;
            }
            if (!this.aborted)
            {
                photonPeer.DisconnectClient();
                this.aborted = true;
            }
        }

        /// <summary>
        /// Logs the init request.
        /// </summary>
        /// <param name="peer">The peer.</param>
        /// <param name="channelId">The channel id.</param>
        /// <param name="data">The data.</param>
        /// <param name="sendResult">The send result.</param>
        private static void LogInitRequest(IPhotonPeer peer, int channelId, byte[] data, SendResult sendResult)
        {
            if (operationDataLogger.IsDebugEnabled)
            {
                operationDataLogger.DebugFormat("SentInitRequest: ConnID={0}, ChannelId={1}, result={2}, data=({3} bytes) {4}", new object[] { peer.GetConnectionID(), channelId, sendResult, data.Length, BitConverter.ToString(data) });
            }
            else if (log.IsDebugEnabled)
            {
                log.DebugFormat("SentInitRequest: ConnID={0}, ChannelId={1}, result={2} size={3} bytes", new object[] { peer.GetConnectionID(), channelId, sendResult, data.Length });
            }
        }

        private void OnReceive(byte[] data, SendParameters sendParameters, int rtt, int rttVariance, int numFailures)
        {
            if (this.serverPeer != null)
            {
                ((IManagedPeer)this.serverPeer).Application_OnReceive(data, sendParameters, rtt, rttVariance, numFailures);
            }
            else
            {
                RtsMessageHeader header;
                if (!this.protocol.TryParseMessageHeader(data, out header))
                {
                    log.Warn("invalid s2s header, disconnecting");
                    this.Disconnect();
                    throw new InvalidDataException("invalid s2s header");
                }
                if (header.MessageType != RtsMessageType.InitResponse)
                {
                    log.Warn("unexpected message type " + header.MessageType);
                    this.Disconnect();
                }
                else
                {
                    this.CreateServerPeer();
                }
            }
        }

        bool IManagedPeer.Application_OnDisconnect(DisconnectReason reasonCode, string reasonDetail, int rtt, int rttVariance, int numFailures)
        {
            lock (this.syncRoot)
            {
                if (this.serverPeer != null)
                {
                    return ((IManagedPeer)this.serverPeer).Application_OnDisconnect(reasonCode, reasonDetail, rtt, rttVariance, numFailures);
                }
            }
            this.executionFiber.Enqueue(delegate
            {
                ((IPhotonApplication)this.application).OnOutboundConnectionFailed(this.photonPeer, this, 0x274c, "Disconnect");
            });
            return false;
        }

        void IManagedPeer.Application_OnReceive(byte[] data, SendParameters sendParameters, int rtt, int rttVariance, int numFailures)
        {
            this.executionFiber.Enqueue(() => this.OnReceive(data, sendParameters, rtt, rttVariance, numFailures));
        }

        void IManagedPeer.Application_OnSendBufferEmpty()
        {
        }

        // Properties
        public string ApplicationName
        {
            get
            {
                return this.appName;
            }
        }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return this.remoteEndPoint;
            }
        }

        public object State
        {
            get
            {
                return this.state;
            }
        }
    }
}
