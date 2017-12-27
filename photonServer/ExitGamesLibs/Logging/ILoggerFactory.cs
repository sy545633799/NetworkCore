using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Logging
{
    public interface ILoggerFactory
    {
        // Methods
        ILogger CreateLogger(string name);
    }

}
