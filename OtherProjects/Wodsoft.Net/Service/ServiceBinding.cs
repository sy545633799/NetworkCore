using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    public class ServiceBinding
    {
        public ServiceBinding(int mainPort)
        {
            if (mainPort < 1 || mainPort > 65535)
                throw new ArgumentOutOfRangeException("mainPort为非法端口。");
            MainPort = mainPort;
        }

        public ServiceBinding(int mainPort, int[] subPorts)
            : this(mainPort)
        {
            foreach (var port in subPorts)
                if (port < 1 || port > 65535)
                    throw new ArgumentOutOfRangeException("subPorts存在非法端口。");
        }

        public ServiceBinding(int mainPort, SecurityMode securityMode)
            : this(mainPort)
        {
            SecurityMode = securityMode;
        }

        public ServiceBinding(int mainPort, int[] subPorts, SecurityMode securityMode)
            : this(mainPort, subPorts)
        {
            SecurityMode = securityMode;
        }

        public int MainPort { get; private set; }

        public int[] SubPorts { get; private set; }

        public SecurityMode SecurityMode { get; private set; }
    }
}
