using System;
using System.Collections.Generic;
using System.Threading;

namespace ExitGames.Threading
{
    /// <summary>
    /// A base class for a simple factory using a <see cref="T:System.Threading.ReaderWriterLockSlim"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public abstract class SimpleBaseFactory<TKey, TValue> : IDisposable
    {
        /// <summary>
        ///  The items.
        /// </summary>
        private readonly Dictionary<TKey, TValue> items = new Dictionary<TKey, TValue>();

        /// <summary>
        /// The rw lock.
        /// </summary>
        private readonly ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Finalizes an instance of the <see cref="T:ExitGames.Threading.SimpleBaseFactory`2"/> class.
        /// </summary>
        ~SimpleBaseFactory()
        {
            Dispose(false);
        }

        /// <summary>
        /// Adds a kay-value pair.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True on success, false if the key already exists.</returns>
        public bool Add(TKey key, TValue value)
        {
            bool flag;
            using (WriteLock.Enter(readerWriterLock))
            {
                if (items.ContainsKey(key))
                {
                    return false;
                }
                items.Add(key, value);
                flag = true;
            }
            return flag;
        }

        /// <summary>
        ///  Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key"> The key of the value to get. </param>
        /// <param name="value"> When this method returns, contains the value associated with the specified key, if the key is found; 
        ///  otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns>True if an element with the specified key was found; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">
        ///     <paramref name="key"/> is null.
        ///  </exception>
        public bool Exists(TKey key, out TValue value)
        {
            bool flag;
            using (ReadLock.Enter(readerWriterLock))
            {
                flag = items.TryGetValue(key, out value);
            }
            return flag;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// If the key does not exist in the dictionary a value is loaded with the <see cref="M:ExitGames.Threading.SimpleBaseFactory`2.CreateItem(`0)"/> method.
        /// </summary>
        /// <param name="key">  The key of the value to get.</param>
        /// <returns> The value.</returns>
        public TValue Get(TKey key)
        {
            TValue local;
            TValue local2;
            using (ReadLock.Enter(readerWriterLock))
            {
                if (items.TryGetValue(key, out local))
                {
                    return local;
                }
            }
            using (ReadLock.Enter(readerWriterLock))
            {
                if (!items.TryGetValue(key, out local))
                {
                    local = CreateItem(key);
                    items.Add(key, local);
                }
                local2 = local;
            }
            return local2;
        }

        /// <summary>
        ///  Removes the value with the specified key.
        /// </summary>
        /// <param name="key"> 
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// True if the element is successfully found and removed; otherwise, false. 
        ///  This method returns false if key is not found.
        ///  </returns>
        public bool Remove(TKey key)
        {
            bool flag;
            using (WriteLock.Enter(readerWriterLock))
            {
                TValue local;
                if (items.TryGetValue(key, out local))
                {
                    items.Remove(key);
                    DisposeItem(key, local);
                    return true;
                }
                flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Calls <see cref="M:ExitGames.Threading.SimpleBaseFactory`2.DisposeItem(`0,`1)"/> for each item and clears the dictionary.
        /// </summary>
        public void Reset()
        {
            using (WriteLock.Enter(readerWriterLock))
            {
                foreach (KeyValuePair<TKey, TValue> pair in items)
                {
                    DisposeItem(pair.Key, pair.Value);
                }
                items.Clear();
            }
        }

        /// <summary>
        ///  Releases all resources used by the current instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///  This method creates / loads a value for the key.
        /// </summary>
        /// <param name="key"> The key.</param>
        /// <returns>The value.</returns>
        protected abstract TValue CreateItem(TKey key);

        /// <summary>
        /// Releases all resources used by the current instance.
        /// </summary>
        /// <param name="disposing"> True if called from <see cref="M:ExitGames.Threading.SimpleBaseFactory`2.Dispose"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Reset();
                readerWriterLock.Dispose();
            }
        }

        /// <summary>
        ///  Called when a key-value pair is removed.
        /// </summary>
        /// <param name="key">  The removed key.</param>
        /// <param name="value">The removed value.</param>
        protected abstract void DisposeItem(TKey key, TValue value);

        /// <summary>
        /// Gets a reference to the underlying dictionary that contains all existing instances.
        /// </summary>
        /// <remarks>
        /// Access to this dictionary needs to be syncronized with <see cref="T:ExitGames.Threading.ReadLock"/> or <see cref="T:ExitGames.Threading.WriteLock"/>.
        /// </remarks>
        protected Dictionary<TKey, TValue> Items
        {
            get
            {
                return items;
            }
        }

        /// <summary>
        ///  Gets the used <see cref="T:System.Threading.ReaderWriterLockSlim"/>
        /// </summary>
        protected ReaderWriterLockSlim ReaderWriterLock
        {
            get
            {
                return readerWriterLock;
            }
        }
    }
}
