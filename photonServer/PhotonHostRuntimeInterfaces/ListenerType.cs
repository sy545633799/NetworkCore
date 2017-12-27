using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    public enum ListenerType
    {
        ENetListener,
        TCPListener,
        WebSocketListener,
        PolicyListener,
        TCPChunkListener,
        UDPChunkListener,
        HTTPListener
    }
}
