using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class OperationContractAttribute : Attribute
    {
        public OperationContractAttribute()
        {
            Mode = OperationMode.Server;
        }

        public OperationContractAttribute(OperationMode mode)
        {
            Mode = mode;
        }

        public OperationMode Mode { get; private set; }
    }
}
