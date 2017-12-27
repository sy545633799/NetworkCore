using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Executes pending action(s). 
    /// </summary>
    public interface IExecutor
    {
        /// <summary>
        /// Executes a single action. 
        /// </summary>
        /// <param name="toExecute">
        /// </param>
        void Execute(Action toExecute);
        /// <summary>
        /// Executes all actions. 
        /// </summary>
        /// <param name="toExecute"></param>
        void Execute(List<Action> toExecute);
    }
}
