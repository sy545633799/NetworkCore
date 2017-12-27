using System;
using System.Collections.Generic;
using System.Threading;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Default implementation. 
    /// </summary>
    public class DefaultQueue : IQueue
    {
        private readonly object _lock;
        private readonly IExecutor _executor;
        private bool _running;
        private List<Action> _actions;
        private List<Action> _toPass;

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
            {
                Monitor.Wait(_lock);
            }
            return _running;
        }

        /// <summary>
        /// Default queue with default executor
        /// </summary>
        public DefaultQueue()
            : this(new DefaultExecutor())
        {
        }

        /// <summary>
        /// Default queue with custom executor
        /// </summary>
        /// <param name="executor"></param>
        public DefaultQueue(IExecutor executor)
        {
            _lock = new object();
            _running = true;
            _actions = new List<Action>();
            _toPass = new List<Action>();
            _executor = executor;
        }

        private List<Action> DequeueAll()
        {
            lock (_lock)
            {
                if (ReadyToDequeue())
                {
                    Lists.Swap(ref this._actions, ref this._toPass);
                    _actions.Clear();
                    return _toPass;
                }
                return null;
            }
        }

        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _actions.Add(action);
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Remove all actions and execute.
        /// </summary>
        /// <returns></returns>
        public bool ExecuteNextBatch()
        {
            List<Action> toExecute = DequeueAll();
            if (toExecute == null)
            {
                return false;
            }
            _executor.Execute(toExecute);
            return true;
        }

        /// <summary>
        /// Execute actions until stopped.
        /// </summary>
        public void Run()
        {
            while (ExecuteNextBatch())
            {
            }
        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                _running = false;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
