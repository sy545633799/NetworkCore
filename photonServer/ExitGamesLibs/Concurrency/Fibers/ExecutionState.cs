using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Concurrency.Fibers
{
    ///<summary>
    /// Fiber execution state management
    ///</summary>
    public enum ExecutionState
    {
        ///<summary>
        /// Created but not running
        ///</summary>
        Created,
        ///<summary>
        /// After start
        ///</summary>
        Running,
        ///<summary>
        /// After stopped
        ///</summary>
        Stopped
    }
}
