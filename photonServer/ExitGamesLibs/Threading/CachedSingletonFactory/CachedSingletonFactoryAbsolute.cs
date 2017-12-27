using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Caching;
using ExitGames.Logging;

namespace ExitGames.Threading.CachedSingletonFactory
{      
    /// <summary>
    /// A <see cref="T:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2"/> for absolute caching.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    [DebuggerStepThrough]
    public class CachedSingletonFactoryAbsolute<TKey, TValue> : SynchronizedCachedSingletonFactory<TKey, TValue>
    {
        private static readonly ILogger logger;

        static CachedSingletonFactoryAbsolute()
        {
            CachedSingletonFactoryAbsolute<TKey, TValue>.logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.CachedSingletonFactory.CachedSingletonFactoryAbsolute`2 " > class. 
        /// </summary>
        /// <param name="defaultCreateMethod">The default create method.</param>
        /// <param name="removeCallback">The remove callback.</param>
        /// <param name="cacheTimeSpan">The cache time span.</param>
        /// <param name="lockTimeout">The lock timeout in ms.</param>
        public CachedSingletonFactoryAbsolute(CreateMethodDelegate<TKey, TValue> defaultCreateMethod, Action<TKey, TValue> removeCallback, TimeSpan cacheTimeSpan, int lockTimeout)
            : base(defaultCreateMethod, removeCallback, cacheTimeSpan, lockTimeout)
        {
        }

        /// <summary>
        /// Inserts an item into the asp net cache with absolute caching. 
        /// </summary>
        /// <param name="key">The key. </param>
        /// <param name="instance">The instance.</param>
        /// <param name="cacheTimeSpan">The total time to keep the instance in cache.</param>
        /// <param name="callback">The remove callback.</param>
        protected override void InsertIntoCache(string key, CacheValue<TKey, TValue> instance, TimeSpan cacheTimeSpan, CacheItemRemovedCallback callback)
        {
            //base.Cache[key] instance, null, Add(cacheTimeSpan), Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable, callback);
        }

        /// <summary>
        /// Gets or sets the absolute cache timeout. Zero values are converted to 1 second.
        /// </summary>
        public override TimeSpan CacheTimeOut
        {
            get
            {
                return base.CacheTimeOut;
            }
            set
            {
                if (value.Equals(TimeSpan.Zero))
                {
                    CachedSingletonFactoryAbsolute<TKey, TValue>.logger.Warn("Zero is not a valid cache timeout, using 1 seconds instead");
                    base.CacheTimeOut = new TimeSpan(1L);
                }
                else
                {
                    base.CacheTimeOut = value;
                }
            }
        }
    }
}
