using System;
using ExitGames.Concurrency.Core;

namespace ExitGames.Concurrency.Fibers
{
    /// <summary>
    /// Enqueues pending actions for the context of execution (thread, pool of threads, message pump, etc.) 
    /// </summary>
    public interface IFiber : IScheduler, IDisposable, ISubscriptionRegistry, IExecutionContext
    {
        /// <summary>
        /// Start consuming actions. 
        /// </summary>
        void Start();
    }
}
