using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    public enum MessageReliablity
    {
        Flush = 0x10,
        Reliable = 3,
        UnReliable = 2,
        UnSequenced = 1
    }

}
