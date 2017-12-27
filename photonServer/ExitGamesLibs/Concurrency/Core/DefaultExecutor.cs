using System;
using System.Collections.Generic;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Default executor. 
    /// </summary>
    public class DefaultExecutor : IExecutor
    {
        // Fields
        private bool _running = true;

        /// <summary>
        /// Executes a single action. 
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(Action toExecute)
        {
            if (_running)
            {
                toExecute();
            }
        }

        /// <summary>
        /// Executes all actions.
        /// </summary>
        /// <param name="toExecute"></param>
        public void Execute(List<Action> toExecute)
        {
            foreach (Action action in toExecute)
            {
                Execute(action);
            }
        }

        /// <summary>
        /// When disabled, actions will be ignored by executor. The executor is typically disabled at shutdown
        /// to prevent any pending actions from being executed. 
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _running;
            }
            set
            {
                _running = value;
            }
        }
    }
}
