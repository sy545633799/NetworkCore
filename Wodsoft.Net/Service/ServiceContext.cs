using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wodsoft.Net.Service
{
    public class ServiceContext
    {
        [ThreadStatic]
        internal static ServiceContext Context;

        public ServiceContext(ServiceUser user, ServiceChannel channel)
        {
            User = user;
            Channel = channel;
            Session = new ServiceSessionState();
        }

        public static ServiceContext Current
        {
            get
            {
                return Context;
            }
        }

        public ServiceChannel Channel { get; private set; }

        public ServiceSessionState Session { get; private set; }

        public ServiceUser User { get; private set; }
    }
}
