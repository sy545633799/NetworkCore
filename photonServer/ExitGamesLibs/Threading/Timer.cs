using System;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Concurrency.Fibers;
using ExitGames.Logging;

namespace ExitGames.Threading
{
    /// <summary>
    /// This class is a manager for scheduled callbacks. 
    /// </summary>
    public class Timer
    {
        /// <summary>
        /// The logger.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// All existing timers.
        /// </summary>
        private readonly Dictionary<Guid, IDisposable> timers = new Dictionary<Guid, IDisposable>();

        /// <summary>
        /// The execution fiber.
        /// </summary>
        private PoolFiber fiber = new PoolFiber();

        /// <summary>
        /// Indicates whether the timer is still running.
        /// </summary>
        private bool running = false;

        /// <summary>
        /// Schedules a callback for a specific time.
        /// </summary>
        /// <param name="utcExecutionTime"> 
        /// The execution time in UTC.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        /// <returns>
        /// An ID that can be used to abort the timer with <see cref="M:ExitGames.Threading.Timer.RemoveAction(System.Guid)"/>.
        /// </returns>
        public Guid AddAction(DateTime utcExecutionTime, Action callback)
        {
            Guid id = Guid.NewGuid();
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("AddAction - enqueued adding '{0}.{1}' with id {2} ", callback, callback, id);
            }
            fiber.Enqueue(() => DoAddAction(id, utcExecutionTime, callback));
            return id;
        }

        /// <summary>
        /// Aborts all timers.
        /// </summary>
        public void ClearActions()
        {
            fiber.Enqueue(DoClearActions);
        }

        /// <summary>
        /// Immediately invoks an async callback.
        /// </summary>
        /// <param name="callback"> 
        /// The callback to invoke.
        /// </param>
        public void ExecuteAction(Action callback)
        {
            callback.BeginInvoke(new AsyncCallback(ExecutionEndCallback), null);
        }

        /// <summary>
        /// Removes a scheduled action that has been added with <see cref="M:ExitGames.Threading.Timer.AddAction(System.DateTime,System.Action)"/>.
        /// </summary>
        /// <param name="id">
        /// The action id assigned by <see cref="M:ExitGames.Threading.Timer.AddAction(System.DateTime,System.Action)"/>.
        /// </param>
        public void RemoveAction(Guid id)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("RemoveAction - enqueued removal of id {0}", id);
            }
            fiber.Enqueue(() => DoRemoveAction(id));
        }

        /// <summary>
        /// Start executing callbacks.
        /// </summary>       
        public void Start()
        {
            fiber.Start();
            running = true;
        }

        /// <summary>
        ///  Stops all timers.
        /// </summary>
        public void Stop()
        {
            fiber.Dispose();
            fiber = new PoolFiber();
            running = false;
        }

        /// <summary>
        /// Callback for all async executions.
        /// </summary>
        /// <param name="ar">
        /// The async result.
        /// </param>
        private static void ExecutionEndCallback(IAsyncResult ar)
        {
            try
            {
                //Action action = (Action) .~((AsyncResult) ar);
                //.~(action, ar);
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        /// <summary>
        /// Adds an action. Running on the fiber. 
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="utcExecutionTime">
        /// The utc execution time.
        /// </param>
        /// <param name="callback">
        /// The callback.
        /// </param>
        private void DoAddAction(Guid id, DateTime utcExecutionTime, Action callback)
        {
            try
            {
                long totalMilliseconds = (long)utcExecutionTime.Subtract(DateTime.UtcNow).TotalMilliseconds;
                if (totalMilliseconds < 1L)
                {
                    ExecuteAction(callback);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("DoAddAction - invoked action '{0}.{1}' with id {2}", callback, callback, id);
                    }
                }
                else
                {
                    Action action = () => ExecuteTimerAction(callback, id);

                    IDisposable disposable = fiber.Schedule(action, totalMilliseconds);
                    timers.Add(id, disposable);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("DoAddAction - added timer for action '{0}.{1}' at {2}.{3} with id {4}", callback, callback, utcExecutionTime, utcExecutionTime.Millisecond, id);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        /// <summary>
        ///  Clears all actions. Running on the fiber.
        /// </summary>
        private void DoClearActions()
        {
            try
            {
                foreach (KeyValuePair<Guid, IDisposable> pair in timers)
                {
                    pair.Value.Dispose();
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("DoClearActions - removed timer with id {0}", new object[] { pair.Key });
                    }
                }
                timers.Clear();
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        /// <summary>
        /// Removes an action. Running on the fiber.
        /// </summary>
        /// <param name="id">
        /// The action id.
        /// </param>
        private void DoRemoveAction(Guid id)
        {
            try
            {
                IDisposable disposable;
                if (timers.TryGetValue(id, out disposable))
                {
                    disposable.Dispose();
                    timers.Remove(id);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("DoRemoveAction - removed timer with id {0}", id);
                    }
                }
                else if (log.IsDebugEnabled)
                {
                    log.DebugFormat("DoRemoveAction - timer removal with id {0} failed", id);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        /// <summary>
        ///  Executes a scheduled action. Runs on the fiber.
        /// </summary>
        /// <param name="callback"> 
        /// The scheduled action.
        /// </param>
        /// <param name="id">
        /// The action id.
        /// </param>
        private void ExecuteTimerAction(Action callback, Guid id)
        {
            try
            {
                if (timers.Remove(id))
                {
                    ExecuteAction(callback);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("ExecuteTimerAction '{0}.{1}' with id {2}", callback, callback, id);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                log.Error(exception);
            }
        }

        /// <summary>
        /// Gets a value indicating whether Running.
        /// </summary>
        public bool Running
        {
            get
            {
                return running;
            }
        }
    }
}
