using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Wodsoft.Net.Service
{
    public class ChannelNotExistException : Exception
    {
        public ChannelNotExistException(string channelName, IPEndPoint endPoint)
            : base("频道不存在。")
        {
            Source = endPoint.ToString() + "/" + channelName;
        }
    }
}
