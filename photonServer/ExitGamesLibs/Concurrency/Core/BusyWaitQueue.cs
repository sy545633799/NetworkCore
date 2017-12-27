using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Busy waits on lock to execute.  Can improve performance in certain situations.
    /// </summary>
    public class BusyWaitQueue : IQueue
    {
        private readonly object _lock;
        private readonly IExecutor _executor;
        private readonly int _spinsBeforeTimeCheck;
        private readonly int _msBeforeBlockingWait;

        private bool _running;

        private List<Action> _actions;
        private List<Action> _toPass;

        // Methods
        private bool TryBlockingWait(Stopwatch stopwatch, ref int spins)
        {
            if (spins++ >= _spinsBeforeTimeCheck)
            {
                spins = 0;
                if (stopwatch.ElapsedMilliseconds > _msBeforeBlockingWait)
                {
                    Monitor.Wait(_lock);
                    stopwatch.Restart();
                    return true;
                }
            }
            return false;
        }

        private List<Action> TryDequeue()
        {
            if (_actions.Count > 0)
            {
                Lists.Swap(ref _actions, ref _toPass);
                _actions.Clear();
                return _toPass;
            }
            return null;
        }

        /// <summary>
        /// BusyWaitQueue with default executor.
        /// </summary>
        /// <param name="spinsBeforeTimeCheck"></param>
        /// <param name="msBeforeBlockingWait"></param>
        public BusyWaitQueue(int spinsBeforeTimeCheck, int msBeforeBlockingWait)
            : this(new DefaultExecutor(), spinsBeforeTimeCheck, msBeforeBlockingWait)
        {
        }

        /// <summary>
        /// BusyWaitQueue with custom executor.
        /// </summary>
        /// <param name="executor"></param>
        /// <param name="spinsBeforeTimeCheck"></param>
        /// <param name="msBeforeBlockingWait"></param>
        public BusyWaitQueue(IExecutor executor, int spinsBeforeTimeCheck, int msBeforeBlockingWait)
        {
            _lock = new object();
            _running = true;
            _actions = new List<Action>();
            _toPass = new List<Action>();
            _executor = executor;
            _spinsBeforeTimeCheck = spinsBeforeTimeCheck;
            _msBeforeBlockingWait = msBeforeBlockingWait;
        }

        private List<Action> DequeueAll()
        {
            int spins = 0;
            Stopwatch stopwatch = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    while (!Monitor.TryEnter(_lock)) { }

                    if (!_running) break;
                    var toReturn = TryDequeue();
                    if (toReturn != null) return toReturn;

                    if (TryBlockingWait(stopwatch, ref spins))
                    {
                        if (!_running) break;
                        toReturn = TryDequeue();
                        if (toReturn != null) return toReturn;
                    }
                }
                finally
                {
                    Monitor.Exit(_lock);
                }
                Thread.Yield();
            }
            return null;
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

        private bool ExecuteNextBatch()
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
