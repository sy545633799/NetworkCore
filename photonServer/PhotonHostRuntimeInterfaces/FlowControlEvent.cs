using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotonHostRuntimeInterfaces
{
    public enum FlowControlEvent
    {
        FlowControlAllOk,
        FlowControlBuffering,
        FlowControlBufferSpaceLow,
        FlowControlSendsDenied
    }

}
