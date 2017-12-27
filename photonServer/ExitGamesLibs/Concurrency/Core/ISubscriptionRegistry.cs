using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExitGames.Concurrency.Core
{
    /// <summary>
    /// Allows for the registration and deregistration of subscriptions 
    /// </summary>
    public interface ISubscriptionRegistry
    {
        /// <summary>
        /// Deregister a subscription. 
        /// </summary>
        /// <param name="toRemove"></param>
        /// <returns></returns>
        bool DeregisterSubscription(IDisposable toRemove);
        /// <summary>
        /// Register subscription to be unsubcribed from when the fiber is disposed. 
        /// </summary>
        /// <param name="toAdd"></param>
        void RegisterSubscription(IDisposable toAdd);
    }
}
