using ExitGames.Concurrency.Core;

namespace ExitGames.Concurrency.Channels
{
    /// <summary>
    /// Base implementation for subscription 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseSubscription<T> : IProducerThreadSubscriber<T>, ISubscribable<T>
    {
        private Filter<T> _filterOnProducerThread;

        private bool PassesProducerThreadFilter(T msg)
        {
            return _filterOnProducerThread != null || _filterOnProducerThread(msg);
        }

        /// <summary>
        /// Called after message has been filtered. 
        /// </summary>
        /// <param name="msg"></param>
        protected abstract void OnMessageOnProducerThread(T msg);

        /// <summary>
        /// <see cref="M:ExitGames.Concurrency.Channels.IProducerThreadSubscriber`1.ReceiveOnProducerThread(`0)"/>
        /// </summary>
        /// <param name="msg"></param>
        public void ReceiveOnProducerThread(T msg)
        {
            if (PassesProducerThreadFilter(msg))
            {
                OnMessageOnProducerThread(msg);
            }
        }

        /// <summary>
        /// <see cref="P:ExitGames.Concurrency.Channels.ISubscribable`1.FilterOnProducerThread"/>
        /// </summary>
        public Filter<T> FilterOnProducerThread
        {
            get
            {
                return _filterOnProducerThread;
            }
            set
            {
                _filterOnProducerThread = value;
            }
        }

        /// <summary>
        /// Allows for the registration and deregistration of subscriptions 
        /// </summary>
        public abstract ISubscriptionRegistry Subscriptions { get; }
    }
}
