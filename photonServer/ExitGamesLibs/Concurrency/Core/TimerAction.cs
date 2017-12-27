using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExitGames.Concurrency.Core
{
    internal class TimerAction : IDisposable
    {
        // Fields
        private readonly long _firstIntervalInMs;
        private readonly long _intervalInMs;
        private Action _action;

        private Timer _timer;
        private bool _cancelled;

        // Methods
        public void ExecuteOnTimerThread(ISchedulerRegistry registry)
        {
            if (_intervalInMs == Timeout.Infinite || _cancelled)
            {
                registry.Remove(this);
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
            if (!_cancelled)
            {
                registry.Enqueue(ExecuteOnFiberThread);
            }
        }

        public void ExecuteOnFiberThread()
        {
            if (!_cancelled)
            {
                _action();
            }
        }

        public TimerAction(Action action, long firstIntervalInMs, long intervalInMs)
        {
            _action = action;
            _firstIntervalInMs = firstIntervalInMs;
            _intervalInMs = intervalInMs;
        }

        public virtual void Dispose()
        {
            _cancelled = true;
        }

        public void Schedule(ISchedulerRegistry registry)
        {
            _timer = new Timer(x => ExecuteOnTimerThread(registry), null, _firstIntervalInMs, _intervalInMs);
        }
    }

}
