using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Holds on to actions until the execution context can process them. 
    /// </summary>
    public interface IQueue
    {
        /// <summary>
        /// Enqueues action for execution context to process. 
        /// </summary>
        /// <param name="action"></param>
        void Enqueue(Action action);
        /// <summary>
        /// Start consuming actions. 
        /// </summary>
        void Run();
        /// <summary>
        /// Stop consuming actions. 
        /// </summary>
        void Stop();
    }
}
