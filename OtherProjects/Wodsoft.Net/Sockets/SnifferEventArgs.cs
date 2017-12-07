using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Sockets
{
    public class SnifferEventArgs : EventArgs
    {
        public SnifferEventArgs(byte[] buffer)
        {
            Buffer = buffer;
        }

        public byte[] Buffer { get; private set; }

        public bool Handle { get; set; }

        public SnifferPacket Packet { get; internal set; }
    }
}
