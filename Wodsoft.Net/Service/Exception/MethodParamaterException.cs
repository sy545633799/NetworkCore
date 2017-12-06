using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Wodsoft.Net.Service
{
    public class MethodParamaterException : Exception
    {
        public MethodParamaterException(string channelName, string methodName, IPEndPoint endPoint, object[] args)
            : base("方法的参数错误。")
        {
            Source = endPoint.ToString() + "/" + channelName + "/" + methodName;
            Paramaters = args;
        }

        public object[] Paramaters { get; private set; }
    }
}
