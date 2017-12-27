using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    public enum SendResults
    {
        SentOk,
        SendBufferFull,
        SendDisconnected,
        SendMsgTooBig,
        SendFailed,
        SendInvalidChannel,
        SendInvalidContentType
    }

}
