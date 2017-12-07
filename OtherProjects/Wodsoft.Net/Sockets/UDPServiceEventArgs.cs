using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace Wodsoft.Net.Sockets
{
    public class UDPServiceEventArgs : EventArgs
    {
        public IPEndPoint EndPoint { get; internal set; }

        public SocketAsyncOperation Operation { get; internal set; }

        public byte[] Data { get; internal set; }

        public int DataLength { get; internal set; }
    }
}
