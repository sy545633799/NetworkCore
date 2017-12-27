using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Web.Caching;
using ExitGames.Logging;

namespace ExitGames.Threading.CachedSingletonFactory
{
    /// <summary>
    /// A <see cref="T:ExitGames.Threading.SynchronizedSingletonFactory`2"/> that uses the ASP.NET cache.
    /// It offers limited lifetime for items and requires no locking to access items that are cached.
    /// The subclasses support <see cref="T:ExitGames.Threading.CachedSingletonFactory.CachedSingletonFactorySliding`2">sliding</see> and <see cref="T:ExitGames.Threading.CachedSingletonFactory.CachedSingletonFactoryAbsolute`2">absolute</see> timeouts.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class SynchronizedCachedSingletonFactory<TKey, TValue> : SynchronizedSingletonFactory<CacheKey<TKey, TValue>, CacheValue<TKey, TValue>>
    {
        // Fields
        private static readonly ILogger logger;
        private readonly Dictionary<TKey, CacheKey<TKey, TValue>> cacheKeys;
        private readonly Cache cache;
        private readonly string name;
        private readonly ReaderWriterLockSlim readerWriterLockSlim;
        private TimeSpan cacheTimeOut;

        // Properties
        protected Cache Cache
        {
            get
            {
                return this.cache;
            }
        }

        public virtual TimeSpan CacheTimeOut
        {
            get
            {
                return this.cacheTimeOut;
            }
            set
            {
                this.cacheTimeOut = value;
            }
        }

        public CreateMethodDelegate<TKey, TValue> DoCreateMethod { get; set; }

        public Action<TKey, TValue> RemoveCallback { get; set; }

        // Methods
        private CacheKey<TKey, TValue> GetCacheKey(TKey local1, CreateMethodDelegate<TKey, TValue> delegate1)
        {
            Guid guid = Guid.NewGuid();
            return new CacheKey<TKey, TValue>(this.name + guid.ToString("D"), local1, delegate1);
        }

        private void CacheItemRemove(string name, object obj1, CacheItemRemovedReason reason1)
        {
            try
            {
                CacheValue<TKey, TValue> value2 = (CacheValue<TKey, TValue>)obj1;
                if (reason1 != CacheItemRemovedReason.Removed)
                {
                    base.Remove(value2.Key);
                }
                this.TryCacheItemRemove(value2.Key.Key);
                if (this.RemoveCallback != null)
                {
                    this.RemoveCallback(value2.Key.Key, value2.Value);
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                SynchronizedCachedSingletonFactory<TKey, TValue>.logger.Error(exception);
            }
        }

        private void TryCacheItemRemove(TKey local1)
        {
            IDisposable disposable = WriteLock.TryEnter(this.readerWriterLockSlim, base.LockTimeout);
            try
            {
                this.cacheKeys.Remove(local1);
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private void TryAdd(TKey local1, CacheKey<TKey, TValue> key1)
        {
            IDisposable disposable = WriteLock.TryEnter(this.readerWriterLockSlim, base.LockTimeout);
            try
            {
                this.cacheKeys.Add(local1, key1);
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        private bool CacheTryGet(TKey key, out CacheKey<TKey, TValue> cacheKey)
        {
            bool flag;
            IDisposable disposable = ReadLock.TryEnter(this.readerWriterLockSlim, base.LockTimeout);
            try
            {
                flag = this.cacheKeys.TryGetValue(key, out cacheKey);
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return flag;
        }

        static SynchronizedCachedSingletonFactory()
        {
            SynchronizedCachedSingletonFactory<TKey, TValue>.logger = LogManager.GetCurrentClassLogger();
        }

        protected SynchronizedCachedSingletonFactory(CreateMethodDelegate<TKey, TValue> defaultCreateMethod, Action<TKey, TValue> removeCallback, TimeSpan cacheTimeSpan, int lockTimeout)
            : base(null, lockTimeout)
        {
            this.cacheKeys = new Dictionary<TKey, CacheKey<TKey, TValue>>();
            this.readerWriterLockSlim = new ReaderWriterLockSlim();
            this.cache = new Cache();      
            this.name =  Guid.NewGuid().ToString("D")+".";
            this.CacheTimeOut = cacheTimeSpan;
            this.DoCreateMethod = defaultCreateMethod;
            base.CreateMethod = new CreateMethodDelegate<CacheKey<TKey, TValue>, CacheValue<TKey, TValue>>(this.Create);
            this.RemoveCallback = removeCallback;
        }

        private bool Create(CacheKey<TKey, TValue> key, out CacheValue<TKey, TValue> value)
        {
            TValue local;
            if (this.DoCreateMethod(key.Key, out local))
            {
                value = new CacheValue<TKey, TValue>(key, local);
                return true;
            }
            value = new CacheValue<TKey, TValue>();
            return false;
        }

        protected override void DoAdd(CacheKey<TKey, TValue> key, CacheValue<TKey, TValue> value)
        {
            base.DoAdd(key, value);
            this.TryAdd(key.Key, key);
            this.InsertIntoCache(key.AspCacheKey, value, this.cacheTimeOut, new CacheItemRemovedCallback(this.CacheItemRemove));
        }

        public TValue Get(TKey key)
        {
            return this.Get(key, this.DoCreateMethod);
        }

        public virtual TValue Get(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            return this.GetBlockingInstance(key, createMethod);
        }

        public TValue GetBlockingInstance(TKey key)
        {
            return this.GetBlockingInstance(key, this.DoCreateMethod);
        }

        public TValue GetBlockingInstance(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            CacheKey<TKey, TValue> key2;
            if (!this.CacheTryGet(key, out key2))
            {
                key2 = this.GetCacheKey(key, createMethod);
            }
            return base.GetBlockingInstance(key2).Value;
        }

        public TValue GetBlockingOverall(TKey key)
        {
            return this.GetBlockingOverall(key, this.DoCreateMethod);
        }

        public TValue GetBlockingOverall(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            CacheKey<TKey, TValue> key2;
            if (!this.CacheTryGet(key, out key2))
            {
                key2 = this.GetCacheKey(key, createMethod);
            }
            return base.GetBlockingOverall(key2).Value;
        }

        public TValue GetNonBlocking(TKey key)
        {
            return this.GetNonBlocking(key, this.DoCreateMethod);
        }

        public TValue GetNonBlocking(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            CacheKey<TKey, TValue> key2;
            if (!this.CacheTryGet(key, out key2))
            {
                key2 = this.GetCacheKey(key, createMethod);
            }
            return base.GetNonBlocking(key2).Value;
        }

        protected abstract void InsertIntoCache(string key, CacheValue<TKey, TValue> instance, TimeSpan cacheTimeSpan, CacheItemRemovedCallback callback);
        public override bool Remove(CacheKey<TKey, TValue> key)
        {
            if (base.Remove(key))
            {
                this.cache.Remove(key.AspCacheKey);
                return true;
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            CacheKey<TKey, TValue> key2;
            return (this.CacheTryGet(key, out key2) && this.Remove(key2));
        }

        public override bool TryGet(CacheKey<TKey, TValue> key, out CacheValue<TKey, TValue> value)
        {
            object obj2 = this.cache[key.AspCacheKey];
            if (obj2 == null)
            {
                return base.TryGet(key, out value);
            }
            value = (CacheValue<TKey, TValue>)obj2;
            return true;
        }

        public virtual bool TryGet(TKey key, out TValue value)
        {
            CacheKey<TKey, TValue> key2;
            CacheValue<TKey, TValue> value2;
            if (this.CacheTryGet(key, out key2) && this.TryGet(key2, out value2))
            {
                value = value2.Value;
                return true;
            }
            value = default(TValue);
            return false;
        }
    }

}
