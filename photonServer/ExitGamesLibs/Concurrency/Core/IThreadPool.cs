using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// A thread pool for executing asynchronous actions.
    /// </summary>
    public interface IThreadPool
    {
        /// <summary>
        /// Enqueue action for execution. 
        /// </summary>
        /// <param name="callback"></param>
        void Queue(WaitCallback callback);
    }
}
