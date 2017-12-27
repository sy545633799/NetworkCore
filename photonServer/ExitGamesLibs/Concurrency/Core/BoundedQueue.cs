using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Queue with bounded capacity.  Will throw exception if capacity does not recede prior to wait time.
    /// </summary>
    public class BoundedQueue : IQueue
    {
        private readonly object _lock;
        private readonly IExecutor _executor;

        private bool _running;

        private List<Action> _actions;
        private List<Action> _toPass;

        /// <summary>
        /// Max number of actions to be queued.
        /// </summary>
        public int MaxDepth { get; set; }

        /// <summary>
        /// Max time to wait for space in the queue.
        /// </summary>
        public int MaxEnqueueWaitTimeInMs { get; set; }

        private bool SpaceAvailable(int toAdd)
        {
            if (!_running)
            {
                return false;
            }
            while (MaxDepth > 0 && _actions.Count + toAdd > MaxDepth)
            {
                if (MaxEnqueueWaitTimeInMs <= 0)
                {
                    throw new QueueFullException(_actions.Count);
                }
                Monitor.Wait(_lock, MaxEnqueueWaitTimeInMs);
                if (!_running)
                {
                    return false;
                }
                if (MaxDepth > 0 && _actions.Count + toAdd > MaxDepth)
                {
                    throw new QueueFullException(_actions.Count);
                }
            }
            return true;
        }

        private bool ReadyToDequeue()
        {
            while (_actions.Count == 0 && _running)
            {
                Monitor.Wait(_lock);
            }
            if (!_running)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates a bounded queue with the default executor.
        /// </summary>
        public BoundedQueue()
            : this(new DefaultExecutor())
        {
        }

        /// <summary>
        /// Creates a bounded queue with a custom executor.
        /// </summary>
        /// <param name="executor"></param>
        public BoundedQueue(IExecutor executor)
        {
            _lock = new object();
            _running = true;
            MaxDepth = -1;
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
                    Lists.Swap(ref _actions, ref _toPass);
                    _actions.Clear();

                    Monitor.PulseAll(_lock);
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
                if (SpaceAvailable(1))
                {
                    _actions.Add(action);
                    Monitor.PulseAll(_lock);
                }
            }
        }

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
