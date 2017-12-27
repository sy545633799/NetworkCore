using System;
using System.Collections.Generic;
using ExitGames.Concurrency.Core;
using ExitGames.Concurrency.Fibers;

namespace ExitGames.Concurrency.Channels
{
    /// <summary>
    /// Default Channel Implementation. Methods are thread safe.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Channel<T> : IPublisher<T>, ISubscriber<T>, IChannel<T>
    {
        private Action<T> _subscribers;
        private readonly object _lock = new object();

        /// <summary>
        /// Remove all subscribers. 
        /// </summary>
        public void ClearSubscribers()
        {
            _subscribers = null;
        }

        /// <summary>
        /// <see cref="M:ExitGames.Concurrency.Channels.IPublisher`1.Publish(`0)"/>
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public bool Publish(T msg)
        {
            Action<T> action = _subscribers;
            if (action != null)
            {
                action(msg);
                return true;
            }
            return false;
        }

        /// <summary>
        /// <see cref="M:ExitGames.Concurrency.Channels.ISubscriber`.Subscribe(ExitGames.Concurrency.Fibers.IFiber,System.Action{`0})"/>
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IFiber fiber, Action<T> receive)
        {
            return SubscribeOnProducerThreads(new ChannelSubscription<T>(fiber, receive));
        }

        /// <summary>
        /// Subscribes to actions on producer threads. Subscriber could be called from multiple threads. 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public IDisposable SubscribeOnProducerThreads(IProducerThreadSubscriber<T> subscriber)
        {
            return SubscribeOnProducerThreads(subscriber.ReceiveOnProducerThread, subscriber.Subscriptions);
        }

        /// <summary>
        /// Subscribes an action to be executed for every action posted to the channel. Action should be thread safe. Action may be invoked on multiple threads. 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="subscriptions"></param>
        /// <returns></returns>
        private IDisposable SubscribeOnProducerThreads(Action<T> subscriber, ISubscriptionRegistry subscriptions)
        {
            _subscribers += subscriber;

            var unsubscriber = new Unsubscriber<T>(subscriber, this, subscriptions);
            subscriptions.RegisterSubscription(unsubscriber);

            return unsubscriber;
        }

        /// <summary>
        /// <see cref="M:ExitGames.Concurrency.Channels.ISubscriber`1.SubscribeToBatch(ExitGames.Concurrency.Fibers.IFiber,System.Action{System.Collections.Generic.IList{`0},System.Int32)"/>
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IDisposable SubscribeToBatch(IFiber fiber, Action<IList<T>> receive, int intervalInMs)
        {
            return SubscribeOnProducerThreads(new BatchSubscriber<T>(fiber, receive, intervalInMs));
        }

        /// <summary>
        /// <see cref="M:ExitGames.Concurrency.Channels.Isuscriber`1.SubscribeToKeyedBatch``1(ExitGames.Concurrency.Fibers.IFiber,System.Converter{`0,``0},System.Action{System.Collections.Generic.IDictionary{``0,`0}},System.Int32"/>
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <param name="fiber"></param>
        /// <param name="keyResolver"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IDisposable SubscribeToKeyedBatch<K>(IFiber fiber, Converter<T, K> keyResolver, Action<IDictionary<K, T>> receive, int intervalInMs)
        {
            return SubscribeOnProducerThreads(new KeyedBatchSubscriber<K, T>(keyResolver, receive, fiber, intervalInMs));
        }

        /// <summary>
        /// Subscription that delivers the latest message to the consuming thread. If a newer message arrives before the consuming thread has a chance to process the message, the pending message is replaced by the newer message. The old message is discarded. 
        /// </summary>
        /// <param name="fiber"></param>
        /// <param name="receive"></param>
        /// <param name="intervalInMs"></param>
        /// <returns></returns>
        public IDisposable SubscribeToLast(IFiber fiber, Action<T> receive, int intervalInMs)
        {
            return SubscribeOnProducerThreads(new LastSubscriber<T>(receive, fiber, intervalInMs));
        }

        internal void Unsubscribe(Action<T> toUnsubscribe)
        {
            _subscribers -= toUnsubscribe;
        }

        public bool HasSubscriptions
        {
            get
            {
                return _subscribers != null;
            }
        }

        /// <summary>
        /// Number of subscribers 
        /// </summary>
        public int NumSubscribers
        {
            get
            {
                var evnt = _subscribers; // copy reference for thread safety
                return evnt == null ? 0 : evnt.GetInvocationList().Length;
            }
        }
    }
}
