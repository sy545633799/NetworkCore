using System;
using ExitGames.Concurrency.Channels;
using ExitGames.Concurrency.Core;
using ExitGames.Diagnostics.Counter;

namespace Photon.SocketServer.Concurrency
{
    /// <summary>
    /// Subclass of <see cref="T:ExitGames.Concurrency.Channels.Channel`1">ExitGames.Concurrency.Channels.Channel&lt;T&gt;</see>.
    /// Uses an <see cref="T:ExitGames.Diagnostics.Counter.ICounter">ExitGames.Diagnostics.Counter.ICounter</see> to track the amount of published messages.
    /// </summary>
    /// <typeparam name="T">The type of message published in this channel.</typeparam>
    public class MessageChannel<T> : Channel<T>, IDisposable
    {
        /// <summary>
        /// The counter.
        /// </summary>
        private readonly IDisposable counter;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Concurrency.MessageChannel`1"/> class.
        /// </summary>
        /// <param name="publishCounter">The counter to track the amount of published messages.</param>
        public MessageChannel(ICounter publishCounter)
        {
            this.counter = base.SubscribeOnProducerThreads(new Executor(publishCounter));
        }

        /// <summary>
        /// Calls <see cref="M:Photon.SocketServer.Concurrency.MessageChannel`1.Dispose(System.Boolean)"/>.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the counter and clears all subscribers.
        /// </summary>
        /// <param name="disposing">Indicates wheter called from <see cref="M:Photon.SocketServer.Concurrency.MessageChannel`1.Dispose"/> or from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.counter.Dispose();
                base.ClearSubscribers();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="T:Photon.SocketServer.Concurrency.MessageChannel`1"/> class.
        /// </summary>
        ~MessageChannel()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This class counts the number of published messages.
        /// </summary>
        private sealed class Executor : IProducerThreadSubscriber<T>, ISubscriptionRegistry
        {
            /// <summary>
            /// The counter.
            /// </summary>
            private readonly ICounter counter;

            /// <summary>
            ///  Initializes a new instance of the <see cref="T:Photon.SocketServer.Concurrency.MessageChannel`1.Executor"/> class.
            /// </summary>
            /// <param name="counter">The counter.</param>
            public Executor(ICounter counter)
            {
                this.counter = counter;
            }

            /// <summary>
            /// Deregister Subscription - does nothing.
            /// </summary>
            /// <param name="toRemove"> The to remove.</param>
            /// <returns> Always false.</returns>
            public bool DeregisterSubscription(IDisposable toRemove)
            {
                return false;
            }

            /// <summary>
            /// Receive message on producer thread.
            /// </summary>
            /// <param name="msg">  The msg.</param>
            public void ReceiveOnProducerThread(T msg)
            {
                this.counter.Increment();
            }

            /// <summary>
            /// Register Subscription - does nothing.
            /// </summary>
            /// <param name="toAdd"> The to add.</param>
            public void RegisterSubscription(IDisposable toAdd)
            {
            }

            /// <summary>
            /// Gets Subscriptions.
            /// </summary>
            public ISubscriptionRegistry Subscriptions
            {
                get
                {
                    return this;
                }
            }
        }
    }
}
