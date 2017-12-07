using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Wodsoft.Net.Service
{
    public class InstanceProxy
    {
        private List<MethodInfo> Methods;
        private InvokeMethodDelegate InvokeMethod;
        private ServiceChannel Channel;

        internal InstanceProxy(ServiceChannel channel, List<MethodInfo> methods, InvokeMethodDelegate invokeMethod)
        {
            Channel = channel;
            Methods = methods;
            InvokeMethod = invokeMethod;
        }

        public object Invoke(MethodInfo method, params object[] args)
        {
            if (!Methods.Contains(method))
                throw new ArgumentException("方法不属于此实例。");
            return InvokeMethod(Channel, method, args);
        }
    }

    public delegate object InvokeMethodDelegate(ServiceChannel channel, MethodInfo method, object[] args);
}
