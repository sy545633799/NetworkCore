using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using ExitGames.Logging;
using Photon.SocketServer.Diagnostics;
using Photon.SocketServer.Diagnostics.Configuration;
using Photon.SocketServer.Rpc.Protocols.GpBinaryByte;
using Photon.SocketServer.Rpc.Protocols.Json;
using Photon.SocketServer.ServerToServer;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer
{       
    /// <summary>
    /// The base class for a Photon application. 
               ///The main method to override is <see cref="M:Photon.SocketServer.ApplicationBase.CreatePeer(Photon.SocketServer.InitRequest)">CreatePeer</see>. 
    ///See <see cref="M:Photon.SocketServer.ApplicationBase.Setup">Setup</see> for initialization recommendations.
    /// </summary>
    public abstract class ApplicationBase : IPhotonApplication, IPhotonControl
    {
        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages to the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// An <see cref="T:ExitGames.Logging.ILogger"/> instance used to log operations to the logging framework.
        /// </summary>
        private static readonly ILogger operationDataLogger = LogManager.GetLogger("OperationData");

        /// <summary>
        /// The application path.
        /// </summary>
        private readonly string applicationPath;

        /// <summary>
        /// The application path.
        /// </summary>
        private readonly string applicationRootPath;

        /// <summary>
        /// The binary path.
        /// </summary>
        private readonly string binaryPath;

        /// <summary>
        /// The current number of peers.
        /// </summary>
        private int peerCount;

        /// <summary>
        /// Set at <see cref="M:PhotonHostRuntimeInterfaces.IPhotonControl.OnPhotonRunning"/> to 1 
        /// and at <see cref="M:PhotonHostRuntimeInterfaces.IPhotonControl.OnStopRequested"/> to 0.
        /// </summary>
        private int running;

        private readonly string photonExeFullDirectory;

        // Methods
        static ApplicationBase()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.ApplicationBase"/> class.
        /// </summary>
        protected ApplicationBase()
        {
            Instance = this;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(this.CurrentDomainOnUnhandledException);
            AppDomainSetup setupInformation = AppDomain.CurrentDomain.SetupInformation;
            if ((setupInformation.AppDomainInitializerArguments != null) && (setupInformation.AppDomainInitializerArguments.Length > 3))
            {
                this.applicationPath = setupInformation.AppDomainInitializerArguments[0];
                this.UnmanagedLogPath = setupInformation.AppDomainInitializerArguments[1];
                this.photonExeFullDirectory = setupInformation.AppDomainInitializerArguments[2];
                this.applicationRootPath = setupInformation.AppDomainInitializerArguments[3];
                this.binaryPath = setupInformation.AppDomainInitializerArguments[4];
            }
            else
            {
                this.UnmanagedLogPath = string.Empty;
                this.photonExeFullDirectory = string.Empty;
                this.applicationRootPath = string.Empty;
                this.applicationPath = Environment.CurrentDirectory;
                this.binaryPath = this.GetBinaryPath(Path.Combine(this.applicationPath, "/bin"));
            }
        }

        /// <summary>
        /// Sends an event to a list of peers.
            /// This method serializes the data just once per protocol instead of once per peer.
        /// </summary>
        /// <typeparam name="TPeer">A <see cref="T:Photon.SocketServer.PeerBase"/> subclass type.</typeparam>
        /// <param name="eventData">The event to send.</param>
        /// <param name="peers">The peers to send the event to.</param>
        /// <param name="sendParameters">The send options.</param>
        public void BroadCastEvent<TPeer>(IEventData eventData, IEnumerable<TPeer> peers, SendParameters sendParameters) where TPeer : PeerBase
        {
            if (sendParameters.Encrypted)
            {
                foreach (TPeer local in peers)
                {
                    local.SendEvent(eventData, sendParameters);
                }
            }
            else
            {
                BroadcastEventData[] dataArray = new BroadcastEventData[Protocol.MaxProtocolType];
                foreach (TPeer local2 in peers)
                {
                    BroadcastEventData data = dataArray[(int)local2.Protocol.ProtocolType];
                    if (data == null)
                    {
                        byte[] introduced14 = eventData.Serialize(local2.Protocol);
                        data = new BroadcastEventData(introduced14, local2.MessageContentType);
                        dataArray[(int)local2.Protocol.ProtocolType] = data;
                    }
                    data.Peers.Add(local2);
                    local2.OnSend(data.Data.Length);
                }
                MessageReliablity reliability = sendParameters.Unreliable ? MessageReliablity.UnReliable : MessageReliablity.Reliable;
                if (sendParameters.Flush)
                {
                    reliability |= MessageReliablity.Flush;
                }
                for (int i = 0; i < dataArray.Length; i++)
                {
                    if (dataArray[i] != null)
                    {
                        SendResults[] resultsArray;
                        IPhotonPeer[] unmanagedPeers = dataArray[i].GetUnmanagedPeers();
                        bool flag = this.ApplicationSink.BroadcastEvent(unmanagedPeers, dataArray[i].Data, reliability, sendParameters.ChannelId, dataArray[i].MessageContentType, out resultsArray);
                        PhotonCounter.EventSentCount.IncrementBy((long)dataArray[i].Peers.Count);
                        PhotonCounter.EventSentPerSec.IncrementBy((long)dataArray[i].Peers.Count);
                        if (!flag)
                        {
                            if (resultsArray == null)
                            {
                                if (log.IsWarnEnabled)
                                {
                                    log.WarnFormat("BroadcastEvent returned unexpected null for sendResults parameter.", new object[0]);
                                }
                            }
                            else
                            {
                                for (int j = 0; j < dataArray[i].Peers.Count; j++)
                                {
                                    switch (resultsArray[j])
                                    {
                                        case SendResults.SendBufferFull:
                                            dataArray[i].Peers[j].OnSendBufferFull();
                                            break;

                                        case SendResults.SendDisconnected:
                                        case SendResults.SendMsgTooBig:
                                        case SendResults.SendFailed:
                                        case SendResults.SendInvalidChannel:
                                            dataArray[i].Peers[j].OnSendFailed((SendResult)resultsArray[j], sendParameters, dataArray[i].Data.Length);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  This method overload is obsolete; use ConnectToServerTcp.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="applicationName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [Obsolete("Use ConnectToServerTcp() instead.")]
        public bool ConnectToServer(IPEndPoint remoteEndPoint, string applicationName, object state)
        {
            return this.ConnectToServerTcp(remoteEndPoint, applicationName, state);
        }

        /// <summary>
        /// This method overload is obsolete; use ConnectToServerMuxTcp.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="useMux"></param>
        /// <param name="applicationName"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        [Obsolete("Use ConnectToServerTcp() or ConnectToServerMuxTcp() instead.")]
        public bool ConnectToServer(IPEndPoint remoteEndPoint, bool useMux, string applicationName, object state)
        {
            if (useMux)
            {
                return this.ConnectToServerMuxTcp(remoteEndPoint, applicationName, state);
            }
            return this.ConnectToServerTcp(remoteEndPoint, applicationName, state, null);
        }

        /// <summary>
        ///  This method overload is obsolete; use ConnectToServerTcp.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="applicationName"></param>
        /// <param name="state"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        [Obsolete("Use ConnectToServerTcp instead.")]
        public bool ConnectToServer(IPEndPoint remoteEndPoint, string applicationName, object state, IRpcProtocol protocol)
        {
            return this.ConnectToServerTcp(remoteEndPoint, applicationName, state, protocol);
        }

        /// <summary>
        ///  This method overload is obsolete; use ConnectToServerUdp.
        /// </summary>
        /// <param name="remoteEndPoint"></param>
        /// <param name="applicationName"></param>
        /// <param name="state"></param>
        /// <param name="numChannels"></param>
        /// <param name="mtu"></param>
        /// <returns></returns>
        [Obsolete("Use ConnectToServerUdp instead.")]
        public bool ConnectToServer(IPEndPoint remoteEndPoint, string applicationName, object state, byte numChannels, short? mtu)
        {
            return this.ConnectToServerUdp(remoteEndPoint, applicationName, state, numChannels, mtu);
        }

        /// <summary>
        /// Establishes a logical, multiplexed TCP connection between two Photon instances. 
        /// Multiple logical connections are sharing a single physical connection.
        /// <see cref="M:Photon.SocketServer.ApplicationBase.CreateServerPeer(Photon.SocketServer.InitResponse,System.Object)"/> is called once a logical connection is established.
              ///<see cref="M:Photon.SocketServer.ApplicationBase.OnServerConnectionFailed(System.Int32,System.String,System.Object)"/> is called if the logical connection fails.
              ///If a phyiscal connection exists when <see cref="M:Photon.SocketServer.ApplicationBase.ConnectToServerMuxTcp(System.Net.IPEndPoint,System.String,System.Object)"/> is called, it is used; otherwise a physical connection is established. 
        /// If the physical connection is aborted, all logical connections are aborted as well. 
        /// </summary>
        /// <param name="remoteEndPoint">
        /// The remote endpoint to connect to.
        /// </param>
        /// <param name="applicationName">
        /// The application name to connect to.
        /// </param>
        /// <param name="state">
        /// A state object that is returned with the callback.
        /// </param>
        /// <returns>
        /// Returns true if outbound connections are allowed (if <see 
        /// cref="P:Photon.SocketServer.ApplicationBase.Running"/> is true).
        /// </returns>
        public bool ConnectToServerMuxTcp(IPEndPoint remoteEndPoint, string applicationName, object state)
        {
            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException("applicationName");
            }
            if (this.Running)
            {
                new TemporaryServerPeer(this, remoteEndPoint, applicationName, true, state).Connect();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Establishes a TCP connection between two Photon instances.
        /// <see cref="M:Photon.SocketServer.ApplicationBase.CreateServerPeer(Photon.SocketServer.InitResponse,System.Object)"/> is called once the connection is established.
        /// <see cref="M:Photon.SocketServer.ApplicationBase.OnServerConnectionFailed(System.Int32,System.String,System.Object)"/> is called if the connection fails. 
        /// </summary>
        /// <param name="remoteEndPoint">
        /// The remote endpoint to connect to.
        /// </param>
        /// <param name="applicationName">
        /// The application name to connect to.
        /// </param>
        /// <param name="state">
        /// A state object that is returned with the callback.
        /// </param>
        /// <returns>
        /// Returns true if outbound connections are allowed (if <see 
        /// cref="P:Photon.SocketServer.ApplicationBase.Running"/> is true).
        /// </returns>
        public bool ConnectToServerTcp(IPEndPoint remoteEndPoint, string applicationName, object state)
        {
            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException("applicationName");
            }
            if (this.Running)
            {
                new TemporaryServerPeer(this, remoteEndPoint, applicationName, false, state).Connect();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Establishes a TCP connection between two Photon instances.
        /// <see cref="M:Photon.SocketServer.ApplicationBase.CreateServerPeer(Photon.SocketServer.InitResponse,System.Object)"/> is called once the connection is established.
        /// <see cref="M:Photon.SocketServer.ApplicationBase.OnServerConnectionFailed(System.Int32,System.String,System.Object)"/> is called if the connection fails.  
        /// </summary>
        /// <param name="remoteEndPoint">
        ///  The remote endpoint to connect to.
        ///  </param>
        /// <param name="applicationName">
        /// The application name to connect to.
        /// </param>
        /// <param name="state">
        /// A state object that is returned with the callback.
        /// </param>
        /// <param name="protocol">
        /// The <see cref="T:Photon.SocketServer.IRpcProtocol"/> used to serialze message data./&gt;
        /// </param>
        /// <returns>
        /// Returns true if outbound connections are allowed (if <see
        /// cref="P:Photon.SocketServer.ApplicationBase.Running"/> is true).
        /// </returns>
        public bool ConnectToServerTcp(IPEndPoint remoteEndPoint, string applicationName, object state, IRpcProtocol protocol)
        {
            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException("applicationName");
            }
            if (this.Running)
            {
                new TemporaryServerPeer(this, remoteEndPoint, applicationName, state, false, protocol).Connect();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Establishes an UDP connection between two Photon instances.
        /// <see cref="M:Photon.SocketServer.ApplicationBase.CreateServerPeer(Photon.SocketServer.InitResponse,System.Object)"/> is called once the connection is established.
        /// <see cref="M:Photon.SocketServer.ApplicationBase.OnServerConnectionFailed(System.Int32,System.String,System.Object)"/> is called if the connection fails.
        /// </summary>
        /// <param name="remoteEndPoint">
        /// The remote endpoint to connect to.
        /// </param>
        /// <param name="applicationName">
        /// The application name to connect to.
        /// </param>
        /// <param name="state">
        /// A state object that is returned with the callback.
        /// </param>
        /// <param name="numChannels">
        ///  The number of channels used by the connection. Channels are prioritized (the lower the channel number, the higher the priority) 
        ///  </param>
        /// <param name="mtu">
        /// Maximum transfer unit - specifies the max data size of each UDP package (in bytes).
        /// Bigger packages will be fragmented. The default value is 1200.
        /// </param>
        /// <returns>
        /// Returns true if outbound connections are allowed (if <see 
        /// cref="P:Photon.SocketServer.ApplicationBase.Running"/> is true).
        /// </returns>
        public bool ConnectToServerUdp(IPEndPoint remoteEndPoint, string applicationName, object state, byte numChannels, short? mtu)
        {
            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException("applicationName");
            }
            if (this.Running)
            {
                new TemporaryServerPeer(this, remoteEndPoint, applicationName, state, numChannels, mtu).Connect();
                return true;
            }
            return false;
        }

        /// <summary>
        ///  Establishes an WebSocket connection between two Photon instances.
             ///   <see cref="M:Photon.SocketServer.ApplicationBase.CreateServerPeer(Photon.SocketServer.InitResponse,System.Object)"/> is called once the connection is established.
        ///  <see cref="M:Photon.SocketServer.ApplicationBase.OnServerConnectionFailed(System.Int32,System.String,System.Object)"/> is called if the connection fails.
        /// </summary>
        /// <param name="remoteEndPoint">
        ///  The remote endpoint to connect to.
        /// </param>
        /// <param name="applicationName">
        /// The application name to connect to.
        /// </param>
        /// <param name="state">
        /// A state object that is returned with the callback.
        /// </param>
        /// <param name="webSocketVersion">
        /// The websocket protocol version (currently supported: RF6455 and HyBi10)
        /// </param>
        /// <param name="protocol">
        ///  The protocol to serialize the message data. 
        ///  </param>
        /// <returns>
        /// Returns true if outbound connections are allowed (if <see 
        /// cref="P:Photon.SocketServer.ApplicationBase.Running"/> is true).
        /// </returns>
        [CLSCompliant(false)]
        public bool ConnectToServerWebSocket(IPEndPoint remoteEndPoint, string applicationName, object state, WebSocketVersion webSocketVersion, IRpcProtocol protocol)
        {
            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException("applicationName");
            }
            if (this.Running)
            {
                new TemporaryServerPeer(this, remoteEndPoint, applicationName, state, webSocketVersion, protocol).Connect();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Establishes an WebSocket connection between two Photon instances, using the Hixie76 WebSocket protocol.
        ///     <see cref="M:Photon.SocketServer.ApplicationBase.CreateServerPeer(Photon.SocketServer.InitResponse,System.Object)"/> is called once the connection is established.
        ///  <see cref="M:Photon.SocketServer.ApplicationBase.OnServerConnectionFailed(System.Int32,System.String,System.Object)"/> is called if the connection fails.
        /// </summary>
        /// <param name="remoteEndPoint">
        ///  The remote endpoint to connect to.
        ///  </param>
        /// <param name="applicationName">
        /// The application name to connect to.
        /// </param>
        /// <param name="state">
        /// A state object that is returned with the callback.
        /// </param>
        /// <param name="origin">
        /// The origin of the request.
        /// </param>
        /// <returns>Returns true if outbound connections are allowed (if <see 
        /// cref="P:Photon.SocketServer.ApplicationBase.Running"/> is true).
        /// </returns>
        public bool ConnectToServerWebSocketHixie76(IPEndPoint remoteEndPoint, string applicationName, object state, string origin)
        {
            if (remoteEndPoint == null)
            {
                throw new ArgumentNullException("remoteEndPoint");
            }
            if (applicationName == null)
            {
                throw new ArgumentNullException("applicationName");
            }
            if (this.Running)
            {
                new TemporaryServerPeer(this, remoteEndPoint, applicationName, state, origin).Connect();
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method is called by the <see cref="M:PhotonHostRuntimeInterfaces.IPhotonApplication.OnInit(PhotonHostRuntimeInterfaces.IPhotonPeer,System.Byte[],System.Byte)">IPhotonApplication.OnInit</see> implementation of this class.
        /// The inheritor should return a <see cref="T:Photon.SocketServer.PeerBase"/> implementation.
        /// </summary>
        /// <param name="initRequest">The initialization request.</param>
        /// <returns> A new instance of <see cref="T:Photon.SocketServer.PeerBase"/> or  null.</returns>
        protected abstract PeerBase CreatePeer(InitRequest initRequest);
        private void CreatePeerInternal(object state)
        {
            try
            {
                InitRequest initRequest = (InitRequest)state;
                PeerBase pObj = this.CreatePeer(initRequest);
                if (pObj == null)
                {
                    initRequest.PhotonPeer.DisconnectClient();
                }
                else
                {
                    initRequest.PhotonPeer.SetUserData(pObj);
                    this.IncrementPeerCounter();
                    byte[] data = initRequest.Protocol.SerializeInitResponse();
                    SendResult result = (SendResult)initRequest.PhotonPeer.Send(data, MessageReliablity.Flush | MessageReliablity.Reliable, 0, pObj.MessageContentType);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OnInit - response sent to ConnId {0} with SendResult {1}", new object[] { initRequest.PhotonPeer.GetConnectionID(), result });
                    }
                    if (operationDataLogger.IsDebugEnabled)
                    {
                        operationDataLogger.DebugFormat("OnInit - ConnID={0}, send data=({1} bytes) {2}", new object[] { initRequest.PhotonPeer.GetConnectionID(), data.Length, BitConverter.ToString(data) });
                    }
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        /// <summary>
        /// Invoked if an connection attempt to a server succeeds.
        ///The inheritor can return an instance of <see cref="T:Photon.SocketServer.ServerToServer.ServerPeerBase"/>; the default implementation returns null.
        /// </summary>
        /// <param name="initResponse"> The init response from the application the peer connected to.</param>
        /// <param name="state"> A state object.</param>
        /// <returns>An instance of <see cref="T:Photon.SocketServer.ServerToServer.ServerPeerBase"/> or null.</returns>
        protected internal virtual ServerPeerBase CreateServerPeer(InitResponse initResponse, object state)
        {
            return null;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            if (log.IsErrorEnabled)
            {
                log.Error((Exception)unhandledExceptionEventArgs.ExceptionObject);
            }
            if (unhandledExceptionEventArgs.IsTerminating && log.IsWarnEnabled)
            {
                log.Warn("Process is terminating.");
            }
        }

        /// <summary>
        /// Decrements the peer counter.
        /// </summary>
        internal void DecrementPeerCounter()
        {
            PhotonCounter.SessionCount.Decrement();
            Interlocked.Decrement(ref this.peerCount);
        }

        /// <summary>
        /// Helper method to get the path to the current assembly.
        /// </summary>
        /// <param name="defaultPath"></param>
        /// <returns></returns>
        private string GetBinaryPath(string defaultPath)
        {
            string uri = string.Empty;
            try
            {
                uri = base.GetType().Assembly.CodeBase;
                UriBuilder builder = new UriBuilder(uri);
                return Path.GetDirectoryName(Uri.UnescapeDataString(builder.Path));
            }
            catch (Exception exception)
            {
                log.WarnFormat("Failed to get binary path: codeBase={0}, exception={1}", new object[] { uri, exception.Message });
                return defaultPath;
            }
        }

        /// <summary>
        ///  Increments the peer counter.
        /// </summary>
        internal void IncrementPeerCounter()
        {
            PhotonCounter.SessionCount.Increment();
            Interlocked.Increment(ref this.peerCount);
        }

        [CLSCompliant(false)]
        public string[] ListenerList(out ListenerStatus[] status)
        {
            return this.ControlListeners.GetListeners(out status);
        }

        public bool ListenerStart(string name)
        {
            return this.ControlListeners.StartListener(name);
        }

        public bool ListenerStop(string name)
        {
            return this.ControlListeners.StopListener(name);
        }

        /// <summary>
        /// Invoked if a connection attempt to a server fails.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage"> The error message.</param>
        /// <param name="state">The state.</param>
        protected virtual void OnServerConnectionFailed(int errorCode, string errorMessage, object state)
        {
            log.ErrorFormat("Server connection failed with error {0}: {1}", new object[] { errorCode, errorMessage });
        }

        /// <summary>
        /// Called when the application is started.
        ///  This method calls <see cref="M:Photon.SocketServer.ApplicationBase.Setup"/>.
        /// </summary>
        /// <param name="instanceName">The name of the instance.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="sink">The sink.</param>
        /// <param name="controlListners">The photon listener controler</param>
        /// <param name="applicationsCounter"> An IPhotonApplicationsCounter instance used to read photon socket server unmanged counters.</param>
        /// <param name="unmanagedLogDirectory"> The log path of Photon. </param>
        /// <returns>PhotonApplication object.</returns>
        [CLSCompliant(false)]
        public IPhotonApplication OnStart(string instanceName, string applicationName, IPhotonApplicationSink sink, IControlListeners controlListners, IPhotonApplicationsCounter applicationsCounter, string unmanagedLogDirectory)
        {
            this.ApplicationName = applicationName;
            this.PhotonInstanceName = instanceName;
            this.ApplicationSink = sink;
            this.UnmanagedLogPath = unmanagedLogDirectory;
            this.ControlListeners = controlListners;
            return this;
        }

        /// <summary>
        /// Called when photon starts a new app domain for the same application.
              ///New connections will connect to the new app domain.
        ///This app domain continues to receive operations until from existing connections until the last peer disconnects.
        /// </summary>
        /// <remarks>This feature requires the AutoRestart setting in the PhotonServer.config. 
              ///Please refer to the configuration manual for more details.
        ///</remarks>
        protected virtual void OnStopRequested()
        {
        }

        /// <summary>
        /// Called by the unmanaged socket server if a peer disconnects (or is disconnected).
        /// </summary>
        /// <param name="photonPeer">The peer which disconnected.</param>
        /// <param name="userData"> The user data.</param>
        /// <param name="reasonCode">The disconnect reason code.</param>
        /// <param name="reasonDetail">The disconnect reason detail.</param>
        /// <param name="rtt">The round trip time.</param>
        /// <param name="rttVariance">The round trip time variance.</param>
        /// <param name="numFailures"> The amount of failures. </param>
        void IPhotonApplication.OnDisconnect(IPhotonPeer photonPeer, object userData, DisconnectReason reasonCode, string reasonDetail, int rtt, int rttVariance, int numFailures)
        {
            try
            {
                IManagedPeer peer = userData as IManagedPeer;
                if (peer != null)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OnDisconnect - ConnID={0}", new object[] { photonPeer.GetConnectionID() });
                    }
                    if (peer.Application_OnDisconnect(reasonCode, reasonDetail, rtt, rttVariance, numFailures))
                    {
                        this.DecrementPeerCounter();
                    }
                }
                else if (log.IsDebugEnabled)
                {
                    log.DebugFormat("OnDisconnect - Peer not found: {0}", new object[] { photonPeer.GetConnectionID() });
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
            finally
            {
                photonPeer.SetUserData(null);
            }
        }

        /// <summary>
        /// Called by the unmanaged socket server when a peer's send buffer reaches it's limit or is freed again.
        /// </summary>
        /// <param name="photonPeer"> The unmanaged peer.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="flowControlEvent">The flow control event.</param>
        void IPhotonApplication.OnFlowControlEvent(IPhotonPeer photonPeer, object userData, FlowControlEvent flowControlEvent)
        {
            try
            {
                IManagedPeer peer = userData as IManagedPeer;
                if (peer == null)
                {
                    log.ErrorFormat("OnFlowControlEvent - Peer {0}'s user data is of wrong type or null: {1}; event {2}", new object[] { photonPeer.GetConnectionID(), userData, flowControlEvent });
                    photonPeer.DisconnectClient();
                }
                else
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OnFlowControlEvent: Peer={0}; event={1}", new object[] { photonPeer.GetConnectionID(), flowControlEvent });
                    }
                    if (flowControlEvent == FlowControlEvent.FlowControlAllOk)
                    {
                        peer.Application_OnSendBufferEmpty();
                    }
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
        }

        /// <summary>
        /// Called by the unmanaged socket server when a peer intializes the connection.
        /// This method determines the used rpc protocol and then calls <see cref="M:Photon.SocketServer.ApplicationBase.CreatePeer(Photon.SocketServer.InitRequest)">CreatePeer</see>.
        /// </summary>
        /// <param name="nativePeer">The photon peer.</param>
        /// <param name="data"> The data.</param>
        /// <param name="channelCount">The number of channels that will be used by this peer. </param>
        void IPhotonApplication.OnInit(IPhotonPeer nativePeer, byte[] data, byte channelCount)
        {
            try
            {
                InitRequest request;
                PhotonCounter.InitPerSec.Increment();
                if (operationDataLogger.IsDebugEnabled)
                {
                    operationDataLogger.DebugFormat("OnInit - ConnID={0}, data=({1} bytes) {2}", new object[] { nativePeer.GetConnectionID(), data.Length, BitConverter.ToString(data) });
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("OnInit - ConnID={0}, IP {1} on port {2}, type = {3}", new object[] { nativePeer.GetConnectionID(), nativePeer.GetRemoteIP(), nativePeer.GetLocalPort(), nativePeer.GetListenerType() });
                }
                if (nativePeer.GetListenerType() == ListenerType.HTTPListener)
                {
                    string str = "";
                    if (data.Length > 7)
                    {
                        str = Encoding.ASCII.GetString(data, 7, data.Length - 7);
                        log.DebugFormat("OnInit - {0}", new object[] { str });
                    }
                    log.DebugFormat("OnInit - HttpPeer cnnected. " + str, new object[0]);
                    if (str.Contains("json"))
                    {
                        request = new InitRequest("LiteLobby", new Version(0, 3, 6), JsonProtocol.Instance);
                    }
                    else
                    {
                        request = new InitRequest("LiteLobby", new Version(0, 3, 6), GpBinaryByteProtocolV16.HeaderV2Instance);
                    }
                }
                else if (nativePeer.GetListenerType() == ListenerType.WebSocketListener)
                {
                    if (!Protocol.TryParseWebSocketInitRequest(data, this.ApplicationName, Versions.DefaultWebsocketPeerVersion, out request))
                    {
                        log.Warn("OnInit - Failed to parse init request for WebSocket peer: {0}" + data);
                        nativePeer.DisconnectClient();
                        return;
                    }
                }
                else if (!Protocol.TryParseInitRequest(data, out request))
                {
                    if (log.IsDebugEnabled)
                    {
                        if (data.Length == 0)
                        {
                            log.Debug("OnInit - Data must contain at least one byte.");
                        }
                        else
                        {
                            log.DebugFormat("OnInit - Failed to parse init request with protocol {0}", new object[] { data[0] });
                        }
                    }
                    nativePeer.DisconnectClient();
                    return;
                }
                request.PhotonPeer = nativePeer;
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.CreatePeerInternal), request);
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
        }

        /// <summary>
        /// Callback for established outbound connections.
        /// </summary>
        /// <param name="photonPeer">The photon peer.</param>
        /// <param name="data">Contains the response headers from the handshake negotiation for outbound websocket connections; empty for all other connection types.</param>
        /// <param name="userData">The user data.</param>
        void IPhotonApplication.OnOutboundConnectionEstablished(IPhotonPeer photonPeer, byte[] data, object userData)
        {
            try
            {
                TemporaryServerPeer peer = userData as TemporaryServerPeer;
                if (peer != null)
                {
                    peer.Application_OnOutboundConnectionEstablished(photonPeer);
                }
                else
                {
                    log.ErrorFormat("Outbound connection established for peer {0} but user data contained unexpected data: {1}; aborting connection.", new object[] { photonPeer.GetConnectionID(), userData });
                    photonPeer.DisconnectClient();
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
        }

        /// <summary>
        /// Callback for failed outbound connections.
        /// </summary>
        /// <param name="photonPeer"> The photon peer.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="errorCode">The error Code.</param>
        /// <param name="errorMessage">The error message</param>
        void IPhotonApplication.OnOutboundConnectionFailed(IPhotonPeer photonPeer, object userData, int errorCode, string errorMessage)
        {
            try
            {
                TemporaryServerPeer peer = userData as TemporaryServerPeer;
                if (peer != null)
                {
                    this.OnServerConnectionFailed(errorCode, errorMessage, peer.State);
                }
                else
                {
                    log.ErrorFormat("Outbound connection failed for peer {0}: {1} - {2}; user data contained unexpected data: {3}", new object[] { photonPeer.GetConnectionID(), errorCode, errorMessage, userData });
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
            finally
            {
                photonPeer.SetUserData(null);
            }
        }

        /// <summary>
        /// Called by the unmanaged socket server when new data was received.
        /// </summary>
        /// <param name="photonPeer">The peer who sent the operation.</param>
        /// <param name="userData">The user data.</param>
        /// <param name="data">The data for the operation.</param>
        /// <param name="reliability">Message reliable flags for the operation.</param>
        /// <param name="channelId">The channel ID.</param>
        /// <param name="messageContentType">The Message Content Type.</param>
        /// <param name="rtt"> The round trip time.</param>
        /// <param name="rttVariance">The round trip time variance.</param>
        /// <param name="numFailures"> The number of failures.</param>
        void IPhotonApplication.OnReceive(IPhotonPeer photonPeer, object userData, byte[] data, MessageReliablity reliability, byte channelId, MessageContentType messageContentType, int rtt, int rttVariance, int numFailures)
        {
            try
            {
                PhotonCounter.OperationReceiveCount.Increment();
                PhotonCounter.OperationReceivePerSec.Increment();
                if (operationDataLogger.IsDebugEnabled)
                {
                    operationDataLogger.DebugFormat("OnReceive - ConnID={0}, data=({1} bytes) {2}", new object[] { photonPeer.GetConnectionID(), data.Length, BitConverter.ToString(data) });
                }
                IManagedPeer peer = userData as IManagedPeer;
                if (peer == null)
                {
                    log.ErrorFormat("OnReceive - Peer {0}'s user data is of wrong type or null: {1}", new object[] { photonPeer.GetConnectionID(), userData });
                    photonPeer.DisconnectClient();
                }
                else
                {
                    SendParameters sendParameters = new SendParameters
                    {
                        Unreliable = reliability == MessageReliablity.UnReliable,
                        ChannelId = channelId
                    };
                    peer.Application_OnReceive(data, sendParameters, rtt, rttVariance, numFailures);
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
        }

        /// <summary>
        ///  This method is called when photon is ready to serve new connections with the current application.
        /// </summary>
        void IPhotonControl.OnPhotonRunning()
        {
            try
            {
                Interlocked.Exchange(ref this.running, 1);
                this.Setup();
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Application start: AppId={0}; AppPath={1}, Type={2} ", new object[] { this.ApplicationName, this.ApplicationPath, Instance.GetType() });
                }
                if (PhotonSettings.Default.CounterPublisher.Enabled)
                {
                    if (PhotonSettings.Default.CounterPublisher.AddDefaultAppCounter)
                    {
                        CounterPublisher.DefaultInstance.AddStaticCounterClass(typeof(PhotonCounter), this.ApplicationName);
                    }
                    CounterPublisher.DefaultInstance.Start();
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
        }

        /// <summary>
        ///  Called when the application is stopped.
        /// This method calls <see cref="M:Photon.SocketServer.ApplicationBase.TearDown"/>.
        /// </summary>
        void IPhotonControl.OnStop()
        {
            try
            {
                Interlocked.Exchange(ref this.running, 0);
                this.TearDown();
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Application stop: AppId={0}", new object[] { this.ApplicationName });
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
        }

        /// <summary>
        /// Called when photon starts a new app domain for the same application.
              ///New connections will connect to the new app domain.
        ///This app domain continues to receive operations from existing connections until the last peer disconnects.
        /// </summary>
        /// <remarks>This feature requires the EnableAutoRestart setting in the PhotonServer.config. 
        ///Please refer to the configuration manual for more details.</remarks>
        void IPhotonControl.OnStopRequested()
        {
            try
            {
                Interlocked.Exchange(ref this.running, 0);
                this.OnStopRequested();
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Application is stopping: AppId={0}", new object[] { this.ApplicationName });
                }
            }
            catch (Exception exception)
            {
                log.Error(exception);
                throw;
            }
        }
                   
        /// <summary>
        /// This method is called when the current application has been started.
        /// The inheritor can setup log4net here and execute other initialization routines here.
        /// </summary>
        /// <example>
        /// log4net initialization:
        ///      <code>
        ///        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
       ///         protected override void Setup()
        ///        {
       ///         // configure log4net with a config file
       ///         var configFileInfo = new FileInfo(Path.Combine(this.BinaryPath, "log4net.config"));
       ///         XmlConfigurator.ConfigureAndWatch(configFileInfo);
      ///      
       ///         // redirect photon sdk internal logging to log4net
       ///         ExitGames.Logging.LogManager.SetLoggerFactory(ExitGames.Logging.Log4Net.Log4NetLoggerFactory.Instance);
       ///         }
        ///      </code>
        /// </example>
        protected abstract void Setup();

        /// <summary>
        /// This method is called when the current application is being stopped.
        /// The inheritor can execute cleanup routines here.
        /// </summary>
        protected abstract void TearDown();

        /// <summary>
        /// Gets the application name set in PhotonServer.config.
        /// </summary>
        public string ApplicationName { get; private set; }

        /// <summary>
        /// Gets the application path set in PhotonServer.config.
        /// </summary>
        public string ApplicationPath
        {
            get
            {
                return this.applicationPath;
            }
        }

        /// <summary>
        /// Gets the path of the application root path - base location of all applications.
        /// </summary>
        public string ApplicationRootPath
        {
            get
            {
                return this.applicationRootPath;
            }
        }

        /// <summary>
        ///  Gets the photon application sink.
        /// </summary>
        internal IPhotonApplicationSink ApplicationSink { get; private set; }

        /// <summary>
        ///  Gets the path of the application binaries.
        /// </summary>
        public string BinaryPath
        {
            get
            {
                return this.binaryPath;
            }
        }

        internal IControlListeners ControlListeners { get; private set; }

        /// <summary>
        ///  Gets the application instance.
        /// </summary>
        public static ApplicationBase Instance { get; private set; }

        /// <summary>
        /// Gets the number of peers currently connected to the application.
        /// </summary>
        public int PeerCount
        {
            get
            {
                return this.peerCount;
            }
        }

        /// <summary>
        ///  Gets the name of the photon instance.
        /// </summary>
        public string PhotonInstanceName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the application is running (the time between <see 
        /// cref="M:Photon.SocketServer.ApplicationBase.Setup"/> and <see 
        /// cref="M:Photon.SocketServer.ApplicationBase.OnStopRequested"/>).
        /// </summary>
        public bool Running
        {
            get
            {
                return (this.running == 1);
            }
        }

        /// <summary>
        /// Gets the log path of Photon.
        /// </summary>
        public string UnmanagedLogPath { get; private set; }
    }
}
