using System;
using System.Collections.Generic;
using System.Threading;
using ExitGames.Concurrency.Core;
using ExitGames.Diagnostics.Counter;

namespace ExitGames.Concurrency.Fibers
{
    public class PoolFiber : IScheduler, IDisposable, IExecutionContext, IFiber, ISubscriptionRegistry
    {
        // Fields
        private readonly Subscriptions _subscriptions = new Subscriptions();
        private readonly object _lock = new object();
        private readonly IThreadPool _pool;
        private readonly Scheduler _timer;
        private readonly IExecutor _executor;

        private List<Action> _queue = new List<Action>();
        private List<Action> _toPass = new List<Action>();

        private ExecutionState _started = ExecutionState.Created;
        private bool _flushPending;

        public PoolFiber(IThreadPool pool, IExecutor executor)
        {

            _timer = new Scheduler(this);
            _pool = pool;
            _executor = executor;
            CounterDequeue = Counter.Instance;
            CounterEnqueue = Counter.Instance;
        }

        public PoolFiber(IExecutor executor)
            : this(new DefaultThreadPool(), executor)
        {
        }

        public PoolFiber()
            : this(new DefaultThreadPool(), new DefaultExecutor())
        {
        }

        public void Enqueue(Action action)
        {
            lock (_lock)
            {
                _queue.Add(action);
                if (_started == ExecutionState.Created)
                {
                    return;
                }
                if (!_flushPending)
                {
                    _pool.Queue(new WaitCallback(Flush));
                    _flushPending = true;
                }
            }
            CounterEnqueue.Increment();
        }

        public void RegisterSubscription(IDisposable toAdd)
        {
            _subscriptions.Add(toAdd);
        }

        public bool DeregisterSubscription(IDisposable toRemove)
        {
            return _subscriptions.Remove(toRemove);
        }

        public int NumSubscriptions
        {
            get
            {
                return _subscriptions.Count;
            }
        }

        private void Flush(object state)
        {
            var toExecute = ClearActions();
            if (toExecute != null)
            {
                _executor.Execute(toExecute);
                lock (_lock)
                {
                    if (_queue.Count > 0)
                    {
                        _pool.Queue(Flush);
                    }
                    else
                    {
                        _flushPending = false;
                    }
                }
                CounterDequeue.IncrementBy((long)toExecute.Count);
            }
        }

        private List<Action> ClearActions()
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flushPending = false;
                    return null;
                }
                Lists.Swap(ref _queue, ref _toPass);
                _queue.Clear();
                return _toPass;
            }
        }

        public IDisposable Schedule(Action action, long firstInMs)
        {
            return _timer.Schedule(action, firstInMs);
        }

        public IDisposable ScheduleOnInterval(Action action, long firstInMs, long regularInMs)
        {
            return _timer.ScheduleOnInterval(action, firstInMs, regularInMs);
        }

        public void Start()
        {
            if (_started == ExecutionState.Running)
            {
                throw new ThreadStateException("Already Started");
            }
            _started = ExecutionState.Running;
            Enqueue(() => { });
        }

        public void Stop()
        {
            _timer.Dispose();
            _started = ExecutionState.Stopped;
            _subscriptions.Dispose();
        }

        public void Dispose()
        {
            Stop();
        }

        // Properties
        public ICounter CounterDequeue { get; set; }

        public ICounter CounterEnqueue { get; set; }

        private class Counter : ICounter
        {
            // Fields
            public static readonly ICounter Instance = new PoolFiber.Counter();

            // Methods
            public long Decrement()
            {
                return 0L;
            }

            public CounterType CounterType
            {
                get
                {
                    return CounterType.Undefined;
                }
            }

            public string Name
            {
                get
                {
                    return string.Empty;
                }
            }

            public float GetNextValue()
            {
                return 0f;
            }

            public long Increment()
            {
                return 0L;
            }

            public long IncrementBy(long value)
            {
                return 0L;
            }
        }
    }
}
