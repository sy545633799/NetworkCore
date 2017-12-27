using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    public enum PeerType
    {
        ENetPeer,
        TCPPeer,
        WebSocketPeer,
        XMLPeer,
        TCPChunkPeer,
        UDPChunkPeer,
        S2SPeer,
        TCPMUXPeer,
        ENetOutboundPeer,
        WebSocketOutboundPeer,
        HTTPPeer
    }

}
