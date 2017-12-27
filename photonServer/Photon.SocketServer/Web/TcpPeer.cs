using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ExitGames.Logging;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.Web
{
    /// <summary>
    ///  Base class for Tcp client implementations.
    /// </summary>
    internal class TcpPeer : ITcpListener, IPhotonPeer, IDisposable
    {
        // Fields
        private readonly IPhotonApplication application;
        private int connected;
        private readonly int connectionId;
        private static int connectionIdCounter;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private Timer pingTimer;
        private string remoteAddress;
        private ushort remotePort;
        private Socket socketConnection;
        private readonly TcpReader tcpReader;
        private object userData;

        /// <summary>
        ///   Initializes a new instance of the <see cref="T:Photon.SocketServer.Web.TcpReader"/> class.
        /// </summary>
        /// <param name="application"></param>
        internal TcpPeer(IPhotonApplication application)
        {
            this.application = application;
            this.tcpReader = new TcpReader(this);
            this.connectionId = Interlocked.Increment(ref connectionIdCounter);
        }

        public void AbortClient()
        {
            this.DisconnectClient();
        }

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
                    this.application.OnDisconnect(this, this.userData, DisconnectReason.ManagedDisconnect, string.Empty, 0, 0, 0);
                }
            }
            else if (!socket.ReceiveAsync(e))
            {
                this.OnReceiveAsyncCompleted(socket, e);
            }
        }

        public void Connect(string address, ushort port)
        {
            IPAddress address2;
            if (!IPAddress.TryParse(address, out address2))
            {
                throw new InvalidOperationException("invalid adress specified");
            }
            this.remoteAddress = address;
            this.remotePort = port;
            IPEndPoint point = new IPEndPoint(address2, port);
            this.socketConnection = new Socket(point.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs e = new SocketAsyncEventArgs
            {
                RemoteEndPoint = point
            };
            e.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnConnectAsyncCompleted);
            if (!this.socketConnection.ConnectAsync(e))
            {
                this.OnConnectAsyncCompleted(this.socketConnection, e);
            }
        }

        public void DisconnectClient()
        {
            if (this.socketConnection != null)
            {
                this.socketConnection.Disconnect(false);
            }
            Timer pingTimer = this.pingTimer;
            if (pingTimer != null)
            {
                pingTimer.Dispose();
                this.pingTimer = null;
            }
        }

        public void Dispose()
        {
            if (this.pingTimer != null)
            {
                this.pingTimer.Dispose();
                this.pingTimer = null;
            }
            if (this.socketConnection != null)
            {
                this.socketConnection.Close();
                this.socketConnection = null;
            }
        }

        public void Flush()
        {
        }

        public int GetConnectionID()
        {
            return this.connectionId;
        }

        public ListenerType GetListenerType()
        {
            return ListenerType.TCPListener;
        }

        public string GetLocalIP()
        {
            if (this.socketConnection == null)
            {
                return string.Empty;
            }
            IPEndPoint localEndPoint = this.socketConnection.LocalEndPoint as IPEndPoint;
            if (localEndPoint == null)
            {
                return string.Empty;
            }
            return localEndPoint.Address.ToString();
        }

        public ushort GetLocalPort()
        {
            if (this.socketConnection == null)
            {
                return 0;
            }
            IPEndPoint localEndPoint = this.socketConnection.LocalEndPoint as IPEndPoint;
            if (localEndPoint == null)
            {
                return 0;
            }
            return (ushort)localEndPoint.Port;
        }

        public PeerType GetPeerType()
        {
            return PeerType.TCPPeer;
        }

        public string GetRemoteIP()
        {
            return this.remoteAddress;
        }

        public ushort GetRemotePort()
        {
            return this.remotePort;
        }

        public void GetStats(out int rtt, out int rttVariance, out int numFailures)
        {
            rtt = 0;
            rttVariance = 0;
            numFailures = 0;
        }

        public object GetUserData()
        {
            return this.userData;
        }

        private bool HandleSocketException(SocketException exception, out SendResult sendResult)
        {
            switch (exception.SocketErrorCode)
            {
                case SocketError.ConnectionReset:
                    this.Connected = false;
                    this.application.OnDisconnect(this, this.userData, DisconnectReason.ManagedDisconnect, string.Format(exception.Message, new object[0]), 0, 0, 0);
                    sendResult = SendResult.Disconnected;
                    return true;

                case SocketError.NoBufferSpaceAvailable:
                    sendResult = SendResult.SendBufferFull;
                    return true;
            }
            this.Connected = false;
            this.application.OnDisconnect(this, this.userData, DisconnectReason.ManagedDisconnect, string.Format(exception.Message, new object[0]), 0, 0, 0);
            sendResult = SendResult.Disconnected;
            return true;
        }

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
                    this.application.OnOutboundConnectionFailed(this, this.userData, (int)e.SocketError, string.Format("Connect failed: {0}", e.SocketError));
                }
                else
                {
                    Interlocked.Exchange(ref this.connected, 1);
                    this.application.OnOutboundConnectionEstablished(this, null, this.userData);
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs
                    {
                        RemoteEndPoint = e.RemoteEndPoint
                    };
                    args.SetBuffer(new byte[0x800], 0, 0x800);
                    args.Completed += new EventHandler<SocketAsyncEventArgs>(this.OnReceiveAsyncCompleted);
                    this.LocalEndPoint = (IPEndPoint)this.socketConnection.LocalEndPoint;
                    this.BeginReceive(this.socketConnection, args);
                    int dueTime = HttpSettings.Default.S2SPingFrequency;
                    this.pingTimer = new Timer(new TimerCallback(this.OnPingTimerElapsed), null, dueTime, dueTime);
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
                this.application.OnOutboundConnectionFailed(this, this.userData, (int)e.SocketError, string.Format("Connect failed: {0}", exception.Message));
            }
            finally
            {
                e.Dispose();
            }
        }

        public void OnPingResponse(int serverTime, int clienttime)
        {
        }

        private void OnPingTimerElapsed(object state)
        {
            if (this.SendPing() != SendResult.Ok)
            {
                this.pingTimer.Dispose();
            }
        }

        public void OnReceive(byte[] data, byte channelId, MessageReliablity reliablity)
        {
            this.application.OnReceive(this, this.userData, data, reliablity, channelId, MessageContentType.Binary, 0, 0, 0);
        }

        private void OnReceiveAsyncCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if ((e.LastOperation == SocketAsyncOperation.Disconnect) || (e.BytesTransferred == 0))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OnReceiveAsyncCompleted Disconnect: {0}", new object[] { e.SocketError });
                    }
                    if (this.Connected)
                    {
                        this.Connected = false;
                        e.Dispose();
                        this.application.OnDisconnect(this, this.userData, DisconnectReason.ManagedDisconnect, string.Empty, 0, 0, 0);
                    }
                }
                else if (e.SocketError != SocketError.Success)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("OnReceiveAsyncCompleted error: {0}", new object[] { e.SocketError });
                    }
                    if (this.Connected)
                    {
                        this.Connected = false;
                        e.Dispose();
                        this.application.OnDisconnect(this, this, DisconnectReason.ManagedDisconnect, string.Empty, 0, 0, 0);
                    }
                }
                else
                {
                    this.tcpReader.Parse(e.Buffer, e.BytesTransferred);
                    this.BeginReceive((Socket)sender, e);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException exception)
            {
                log.Warn(exception);
            }
            catch (Exception exception2)
            {
                log.Error(exception2);
            }
        }

        SendResults IPhotonPeer._InternalBroadcastSend(byte[] data, MessageReliablity reliability, byte channelId, MessageContentType messageContentType)
        {
            return SendResults.SentOk;
        }

        IntPtr IPhotonPeer._InternalGetPeerInfo(int why)
        {
            return IntPtr.Zero;
        }

        public SendResults Send(byte[] data, MessageReliablity reliability, byte channelId, MessageContentType messageContentType)
        {
            if (!this.socketConnection.Connected)
            {
                return SendResults.SendDisconnected;
            }
            int num = data.Length + 7;
            byte num2 = (byte)reliability;
            byte[] array = new byte[] { 0xfb, (byte)((num >> 0x18) & 0xff), (byte)((num >> 0x10) & 0xff), (byte)((num >> 8) & 0xff), (byte)(num & 0xff), channelId, num2 };
            List<ArraySegment<byte>> buffers = new List<ArraySegment<byte>>(2) {
            new ArraySegment<byte>(array),
            new ArraySegment<byte>(data)
        };
            this.socketConnection.Send(buffers);
            return SendResults.SentOk;
        }

        /// <summary>
        ///  Sends a ping request to the server.
        /// The ping request will be send with <see cref="P:System.Environment.TickCount"/> 
        /// as the tme stamp.
        /// </summary>
        /// <returns>
        /// Returns OK or disconnected.
        /// </returns>
        private SendResult SendPing()
        {
            return this.SendPing(Environment.TickCount);
        }

        /// <summary>
        /// Sends a ping request to the server.
        /// </summary>
        /// <param name="timeStamp">
        ///  A user definined time stamp. 
        ///   The time stamp will be send back by the server with the ping response
        ///   ans can be used to mesure the duration of the request.
        /// </param>
        /// <returns>
        /// Returns OK or disconnected.
        /// </returns>
        private SendResult SendPing(int timeStamp)
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

        public void SetDebugString(string debugString)
        {
        }

        public void SetUserData(object data)
        {
            Interlocked.Exchange(ref this.userData, data);
        }

        /// <summary>
        ///  Gets a value indicating whether this instance is connected to a remote host.
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

        public IPEndPoint LocalEndPoint { get; private set; }
    }
}
