using System.Collections.Generic;
using PhotonHostRuntimeInterfaces;

namespace Photon.SocketServer
{
    internal class BroadcastEventData
    {
        // Fields
        public readonly byte[] Data;
        public readonly MessageContentType MessageContentType;
        public readonly List<PeerBase> Peers;

        // Methods
        public BroadcastEventData(byte[] data, MessageContentType messageContentType)
        {
            this.Data = data;
            this.MessageContentType = messageContentType;
            this.Peers = new List<PeerBase>();
        }

        public IPhotonPeer[] GetUnmanagedPeers()
        {
            IPhotonPeer[] peerArray = new IPhotonPeer[this.Peers.Count];
            for (int i = 0; i < this.Peers.Count; i++)
            {
                peerArray[i] = this.Peers[i].UnmanagedPeer;
            }
            return peerArray;
        }
    }

}
