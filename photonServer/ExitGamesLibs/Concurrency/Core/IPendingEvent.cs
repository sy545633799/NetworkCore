using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// A scheduled event. 
    /// </summary>
    public interface IPendingEvent : IDisposable
    {
        /// <summary>
        /// Execute this event and optionally schedule another execution. 
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        IPendingEvent Execute(long currentTime);

        /// <summary>
        /// Time of expiration for this event 
        /// </summary>
        long Expiration { get; }
    }
}
