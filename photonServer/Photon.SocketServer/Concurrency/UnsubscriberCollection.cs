using System;

namespace Photon.SocketServer.Concurrency
{
    /// <summary>
    /// This is a collection of <see cref="T:ExitGames.Concurrency.Channels.Channel`1">channel</see> subscriptions.
    ///<see cref="M:Photon.SocketServer.Concurrency.UnsubscriberCollection.Dispose"/> unsubcribes all subscriptions.
    /// </summary>
    public sealed class UnsubscriberCollection : IDisposable
    {
        /// <summary>
        ///  The unsubscriber.
        /// </summary>
        private readonly IDisposable[] unsubscriber;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Photon.SocketServer.Concurrency.UnsubscriberCollection"/> class.
        /// </summary>
        /// <param name="unsubscriber">The unsubscriber.</param>
        public UnsubscriberCollection(params IDisposable[] unsubscriber)
        {
            this.unsubscriber = unsubscriber;
        }

        /// <summary>
        ///  The dispose.
        /// </summary>
        public void Dispose()
        {
            if (this.unsubscriber != null)
            {
                foreach (IDisposable disposable in this.unsubscriber)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
