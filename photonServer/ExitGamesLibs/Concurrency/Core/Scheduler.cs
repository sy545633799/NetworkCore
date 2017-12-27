using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Enqueues actions on to context after schedule elapses
    /// </summary>
    public class Scheduler : IDisposable, IScheduler, ISchedulerRegistry
    {
        private volatile bool _running = true;
        private readonly IExecutionContext _executionContext;
        private List<IDisposable> _pending = new List<IDisposable>();

        private void AddPending(TimerAction pending)
        {
            Action addAction = delegate
            {
                if (_running)
                {
                    _pending.Add(pending);
                    pending.Schedule(this);
                }
            };
            _executionContext.Enqueue(addAction);
        }

        /// <summary>
        /// Constructs new instance.
        /// </summary>
        /// <param name="executionContext"></param>
        public Scheduler(IExecutionContext executionContext)
        {
            _executionContext = executionContext;
        }

        /// <summary>
        /// Cancels all pending actions
        /// </summary>
        public void Dispose()
        {
            _running = false;
            var old = Interlocked.Exchange(ref _pending, new List<IDisposable>());
            foreach (var timer in old)
            {
                timer.Dispose();
            }
        }

        /// <summary>
        /// Enqueues actions on to context immediately.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _executionContext.Enqueue(action);
        }

        public void Remove(IDisposable toRemove)
        {
            _executionContext.Enqueue(() => _pending.Remove(toRemove));
        }

        /// <summary>
        /// Enqueues action on to context after timer elapses. 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <returns></returns>
        public IDisposable Schedule(Action action, long firstInMs)
        {
            if (firstInMs <= 0)
            {
                var pending = new PendingAction(action);
                _executionContext.Enqueue(new Action(pending.Execute));
                return pending;
            }
            else
            {
                var pending = new TimerAction(action, firstInMs, Timeout.Infinite);
                AddPending(pending);
                return pending;
            }
        }

        /// <summary>
        /// Enqueues actions on to context after schedule elapses.  
        /// </summary>
        /// <param name="action"></param>
        /// <param name="firstInMs"></param>
        /// <param name="regularInMs"></param>
        /// <returns></returns>
        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            var pending = new TimerAction(action, firstInMs, regularInMs);
            AddPending(pending);
            return pending;
        }
    }
}
