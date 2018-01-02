using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using NetworkCore.IOCP.Common;

namespace NetworkCore.IOCP.Server
{
    public class UserToken : BaseClient
    {
        public UserToken(int num)
          : base(num)
        {
        }
    }
}
