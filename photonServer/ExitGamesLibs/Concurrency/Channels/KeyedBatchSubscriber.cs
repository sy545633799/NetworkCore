using System;
using System.Collections.Generic;
using ExitGames.Concurrency.Core;
using ExitGames.Concurrency.Fibers;

namespace ExitGames.Concurrency.Channels
{
    /// <summary>
    /// Channel subscription that drops duplicates based upon a key.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class KeyedBatchSubscriber<K, T> : BaseSubscription<T>
    {
        private readonly object _batchLock = new object();
        private readonly IFiber _fiber;
        private readonly Action<IDictionary<K, T>> _target;
        private readonly int _intervalInMs;
        private readonly Converter<T, K> _keyResolver;
        private Dictionary<K, T> _pending;

        private IDictionary<K, T> ClearPending()
        {
            lock (_batchLock)
            {
                if ((_pending == null) || (_pending.Count == 0))
                {
                    _pending = null;
                    return null;
                }
                IDictionary<K, T> toReturn = _pending;
                _pending = null;
                return toReturn;
            }

        }

        /// <summary>
        /// Construct new instance.
        /// </summary>
        /// <param name="keyResolver"></param>
        /// <param name="target"></param>
        /// <param name="fiber"></param>
        /// <param name="flushIntervalInMs"></param>
        public KeyedBatchSubscriber(Converter<T, K> keyResolver, Action<IDictionary<K, T>> target, IFiber fiber, int flushIntervalInMs)
        {
            _keyResolver = keyResolver;
            _fiber = fiber;
            _target = target;
            _intervalInMs = flushIntervalInMs;
        }

        public void Flush()
        {
            IDictionary<K, T> dictionary = ClearPending();
            if (dictionary != null)
            {
                _target(dictionary);
            }
        }

        /// <summary>
        /// received on delivery thread
        /// </summary>
        /// <param name="msg"></param>
        protected override void OnMessageOnProducerThread(T msg)
        {
            lock (_batchLock)
            {
                K key = _keyResolver(msg);
                if (_pending == null)
                {
                    _pending = new Dictionary<K, T>();
                    _fiber.Schedule(Flush, _intervalInMs);
                }
                _pending[key] = msg;
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
