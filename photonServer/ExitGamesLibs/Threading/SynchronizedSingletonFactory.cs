using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExitGames.Threading
{
    /// <summary>
    /// This class is used to create instances that are unique per key in a multi-threaded environment.
    ///  It uses a <see cref="T:System.Threading.ReaderWriterLockSlim"/> for read and write access to the <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.Instances"/>. 
    ///  Instance creations are synchronized with a <see cref="T:System.Threading.Monitor"/> on an object that is unique per key.
    ///  This approach is designed to minimize the impact of long running instance creations on other threads.
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TValue">The type of value.</typeparam>
    /// <remarks>
    /// Instance members are thread safe unless specified otherwise. 
    /// </remarks>
    [DebuggerStepThrough]
    public class SynchronizedSingletonFactory<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> instances;
        private readonly int lockTimeout;
        private readonly ReaderWriterLockSlim readerWriterLockSlim;
        private readonly Dictionary<TKey, object> syncObjects;
        private CreateMethodDelegate<TKey, TValue> defaultCreateMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Threading.SynchronizedSingletoFactory`2"/> class.
        /// </summary>
        /// <param name="defaultCreateMethod">The default create method.</param>
        /// <param name="lockTimeout">The max timeout to wait to enter a critical section.</param>
        public SynchronizedSingletonFactory(CreateMethodDelegate<TKey, TValue> defaultCreateMethod, int lockTimeout)
        {
            this.syncObjects = new Dictionary<TKey, object>();
            this.lockTimeout = lockTimeout;
            this.readerWriterLockSlim = new ReaderWriterLockSlim();
            this.defaultCreateMethod = defaultCreateMethod;
            this.instances = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Adds a value if the key has not been added before. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if the value was added.</returns>
        public virtual bool Add(TKey key, TValue value)
        {
            IDisposable disposable = this.WriterLock();
            try
            {
                if (!this.Instances.ContainsKey(key))
                {
                    this.DoAdd(key, value);
                    return true;
                }
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a value to the <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.Instances"/> and the cache. 
        /// Calling methods need to guard the <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.Instances"/> with a <see cref="M:ExitGames.Threading.SynchronizedSingletonFactory`2.WriterLock"/>.
        /// Calling methods are: <see cref="M:ExitGames.Threading.SynchronizedSingletonFactory`2.Add(`0,`1)"/>,
        ///<see cref="M:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.GetBlockingInstance(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">GetBlockingInstance</see>,
        ///<see cref="M:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.GetNonBlocking(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">GetNonBlocking</see> and
        ///<see cref="M:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.GetBlockingOverall(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">GetBlockingOverall</see>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        protected virtual void DoAdd(TKey key, TValue value)
        {
            this.Instances.Add(key, value);
        }

        /// <summary>
        /// This method iterates over a copy of <see cref="P:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Instances"/> and executes the <paramref name="action"/> on each item.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ForAll(Action<TValue> action)
        {
            Dictionary<TKey, TValue> dictionary;
            IDisposable disposable = this.ReaderLock();
            try
            {
                dictionary = new Dictionary<TKey, TValue>(this.Instances);
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            foreach (TValue local in dictionary.Values)
            {
                action(local);
            }
        }

        /// <summary>
        /// This method iterates over a copy of <see cref="P:ExitGames.Threading.UnsynchronizedSingletonFactory`2.Instances"/>.
        /// The <paramref name="selector"/> parameter selects a value of each instance.
        /// These values are combined with the <paramref name="aggregateFunction"/>.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value.</typeparam>
        /// <param name="selector">The action that maps a value to each instance.</param>
        /// <param name="aggregateFunction">The function that combines all selector results.</param>
        /// <param name="seed">The result value to start with.</param>
        /// <returns>An aggregegated value from all instances.</returns>
        public TResult ForAll<TResult>(Func<TValue, TResult> selector, Func<TResult, TResult, TResult> aggregateFunction, TResult seed)
        {
            Dictionary<TKey, TValue> dictionary;
            IDisposable disposable = this.ReaderLock();
            try
            {
                dictionary = new Dictionary<TKey, TValue>(this.Instances);
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            return dictionary.Values.Select<TValue, TResult>(selector).Aggregate<TResult, TResult>(seed, aggregateFunction);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one with the default <see cref="P:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.DoCreateMethod"/>.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value for the key. </returns>
        public TValue Get(TKey key)
        {
            return this.Get(key, this.defaultCreateMethod);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="createMethod">The creation method.</param>
        /// <returns>The value.</returns>
        /// <remarks>
        /// The default implementation uses the <see cref="M:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.GetBlockingInstance(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">GetBlockingInstance</see> algorithm.
        /// Override to change the behavior to
        /// <see cref="M:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.GetNonBlocking(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">GetNonBlocking</see> 
        /// or
        /// <see cref="M:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.GetBlockingOverall(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">GetBlockingOverall</see>
        /// </remarks>
        public virtual TValue Get(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            return this.GetBlockingInstance(key, createMethod);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one with the default <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.CreateMethod"/>.
        /// The creation of a new instance is guarded with a sync root that is unique per key.
        /// This algorithm is ideal for creation methods that do not return fast.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public TValue GetBlockingInstance(TKey key)
        {
            return this.GetBlockingInstance(key, this.defaultCreateMethod);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one. The creation of a new instance is guarded with a sync root that is unique per key. This algorithm is ideal for creation methods that do not return fast. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="createMethod">The creation method.</param>
        /// <returns>The value.</returns>
        public TValue GetBlockingInstance(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            TValue local;
            if (!this.TryGet(key, out local))
            {
                object obj2;
                bool flag;
                IDisposable disposable = Lock.TryEnter(this.syncObjects, this.lockTimeout);
                try
                {
                    if (!this.syncObjects.TryGetValue(key, out obj2))
                    {
                        obj2 = new object();
                        this.syncObjects.Add(key, obj2);
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                if (flag)
                {
                    try
                    {
                        this.GetBlockingInstance(key, createMethod, obj2, out local);
                        return local;
                    }
                    finally
                    {
                        IDisposable disposable2 = Lock.TryEnter(this.syncObjects, this.lockTimeout);
                        try
                        {
                            this.syncObjects.Remove(key);
                        }
                        finally
                        {
                            if (disposable2 != null)
                            {
                                disposable2.Dispose();
                            }
                        }
                    }
                }
                this.GetBlockingInstance(key, createMethod, obj2, out local);
            }
            return local;
        }

        /// <summary>
        /// Helper method of <see cref="M:ExitGames.Threading.SynchronizedSingletonFactory`2.GetBlockingInstance(`0,ExitGames.Threading.CreateMethodDelegate{`0,`1})">GetBlockingInstance</see> that creates the instance.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="createMethod">The creation method method. </param>
        /// <param name="syncObject">The sync object for the key.</param>
        /// <param name="result">The result value.</param>
        private void GetBlockingInstance(TKey key, CreateMethodDelegate<TKey, TValue> createMethod, object syncObject, out TValue result)
        {
            IDisposable disposable = Lock.TryEnter(syncObject, this.lockTimeout);
            try
            {
                if (!this.TryGet(key, out result) && createMethod(key, out result))
                {
                    IDisposable disposable2 = this.WriterLock();
                    try
                    {
                        TValue local;
                        if (!this.Instances.TryGetValue(key, out local))
                        {
                            this.DoAdd(key, result);
                        }
                        else
                        {
                            result = local;
                        }
                    }
                    finally
                    {
                        if (disposable2 != null)
                        {
                            disposable2.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one with the default <see cref="P:ExitGames.Threading.CachedSingletonFactory.SynchronizedCachedSingletonFactory`2.DoCreateMethod"/>.
        /// The creation of a new instance is guarded with a global <see cref="M:ExitGames.Threading.SynchronizedSingletonFactory`2.WriterLock"/>.
        /// This algorithm is ideal for creation methods that return very fast.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public TValue GetBlockingOverall(TKey key)
        {
            return this.GetBlockingOverall(key, this.defaultCreateMethod);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one.
        ///  The creation of a new instance is guarded with a global <see cref="M:ExitGames.Threading.SynchronizedSingletonFactory`2.WriterLock"/>.
        ///  This algorithm is ideal for creation methods that return very fast.
        /// </summary>
        /// <param name="key">The key. </param>
        /// <param name="createMethod">The creation method.</param>
        /// <returns>The value.</returns>
        public TValue GetBlockingOverall(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            TValue local;
            if (!this.TryGet(key, out local))
            {
                IDisposable disposable = this.WriterLock();
                try
                {
                    if (this.Instances.TryGetValue(key, out local))
                    {
                        return local;
                    }
                    if (createMethod(key, out local))
                    {
                        this.DoAdd(key, local);
                        return local;
                    }
                    return default(TValue);
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            return local;
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one with the default <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.CreateMethod"/>.
        /// The creation of a new instance is not guarded. This introduces a risk that the creation method is called multiple times for the same key at the same time. Only one of the created values is added.
        /// This algorithm is ideal for creation methods that are either not likely to be called multiple times at the same time or that have an unpredictable execution time and a low usage of local reosurces. 
        /// </summary>
        /// <param name="key">The key. </param>
        /// <returns>The value. </returns>
        public TValue GetNonBlocking(TKey key)
        {
            return this.GetNonBlocking(key, this.defaultCreateMethod);
        }

        /// <summary>
        /// Gets an existing value for a key or creates a new one. The creation of a new instance is not guarded. This introduces a risk that the creation method is called multiple times for the same key at the same time. Only one of the created values is added. This algorithm is ideal for creation methods that are either not likely to be called multiple times at the same time or that have an unpredictable execution time and a low usage of local reosurces. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="createMethod">The creation method.</param>
        /// <returns>The value.</returns>
        public TValue GetNonBlocking(TKey key, CreateMethodDelegate<TKey, TValue> createMethod)
        {
            TValue local;
            TValue local2;
            if (this.TryGet(key, out local))
            {
                return local;
            }
            if (createMethod(key, out local2))
            {
                IDisposable disposable = this.WriterLock();
                try
                {
                    if (!this.Instances.TryGetValue(key, out local))
                    {
                        local = local2;
                        this.DoAdd(key, local);
                    }
                    return local;
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
            return default(TValue);
        }

        /// <summary>
        /// Enters a critical read section. Exit the critical section by disposing the return value. 
        /// </summary>
        /// <returns>A disposable read lock. </returns>
        /// <exception cref="T:ExitGames.Threading.LockTimeoutException">
        /// A read lock could not be obtained within the <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.LockTimeout"/>.
        /// </exception>
        protected IDisposable ReaderLock()
        {
            return ReadLock.TryEnter(this.readerWriterLockSlim, this.lockTimeout);
        }

        /// <summary>
        /// Removes a value from the <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.Instances"/>. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if key was found and removed, otherwise false.</returns>
        public virtual bool Remove(TKey key)
        {
            bool flag;
            IDisposable disposable = this.WriterLock();
            try
            {
                flag = this.Instances.Remove(key);
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

        /// <summary>
        /// Tries to get an existing value for the key. 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if a value was found, otherwise false.</returns>
        public virtual bool TryGet(TKey key, out TValue value)
        {
            bool flag;
            IDisposable disposable = this.ReaderLock();
            try
            {
                flag = this.Instances.TryGetValue(key, out value);
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

        /// <summary>
        /// Enters a critical write section. Exit the critical section by disposing the return value. 
        /// </summary>
        /// <returns>A disposable write lock.</returns>
        /// <exception cref="T:ExitGames.Threading.LockTimeoutException">
        /// A write lock could not be obtained within the <see cref="P:ExitGames.Threading.SynchronizedSingletonFactory`2.LockTimeout"/>.
        /// </exception>
        protected IDisposable WriterLock()
        {
            return WriteLock.TryEnter(this.readerWriterLockSlim, this.lockTimeout);
        }

        /// <summary>
        /// Gets the number of added values. 
        /// </summary>
        public int Count
        {
            get
            {
                int count;
                IDisposable disposable = this.ReaderLock();
                try
                {
                    count = this.Instances.Count;
                }
                finally
                {
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Gets or sets the default creation method for values. 
        /// </summary>
        public CreateMethodDelegate<TKey, TValue> CreateMethod
        {
            get
            {
                return this.defaultCreateMethod;
            }
            set
            {
                Interlocked.Exchange<CreateMethodDelegate<TKey, TValue>>(ref this.defaultCreateMethod, value);
            }
        }

        /// <summary>
        /// Gets a reference to the underlying dictionary that contains all existing instances. 
        /// </summary>
        /// <remarks>
        /// Access to this dictionary needs to be syncronized with <see cref="T:ExitGames.Threading.ReadLock"/> or <see cref="T:ExitGames.Threading.WriteLock"/>. 
        /// </remarks>
        protected Dictionary<TKey, TValue> Instances
        {
            get
            {
                return this.instances;
            }
        }

        /// <summary>
        /// Gets the maxium timeout for critical sections.
        /// </summary>
        protected int LockTimeout
        {
            get
            {
                return this.lockTimeout;
            }
        }
    }

}
