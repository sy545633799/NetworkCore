using System;
using System.Threading;
using System.Web;
using System.Web.Caching;
using ExitGames.Logging;
using ExitGames.Threading;

namespace Photon.SocketServer.Web
{
    internal class PeerCache
    {
        // Fields
        private static int currentConnectionCount;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        // Events
        public event Action<PhotonHttpPeer> OnPeerExpired;

        // Methods
        public PeerCache()
        {
            this.PeerExpiration = TimeSpan.FromMinutes(5.0);
        }

        private void OnPeerRemoveCallBack(string key, object value, CacheItemRemovedReason reason)
        {
            PhotonHttpPeer peer = (PhotonHttpPeer)value;
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Removed peer: id={0}, reason={1}", new object[] { peer.GetConnectionID(), reason });
            }
            if ((reason == CacheItemRemovedReason.Expired) || (reason == CacheItemRemovedReason.Underused))
            {
                Interlocked.Decrement(ref currentConnectionCount);
                Action<PhotonHttpPeer> onPeerExpired = this.OnPeerExpired;
                if (onPeerExpired != null)
                {
                    onPeerExpired(peer);
                }
            }
        }

        public bool RemovePeer(string connectionId)
        {
            return (HttpRuntime.Cache.Remove(connectionId) != null);
        }

        public bool TryAddPeer(PhotonHttpPeer photonPeer)
        {
            using (WriteLock.Enter(this.readerWriterLock))
            {
                if (HttpRuntime.Cache.Get(photonPeer.PeerId) is PhotonHttpPeer)
                {
                    return false;
                }
                HttpRuntime.Cache.Add(photonPeer.PeerId, photonPeer, null, Cache.NoAbsoluteExpiration, this.PeerExpiration, CacheItemPriority.Normal, new CacheItemRemovedCallback(this.OnPeerRemoveCallBack));
                return true;
            }
        }

        public bool TryGetPeer(HttpContext context, string connectionId, out PhotonHttpPeer peer)
        {
            using (ReadLock.Enter(this.readerWriterLock))
            {
                peer = context.Cache.Get(connectionId) as PhotonHttpPeer;
            }
            return (peer != null);
        }

        /// <summary>
        /// Gets or sets the time span after which a peer will be removed from the cache due inactivity.
        ///   The default value is 5 minutes.
        /// </summary>
        public TimeSpan PeerExpiration { get; set; }
    }
}
