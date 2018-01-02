using NetworkCore.IOCP.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace NetworkCore.IOCP.Events
{
    public class MessageEventArgs: EventArgs
    {
        public byte[] Data { get; private set; }
        public int DataLenth { get; private set; }
        public BaseClient UserToken { get;internal set; }

        public MessageEventArgs()
        {
            Data = new byte[1024 * 1024];
        }

        internal void SetBuffer(Array sourceArray, int offset, int length)
        {
            DataLenth = length;
            Array.Copy(sourceArray, offset, Data, 0, length);
        }
    }
}
