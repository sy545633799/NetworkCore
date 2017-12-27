using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer.Web
{
    internal class PhotonHttpPeer : IPhotonPeer
    {
        // Fields
        private readonly HttpApplicationHandler application;
        private readonly int connectionId;
        private static int connectionIdCounter;
        internal InvocationCache InvocationCache = new InvocationCache();
        private readonly string localAddress;
        private readonly ushort localPort;
        public readonly string PeerId;
        private readonly Queue<byte[]> queue = new Queue<byte[]>();
        private readonly string remoteAddress;
        private object userData;

        // Methods
        internal PhotonHttpPeer(string peerId, HttpApplicationHandler application, HttpContext context)
        {
            this.application = application;
            this.PeerId = peerId;
            this.connectionId = Interlocked.Increment(ref connectionIdCounter);
            this.localAddress = context.Request.Url.AbsoluteUri;
            this.localPort = (ushort)context.Request.Url.Port;
            this.remoteAddress = context.Request.UserHostAddress;
        }

        public void AbortClient()
        {
            this.application.DisconnectPeer(this);
        }

        public List<byte[]> DequeueAll()
        {
            List<byte[]> list = new List<byte[]>();
            lock (this.queue)
            {
                while (this.queue.Count > 0)
                {
                    list.Add(this.queue.Dequeue());
                }
            }
            return list;
        }

        public void DisconnectClient()
        {
            this.application.DisconnectPeer(this);
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
            return this.localAddress;
        }

        public ushort GetLocalPort()
        {
            return this.localPort;
        }

        public PeerType GetPeerType()
        {
            return PeerType.ENetPeer;
        }

        public string GetRemoteIP()
        {
            return this.remoteAddress;
        }

        public ushort GetRemotePort()
        {
            return 0;
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
            lock (this.queue)
            {
                this.Queue.Enqueue(data);
            }
            return SendResults.SentOk;
        }

        public void SetDebugString(string debugString)
        {
        }

        public void SetUserData(object value)
        {
            Interlocked.Exchange(ref this.userData, value);
        }

        // Properties
        internal Queue<byte[]> Queue
        {
            get
            {
                return this.queue;
            }
        }
    }
}
