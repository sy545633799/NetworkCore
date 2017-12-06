using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    public class ConnectFailedException : Exception
    {
        public ConnectFailedException(string errorMsg)
            : base(errorMsg)
        {
        }
    }
}
