using System;
using ExitGames.Concurrency.Core;
using ExitGames.Concurrency.Fibers;

namespace ExitGames.Concurrency.Channels
{
    /// <summary>
    /// Subscribes to last action received on the channel. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LastSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();
        private readonly IFiber _fiber;
        private readonly Action<T> _target;
        private readonly int _intervalInMs;
        private bool _flushPending;
        private T _pending;

        // Methods
        private T ClearPending()
        {
            lock (_batchLock)
            {
                _flushPending = false;
                return _pending;
            }
        }

        /// <summary>
        /// New instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="fiber"></param>
        /// <param name="flushIntervalInMs"></param>
        public LastSubscriber(Action<T> target, IFiber fiber, int flushIntervalInMs)
        {
            _batchLock = new object();
            _fiber = fiber;
            _target = target;
            _intervalInMs = flushIntervalInMs;
        }

        private void Flush()
        {
            T local = ClearPending();
            _target(local);
        }

        /// <summary>
        ///  Receives message from producer thread.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (_batchLock)
            {
                if (!_flushPending)
                {
                    _fiber.Schedule(Flush, _intervalInMs);
                    _flushPending = true;
                }
                _pending = msg;
            }
        }

        /// <summary>
        /// Allows for the registration and deregistration of subscriptions
        /// </summary>
        public override ISubscriptionRegistry Subscriptions
        {
            get
            {
                return _fiber;
            }
        }
    }
}
