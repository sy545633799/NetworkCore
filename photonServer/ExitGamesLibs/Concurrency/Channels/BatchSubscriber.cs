using System;
using System.Collections.Generic;
using ExitGames.Concurrency.Core;
using ExitGames.Concurrency.Fibers;

namespace ExitGames.Concurrency.Channels
{
    public class BatchSubscriber<T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();
        private readonly IFiber _fiber;
        private readonly Action<IList<T>> _receive;
        private readonly long _intervalInMs;
        private List<T> _pending;

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="interval"></param>
        public BatchSubscriber(IFiber fiber, Action<IList<T>> receive, long intervalInMs)
        {
            _fiber = fiber;
            _receive = receive;
            _intervalInMs = intervalInMs;
        }

        private void Flush()
        {
            IList<T> toFlush = null;
            lock (_batchLock)
            {
                if (_pending != null)
                {
                    toFlush = _pending;
                    _pending = null;
                }
            }
            if (toFlush != null)
            {
                _receive(toFlush);
            }
        }

        /// <summary>
        /// Receives message and batches as needed.
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (_batchLock)
            {
                if (_pending == null)
                {
                    _pending = new List<T>();
                    _fiber.Schedule(Flush, _intervalInMs);
                }
                _pending.Add(msg);
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
