using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ServiceModeAttribute : Attribute
    {
        public ServiceModeAttribute(ServiceMode mode)
        {
            Mode = mode;
        }

        public ServiceMode Mode { get; private set; }
    }
}
