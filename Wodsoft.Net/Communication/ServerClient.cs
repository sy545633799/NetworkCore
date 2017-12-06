using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wodsoft.Net.Sockets;

namespace Wodsoft.Net.Communication
{
    public class ServerClient : CommunicationBase
    {
        public ServerClient(ISocket socket) : base(socket) { }

        public Credential Credential { get; internal set; }

        public bool IsAuthenticated { get; internal set; }

        public override bool IsConnected
        {
            get
            {
                return base.IsConnected && IsAuthenticated;
            }
        }
    }
}
